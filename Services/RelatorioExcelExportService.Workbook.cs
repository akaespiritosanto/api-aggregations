namespace api_aggregations.Services;

using System.Globalization;
using Expedita.Export.Excel;

public sealed partial class RelatorioExcelExportService
{
    private static byte[] CriarArquivoExcel(ExportTable tabela, string mostrar)
    {
        var pastaTemporaria = Path.Combine(Path.GetTempPath(), "api-agregations-excel");
        var nomeFicheiro = $"relatorio-{Guid.NewGuid():N}.xlsx";
        var caminhoFicheiro = Path.Combine(pastaTemporaria, nomeFicheiro);

        Directory.CreateDirectory(pastaTemporaria);

        var totalLinhas = tabela.Linhas.Count + 3;
        var totalColunas = tabela.Colunas.Count + 2;

        // Expedita writes the real workbook. The code below only fills cells.
        var documento = new xlsxDocumento(pastaTemporaria);
        var pagina = documento.AdicionarPagina("Resumo", totalLinhas, totalColunas);

        PreencherCabecalho(pagina, tabela, mostrar, totalColunas);
        PreencherMeses(pagina, tabela, totalColunas);
        PreencherTotalAno(pagina, tabela, totalColunas);

        Stream? outputStream = null;

        documento.Exportar(
            nomeFicheiro,
            xlsxDocumento.tipoExportacao.documentoXls,
            pastaTemporaria,
            ref outputStream);

        outputStream?.Dispose();

        // Expedita creates the workbook correctly, but its merge helper does not
        // produce the exact header merge we need. This fixes only the merge cells.
        CorrigirCelulasUnidas(caminhoFicheiro, totalColunas);

        var bytes = File.ReadAllBytes(caminhoFicheiro);
        File.Delete(caminhoFicheiro);

        return bytes;
    }

    private static void PreencherCabecalho(
        xlsxPagina pagina,
        ExportTable tabela,
        string mostrar,
        int totalColunas)
    {
        var titulo = mostrar == "valor" ? "Resumo por valor" : "Resumo Tempo";

        // These merges are kept for Expedita's internal model. The exact Excel
        // merge references are corrected after export in CorrigirCelulasUnidas.
        pagina.SetMergeRange(0, 0, 1, 0);
        pagina.SetMergeRange(0, 1, 0, totalColunas - 1);

        SetHeaderCell(pagina, 0, 0, "Mês");
        SetHeaderCell(pagina, 0, 1, titulo);

        for (var index = 0; index < tabela.Colunas.Count; index++)
        {
            var nomeColuna = ObterNomeColunaExcel(tabela.Colunas[index]);
            SetHeaderCell(pagina, 1, index + 1, nomeColuna);
        }

        SetHeaderCell(pagina, 1, totalColunas - 1, "Totais");
    }

    private static string ObterNomeColunaExcel(string nomeColuna)
    {
        var nomeColunaMinusculas = nomeColuna.ToLowerInvariant();

        // The database names may change slightly, so match by the important word.
        if (nomeColunaMinusculas.Contains("secar"))
        {
            return "Maquina Secar 1";
        }

        if (nomeColunaMinusculas.Contains("lavar"))
        {
            return "Maquina Lavar 2";
        }

        return nomeColuna;
    }

    private static void PreencherMeses(xlsxPagina pagina, ExportTable tabela, int totalColunas)
    {
        for (var index = 0; index < tabela.Linhas.Count; index++)
        {
            var linha = tabela.Linhas[index];
            var rowIndex = index + 2;

            SetTextCell(pagina, rowIndex, 0, linha.Mes);

            for (var columnIndex = 0; columnIndex < tabela.Colunas.Count; columnIndex++)
            {
                var coluna = tabela.Colunas[columnIndex];
                linha.Valores.TryGetValue(coluna, out var valor);
                SetNumberCell(pagina, rowIndex, columnIndex + 1, valor);
            }

            SetNumberCell(pagina, rowIndex, totalColunas - 1, linha.Total);
        }
    }

    private static void PreencherTotalAno(xlsxPagina pagina, ExportTable tabela, int totalColunas)
    {
        var rowIndex = tabela.Linhas.Count + 2;
        SetHeaderCell(pagina, rowIndex, 0, "Total Ano");

        for (var columnIndex = 0; columnIndex < tabela.Colunas.Count; columnIndex++)
        {
            var coluna = tabela.Colunas[columnIndex];
            tabela.TotaisAno.TryGetValue(coluna, out var valor);
            SetNumberCell(pagina, rowIndex, columnIndex + 1, valor);
        }

        SetNumberCell(pagina, rowIndex, totalColunas - 1, tabela.TotaisAno.Values.Sum());
    }

    private static void SetHeaderCell(xlsxPagina pagina, int rowIndex, int columnIndex, string value)
    {
        var cell = new xlsxCelula(value);
        cell.tipoValor = xlsxCelula.tiposValor.TextoHeader;
        pagina.set_Celula(rowIndex, columnIndex, cell);
    }

    private static void SetTextCell(xlsxPagina pagina, int rowIndex, int columnIndex, string value)
    {
        var cell = new xlsxCelula(value);
        cell.tipoValor = xlsxCelula.tiposValor.Texto;
        pagina.set_Celula(rowIndex, columnIndex, cell);
    }

    private static void SetNumberCell(xlsxPagina pagina, int rowIndex, int columnIndex, decimal value)
    {
        if (value == 0)
        {
            SetTextCell(pagina, rowIndex, columnIndex, string.Empty);
            return;
        }

        // Use invariant culture so decimal values are stored safely inside the
        // workbook. Excel can still display them with the user's local settings.
        var cell = new xlsxCelula(value.ToString("0.00", CultureInfo.InvariantCulture));
        cell.tipoValor = xlsxCelula.tiposValor.Real;
        pagina.set_Celula(rowIndex, columnIndex, cell);
    }
}
