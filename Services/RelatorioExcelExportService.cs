namespace api_aggregations.Services;

using api_aggregations.Dtos;
using api_aggregations.Exceptions;

public sealed partial class RelatorioExcelExportService
{
    private readonly RelatorioValoresEDuracaoReservasService _relatorioService;

    public RelatorioExcelExportService(RelatorioValoresEDuracaoReservasService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    public async Task<byte[]> CriarExcelAsync(ExportarTotaisQuery query, CancellationToken cancellationToken)
    {
        var agruparPor = NormalizarTexto(query.agruparPor);
        var mostrar = NormalizarTexto(query.mostrar);

        ValidarOpcoesExportacao(agruparPor, mostrar);

        // Step 1: convert report data into a simple table.
        // Step 2: write that table into a real .xlsx file.
        var tabela = agruparPor == "produto"
            ? await CriarTabelaProdutoAsync(query, mostrar, cancellationToken)
            : await CriarTabelaLugarAsync(query, mostrar, cancellationToken);

        return CriarArquivoExcel(tabela, mostrar);
    }

    private static string NormalizarTexto(string? texto)
    {
        return (texto ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static void ValidarOpcoesExportacao(string agruparPor, string mostrar)
    {
        if (agruparPor != "produto" && agruparPor != "lugar")
        {
            throw new BadRequestException("Use agruparPor=produto ou agruparPor=lugar.");
        }

        if (mostrar != "valor" && mostrar != "duracao")
        {
            throw new BadRequestException("Use mostrar=valor ou mostrar=duracao.");
        }
    }

    private sealed record ExportTable(
        List<string> Colunas,
        List<ExportRow> Linhas,
        Dictionary<string, decimal> TotaisAno,
        Dictionary<string, string> ColumnDisplayNames);

    private sealed class ExportRow
    {
        public ExportRow(string mes)
        {
            Mes = mes;
        }

        public string Mes { get; }
        public Dictionary<string, decimal> Valores { get; } = new(StringComparer.OrdinalIgnoreCase);
        public decimal Total { get; set; }
    }
}
