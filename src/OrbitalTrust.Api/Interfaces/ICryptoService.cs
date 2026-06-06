namespace OrbitalTrust.Api.Interfaces;

/// <summary>
/// INTERFACE — contrato do serviço de criptografia (integração com Cibersegurança).
/// Injetada via DI; implementada por AesGcmCryptoService (AES-256-GCM).
/// </summary>
public interface ICryptoService
{
    string Encriptar(string textoPlano);
    string Decriptar(string textoCifrado);
}
