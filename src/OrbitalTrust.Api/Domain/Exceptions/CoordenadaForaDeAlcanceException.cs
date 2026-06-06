namespace OrbitalTrust.Api.Domain.Exceptions;

/// <summary>
/// EXCEÇÃO CUSTOMIZADA — latitude/longitude fora do intervalo geográfico válido.
/// Mapeada para 400 Bad Request no middleware global.
/// </summary>
public class CoordenadaForaDeAlcanceException : Exception
{
    public CoordenadaForaDeAlcanceException(string mensagem) : base(mensagem)
    {
    }
}
