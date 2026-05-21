namespace api_aggregations.Dtos;

public sealed class ExportarTotaisQuery : TotaisQuery
{
    /// <summary>produto or lugar</summary>
    public string? agruparPor { get; set; }

    /// <summary>valor or duracao</summary>
    public string? mostrar { get; set; }
}
