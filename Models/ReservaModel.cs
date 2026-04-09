namespace api_aggregations.Models;

public class Reserva
{
    public int id { get; set; }

    public string? numero { get; set; }
    public byte? tipo { get; set; }
    public short? estado { get; set; }

    public DateTime? data_pedido { get; set; }
    public DateTime? data_anulacao { get; set; }

    public int id_vendedor { get; set; }

    public string? referencia { get; set; }
    public string? observacoes { get; set; }

    public string id_externo { get; set; }
    public int sistema_externo { get; set; }

    public DateTime data_actualizacao { get; set; }

    public byte estado_pagamento { get; set; }

    public int id_canal { get; set; }

    public string nome_utilizador_confirmacao { get; set; }
}