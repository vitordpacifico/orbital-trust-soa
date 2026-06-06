namespace OrbitalTrust.Api.Domain.Enums;

/// <summary>
/// Tipo do sensor que produziu a leitura. Cada tipo tem comportamento próprio
/// no cálculo de confiança (ver SensorOptico / SensorTermico) — base do POLIMORFISMO.
/// </summary>
public enum TipoSensor
{
    Optico,
    Termico
}
