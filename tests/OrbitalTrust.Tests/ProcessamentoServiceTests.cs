using Microsoft.Extensions.Configuration;
using OrbitalTrust.Api.Domain.Enums;
using OrbitalTrust.Api.Domain.Exceptions;
using OrbitalTrust.Api.DTOs;
using OrbitalTrust.Api.Services;
using Xunit;

namespace OrbitalTrust.Tests;

public class ProcessamentoServiceTests
{
    private static ProcessamentoService CriarServico(out FakeAlertaRepository alertaRepo)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Crypto:Key"] = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY="
            })
            .Build();

        var crypto = new AesGcmCryptoService(config);
        var calc = new CalculadoraICO();
        var leituraRepo = new FakeLeituraRepository();
        alertaRepo = new FakeAlertaRepository();

        return new ProcessamentoService(calc, crypto, leituraRepo, alertaRepo);
    }

    private static LeituraInputDTO LeituraValida() => new()
    {
        SensorId = Guid.NewGuid(),
        TipoSensor = TipoSensor.Optico,
        Latitude = -23.5505,
        Longitude = -46.6333,
        CoberturaNuvem = 0.1,
        QualidadeImagem = 0.95,
        ConfiancaAnalise = 0.9,
        RiscoEstimado = 0.5,
        TipoEvento = TipoEventoAmbiental.Queimada
    };

    [Fact]
    public async Task ProcessarLeitura_ComValorForaDeFaixa_LancaLeituraInvalida()
    {
        var service = CriarServico(out _);
        var dto = LeituraValida();
        dto.CoberturaNuvem = 1.5; // fora de [0,1]

        await Assert.ThrowsAsync<LeituraInvalidaException>(() => service.ProcessarLeitura(dto));
    }

    [Fact]
    public async Task ProcessarLeitura_RiscoAltoEIcoAlto_GeraAlertaNivelAltoOuCritico()
    {
        var service = CriarServico(out var alertaRepo);
        var dto = LeituraValida();
        // Térmico com pouca nuvem + qualidade/confiança altas -> ICO alto.
        dto.TipoSensor = TipoSensor.Termico;
        dto.CoberturaNuvem = 0.0;
        dto.QualidadeImagem = 1.0;
        dto.ConfiancaAnalise = 1.0;
        dto.RiscoEstimado = 0.95; // alto

        var resultado = await service.ProcessarLeitura(dto);

        Assert.True(resultado.NivelRisco is NivelRisco.Alto or NivelRisco.Critico);
        Assert.Single(alertaRepo.Itens); // alerta foi persistido
        // Coordenada de saída vem descriptografada corretamente.
        Assert.Equal(-23.5505, resultado.Latitude, precision: 4);
    }

    [Fact]
    public async Task ProcessarLeitura_RiscoAltoMasIcoBaixo_RebaixaParaModerado()
    {
        // REGRA DE OURO — risco alto, mas dado de baixa confiabilidade -> Moderado.
        var service = CriarServico(out _);
        var dto = LeituraValida();
        dto.TipoSensor = TipoSensor.Optico;
        dto.CoberturaNuvem = 0.95; // muita nuvem derruba a confiança do óptico
        dto.QualidadeImagem = 0.3;
        dto.ConfiancaAnalise = 0.2;
        dto.RiscoEstimado = 0.85; // alto

        var resultado = await service.ProcessarLeitura(dto);

        Assert.Equal(NivelRisco.Moderado, resultado.NivelRisco);
        Assert.Contains("BAIXA", resultado.Mensagem.ToUpperInvariant());
    }
}
