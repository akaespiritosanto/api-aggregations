namespace api_aggregations.Services;

using System.Globalization;
using api_aggregations.Dtos;

public sealed partial class RelatorioExcelExportService
{
    private static readonly CultureInfo PtPtCulture = new("pt-PT");

    private async Task<ExportTable> CriarTabelaProdutoAsync(
        TotaisQuery query,
        string mostrar,
        CancellationToken cancellationToken)
    {
        var dados = await _relatorioService.GetTotaisProdutoPorRefDispBaseAsync(query, cancellationToken);

        // The Excel columns come from all monthly rows plus the yearly totals.
        // Distinct avoids repeated columns when the same product appears in many months.
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

        return new ExportTable(colunas, linhas.Values.ToList(), totaisAno);
    }

    private async Task<ExportTable> CriarTabelaLugarAsync(
        TotaisQuery query,
        string mostrar,
        CancellationToken cancellationToken)
    {
        var dados = await _relatorioService.GetTotaisLugarPorLugarAsync(query, cancellationToken);

        // For places, the visible column name includes the DispBase reference
        // when it exists, so equal place names can still be identified.
        var colunas = dados.meses
            .SelectMany(mes => mes.lugares.Select(lugar => CriarNomeLugar(lugar.nome, lugar.refDispBase)))
            .Concat((mostrar == "valor" ? dados.totaisValorLugarAno : dados.totaisDuracaoLugarAno)
                .Select(lugar => CriarNomeLugar(lugar.nome, lugar.refDispBase)))
            .Where(nome => !string.IsNullOrWhiteSpace(nome))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(nome => nome)
            .ToList();

        var linhas = CriarLinhasVazias();

        foreach (var mes in dados.meses)
        {
            var linha = linhas[mes.mes];

            foreach (var lugar in mes.lugares)
            {
                var nomeColuna = CriarNomeLugar(lugar.nome, lugar.refDispBase);
                linha.Valores[nomeColuna] = mostrar == "valor" ? lugar.valor : lugar.duracao;
            }

            linha.Total = mostrar == "valor" ? mes.totalValorMes : mes.totalDuracaoMes;
        }

        var totaisAno = (mostrar == "valor" ? dados.totaisValorLugarAno : dados.totaisDuracaoLugarAno)
            .ToDictionary(lugar => CriarNomeLugar(lugar.nome, lugar.refDispBase), lugar => lugar.valor, StringComparer.OrdinalIgnoreCase);

        return new ExportTable(colunas, linhas.Values.ToList(), totaisAno);
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
        if (string.IsNullOrWhiteSpace(refDispBase))
        {
            return nome;
        }

        return $"{nome} ({refDispBase})";
    }
}
