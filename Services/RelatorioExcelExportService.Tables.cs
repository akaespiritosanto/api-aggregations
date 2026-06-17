namespace api_aggregations.Services;

using System.Globalization;
using api_aggregations.Dtos;
using System.Text.RegularExpressions;

public sealed partial class RelatorioExcelExportService
{
    private static readonly CultureInfo PtPtCulture = new("pt-PT");

    private async Task<ExportTable> CriarTabelaProdutoAsync(
        TotaisQuery query,
        string mostrar,
        CancellationToken cancellationToken)
    {
        // Use the totals grouped by product (AbreviaturaProduto) so the Excel
        // column names are the product abbreviations from the database.
        var dados = await _relatorioService.GetTotaisProdutoAsync(query, cancellationToken);

        // The Excel columns come from all monthly rows plus the yearly totals.
        // Distinct avoids repeated columns when the same product appears in many months.
        // We explicitly use the produto.nome value which, when GetTotaisProdutoAsync
        // is used, contains the AbreviaturaProduto from the DB.
        var colunas = dados.meses
            .SelectMany(mes => mes.produtos.Select(produto => produto.nome))
            .Concat((mostrar == "valor" ? dados.totaisValorProdutoAno : dados.totaisDuracaoProdutoAno)
                .Select(produto => produto.nome))
            .Where(nome => !string.IsNullOrWhiteSpace(nome))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(nome => nome)
            .ToList();

        var linhas = CriarLinhasVazias();

        foreach (var mes in dados.meses)
        {
            var linha = linhas[mes.mes];

            foreach (var produto in mes.produtos)
            {
                linha.Valores[produto.nome] = mostrar == "valor" ? produto.valor : produto.duracao;
            }

            linha.Total = mostrar == "valor" ? mes.totalValorMes : mes.totalDuracaoMes;
        }

        var totaisAno = (mostrar == "valor" ? dados.totaisValorProdutoAno : dados.totaisDuracaoProdutoAno)
            .ToDictionary(produto => produto.nome, produto => produto.valor, StringComparer.OrdinalIgnoreCase);

        // Display names for products are simply the abbreviation from DB.
        var displayNames = colunas.ToDictionary(c => c, c => c, StringComparer.OrdinalIgnoreCase);

        return new ExportTable(colunas, linhas.Values.ToList(), totaisAno, displayNames);
    }

    private async Task<ExportTable> CriarTabelaLugarAsync(
        TotaisQuery query,
        string mostrar,
        CancellationToken cancellationToken)
    {
        var dados = await _relatorioService.GetTotaisLugarPorLugarAsync(query, cancellationToken);

        // For places, use only the place name (lugar.nome) as the Excel column
        // header. If multiple DispBase entries share the same nome, they must
        // produce a single column and their values are summed into that column.
        var rawNomes = dados.meses
            .SelectMany(mes => mes.lugares.Select(l => l.nome ?? string.Empty))
            .Concat((mostrar == "valor" ? dados.totaisValorLugarAno : dados.totaisDuracaoLugarAno)
                .Select(l => l.nome ?? string.Empty))
            .Where(nome => !string.IsNullOrWhiteSpace(nome))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Order by the place name in a human-friendly numeric-aware way so
        // that "Maquina lavar 1", "Maquina lavar 2", "Maquina lavar 10"
        // appear in the expected numeric order.
        var colunasOrdered = rawNomes
            .Select(n => new { Nome = n, Parsed = ParseNomeParaOrdenacao(n) })
            .OrderBy(x => x.Parsed.BaseName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Parsed.Number ?? int.MaxValue)
            .Select(x => x.Nome)
            .ToList();

        var colunas = colunasOrdered;

        var linhas = CriarLinhasVazias();

        foreach (var mes in dados.meses)
        {
            var linha = linhas[mes.mes];

            foreach (var lugar in mes.lugares)
            {
                var nomeColuna = lugar.nome ?? string.Empty;

                if (string.IsNullOrWhiteSpace(nomeColuna))
                {
                    continue;
                }

                var valor = mostrar == "valor" ? lugar.valor : lugar.duracao;

                // Sum values when multiple DispBase entries share the same nome.
                if (linha.Valores.TryGetValue(nomeColuna, out var existente))
                {
                    linha.Valores[nomeColuna] = existente + valor;
                }
                else
                {
                    linha.Valores[nomeColuna] = valor;
                }
            }

            linha.Total = mostrar == "valor" ? mes.totalValorMes : mes.totalDuracaoMes;
        }

        // Aggregate yearly totals by nome only (ignore refDispBase) so columns
        // match and duplicates sum together.
        var totaisFonte = (mostrar == "valor" ? dados.totaisValorLugarAno : dados.totaisDuracaoLugarAno);
        var totaisAno = totaisFonte
            .Where(l => !string.IsNullOrWhiteSpace(l.nome))
            .GroupBy(l => l.nome, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key ?? string.Empty, g => g.Sum(x => x.valor), StringComparer.OrdinalIgnoreCase);

        // Build display names mapping: prefer a non-empty refDispBase when available.
        var allLugares = dados.meses.SelectMany(m => m.lugares).ToList();
        var displayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var nome in colunas)
        {
            var repRef = allLugares
                .Where(l => string.Equals(l.nome, nome, StringComparison.OrdinalIgnoreCase))
                .Select(l => l.refDispBase)
                .FirstOrDefault(r => !string.IsNullOrWhiteSpace(r));

            // If not found in monthly rows, try yearly totals source
            if (string.IsNullOrWhiteSpace(repRef))
            {
                repRef = totaisFonte
                    .Where(t => string.Equals(t.nome, nome, StringComparison.OrdinalIgnoreCase))
                    .Select(t => t.refDispBase)
                    .FirstOrDefault(r => !string.IsNullOrWhiteSpace(r));
            }

            var display = string.IsNullOrWhiteSpace(repRef) ? nome : $"{repRef} {nome}";
            displayNames[nome] = display;
        }

        return new ExportTable(colunas, linhas.Values.ToList(), totaisAno, displayNames);
    }

    private static SortedDictionary<int, ExportRow> CriarLinhasVazias()
    {
        var linhas = new SortedDictionary<int, ExportRow>();

        // Always create the twelve months so the Excel layout stays stable even
        // when some months have no values.
        for (var mes = 1; mes <= 12; mes++)
        {
            var nomeMes = PtPtCulture.TextInfo.ToTitleCase(PtPtCulture.DateTimeFormat.GetMonthName(mes));
            linhas.Add(mes, new ExportRow(nomeMes));
        }

        return linhas;
    }

    private static string CriarNomeLugar(string nome, string refDispBase)
    {
        // The export uses only the place name (nome) as the column header.
        // DispBase is ignored here because multiple DispBase entries sharing
        // the same nome must be aggregated into a single column.
        return nome;
    }

    private static (string BaseName, int? Number) ParseNomeParaOrdenacao(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return (string.Empty, null);
        }

        // Match an optional numeric suffix, e.g. "Maquina lavar 12" -> base="Maquina lavar", number=12
        var m = Regex.Match(nome.Trim(), @"^(.*?)(?:\s+(\d+))?$");

        if (!m.Success)
        {
            return (nome.Trim(), null);
        }

        var baseName = m.Groups[1].Value.Trim();

        if (m.Groups.Count >= 3 && int.TryParse(m.Groups[2].Value, out var num))
        {
            return (baseName, num);
        }

        return (baseName, null);
    }
}
