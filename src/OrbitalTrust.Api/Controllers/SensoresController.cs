using Microsoft.AspNetCore.Mvc;
using OrbitalTrust.Api.DTOs;
using OrbitalTrust.Api.Services;

namespace OrbitalTrust.Api.Controllers;

/// <summary>
/// WEBSERVICE — lista os sensores disponíveis, demonstrando os tipos (óptico e térmico).
/// </summary>
[ApiController]
[Route("api/sensores")]
public class SensoresController : ControllerBase
{
    private readonly CatalogoSensores _catalogo;

    public SensoresController(CatalogoSensores catalogo)
    {
        _catalogo = catalogo;
    }

    /// <summary>GET /api/sensores — lista os sensores cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SensorOutputDTO>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<SensorOutputDTO>> Listar()
    {
        var dtos = _catalogo.Sensores.Select(s => new SensorOutputDTO
        {
            Id = s.Id,
            Nome = s.Nome,
            Tipo = s.Tipo,
            Online = s.Online
        });
        return Ok(dtos);
    }
}
