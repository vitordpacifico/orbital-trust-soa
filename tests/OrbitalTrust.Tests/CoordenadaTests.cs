using OrbitalTrust.Api.Domain.Exceptions;
using OrbitalTrust.Api.Domain.ValueObjects;
using Xunit;

namespace OrbitalTrust.Tests;

public class CoordenadaTests
{
    [Fact]
    public void Construtor_ComLatitudeForaDoIntervalo_Lanca()
    {
        Assert.Throws<CoordenadaForaDeAlcanceException>(() => new Coordenada(200, 0));
    }

    [Fact]
    public void Construtor_ComLongitudeForaDoIntervalo_Lanca()
    {
        Assert.Throws<CoordenadaForaDeAlcanceException>(() => new Coordenada(0, 999));
    }

    [Fact]
    public void SerializarEDesserializar_PreservaValores()
    {
        var original = new Coordenada(-23.5505, -46.6333);

        var s = original.Serializar();
        var recriada = Coordenada.Desserializar(s);

        Assert.Equal(original.Latitude, recriada.Latitude);
        Assert.Equal(original.Longitude, recriada.Longitude);
    }
}
