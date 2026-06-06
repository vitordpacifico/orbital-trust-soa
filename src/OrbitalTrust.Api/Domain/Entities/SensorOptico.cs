using OrbitalTrust.Api.Domain.Enums;

namespace OrbitalTrust.Api.Domain.Entities;

/// <summary>
/// HERANÇA — sensor óptico herda de SensorBase.
/// </summary>
public class SensorOptico : SensorBase
{
    public SensorOptico()
    {
        Tipo = TipoSensor.Optico;
    }

    /// <summary>
    /// POLIMORFISMO — o óptico é BLOQUEADO por nuvem: quanto mais nuvem, menos confiança.
    /// confianca = qualidadeImagem * (1 - coberturaNuvem)
    /// (Mesmo input, fórmula diferente do SensorTermico — eis o polimorfismo.)
    /// </summary>
    public override double CalcularConfiancaLeitura(double coberturaNuvem, double qualidadeImagem)
    {
        return qualidadeImagem * (1 - coberturaNuvem);
    }
}
