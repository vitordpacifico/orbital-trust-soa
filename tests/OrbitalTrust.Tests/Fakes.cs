using OrbitalTrust.Api.Domain.Entities;
using OrbitalTrust.Api.Interfaces;

namespace OrbitalTrust.Tests;

/// <summary>Repositório fake in-memory para isolar os testes do EF Core.</summary>
public class FakeLeituraRepository : ILeituraRepository
{
    public readonly List<LeituraAmbiental> Itens = new();
    private int _seq;

    public Task<int> Adicionar(LeituraAmbiental leitura)
    {
        leitura.Id = ++_seq;
        Itens.Add(leitura);
        return Task.FromResult(leitura.Id);
    }

    public Task<IEnumerable<LeituraAmbiental>> ListarOrdenadoPorData()
        => Task.FromResult(Itens.OrderByDescending(l => l.DataHora).AsEnumerable());

    public Task<LeituraAmbiental?> ObterPorId(int id)
        => Task.FromResult(Itens.FirstOrDefault(l => l.Id == id));
}

/// <summary>Repositório fake in-memory para alertas.</summary>
public class FakeAlertaRepository : IAlertaRepository
{
    public readonly List<Alerta> Itens = new();
    private int _seq;

    public Task<int> Adicionar(Alerta alerta)
    {
        alerta.Id = ++_seq;
        Itens.Add(alerta);
        return Task.FromResult(alerta.Id);
    }

    public Task<IEnumerable<Alerta>> ListarOrdenadoPorData()
        => Task.FromResult(Itens.OrderByDescending(a => a.DataHora).AsEnumerable());

    public Task<Alerta?> ObterPorId(int id)
        => Task.FromResult(Itens.FirstOrDefault(a => a.Id == id));
}
