using System.Globalization;
using CurieCatalogo.Models;

namespace CurieCatalogo.UI;

/// <summary>Helpers de entrada/saida do console com leitura segura (nunca quebra a aplicacao).</summary>
public static class Tela
{
    public static void Titulo(string texto)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"== {texto} ".PadRight(70, '='));
        Console.ResetColor();
    }

    public static void Sucesso(string texto)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[OK] {texto}");
        Console.ResetColor();
    }

    public static void Erro(string texto)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERRO] {texto}");
        Console.ResetColor();
    }

    public static void Aviso(string texto)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[!] {texto}");
        Console.ResetColor();
    }

    public static string LerTexto(string rotulo, string? atual = null)
    {
        var sufixo = atual != null ? $" [{atual}]" : "";
        Console.Write($"{rotulo}{sufixo}: ");
        var valor = Console.ReadLine() ?? "";
        return string.IsNullOrWhiteSpace(valor) && atual != null ? atual : valor;
    }

    public static int LerInteiro(string rotulo, int? atual = null)
    {
        while (true)
        {
            var texto = LerTexto(rotulo, atual?.ToString());
            if (int.TryParse(texto, out var n)) return n;
            Erro("Digite um numero inteiro.");
        }
    }

    public static decimal LerDecimal(string rotulo, decimal? atual = null)
    {
        while (true)
        {
            var texto = LerTexto(rotulo, atual?.ToString("0.##", CultureInfo.InvariantCulture))
                        .Replace(',', '.');
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)) return d;
            Erro("Digite um numero (ex.: 31.5).");
        }
    }

    public static Categoria LerCategoria(Categoria? atual = null)
    {
        var nomes = Enum.GetNames<Categoria>();
        Console.WriteLine("Categorias: " + string.Join(", ", nomes.Select((n, i) => $"{i + 1}-{n}")));
        while (true)
        {
            var texto = LerTexto("Categoria (numero ou nome)", atual?.ToString());
            if (int.TryParse(texto, out var i) && i >= 1 && i <= nomes.Length)
                return (Categoria)(i - 1);
            if (Enum.TryParse<Categoria>(texto, ignoreCase: true, out var c))
                return c;
            Erro("Categoria invalida.");
        }
    }

    public static void Tabela(IEnumerable<Experimento> itens)
    {
        var lista = itens.ToList();
        if (lista.Count == 0) { Aviso("Nenhum experimento encontrado."); return; }
        Console.WriteLine($"{"ID",4} | {"NOME",-28} | {"OBJETO",-26} | {"CATEG.",-9} | NIVEL | {"FAIXA",-22} | STATUS");
        Console.WriteLine(new string('-', 118));
        foreach (var e in lista) Console.WriteLine(e);
        Console.WriteLine($"{lista.Count} registro(s).");
    }

    public static void Detalhe(Experimento e)
    {
        Console.WriteLine($"ID:          {e.Id}");
        Console.WriteLine($"Nome:        {e.Nome}");
        Console.WriteLine($"Objeto:      {e.Objeto}");
        Console.WriteLine($"Categoria:   {e.Categoria}");
        Console.WriteLine($"Faixa CPM:   {(e.EhTeorico ? "teorico (sem medicao)" : $"{e.CpmMin:0.##} a {e.CpmMax:0.##}")}");
        Console.WriteLine($"Nivel:       {e.Nivel}");
        Console.WriteLine($"Status:      {(e.Publicado ? "PUBLICADO" : "rascunho")}");
        Console.WriteLine($"Criado em:   {e.CriadoEm:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"Explicacao:  {e.Explicacao}");
    }

    public static void Pausa()
    {
        Console.WriteLine();
        Console.Write("Pressione ENTER para continuar...");
        Console.ReadLine();
    }
}
