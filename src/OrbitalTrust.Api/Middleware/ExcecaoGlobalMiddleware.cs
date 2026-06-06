using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OrbitalTrust.Api.Domain.Exceptions;

namespace OrbitalTrust.Api.Middleware;

/// <summary>
/// TRATAMENTO DE EXCEÇÕES GLOBAL — sistemas críticos não quebram. Captura exceções de
/// domínio e mapeia para ProblemDetails com o status HTTP correto (sem vazar stack trace).
/// </summary>
public class ExcecaoGlobalMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExcecaoGlobalMiddleware> _logger;

    public ExcecaoGlobalMiddleware(RequestDelegate next, ILogger<ExcecaoGlobalMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await TratarAsync(context, ex);
        }
    }

    private async Task TratarAsync(HttpContext context, Exception ex)
    {
        var (status, titulo, detalhe) = ex switch
        {
            LeituraInvalidaException => (StatusCodes.Status400BadRequest, "Leitura inválida", ex.Message),
            CoordenadaForaDeAlcanceException => (StatusCodes.Status400BadRequest, "Coordenada fora de alcance", ex.Message),
            SensorOfflineException => (StatusCodes.Status503ServiceUnavailable, "Sensor offline", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado ao processar a requisição.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Erro não tratado no pipeline.");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalhe
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
