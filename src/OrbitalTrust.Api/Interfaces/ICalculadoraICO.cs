using OrbitalTrust.Api.Domain.ValueObjects;

namespace OrbitalTrust.Api.Interfaces;

/// <summary>
/// INTERFACE — contrato do cálculo do ICO. Injetada via DI.
/// </summary>
public interface ICalculadoraICO
{
    IndiceConfiabilidade Calcular(double confiancaSensor, double confiancaAnalise);
}
