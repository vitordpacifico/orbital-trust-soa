using OrbitalTrust.Api.Domain.Entities;
using Xunit;

namespace OrbitalTrust.Tests;

public class SensorPolimorfismoTests
{
    [Fact]
    public void MesmaNuvemAlta_OpticoETermico_RetornamConfiancasDiferentes()
    {
        // POLIMORFISMO — mesma entrada, fórmulas diferentes por subtipo.
        SensorBase optico = new SensorOptico();
        SensorBase termico = new SensorTermico();

        double coberturaNuvem = 0.8;
        double qualidadeImagem = 1.0;

        var confOptico = optico.CalcularConfiancaLeitura(coberturaNuvem, qualidadeImagem);   // 1*(1-0.8)=0.2
        var confTermico = termico.CalcularConfiancaLeitura(coberturaNuvem, qualidadeImagem); // 1*(1-0.32)=0.68

        Assert.NotEqual(confOptico, confTermico);
        // Térmico enxerga melhor através da nuvem -> confiança maior.
        Assert.True(confTermico > confOptico);
        Assert.Equal(0.2, confOptico, precision: 6);
        Assert.Equal(0.68, confTermico, precision: 6);
    }

    [Fact]
    public void RegistrarLeitura_IncrementaContadorEstaticoGlobal()
    {
        // MEMBRO ESTÁTICO — TotalLeiturasGlobais é compartilhado por todas as instâncias.
        var antes = SensorBase.TotalLeiturasGlobais;
        var sensor = new SensorOptico();

        sensor.RegistrarLeituraProcessada();

        Assert.Equal(antes + 1, SensorBase.TotalLeiturasGlobais);
        Assert.Equal(1, sensor.LeiturasProcessadas);
    }
}
