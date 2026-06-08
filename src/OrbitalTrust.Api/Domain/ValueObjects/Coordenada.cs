using OrbitalTrust.Api.Domain.Exceptions;

namespace OrbitalTrust.Api.Domain.ValueObjects;

/// <summary>
/// VALUE OBJECT imutável (readonly record struct) — representa um ponto geográfico.
/// Implementado como struct por ser um tipo de valor pequeno, imutável e sem identidade
/// própria: duas coordenadas com a mesma lat/long são iguais. Valida o intervalo no
/// construtor (invariante sempre garantida) e sabe se serializar para persistência.
/// </summary>
public readonly record struct Coordenada
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Coordenada(double latitude, double longitude)
    {
        // Invariante do VO: coordenada só existe se for geograficamente válida.
        if (latitude < -90 || latitude > 90)
            throw new CoordenadaForaDeAlcanceException(
                $"Latitude {latitude} fora do intervalo válido [-90, 90].");

        if (longitude < -180 || longitude > 180)
            throw new CoordenadaForaDeAlcanceException(
                $"Longitude {longitude} fora do intervalo válido [-180, 180].");

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Serializa para "lat;long" — formato que será criptografado antes de persistir.</summary>
    public string Serializar() => $"{Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)};{Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

    /// <summary>Reconstrói uma Coordenada a partir de "lat;long" (após descriptografar).</summary>
    public static Coordenada Desserializar(string s)
    {
        var partes = s.Split(';');
        if (partes.Length != 2)
            throw new CoordenadaForaDeAlcanceException($"Formato de coordenada inválido: '{s}'.");

        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var lat = double.Parse(partes[0], inv);
        var lon = double.Parse(partes[1], inv);
        return new Coordenada(lat, lon);
    }
}
