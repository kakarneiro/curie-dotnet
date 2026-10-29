namespace CurieCatalogo.Validation;

/// <summary>Erro de validacao de atributos. Carrega todas as mensagens encontradas.</summary>
public class ValidacaoException : Exception
{
    public IReadOnlyList<string> Erros { get; }

    public ValidacaoException(IEnumerable<string> erros)
        : base("Dados invalidos: " + string.Join(" ", erros))
    {
        Erros = erros.ToList();
    }
}
