using OrbitalTrust.Api.Services;
using Xunit;

namespace OrbitalTrust.Tests;

public class CalculadoraICOTests
{
    [Fact]
    public void Calcular_ComConfiancasAltas_RetornaIcoAltoCategoriaAlta()
    {
        var calc = new CalculadoraICO();

        // ICO = (0.9*0.6 + 0.8*0.4)*100 = (0.54 + 0.32)*100 = 86
        var ico = calc.Calcular(confiancaSensor: 0.9, confiancaAnalise: 0.8);

        Assert.Equal(86, ico.Valor, precision: 6);
        Assert.Equal("ALTA", ico.Categoria);
    }

    [Theory]
    [InlineData(0.9, 0.8, "ALTA")]   // 86
    [InlineData(0.6, 0.5, "MEDIA")]  // (0.36 + 0.20)*100 = 56
    [InlineData(0.2, 0.3, "BAIXA")]  // (0.12 + 0.12)*100 = 24
    public void Calcular_CategoriaCoerenteComValor(double cs, double ca, string categoriaEsperada)
    {
        var calc = new CalculadoraICO();
        var ico = calc.Calcular(cs, ca);
        Assert.Equal(categoriaEsperada, ico.Categoria);
    }
}
