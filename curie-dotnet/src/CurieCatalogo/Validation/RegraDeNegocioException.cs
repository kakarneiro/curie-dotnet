namespace CurieCatalogo.Validation;

/// <summary>Violacao de regra de negocio (ex.: publicar experimento sem faixa definida).</summary>
public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem) { }
}
