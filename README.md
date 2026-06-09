# Orbital Trust — Núcleo de Confiabilidade

> **Entrega das disciplinas C# (POO) e SOA & WebServices** — repositório único
> (FIAP, Engenharia de Software, 1º sem/2026). O mapeamento de cada requisito de
> cada disciplina para o código está na **[seção 5 — Mapa de requisitos](#5-mapa-de-requisitos--onde-está-no-código)**.
>
> Parte do projeto **Orbital Trust**, plataforma de monitoramento ambiental que cruza dados de
> satélite (Sentinel-2, Landsat, NASA FIRMS) com sensores visuais para gerar **alertas
> ambientais confiáveis** sobre queimadas, desmatamento, enchentes e seca.

### 🧭 Atalhos para o avaliador

| Disciplina | Comece por |
|---|---|
| **C# (POO)** | [Mapa de requisitos de C# (seção 5)](#5-mapa-de-requisitos--onde-está-no-código) · [Testes xUnit (seção 7)](#7-testes) · [Evidências (seção 8)](#8-evidências) |
| **SOA & WebServices** | [Arquitetura (seção 2)](#2-arquitetura) · [Endpoints da API (seção 4)](#4-endpoints) · [Mapa de requisitos de SOA (seção 5)](#5-mapa-de-requisitos--onde-está-no-código) |

---

## 1. Título e motivação

Sistemas de alerta ambiental enfrentam um problema crônico: **dado ruim vira alarme falso**.
Uma imagem encoberta por nuvem ou de baixa qualidade pode "parecer" uma queimada e disparar
uma resposta cara e desnecessária — ou, pior, minar a confiança nos alertas de verdade.

O diferencial do Orbital Trust é o **ICO — Índice de Confiabilidade Orbital**: o sistema não
diz só *"risco alto"*; ele diz **o quanto se pode confiar naquele dado**, combinando a
cobertura de nuvem, a qualidade da imagem e a confiança da análise. A **regra de ouro** é:
**não gritar alerta em cima de dado não-confiável**. Esse cálculo é o coração da aplicação.

Esta aplicação é uma **API REST (ASP.NET Core)** que recebe leituras de sensores, calcula o
ICO, decide se emite alerta e persiste tudo em SQLite. Ela atende a **duas disciplinas** com
um único código:

- **C# (POO)** — modelagem de domínio com classe abstrata, herança e polimorfismo (sensores),
  interfaces + injeção de dependência, Value Objects/DTOs, tratamento de exceções e testes xUnit.
- **SOA & WebServices** — API REST com controllers, contrato OpenAPI/Swagger, camada de
  serviços desacoplada e persistência (EF Core + SQLite).

Integra ainda o controle de **Cibersegurança** (criptografia AES-256-GCM do campo de
coordenadas). A correspondência requisito → arquivo de cada disciplina está na seção 5.

---

## 2. Arquitetura

```
OrbitalTrust.Nucleo/
├── OrbitalTrust.Nucleo.sln
├── docs/
│   ├── diagrama-fluxo.md         # diagrama Mermaid do pipeline
│   └── evidencias/               # prints de execução
├── src/OrbitalTrust.Api/
│   ├── Program.cs                # DI, EF Core, Swagger, EnsureCreated, middleware
│   ├── Domain/
│   │   ├── Entities/             # SensorBase (abstrata) + subtipos, Satelite, Leitura, Alerta
│   │   ├── ValueObjects/         # Coordenada, IndiceConfiabilidade (records imutáveis)
│   │   ├── Enums/                # TipoSensor, NivelRisco, TipoEventoAmbiental
│   │   └── Exceptions/           # exceções de domínio customizadas
│   ├── DTOs/                     # contratos de entrada/saída da API
│   ├── Interfaces/               # contratos (ICalculadoraICO, ICryptoService, repositórios)
│   ├── Services/                 # CalculadoraICO, AesGcmCryptoService, ProcessamentoService
│   ├── Infrastructure/Data/      # DbContext + repositórios EF Core
│   ├── Middleware/               # tratamento global de exceções -> ProblemDetails
│   └── Controllers/              # Leituras, Alertas, Sensores
└── tests/OrbitalTrust.Tests/     # 5 classes de teste xUnit
```

Camadas: **Domain** (regras e modelos puros) → **Services** (orquestração) →
**Infrastructure** (persistência) → **Controllers** (WebService), com **Interfaces** ligando
tudo via injeção de dependência.

---

## 3. Como rodar

Pré-requisito: **.NET 8 SDK**.

```bash
cd src/OrbitalTrust.Api
dotnet restore
dotnet run
```

A Swagger UI abre automaticamente em **`https://localhost:7277/swagger`**
(ou `http://localhost:5033/swagger`). O banco `orbitaltrust.db` é criado sozinho no primeiro
start (`EnsureCreated`).

> **Roda 100% offline** — nenhuma chamada a serviço externo.

---

## 4. Endpoints

| Método | Rota                  | Descrição                                                        |
|--------|-----------------------|------------------------------------------------------------------|
| `POST` | `/api/leituras`       | Processa uma leitura, calcula o ICO, decide o alerta e persiste. |
| `GET`  | `/api/alertas`        | Lista os alertas (coordenadas **descriptografadas**), por data.  |
| `GET`  | `/api/alertas/{id}`   | Retorna um alerta; `404` se não existir.                          |
| `GET`  | `/api/sensores`       | Lista os sensores disponíveis (um óptico + um térmico).          |

### Exemplo — `POST /api/leituras`

```json
{
  "sensorId": "11111111-1111-1111-1111-111111111111",
  "tipoSensor": 0,
  "latitude": -23.5505,
  "longitude": -46.6333,
  "coberturaNuvem": 0.1,
  "qualidadeImagem": 0.95,
  "confiancaAnalise": 0.9,
  "riscoEstimado": 0.92,
  "tipoEvento": 0,
  "dataHora": null
}
```

> `tipoSensor`: `0`=Optico, `1`=Termico. `tipoEvento`: `0`=Queimada, `1`=Desmatamento,
> `2`=Enchente, `3`=Seca.

Resposta `201 Created` (coordenada já descriptografada):

```json
{
  "id": 1,
  "dataHora": "2026-06-06T12:00:00Z",
  "tipoEvento": 0,
  "nivelRisco": 3,
  "ico": 91.4,
  "icoCategoria": "ALTA",
  "latitude": -23.5505,
  "longitude": -46.6333,
  "mensagem": "Queimada — risco CRÍTICO, confiabilidade ALTA (ICO 91)."
}
```

---

## 5. Mapa de requisitos → onde está no código

Cada requisito das rubricas mapeado para o arquivo correspondente. A coluna
**Disciplina** indica a quem atende: **C#** (POO), **SOA** (WebServices) ou
**Cyber** (Cibersegurança).

| Disciplina   | Requisito da rubrica            | Onde está                                                                                     |
|:------------:|---------------------------------|-----------------------------------------------------------------------------------------------|
| C#           | **POO / Classes**               | Todo o `Domain/`, `Services/`, `Controllers/`                                                  |
| C#           | **Abstração (classe abstrata)** | `Domain/Entities/SensorBase.cs` (abstrata, método abstrato `CalcularConfiancaLeitura`)         |
| C#           | **Herança**                     | `SensorOptico.cs`, `SensorTermico.cs` herdam de `SensorBase`                                   |
| C#           | **Polimorfismo**                | `CalcularConfiancaLeitura` sobrescrito com fórmulas diferentes; testado em `SensorPolimorfismoTests` |
| C#           | **Encapsulamento**              | `SensorBase._leiturasProcessadas` (campo privado) + `RegistrarLeituraProcessada()`            |
| C#           | **Membro estático**             | `SensorBase.TotalLeiturasGlobais`                                                              |
| C#           | **Value Objects (VO)**          | `Domain/ValueObjects/Coordenada.cs`, `IndiceConfiabilidade.cs` (`readonly record struct` imutáveis + factory) |
| C#           | **DTOs**                        | `DTOs/LeituraInputDTO.cs`, `AlertaOutputDTO.cs`, `SensorOutputDTO.cs`                          |
| C#           | **Structs / Partial**           | `Coordenada` e `IndiceConfiabilidade` são `readonly record struct`; `Program` é `partial class` (`Program.cs`) |
| C#           | **Interfaces**                  | `Interfaces/` (`ICalculadoraICO`, `ICryptoService`, `ILeituraRepository`, `IAlertaRepository`) |
| C#           | **Injeção de Dependência**      | `Program.cs` (registro) + construtores de `ProcessamentoService` e dos controllers            |
| C#           | **Tratamento de exceções**      | `Domain/Exceptions/` + `Middleware/ExcecaoGlobalMiddleware.cs` → `ProblemDetails` (400 / 503 / 500) |
| C#           | **DateTime**                    | `LeituraAmbiental.DataHora`, `Alerta.DataHora`; consulta ordenada por data desc nos repositórios |
| C#           | **Lógica de negócio / fluxo**   | `Services/ProcessamentoService.cs` (pipeline + regra de ouro) e `CalculadoraICO.cs`           |
| **SOA**      | **WebService / API REST**       | `Controllers/` (`[ApiController]`, controllers — não Minimal API)                              |
| **SOA**      | **Contrato / Swagger (OpenAPI)**| `Program.cs` (`AddSwaggerGen` / `UseSwaggerUI`) — documentação navegável dos endpoints         |
| **SOA**      | **Banco de dados**              | `Infrastructure/Data/OrbitalTrustDbContext.cs` + repositórios (EF Core 8 + SQLite)            |
| **Cyber**    | **Criptografia AES-256-GCM**    | `Services/AesGcmCryptoService.cs` (cifra a coordenada antes de persistir)                      |

---

## 6. Integração entre disciplinas

- **GS (núcleo do Orbital Trust):** o **ICO** é o coração do sistema. `CalculadoraICO`
  combina a confiança do sensor (polimórfica) com a confiança da análise, e o
  `ProcessamentoService` aplica a **regra de ouro** cruzando risco × ICO.
- **Cibersegurança:** a coordenada **nunca** é gravada em claro. `AesGcmCryptoService`
  (AES-256-GCM, `System.Security.Cryptography.AesGcm`) cifra o campo antes de persistir e o
  decifra só na montagem do DTO de saída. O serviço é injetado via `ICryptoService` — o mesmo
  ponto serve de **controle de segurança (Cyber)** e de **exemplo de Interface + DI (SOA)**.
- **C#:** as regras de domínio são cobertas por **15 testes xUnit**.

A chave de cripto fica em `Crypto:Key` (base64 de 32 bytes). **Em produção, sobrescreva via
variável de ambiente `CRYPTO__KEY` — nunca versione a chave real.**

---

## 7. Testes

```bash
dotnet test
```

| Classe                       | O que cobre                                                                 |
|------------------------------|-----------------------------------------------------------------------------|
| `CalculadoraICOTests`        | Valor e categoria do ICO para entradas conhecidas (ex.: 0.9/0.8 → 86, ALTA). |
| `CryptoServiceTests`         | Round-trip `Decriptar(Encriptar(x)) == x`; cifrado ≠ claro; chave inválida.  |
| `SensorPolimorfismoTests`    | Óptico × térmico retornam confianças diferentes; contador estático global.   |
| `CoordenadaTests`            | Latitude/longitude fora do intervalo lançam exceção; serialização preserva.  |
| `ProcessamentoServiceTests`  | Valor fora de [0,1] lança; risco alto + ICO alto → Alto/Crítico; **regra de ouro** (risco alto + ICO baixo → Moderado). |

**Resultado atual: 15 testes, 100% aprovados.**

---

## 8. Evidências

As evidências de execução estão em [`docs/evidencias/`](docs/evidencias/), além da
saída completa em texto em [`output.txt`](docs/evidencias/output.txt):

| Print                   | O que comprova                                                        |
|-------------------------|----------------------------------------------------------------------|
| `01-build.png`          | `dotnet build` — compilação com êxito, 0 erros                       |
| `02-test.png`           | `dotnet test` — 15/15 testes aprovados                              |
| `03-run.png`            | Aplicação em execução (API no ar)                                   |
| `04-swagger-ui.png`     | Swagger UI listando os endpoints da API                             |
| `05-post-201.png`       | `POST /api/leituras` → `201` com o alerta e o ICO calculado          |
| `06-get-alertas.png`    | `GET /api/alertas` retornando a coordenada **descriptografada**      |

> A coordenada é persistida **criptografada** (AES-256-GCM) na coluna
> `CoordenadaCriptografada` do SQLite e só é descriptografada na saída da API
> (`06-get-alertas`) — evidência do controle de cibersegurança integrado.

---

## 9. Equipe

| Integrante         | RM                          |
|--------------------|-----------------------------|
| Victor Dias        | RM558017                    |
| Gustavo Paulino    | RM554779                    |
| Guilherme Abe      | RM554743                    |
| Fernando Luiz      | RM555201                    |
| Thomas Reichmann   | RM554812                    |
