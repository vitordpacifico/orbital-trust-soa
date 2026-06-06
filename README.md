# Orbital Trust — Núcleo de Confiabilidade (SOA & WebServices)

> Entrega da disciplina **GS — SOA e WebServices** (FIAP, Engenharia de Software, 1º sem/2026).
> Parte do projeto **Orbital Trust**, plataforma de monitoramento ambiental que cruza dados de
> satélite (Sentinel-2, Landsat, NASA FIRMS) com sensores visuais para gerar **alertas
> ambientais confiáveis** sobre queimadas, desmatamento, enchentes e seca.

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
ICO, decide se emite alerta e persiste tudo em SQLite. Ela integra o controle de
**Cibersegurança** (criptografia AES-256-GCM do campo de coordenadas) e inclui **testes
xUnit** para servir também à disciplina de **C#**.

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

| Requisito da rubrica            | Onde está                                                                                     |
|---------------------------------|-----------------------------------------------------------------------------------------------|
| **POO / Classes**               | Todo o `Domain/`, `Services/`, `Controllers/`                                                  |
| **Abstração (classe abstrata)** | `Domain/Entities/SensorBase.cs` (abstrata, método abstrato `CalcularConfiancaLeitura`)         |
| **Herança**                     | `SensorOptico.cs`, `SensorTermico.cs` herdam de `SensorBase`                                   |
| **Polimorfismo**                | `CalcularConfiancaLeitura` sobrescrito com fórmulas diferentes; testado em `SensorPolimorfismoTests` |
| **Encapsulamento**              | `SensorBase._leiturasProcessadas` (campo privado) + `RegistrarLeituraProcessada()`            |
| **Membro estático**             | `SensorBase.TotalLeiturasGlobais`                                                              |
| **Value Objects**               | `Domain/ValueObjects/Coordenada.cs`, `IndiceConfiabilidade.cs` (records imutáveis + factory)   |
| **DTOs**                        | `DTOs/LeituraInputDTO.cs`, `AlertaOutputDTO.cs`, `SensorOutputDTO.cs`                          |
| **Interfaces**                  | `Interfaces/` (`ICalculadoraICO`, `ICryptoService`, `ILeituraRepository`, `IAlertaRepository`) |
| **Injeção de Dependência**      | `Program.cs` (registro) + construtores de `ProcessamentoService` e dos controllers            |
| **Exceções customizadas**       | `Domain/Exceptions/` + `Middleware/ExcecaoGlobalMiddleware.cs` (mapeia p/ status HTTP)         |
| **DateTime**                    | `LeituraAmbiental.DataHora`, `Alerta.DataHora`; consulta ordenada por data desc nos repositórios |
| **Lógica de negócio / fluxo**   | `Services/ProcessamentoService.cs` (pipeline + regra de ouro) e `CalculadoraICO.cs`           |
| **WebService / API REST**       | `Controllers/` (`[ApiController]`, controllers — não Minimal API)                              |
| **Banco de dados**              | `Infrastructure/Data/OrbitalTrustDbContext.cs` + repositórios (EF Core 8 + SQLite)            |
| **Tratamento de exceções**      | `Middleware/ExcecaoGlobalMiddleware.cs` → `ProblemDetails` (400 / 503 / 500)                   |
| **Documentação / Swagger**      | `Program.cs` (`AddSwaggerGen` / `UseSwaggerUI`)                                                |

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

Os prints ficam em [`docs/evidencias/`](docs/evidencias/). Capturar:

1. `dotnet build` com sucesso.
2. `dotnet test` com todos os testes passando.
3. Swagger UI mostrando os endpoints.
4. `POST /api/leituras` (request + response `201` com o alerta).
5. `GET /api/alertas` retornando a coordenada **descriptografada**.
6. O arquivo `orbitaltrust.db` aberto no **DB Browser for SQLite**, mostrando a coluna
   `CoordenadaCriptografada` como base64 ilegível — esta é a evidência do controle de Cyber:
   **coordenada criptografada em repouso**.

---

## 9. Equipe

| Integrante         | RM                          |
|--------------------|-----------------------------|
| Vitor Pacifico     | RM558017                    |
| Gustavo Paulino    | RM554779                    |
| Fernando Antonio   | RM555201                    |
| Thomas Reichmann   | RM554812                    |
| Guilherme Abe      | RM554743                    |
