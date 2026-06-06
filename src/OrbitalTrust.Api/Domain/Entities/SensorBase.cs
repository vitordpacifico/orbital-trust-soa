using OrbitalTrust.Api.Domain.Enums;

namespace OrbitalTrust.Api.Domain.Entities;

/// <summary>
/// ABSTRAÇÃO — classe base abstrata de todos os sensores. Não pode ser instanciada
/// diretamente; define o contrato CalcularConfiancaLeitura que cada subtipo implementa.
/// </summary>
public abstract class SensorBase
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoSensor Tipo { get; protected set; }
    public bool Online { get; set; } = true;

    // ENCAPSULAMENTO — campo privado só alterável pelo método público abaixo.
    private int _leiturasProcessadas;
    public int LeiturasProcessadas => _leiturasProcessadas;

    // MEMBRO ESTÁTICO — contador global compartilhado por todas as instâncias.
    public static int TotalLeiturasGlobais { get; private set; }

    /// <summary>Registra que este sensor processou uma leitura (encapsula a mutação do estado).</summary>
    public void RegistrarLeituraProcessada()
    {
        _leiturasProcessadas++;
        TotalLeiturasGlobais++;
    }

    /// <summary>
    /// MÉTODO ABSTRATO — cada tipo de sensor calcula a confiança da leitura de um jeito.
    /// Retorna valor entre 0 e 1.
    /// </summary>
    public abstract double CalcularConfiancaLeitura(double coberturaNuvem, double qualidadeImagem);
}
