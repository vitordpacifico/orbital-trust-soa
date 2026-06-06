namespace OrbitalTrust.Api.Domain.Entities;

/// <summary>
/// COMPOSIÇÃO — um satélite agrega vários sensores (lista polimórfica de SensorBase).
/// </summary>
public class Satelite
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    // Lista polimórfica: pode conter SensorOptico e SensorTermico misturados.
    public List<SensorBase> Sensores { get; set; } = new();
}
