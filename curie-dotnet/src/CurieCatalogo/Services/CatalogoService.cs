using CurieCatalogo.Models;
using CurieCatalogo.Repositories;
using CurieCatalogo.Validation;

namespace CurieCatalogo.Services;

/// <summary>
/// Regras de negocio do catalogo de experimentos.
///  1. Nome unico (duas missoes nao podem ter o mesmo nome).
///  2. Um experimento so pode ser PUBLICADO (aparecer no app) se tiver faixa
///     de CPM definida (cpmMax > 0) OU for teorico de nivel 3 (conteudo sem medicao).
///  3. Nivel 3 (avancado) exige explicacao mais completa (minimo 60 caracteres).
///  4. Publicar e uma transicao controlada: so experimento valido e ainda nao publicado.
/// </summary>
public class CatalogoService
{
    private readonly IExperimentoRepository _repo;

    public CatalogoService(IExperimentoRepository repo) => _repo = repo;

    public IReadOnlyList<Experimento> Listar() => _repo.ListarTodos();

    public IReadOnlyList<Experimento> ListarPublicados() =>
        _repo.ListarTodos().Where(e => e.Publicado).ToList();

    public Experimento Buscar(int id) =>
        _repo.BuscarPorId(id) ?? throw new KeyNotFoundException($"Experimento com id {id} nao encontrado.");

    public IReadOnlyList<Experimento> BuscarPorNome(string trecho)
    {
        if (string.IsNullOrWhiteSpace(trecho))
            throw new ValidacaoException(new[] { "Informe um trecho para buscar." });
        return _repo.BuscarPorNome(trecho.Trim());
    }

    public IReadOnlyList<Experimento> BuscarPorCategoria(Categoria categoria) =>
        _repo.BuscarPorCategoria(categoria);

    public Experimento Cadastrar(Experimento novo)
    {
        novo.Validar();
        AplicarRegras(novo, ignorarId: null);
        novo.Publicado = false;               // todo cadastro nasce como rascunho
        return _repo.Adicionar(novo);
    }

    public Experimento Atualizar(Experimento alterado)
    {
        var atual = Buscar(alterado.Id);
        alterado.Validar();
        AplicarRegras(alterado, ignorarId: alterado.Id);
        alterado.CriadoEm = atual.CriadoEm;
        alterado.Publicado = atual.Publicado;  // publicacao so muda pela operacao propria
        _repo.Atualizar(alterado);
        return alterado;
    }

    /// <summary>Regra de negocio principal: publicacao controlada.</summary>
    public Experimento Publicar(int id)
    {
        var exp = Buscar(id);
        if (exp.Publicado)
            throw new RegraDeNegocioException($"O experimento '{exp.Nome}' ja esta publicado.");

        var temFaixa = exp.CpmMax > 0;
        var teoricoAvancado = exp.EhTeorico && exp.Nivel == 3;
        if (!temFaixa && !teoricoAvancado)
            throw new RegraDeNegocioException(
                "So e possivel publicar um experimento com faixa de CPM definida " +
                "ou um conteudo teorico de nivel 3.");

        exp.Publicado = true;
        _repo.Atualizar(exp);
        return exp;
    }

    public Experimento Despublicar(int id)
    {
        var exp = Buscar(id);
        if (!exp.Publicado)
            throw new RegraDeNegocioException($"O experimento '{exp.Nome}' ja esta como rascunho.");
        exp.Publicado = false;
        _repo.Atualizar(exp);
        return exp;
    }

    /// <summary>Simula a avaliacao que a API fara com uma leitura do kit.</summary>
    public string AvaliarLeitura(int id, decimal cpm)
    {
        var exp = Buscar(id);
        if (exp.EhTeorico)
            throw new RegraDeNegocioException("Experimento teorico nao recebe leituras.");
        if (cpm < 0)
            throw new ValidacaoException(new[] { "A leitura em CPM nao pode ser negativa." });

        return exp.LeituraDentroDaFaixa(cpm)
            ? $"{cpm:0.##} CPM esta DENTRO da faixa esperada ({exp.CpmMin:0.##}-{exp.CpmMax:0.##}). Missao concluida!"
            : $"{cpm:0.##} CPM esta FORA da faixa esperada ({exp.CpmMin:0.##}-{exp.CpmMax:0.##}). Sugira repetir a medicao.";
    }

    public (int total, int publicados, int rascunhos) Resumo()
    {
        var todos = _repo.ListarTodos();
        var pub = todos.Count(e => e.Publicado);
        return (todos.Count, pub, todos.Count - pub);
    }

    private void AplicarRegras(Experimento exp, int? ignorarId)
    {
        if (_repo.ExisteNome(exp.Nome, ignorarId))
            throw new RegraDeNegocioException($"Ja existe um experimento chamado '{exp.Nome}'.");

        if (exp.Nivel == 3 && exp.Explicacao.Length < 60)
            throw new RegraDeNegocioException(
                "Experimentos de nivel 3 (avancado) exigem explicacao com pelo menos 60 caracteres.");
    }
}
