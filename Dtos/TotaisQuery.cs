namespace api_aggregations.Dtos;

public sealed class TotaisQuery
{
    /// <summary>optional</summary>
    public int? idServico { get; set; }
    /// <summary>optional</summary>
    public string? dataInicio { get; set; }
    /// <summary>optional</summary>
    public string? dataFim { get; set; }
    /// <summary>optional</summary>
    public int? idDispBase { get; set; }
}
