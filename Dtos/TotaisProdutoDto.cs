namespace api_aggregations.Dtos;

public sealed class TotaisProdutoDto
{
    public required List<RelatorioMesTotaisLugarDto> meses { get; init; }
    public required List<RelatorioTotalProdutoAnoDto> totaisValorProdutoAno { get; init; }
    public required decimal totaisValorAno { get; init; }
    public required List<RelatorioTotalProdutoAnoDto> totaisDuracaoProdutoAno { get; init; }
    public required decimal totalDuracaoAno { get; init; }
}

public sealed class RelatorioMesTotaisLugarDto
{
    public required int ano { get; init; }
    public required int mes { get; init; }
    public required string nome { get; init; }
    public required List<RelatorioProdutoMesDto> produtos { get; init; }
    public required decimal totalValorMes { get; init; }
    public required decimal totalDuracaoMes { get; init; }
}

public sealed class RelatorioProdutoMesDto
{
    public required string id { get; init; }
    public required string nome { get; init; }
    public required decimal valor { get; init; }
    public required decimal duracao { get; init; }
}

public sealed class RelatorioTotalProdutoAnoDto
{
    public required string id { get; init; }
    public required string nome { get; init; }
    public required decimal valor { get; init; }
}
