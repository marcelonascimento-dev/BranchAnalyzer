# Branch Analyzer

Ferramenta desktop para analise de branches Git. Permite verificar status de merge, analisar multiplos branches em lote e visualizar informacoes do branch atual. **Somente leitura** — nao altera o repositorio.

## Funcionalidades

### Status Merge
Compara dois branches (Receptor A vs Feature B) e mostra:
- Status de merge (mergeado ou pendente)
- Commits pendentes de merge com autor, data e mensagem
- Commits ahead (que existem em A mas nao em B)
- Merge base entre os branches
- Arquivos alterados, conflitos potenciais e contribuidores

### Lote (Multi-Branch)
Analise em lote de multiplos branches contra um branch receptor:
- Selecao em massa com checkbox (selecionar todos, desmarcar, inverter)
- **Filtros avancados:**
  - Por nome (busca textual)
  - Por autor do ultimo commit
  - Por tipo/prefixo do branch (feature/, bugfix/, etc.)
  - Por periodo (7d, 15d, 30d, 60d, 90d) — baseado na data do ultimo commit
- Resultado em grid com status, commits pendentes, conflitos e ultimo autor

### Meu Branch
Informacoes do branch atual:
- Status vs remote (ahead/behind)
- Alteracoes locais nao commitadas
- Stashes salvos
- Branches locais com data do ultimo commit
- Grid com ultimos commits do branch

## Exportacao

Todos os resultados podem ser exportados em:
- **CSV** — planilha
- **JSON** — dados estruturados
- **TXT** — relatorio formatado (apenas no modo Lote)

## Configuracoes

O app salva configuracoes em `config.json` na pasta do executavel:
- Ultimo repositorio selecionado
- Posicao e tamanho da janela

## Requisitos

- Windows com .NET 8.0
- Git instalado e acessivel no PATH

## Como usar

1. Abra o app — ele detecta automaticamente o repositorio Git na pasta atual
2. Ou clique em **Selecionar Repo** para escolher outro repositorio
3. O app faz fetch automatico ao abrir para sincronizar branches remotos
4. Navegue pelas abas para usar as funcionalidades

## Build

```bash
dotnet build
dotnet run
```
