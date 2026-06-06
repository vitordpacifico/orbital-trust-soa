using Microsoft.AspNetCore.Mvc;
using OrbitalTrust.Api.DTOs;
using OrbitalTrust.Api.Services;

namespace OrbitalTrust.Api.Controllers;

/// <summary>
/// WEBSERVICE — recebe leituras de sensores e dispara o pipeline de processamento.
/// </summary>
[ApiController]
[Route("api/leituras")]
public class LeiturasController : ControllerBase
{
    private readonly ProcessamentoService _processamento;

    // INJEÇÃO DE DEPENDÊNCIA via construtor.
    public LeiturasController(ProcessamentoService processamento)
    {
        _processamento = processamento;
    }

    /// <summary>
    /// POST /api/leituras — processa a leitura, calcula o ICO, decide o alerta e persiste.
    /// Retorna 201 com o AlertaOutputDTO (coordenada já descriptografada).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AlertaOutputDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AlertaOutputDTO>> Processar([FromBody] LeituraInputDTO dto)
    {
        var resultado = await _processamento.ProcessarLeitura(dto);
        return CreatedAtAction(nameof(Processar), new { id = resultado.Id }, resultado);
    }
}
