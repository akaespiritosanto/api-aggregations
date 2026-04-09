namespace api_aggregations.Dtos;

public sealed class ProdutoReservadoQuery : PaginationQuery
{
    public int? id_reserva { get; set; }
    public int? id_produto { get; set; }
    public short? estado { get; set; }
    public string? referencia { get; set; }
    public byte? agregado { get; set; }
}

