using CurieCatalogo.Validation;

namespace CurieCatalogo.Models;

/// <summary>
/// Entidade EXPERIMENTO: uma missao do kit CURIE. Define qual objeto o aluno mede,
/// a faixa de contagens por minuto (CPM) esperada e a explicacao educativa.
/// Mapeia a tabela T_CURIE_EXPERIMENTO do modelo de dados do projeto.
/// </summary>
public class Experimento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Objeto { get; set; } = string.Empty;
    public Categoria Categoria { get; set; }
    public decimal CpmMin { get; set; }
    public decimal CpmMax { get; set; }
    public int Nivel { get; set; }
    public string Explicacao { get; set; } = string.Empty;
    public bool Publicado { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    /// <summary>Experimento teorico: nao envolve medicao (faixa 0-0).</summary>
    public bool EhTeorico => CpmMin == 0 && CpmMax == 0;

    /// <summary>Avalia se uma leitura do Geiger esta dentro da faixa esperada.</summary>
    public bool LeituraDentroDaFaixa(decimal cpm) => cpm >= CpmMin && cpm <= CpmMax;

    /// <summary>
    /// Validacao dos atributos (invariantes da entidade). Lanca ValidacaoException
    /// com todas as mensagens de uma vez, para o usuario corrigir tudo junto.
    /// </summary>
    public void Validar()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Nome))
            erros.Add("O nome e obrigatorio.");
        else if (Nome.Trim().Length is < 3 or > 100)
            erros.Add("O nome deve ter entre 3 e 100 caracteres.");

        if (string.IsNullOrWhiteSpace(Objeto))
            erros.Add("O objeto medido e obrigatorio.");
        else if (Objeto.Trim().Length > 80)
            erros.Add("O objeto deve ter no maximo 80 caracteres.");

        if (!Enum.IsDefined(typeof(Categoria), Categoria))
            erros.Add("Categoria invalida.");

        if (CpmMin < 0)
            erros.Add("CPM minimo nao pode ser negativo.");
        if (CpmMax < CpmMin)
            erros.Add("CPM maximo deve ser maior ou igual ao CPM minimo.");
        if (CpmMax > 999999.99m)
            erros.Add("CPM maximo acima do limite (999999,99).");

        if (Nivel is < 1 or > 3)
            erros.Add("O nivel deve ser 1 (basico), 2 (intermediario) ou 3 (avancado).");

        if (string.IsNullOrWhiteSpace(Explicacao))
            erros.Add("A explicacao educativa e obrigatoria.");
        else if (Explicacao.Trim().Length is < 20 or > 600)
            erros.Add("A explicacao deve ter entre 20 e 600 caracteres.");

        if (erros.Count > 0)
            throw new ValidacaoException(erros);

        Nome = Nome.Trim();
        Objeto = Objeto.Trim();
        Explicacao = Explicacao.Trim();
    }

    public override string ToString()
    {
        var faixa = EhTeorico ? "teorico (sem medicao)" : $"{CpmMin:0.##}-{CpmMax:0.##} CPM";
        var status = Publicado ? "PUBLICADO" : "rascunho";
        return $"[{Id,3}] {Nome,-28} | {Objeto,-26} | {Categoria,-9} | nivel {Nivel} | {faixa,-22} | {status}";
    }
}
