using OrbitalTrust.Api.Domain.Entities;

namespace OrbitalTrust.Api.Interfaces;

/// <summary>
/// INTERFACE — contrato de persistência dos alertas. Injetada via DI.
/// </summary>
public interface IAlertaRepository
{
    Task<int> Adicionar(Alerta alerta);
    Task<IEnumerable<Alerta>> ListarOrdenadoPorData();
    Task<Alerta?> ObterPorId(int id);
}
