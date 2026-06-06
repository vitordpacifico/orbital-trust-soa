using System.Security.Cryptography;
using System.Text;
using OrbitalTrust.Api.Interfaces;

namespace OrbitalTrust.Api.Services;

/// <summary>
/// INTEGRAÇÃO COM CIBERSEGURANÇA — criptografia simétrica autenticada AES-256-GCM.
/// Implementa ICryptoService e é registrado como SINGLETON (a chave é carregada uma vez).
///
/// Formato do texto cifrado (base64): [nonce(12) | tag(16) | ciphertext(resto)].
/// </summary>
public class AesGcmCryptoService : ICryptoService
{
    private readonly byte[] _key;

    public AesGcmCryptoService(IConfiguration configuration)
    {
        // A chave vem da configuração Crypto:Key (base64 de 32 bytes -> AES-256).
        var keyBase64 = configuration["Crypto:Key"]
            ?? throw new InvalidOperationException("Configuração 'Crypto:Key' não encontrada.");

        _key = Convert.FromBase64String(keyBase64);

        // Validação: AES-256 exige exatamente 32 bytes.
        if (_key.Length != 32)
            throw new InvalidOperationException(
                $"A chave 'Crypto:Key' deve ter 32 bytes (256 bits) após decodificar de base64; tem {_key.Length}.");
    }

    public string Encriptar(string textoPlano)
    {
        var plaintext = Encoding.UTF8.GetBytes(textoPlano);
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize); // 12 bytes
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];                            // 16 bytes
        var ciphertext = new byte[plaintext.Length];

        // .NET 8: usar a sobrecarga com tagSizeInBytes (o construtor sem ele é obsoleto - SYSLIB0053).
        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var combinado = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, combinado, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, combinado, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, combinado, nonce.Length + tag.Length, ciphertext.Length);

        return Convert.ToBase64String(combinado);
    }

    public string Decriptar(string textoCifrado)
    {
        var combinado = Convert.FromBase64String(textoCifrado);

        var nonceSize = AesGcm.NonceByteSizes.MaxSize; // 12
        var tagSize = AesGcm.TagByteSizes.MaxSize;     // 16

        var nonce = new byte[nonceSize];
        var tag = new byte[tagSize];
        var ciphertext = new byte[combinado.Length - nonceSize - tagSize];

        Buffer.BlockCopy(combinado, 0, nonce, 0, nonceSize);
        Buffer.BlockCopy(combinado, nonceSize, tag, 0, tagSize);
        Buffer.BlockCopy(combinado, nonceSize + tagSize, ciphertext, 0, ciphertext.Length);

        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}
