namespace OrbitalTrust.Api.Domain.Exceptions;

/// <summary>
/// EXCEÇÃO CUSTOMIZADA — payload de leitura inválido (valores fora de [0,1],
/// campos faltando). Mapeada para 400 Bad Request no middleware global.
/// </summary>
public class LeituraInvalidaException : Exception
{
    public LeituraInvalidaException(string mensagem) : base(mensagem)
    {
    }
}
