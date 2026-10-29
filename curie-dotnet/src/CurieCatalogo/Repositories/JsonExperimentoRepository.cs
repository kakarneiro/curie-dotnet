using System.Text.Json;
using System.Text.Json.Serialization;
using CurieCatalogo.Models;

namespace CurieCatalogo.Repositories;

/// <summary>
/// Persistencia em arquivo JSON. Os dados sao mantidos entre execucoes:
/// cada alteracao e gravada imediatamente em disco.
/// </summary>
public class JsonExperimentoRepository : IExperimentoRepository
{
    private readonly string _caminho;
    private readonly List<Experimento> _itens;

    private static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public JsonExperimentoRepository(string caminho, string? caminhoSeed = null)
    {
        _caminho = caminho;
        _itens = Carregar(caminho, caminhoSeed);
    }

    public string Caminho => _caminho;

    public IReadOnlyList<Experimento> ListarTodos() =>
        _itens.OrderBy(e => e.Id).ToList();

    public Experimento? BuscarPorId(int id) =>
        _itens.FirstOrDefault(e => e.Id == id);

    public IReadOnlyList<Experimento> BuscarPorNome(string trecho) =>
        _itens.Where(e => e.Nome.Contains(trecho, StringComparison.OrdinalIgnoreCase)
                       || e.Objeto.Contains(trecho, StringComparison.OrdinalIgnoreCase))
              .OrderBy(e => e.Nome).ToList();

    public IReadOnlyList<Experimento> BuscarPorCategoria(Categoria categoria) =>
        _itens.Where(e => e.Categoria == categoria).OrderBy(e => e.Nivel).ThenBy(e => e.Nome).ToList();

    public Experimento Adicionar(Experimento experimento)
    {
        experimento.Id = _itens.Count == 0 ? 1 : _itens.Max(e => e.Id) + 1;
        experimento.CriadoEm = DateTime.Now;
        _itens.Add(experimento);
        Salvar();
        return experimento;
    }

    public void Atualizar(Experimento experimento)
    {
        var indice = _itens.FindIndex(e => e.Id == experimento.Id);
        if (indice < 0)
            throw new KeyNotFoundException($"Experimento {experimento.Id} nao encontrado.");
        _itens[indice] = experimento;
        Salvar();
    }

    public bool ExisteNome(string nome, int? ignorarId = null) =>
        _itens.Any(e => e.Nome.Equals(nome.Trim(), StringComparison.OrdinalIgnoreCase)
                     && (ignorarId == null || e.Id != ignorarId));

    public int Contar() => _itens.Count;

    // ---------- arquivo ----------

    private static List<Experimento> Carregar(string caminho, string? caminhoSeed)
    {
        try
        {
            if (!File.Exists(caminho) && caminhoSeed != null && File.Exists(caminhoSeed))
            {
                // primeira execucao: copia a carga inicial
                Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
                File.Copy(caminhoSeed, caminho);
            }

            if (!File.Exists(caminho))
                return new List<Experimento>();

            var json = File.ReadAllText(caminho);
            return JsonSerializer.Deserialize<List<Experimento>>(json, Opcoes) ?? new List<Experimento>();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"O arquivo {caminho} esta corrompido: {ex.Message}", ex);
        }
    }

    private void Salvar()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_caminho)!);
        var temporario = _caminho + ".tmp";
        File.WriteAllText(temporario, JsonSerializer.Serialize(_itens, Opcoes));
        File.Move(temporario, _caminho, overwrite: true);   // gravacao atomica
    }
}
