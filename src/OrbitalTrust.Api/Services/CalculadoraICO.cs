using OrbitalTrust.Api.Domain.ValueObjects;
using OrbitalTrust.Api.Interfaces;

namespace OrbitalTrust.Api.Services;

/// <summary>
/// LÓGICA DE NEGÓCIO CENTRAL — calcula o ICO (Índice de Confiabilidade Orbital).
/// Implementa ICalculadoraICO (registrada via DI).
/// </summary>
public class CalculadoraICO : ICalculadoraICO
{
    /// <summary>
    /// ICO = (confiancaSensor * 0.6 + confiancaAnalise * 0.4) * 100.
    /// O sensor pesa mais que a análise: confiar na origem do dado vem primeiro.
    /// </summary>
    public IndiceConfiabilidade Calcular(double confiancaSensor, double confiancaAnalise)
    {
        var ico = (confiancaSensor * 0.6 + confiancaAnalise * 0.4) * 100;
        return IndiceConfiabilidade.DeValor(ico);
    }
}
