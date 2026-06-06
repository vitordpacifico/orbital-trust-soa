using Microsoft.Extensions.Configuration;
using OrbitalTrust.Api.Services;
using Xunit;

namespace OrbitalTrust.Tests;

public class CryptoServiceTests
{
    private static AesGcmCryptoService CriarServico()
    {
        // Chave de exemplo (32 bytes em base64) — mesma do appsettings.
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Crypto:Key"] = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY="
            })
            .Build();
        return new AesGcmCryptoService(config);
    }

    [Fact]
    public void Decriptar_DeEncriptar_RetornaTextoOriginal()
    {
        var crypto = CriarServico();
        var original = "-23.5505;-46.6333";

        var cifrado = crypto.Encriptar(original);
        var decifrado = crypto.Decriptar(cifrado);

        Assert.Equal(original, decifrado);
    }

    [Fact]
    public void Encriptar_ProduzTextoDiferenteDoOriginal()
    {
        var crypto = CriarServico();
        var original = "-23.5505;-46.6333";

        var cifrado = crypto.Encriptar(original);

        Assert.NotEqual(original, cifrado);
    }

    [Fact]
    public void Construtor_ComChaveDeTamanhoInvalido_LancaExcecao()
    {
        // "MDEx" decodifica para 3 bytes -> inválido para AES-256.
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Crypto:Key"] = "MDEx" })
            .Build();

        Assert.Throws<InvalidOperationException>(() => new AesGcmCryptoService(config));
    }
}
