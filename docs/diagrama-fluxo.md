# Diagrama de fluxo — `POST /api/leituras`

O diagrama abaixo mostra o pipeline central do Orbital Trust: da chegada da leitura
até a persistência e a resposta com a coordenada descriptografada.

```mermaid
flowchart TD
    A[POST /api/leituras] --> B{Validar DTO}
    B -- inválido --> X[400 LeituraInvalidaException]
    B -- ok --> C[Instanciar Sensor - Optico/Termico]
    C -- offline --> Y[503 SensorOfflineException]
    C --> D[CalcularConfiancaLeitura - polimorfismo]
    D --> E[Construir Coordenada VO]
    E -- fora de alcance --> Z[400 CoordenadaForaDeAlcanceException]
    E --> F[CalculadoraICO]
    F --> G{Risco x ICO}
    G --> H[Criptografar Coordenada AES-256-GCM]
    H --> I[Persistir Leitura + Alerta no SQLite]
    I --> J[Retornar AlertaOutputDTO - coordenada descriptografada]
```

## A "regra de ouro" (nó `Risco x ICO`)

| Risco estimado (ML) | ICO        | Nível resultante                          |
|---------------------|------------|-------------------------------------------|
| `>= 0.9`            | `>= 75`    | **Crítico**                               |
| `>= 0.7`            | `>= 50`    | **Alto**                                  |
| `>= 0.7`            | `< 50`     | **Moderado** (dado de baixa confiabilidade) |
| `< 0.7`             | qualquer   | **Baixo**                                 |

Alerta só é persistido quando o nível é **Moderado ou maior** — o sistema não grita
alarme forte em cima de dado não-confiável.
