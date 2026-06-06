using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrbitalTrust.Api.Infrastructure.Data;
using OrbitalTrust.Api.Infrastructure.Data.Repositories;
using OrbitalTrust.Api.Interfaces;
using OrbitalTrust.Api.Middleware;
using OrbitalTrust.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Controllers (NÃO Minimal API) ----
builder.Services.AddControllers();

// ---- EF Core + SQLite ----
builder.Services.AddDbContext<OrbitalTrustDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=orbitaltrust.db"));

// ---- INJEÇÃO DE DEPENDÊNCIA (registro das interfaces) ----
// Cripto é singleton: a chave AES-256 é carregada uma única vez no startup.
builder.Services.AddSingleton<ICryptoService, AesGcmCryptoService>();
// Catálogo de sensores de exemplo (um óptico + um térmico).
builder.Services.AddSingleton<CatalogoSensores>();
// Calculadora do ICO (sem estado) -> singleton.
builder.Services.AddSingleton<ICalculadoraICO, CalculadoraICO>();
// Repositórios e orquestrador acompanham o DbContext -> scoped.
builder.Services.AddScoped<ILeituraRepository, LeituraRepository>();
builder.Services.AddScoped<IAlertaRepository, AlertaRepository>();
builder.Services.AddScoped<ProcessamentoService>();

// ---- Swagger / OpenAPI (documentação + evidência) ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orbital Trust — Núcleo de Confiabilidade",
        Version = "v1",
        Description = "API REST que calcula o ICO (Índice de Confiabilidade Orbital), " +
                      "decide alertas ambientais e criptografa a coordenada (AES-256-GCM)."
    });
});

var app = builder.Build();

// ---- Cria o banco no startup (sem migrations, pra rodar sem fricção) ----
// Em produção, o ideal seria usar migrations (dotnet ef migrations add / database update).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrbitalTrustDbContext>();
    db.Database.EnsureCreated();
}

// ---- Middleware de exceção global (mapeia exceções -> ProblemDetails) ----
app.UseMiddleware<ExcecaoGlobalMiddleware>();

// Swagger sempre disponível (também serve de evidência fora de Development).
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orbital Trust v1");
    c.RoutePrefix = "swagger"; // Swagger UI em /swagger
});

app.MapControllers();

app.Run();

// Expõe a classe Program para os testes de integração (caso necessário).
public partial class Program { }
