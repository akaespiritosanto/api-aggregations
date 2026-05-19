namespace api_aggregations.Dtos;

public sealed class TotaisLugarDto
{
    public required List<TotaisMesLugarDto> meses { get; init; }
    public required List<TotaisTotalLugarAnoDto> totaisValorLugarAno { get; init; }
    public required decimal totalValorAno { get; init; }
    public required List<TotaisTotalLugarAnoDto> totaisDuracaoLugarAno { get; init; }
    public required decimal totalDuracaoAno { get; init; }
}

public sealed class TotaisMesLugarDto
{
    public required int ano { get; init; }
    public required int mes { get; init; }
    public required string nome { get; init; }
    public required List<TotaisLugarItemDto> lugares { get; init; }
    public required decimal totalValorMes { get; init; }
    public required decimal totalDuracaoMes { get; init; }
}

public sealed class TotaisLugarItemDto
{
    public required string nome { get; init; }
    public required string refDispBase { get; init; }
    public required decimal valor { get; init; }
    public required decimal duracao { get; init; }
}

public sealed class TotaisTotalLugarAnoDto
{
    public required string nome { get; init; }
    public required string refDispBase { get; init; }
    public required decimal valor { get; init; }
}
