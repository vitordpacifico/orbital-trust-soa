namespace OrbitalTrust.Api.Domain.ValueObjects;

/// <summary>
/// VALUE OBJECT imutável (readonly record struct) — o ICO (Índice de Confiabilidade Orbital).
/// Tipo de valor: o índice não tem identidade, é definido apenas pelo seu conteúdo.
/// Construtor privado + factory estática garantem que a Categoria seja sempre
/// coerente com o Valor (encapsulamento da regra de classificação).
/// </summary>
public readonly record struct IndiceConfiabilidade
{
    public double Valor { get; }
    public string Categoria { get; }

    private IndiceConfiabilidade(double valor, string categoria)
    {
        Valor = valor;
        Categoria = categoria;
    }

    /// <summary>FACTORY — cria o índice já classificando a categoria a partir do valor.</summary>
    public static IndiceConfiabilidade DeValor(double valor)
    {
        var categoria = valor >= 75 ? "ALTA"
                      : valor >= 50 ? "MEDIA"
                      : "BAIXA";
        return new IndiceConfiabilidade(valor, categoria);
    }
}
