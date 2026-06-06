using Microsoft.AspNetCore.Mvc;
using OrbitalTrust.Api.Domain.ValueObjects;
using OrbitalTrust.Api.DTOs;
using OrbitalTrust.Api.Interfaces;

namespace OrbitalTrust.Api.Controllers;

/// <summary>
/// WEBSERVICE — consulta dos alertas emitidos (coordenadas DESCRIPTOGRAFADAS na saída).
/// </summary>
[ApiController]
[Route("api/alertas")]
public class AlertasController : ControllerBase
{
    private readonly IAlertaRepository _alertaRepo;
    private readonly ICryptoService _crypto;

    // INJEÇÃO DE DEPENDÊNCIA — repositório + serviço de cripto via interfaces.
    public AlertasController(IAlertaRepository alertaRepo, ICryptoService crypto)
    {
        _alertaRepo = alertaRepo;
        _crypto = crypto;
    }

    /// <summary>GET /api/alertas — lista todos os alertas ordenados por DataHora desc.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlertaOutputDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertaOutputDTO>>> Listar()
    {
        var alertas = await _alertaRepo.ListarOrdenadoPorData();
        return Ok(alertas.Select(MapearParaDto));
    }

    /// <summary>GET /api/alertas/{id} — um alerta; 404 se não existir.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AlertaOutputDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertaOutputDTO>> ObterPorId(int id)
    {
        var alerta = await _alertaRepo.ObterPorId(id);
        if (alerta is null)
            return NotFound();

        return Ok(MapearParaDto(alerta));
    }

    private AlertaOutputDTO MapearParaDto(Domain.Entities.Alerta alerta)
    {
        // DESCRIPTOGRAFA a coordenada só na hora de montar a saída.
        var coord = Coordenada.Desserializar(_crypto.Decriptar(alerta.CoordenadaCriptografada));

        return new AlertaOutputDTO
        {
            Id = alerta.Id,
            DataHora = alerta.DataHora,
            TipoEvento = alerta.TipoEvento,
            NivelRisco = alerta.NivelRisco,
            Ico = System.Math.Round(alerta.IcoValor, 2),
            IcoCategoria = alerta.IcoCategoria,
            Latitude = coord.Latitude,
            Longitude = coord.Longitude,
            Mensagem = alerta.Mensagem
        };
    }
}
