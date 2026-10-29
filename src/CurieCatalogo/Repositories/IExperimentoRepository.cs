using CurieCatalogo.Models;

namespace CurieCatalogo.Repositories;

/// <summary>
/// Contrato de persistencia. A implementacao da Sprint 1 grava em JSON;
/// na Sprint 2 uma implementacao com Entity Framework Core + Oracle
/// (tabela T_CURIE_EXPERIMENTO) pode substituir sem mudar o restante do codigo.
/// </summary>
public interface IExperimentoRepository
{
    IReadOnlyList<Experimento> ListarTodos();
    Experimento? BuscarPorId(int id);
    IReadOnlyList<Experimento> BuscarPorNome(string trecho);
    IReadOnlyList<Experimento> BuscarPorCategoria(Categoria categoria);
    Experimento Adicionar(Experimento experimento);
    void Atualizar(Experimento experimento);
    bool ExisteNome(string nome, int? ignorarId = null);
    int Contar();
}
