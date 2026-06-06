using OrbitalTrust.Api.Domain.Entities;

namespace OrbitalTrust.Api.Interfaces;

/// <summary>
/// INTERFACE — contrato de persistência das leituras. Injetada via DI.
/// </summary>
public interface ILeituraRepository
{
    Task<int> Adicionar(LeituraAmbiental leitura);
    Task<IEnumerable<LeituraAmbiental>> ListarOrdenadoPorData();
    Task<LeituraAmbiental?> ObterPorId(int id);
}
