# CURIE Catálogo — Advanced Business Development with .NET · Sprint 1

Prova de conceito em **.NET 8** de uma funcionalidade do Projeto CURIE: o **catálogo de experimentos** (missões) que o kit e o app usam. Aplicação console com uma entidade do domínio, validações, regras de negócio, cadastro, consulta e persistência em JSON.

## Grupo CURIE — Turma 2TDSPB

| Integrante | RM |
|---|---|
| Matheus Carneiro Maciel | 567753 |
| Murilo Marques | 568224 |
| Paulo Henrique da Silva Kian | 563343 |

## A solução (resumo)

A tecnologia nuclear é pouco compreendida e associada quase só a usinas e acidentes. O **Projeto CURIE** — *“Um laboratório nuclear em cada escola por R$ 300”* — leva um kit de ciência cidadã (ESP32 + sensor Geiger-Müller + NFC) para dentro da escola. Guiados por **missões** no app, os alunos medem a radiação de objetos do cotidiano (banana, sal light, granito, detector de fumaça) e cada medição alimenta um mapa colaborativo nacional. Uma tutora de IA explica os resultados citando fontes da CNEN e do CTMSP.

## O que foi desenvolvido nesta Sprint

A entidade **`Experimento`** é o catálogo de missões: define o objeto a medir, a faixa de contagens por minuto (CPM) esperada, o nível e a explicação educativa. É a tabela `T_CURIE_EXPERIMENTO` do modelo de dados do grupo — a mesma que a API Java referencia ao registrar medições (`id_experimento`).

**Funcionalidade diferente e complementar à API Java**: o Java gerencia escolas, usuários, kits e medições; este console gerencia **o conteúdo pedagógico** que alimenta tudo isso. Um experimento só aparece no app depois de **publicado**, e publicar é uma transição controlada por regra de negócio.

| Requisito | Onde está |
|---|---|
| 1 entidade do domínio com atributos coerentes | `Models/Experimento.cs` (id, nome, objeto, categoria, cpmMin, cpmMax, nivel, explicacao, publicado) |
| Validações dos atributos | `Experimento.Validar()` — obrigatórios, tamanhos, faixa de CPM, nível 1–3; devolve **todos** os erros de uma vez |
| Regras de negócio | `Services/CatalogoService.cs` — nome único; **publicar só com faixa de CPM definida ou conteúdo teórico de nível 3**; nível 3 exige explicação ≥ 60 caracteres; cadastro nasce como rascunho |
| Tratamento de erros com try/catch | `Program.cs` captura `ValidacaoException`, `RegraDeNegocioException`, `KeyNotFoundException`, `IOException` e `Exception` — a aplicação nunca quebra |
| Cadastro | opção 6 do menu |
| Consulta: listagem e busca por critério | opções 1–5: todos, publicados, **por nome/objeto**, **por categoria**, por id |
| Persistência entre execuções | `Repositories/JsonExperimentoRepository.cs` — grava `data/experimentos.json` a cada alteração (escrita atômica) |
| Extra | opção 10 simula a avaliação de uma leitura do kit (dentro/fora da faixa), a mesma lógica que a API usa |

## Como o .NET se aplica à solução do grupo

No CURIE, o conteúdo pedagógico (quais missões existem, que faixa de radiação é esperada para cada objeto, qual explicação o aluno recebe) precisa ser curado por professores e pelo CTMSP antes de chegar ao app — não pode ser editado pelo mesmo caminho que registra medições. O .NET entra como a **ferramenta de gestão do catálogo**: uma aplicação de back-office com validações fortes e um fluxo de publicação controlado, separada da API pública. Nesta Sprint ela é um console com persistência em JSON para provar o domínio; na Sprint 2 o mesmo `IExperimentoRepository` ganha uma implementação com **Entity Framework Core + Oracle** sobre `T_CURIE_EXPERIMENTO`, e a aplicação pode evoluir para uma API ASP.NET Core ou um painel Blazor para a equipe do CTMSP — sem mexer nas regras de negócio, que já estão isoladas no `CatalogoService`. O .NET, com tipagem forte, `record`/classes com invariantes e um ecossistema maduro para aplicações corporativas, é a escolha natural para a parte administrativa da plataforma.

## Arquitetura

```
src/CurieCatalogo
├── Program.cs                 → menu do console e tratamento global de erros (try/catch)
├── Models/
│   ├── Experimento.cs         → entidade + Validar()
│   └── Categoria.cs           → enum alinhado ao CHECK do banco
├── Validation/
│   ├── ValidacaoException.cs  → erros de atributo (lista)
│   └── RegraDeNegocioException.cs
├── Services/
│   └── CatalogoService.cs     → regras de negócio (nome único, publicação controlada, nível 3)
├── Repositories/
│   ├── IExperimentoRepository.cs      → contrato (Sprint 2: EF Core + Oracle)
│   └── JsonExperimentoRepository.cs   → persistência em JSON
├── UI/Console.cs              → leitura segura de entrada, tabelas, cores
└── data/experimentos.seed.json → carga inicial (7 experimentos)
```

## Como executar

Pré-requisito: [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) (`dotnet --version` deve mostrar 8.x).

```bash
git clone https://github.com/kakarneiro/curie-dotnet.git
cd curie-dotnet
dotnet run --project src/CurieCatalogo
```

Na primeira execução o programa copia `experimentos.seed.json` para `experimentos.json` (7 experimentos: 5 publicados, 2 rascunhos). Tudo que você cadastrar ou alterar fica gravado nesse arquivo e aparece na próxima execução.

Pelo Visual Studio: abra `CurieCatalogo.sln` e pressione F5.

## Roteiro de demonstração (2 minutos)

1. **Opção 1** — listar os 7 experimentos (5 publicados, 2 rascunhos).
2. **Opção 3** — buscar `banana` → encontra “A banana radioativa”.
3. **Opção 6** — cadastrar com nome vazio e CPM máximo menor que o mínimo → lista de erros de validação, aplicação continua.
4. **Opção 6** — cadastrar “Fertilizante NPK” (objeto: saco de fertilizante, categoria Mineral, 15–60 CPM, nível 2, explicação ≥ 20 caracteres) → criado como rascunho.
5. **Opção 8** — publicar o id 7 (“Irradiação de alimentos”, teórico nível 2) → **regra de negócio bloqueia**; publicar o id 6 (teórico nível 3) → publica.
6. **Opção 10** — experimento 2, leitura 31.5 → dentro da faixa; leitura 300 → fora da faixa.
7. Fechar (opção 0), abrir de novo, **opção 1** → o “Fertilizante NPK” continua lá (persistência).

## Próximas sprints

- Implementação `OracleExperimentoRepository` com Entity Framework Core sobre `T_CURIE_EXPERIMENTO`.
- Publicação do catálogo via endpoint consumido pela API Java e pelo app Flutter.
- Painel web (ASP.NET Core / Blazor) para a equipe do CTMSP curar o conteúdo.
