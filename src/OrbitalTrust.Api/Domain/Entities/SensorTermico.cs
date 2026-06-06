using OrbitalTrust.Api.Domain.Enums;

namespace OrbitalTrust.Api.Domain.Entities;

/// <summary>
/// HERANÇA — sensor térmico herda de SensorBase.
/// </summary>
public class SensorTermico : SensorBase
{
    public SensorTermico()
    {
        Tipo = TipoSensor.Termico;
    }

    /// <summary>
    /// POLIMORFISMO — o térmico enxerga PARCIALMENTE através da nuvem: a nuvem só atrapalha 40%.
    /// confianca = qualidadeImagem * (1 - coberturaNuvem * 0.4)
    /// (Mesmo input, fórmula diferente do SensorOptico — eis o polimorfismo.)
    /// </summary>
    public override double CalcularConfiancaLeitura(double coberturaNuvem, double qualidadeImagem)
    {
        return qualidadeImagem * (1 - coberturaNuvem * 0.4);
    }
}
