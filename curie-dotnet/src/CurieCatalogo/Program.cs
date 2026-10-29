using CurieCatalogo.Models;
using CurieCatalogo.Repositories;
using CurieCatalogo.Services;
using CurieCatalogo.UI;
using CurieCatalogo.Validation;

// =====================================================================
//  Projeto CURIE - Catalogo de Experimentos (console .NET 8)
//  Challenge FIAP 2026 - CTMSP / Marinha do Brasil - Sprint 1
//  Grupo CURIE - 2TDSPB: Matheus Carneiro Maciel (567753),
//     Murilo Marques (568224), Paulo Henrique da Silva Kian (563343)
// =====================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

var pastaDados = Path.Combine(AppContext.BaseDirectory, "data");
var arquivo = Path.Combine(pastaDados, "experimentos.json");
var seed = Path.Combine(pastaDados, "experimentos.seed.json");

CatalogoService servico;
try
{
    servico = new CatalogoService(new JsonExperimentoRepository(arquivo, seed));
}
catch (Exception ex)
{
    Tela.Erro($"Nao foi possivel carregar os dados: {ex.Message}");
    return 1;
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(@"
   ____ _   _ ____  ___ _____
  / ___| | | |  _ \|_ _| ____|   Catalogo de Experimentos
 | |   | | | | |_) || ||  _|     Projeto CURIE - Challenge FIAP 2026 / CTMSP
 | |___| |_| |  _ < | || |___    Grupo CURIE - 2TDSPB
  \____|\___/|_| \_\___|_____|
");
Console.ResetColor();
Console.WriteLine($"Dados: {arquivo}");

while (true)
{
    var (total, publicados, rascunhos) = servico.Resumo();
    Tela.Titulo($"MENU  ({total} experimentos: {publicados} publicados, {rascunhos} rascunhos)");
    Console.WriteLine(" 1. Listar todos");
    Console.WriteLine(" 2. Listar publicados (o que o app mostra)");
    Console.WriteLine(" 3. Buscar por nome/objeto");
    Console.WriteLine(" 4. Buscar por categoria");
    Console.WriteLine(" 5. Ver detalhes");
    Console.WriteLine(" 6. Cadastrar experimento");
    Console.WriteLine(" 7. Editar experimento");
    Console.WriteLine(" 8. Publicar experimento");
    Console.WriteLine(" 9. Despublicar experimento");
    Console.WriteLine("10. Avaliar uma leitura do kit (simulacao)");
    Console.WriteLine(" 0. Sair");
    Console.Write("Opcao: ");
    var opcao = Console.ReadLine()?.Trim();

    try
    {
        switch (opcao)
        {
            case "1":
                Tela.Titulo("TODOS OS EXPERIMENTOS");
                Tela.Tabela(servico.Listar());
                break;

            case "2":
                Tela.Titulo("EXPERIMENTOS PUBLICADOS");
                Tela.Tabela(servico.ListarPublicados());
                break;

            case "3":
                Tela.Titulo("BUSCA POR NOME OU OBJETO");
                var trecho = Tela.LerTexto("Trecho");
                Tela.Tabela(servico.BuscarPorNome(trecho));
                break;

            case "4":
                Tela.Titulo("BUSCA POR CATEGORIA");
                var cat = Tela.LerCategoria();
                Tela.Tabela(servico.BuscarPorCategoria(cat));
                break;

            case "5":
                Tela.Titulo("DETALHES");
                Tela.Detalhe(servico.Buscar(Tela.LerInteiro("Id")));
                break;

            case "6":
                Tela.Titulo("CADASTRAR EXPERIMENTO");
                var novo = new Experimento
                {
                    Nome = Tela.LerTexto("Nome"),
                    Objeto = Tela.LerTexto("Objeto medido"),
                    Categoria = Tela.LerCategoria(),
                    CpmMin = Tela.LerDecimal("CPM minimo esperado"),
                    CpmMax = Tela.LerDecimal("CPM maximo esperado"),
                    Nivel = Tela.LerInteiro("Nivel (1-3)"),
                    Explicacao = Tela.LerTexto("Explicacao educativa")
                };
                var criado = servico.Cadastrar(novo);
                Tela.Sucesso($"Experimento {criado.Id} cadastrado como rascunho.");
                break;

            case "7":
                Tela.Titulo("EDITAR EXPERIMENTO  (ENTER mantem o valor atual)");
                var atual = servico.Buscar(Tela.LerInteiro("Id"));
                var editado = new Experimento
                {
                    Id = atual.Id,
                    Nome = Tela.LerTexto("Nome", atual.Nome),
                    Objeto = Tela.LerTexto("Objeto medido", atual.Objeto),
                    Categoria = Tela.LerCategoria(atual.Categoria),
                    CpmMin = Tela.LerDecimal("CPM minimo", atual.CpmMin),
                    CpmMax = Tela.LerDecimal("CPM maximo", atual.CpmMax),
                    Nivel = Tela.LerInteiro("Nivel (1-3)", atual.Nivel),
                    Explicacao = Tela.LerTexto("Explicacao", atual.Explicacao)
                };
                servico.Atualizar(editado);
                Tela.Sucesso($"Experimento {editado.Id} atualizado.");
                break;

            case "8":
                Tela.Titulo("PUBLICAR");
                var pub = servico.Publicar(Tela.LerInteiro("Id"));
                Tela.Sucesso($"'{pub.Nome}' publicado. Agora aparece no app CURIE.");
                break;

            case "9":
                Tela.Titulo("DESPUBLICAR");
                var desp = servico.Despublicar(Tela.LerInteiro("Id"));
                Tela.Sucesso($"'{desp.Nome}' voltou para rascunho.");
                break;

            case "10":
                Tela.Titulo("AVALIAR LEITURA DO KIT");
                var idExp = Tela.LerInteiro("Id do experimento");
                var cpm = Tela.LerDecimal("Leitura do Geiger (CPM)");
                Console.WriteLine(servico.AvaliarLeitura(idExp, cpm));
                break;

            case "0":
                Console.WriteLine("Ate a proxima missao!");
                return 0;

            default:
                Tela.Aviso("Opcao invalida.");
                break;
        }
    }
    catch (ValidacaoException ex)
    {
        Tela.Erro("Dados invalidos:");
        foreach (var e in ex.Erros) Console.WriteLine($"   - {e}");
    }
    catch (RegraDeNegocioException ex)
    {
        Tela.Erro($"Regra de negocio: {ex.Message}");
    }
    catch (KeyNotFoundException ex)
    {
        Tela.Erro(ex.Message);
    }
    catch (IOException ex)
    {
        Tela.Erro($"Falha ao gravar o arquivo de dados: {ex.Message}");
    }
    catch (Exception ex)
    {
        Tela.Erro($"Erro inesperado: {ex.Message}");
    }

    Tela.Pausa();
}
