using OrbitalTrust.Api.Domain.Entities;
using OrbitalTrust.Api.Domain.Enums;
using OrbitalTrust.Api.Domain.Exceptions;
using OrbitalTrust.Api.Domain.ValueObjects;
using OrbitalTrust.Api.DTOs;
using OrbitalTrust.Api.Interfaces;

namespace OrbitalTrust.Api.Services;

/// <summary>
/// ORQUESTRADOR DO PIPELINE — é o método central do sistema. Recebe a leitura,
/// calcula o ICO, aplica a "regra de ouro" (não gritar alerta em cima de dado ruim),
/// criptografa a coordenada e persiste leitura + (eventual) alerta.
///
/// INJEÇÃO DE DEPENDÊNCIA — todas as dependências chegam pelo construtor via interfaces.
/// </summary>
public class ProcessamentoService
{
    private readonly ICalculadoraICO _calculadora;
    private readonly ICryptoService _crypto;
    private readonly ILeituraRepository _leituraRepo;
    private readonly IAlertaRepository _alertaRepo;

    public ProcessamentoService(
        ICalculadoraICO calculadora,
        ICryptoService crypto,
        ILeituraRepository leituraRepo,
        IAlertaRepository alertaRepo)
    {
        _calculadora = calculadora;
        _crypto = crypto;
        _leituraRepo = leituraRepo;
        _alertaRepo = alertaRepo;
    }

    public async Task<AlertaOutputDTO> ProcessarLeitura(LeituraInputDTO dto)
    {
        // 1. VALIDAR o DTO — valores normalizados precisam estar em [0,1].
        ValidarFaixa(dto.CoberturaNuvem, nameof(dto.CoberturaNuvem));
        ValidarFaixa(dto.QualidadeImagem, nameof(dto.QualidadeImagem));
        ValidarFaixa(dto.ConfiancaAnalise, nameof(dto.ConfiancaAnalise));
        ValidarFaixa(dto.RiscoEstimado, nameof(dto.RiscoEstimado));

        // 2. INSTANCIAR O SENSOR CERTO conforme o tipo (fábrica simples).
        SensorBase sensor = dto.TipoSensor switch
        {
            TipoSensor.Optico => new SensorOptico { Id = dto.SensorId, Nome = "Sensor Óptico" },
            TipoSensor.Termico => new SensorTermico { Id = dto.SensorId, Nome = "Sensor Térmico" },
            _ => throw new LeituraInvalidaException($"Tipo de sensor desconhecido: {dto.TipoSensor}.")
        };

        // Sistemas críticos: sensor offline não produz leitura confiável.
        if (!sensor.Online)
            throw new SensorOfflineException($"Sensor {sensor.Id} está offline.");

        // 3. POLIMORFISMO — a mesma chamada produz fórmulas diferentes por tipo de sensor.
        var confiancaSensor = sensor.CalcularConfiancaLeitura(dto.CoberturaNuvem, dto.QualidadeImagem);
        sensor.RegistrarLeituraProcessada(); // encapsulamento + contador estático

        // 4. Construir o VALUE OBJECT Coordenada (valida lat/long; pode lançar exceção).
        var coordenada = new Coordenada(dto.Latitude, dto.Longitude);

        // 5. Calcular o ICO.
        var ico = _calculadora.Calcular(confiancaSensor, dto.ConfiancaAnalise);

        // 6. REGRA DE OURO — cruzar risco estimado (ML) com o ICO.
        var (nivel, mensagem) = DecidirNivel(dto, ico);

        // 7. CRIPTOGRAFAR a coordenada (AES-256-GCM) — nunca persistir em claro.
        var coordCriptografada = _crypto.Encriptar(coordenada.Serializar());

        var dataHora = dto.DataHora ?? DateTime.UtcNow; // DateTime

        // 8. Persistir a leitura.
        var leitura = new LeituraAmbiental
        {
            SensorId = dto.SensorId,
            TipoSensor = dto.TipoSensor,
            DataHora = dataHora,
            CoberturaNuvem = dto.CoberturaNuvem,
            QualidadeImagem = dto.QualidadeImagem,
            ConfiancaAnalise = dto.ConfiancaAnalise,
            RiscoEstimado = dto.RiscoEstimado,
            TipoEvento = dto.TipoEvento,
            CoordenadaCriptografada = coordCriptografada,
            IcoValor = ico.Valor,
            NivelRisco = nivel
        };
        var leituraId = await _leituraRepo.Adicionar(leitura);

        // Só emite alerta se o nível for Moderado ou maior.
        Alerta? alerta = null;
        if (nivel >= NivelRisco.Moderado)
        {
            alerta = new Alerta
            {
                LeituraId = leituraId,
                DataHora = dataHora,
                TipoEvento = dto.TipoEvento,
                NivelRisco = nivel,
                IcoValor = ico.Valor,
                IcoCategoria = ico.Categoria,
                Mensagem = mensagem,
                CoordenadaCriptografada = coordCriptografada
            };
            await _alertaRepo.Adicionar(alerta);
        }

        // 9. Montar o DTO de saída — DESCRIPTOGRAFANDO a coordenada (prova do decrypt).
        var coordDecifrada = Coordenada.Desserializar(_crypto.Decriptar(coordCriptografada));

        return new AlertaOutputDTO
        {
            Id = alerta?.Id ?? leituraId,
            DataHora = dataHora,
            TipoEvento = dto.TipoEvento,
            NivelRisco = nivel,
            Ico = Math.Round(ico.Valor, 2),
            IcoCategoria = ico.Categoria,
            Latitude = coordDecifrada.Latitude,
            Longitude = coordDecifrada.Longitude,
            Mensagem = mensagem
        };
    }

    private static void ValidarFaixa(double valor, string campo)
    {
        if (valor < 0 || valor > 1)
            throw new LeituraInvalidaException($"Campo '{campo}' = {valor} fora do intervalo [0,1].");
    }

    /// <summary>
    /// A "regra de ouro" do Orbital Trust: não dispara alarme forte em cima de dado de baixa
    /// confiabilidade. O nível final combina o risco estimado pelo ML com o ICO.
    /// </summary>
    private static (NivelRisco nivel, string mensagem) DecidirNivel(LeituraInputDTO dto, IndiceConfiabilidade ico)
    {
        var icoTxt = $"ICO {Math.Round(ico.Valor)}";

        if (dto.RiscoEstimado >= 0.7)
        {
            if (ico.Valor >= 50)
            {
                // Risco alto E dado confiável -> alerta forte.
                var critico = dto.RiscoEstimado >= 0.9 && ico.Valor >= 75;
                var nivel = critico ? NivelRisco.Critico : NivelRisco.Alto;
                var nivelTxt = critico ? "CRÍTICO" : "ALTO";
                return (nivel,
                    $"{dto.TipoEvento} — risco {nivelTxt}, confiabilidade {ico.Categoria} ({icoTxt}).");
            }

            // Risco alto MAS dado ruim -> rebaixa para Moderado (regra de ouro).
            return (NivelRisco.Moderado,
                $"{dto.TipoEvento} — possível risco alto, porém dado de BAIXA confiabilidade ({icoTxt}). " +
                "Verificação recomendada antes de acionar resposta.");
        }

        return (NivelRisco.Baixo,
            $"{dto.TipoEvento} — risco baixo, confiabilidade {ico.Categoria} ({icoTxt}).");
    }
}
