namespace api_aggregations.Models;

public class Reserva
{
    /// <summary>mandatory</summary>
    public int id { get; set; }

    /// <summary>optional</summary>
    public string? numero { get; set; }
    /// <summary>optional</summary>
    public byte? tipo { get; set; }
    /// <summary>optional</summary>
    public short? estado { get; set; }

    /// <summary>optional</summary>
    public string? data_pedido { get; set; }
    /// <summary>optional</summary>
    public string? data_anulacao { get; set; }

    /// <summary>mandatory</summary>
    public int id_vendedor { get; set; }

    /// <summary>optional</summary>
    public string? referencia { get; set; }
    /// <summary>optional</summary>
    public string? observacoes { get; set; }

    /// <summary>mandatory</summary>
    public string id_externo { get; set; }
    /// <summary>mandatory</summary>
    public int sistema_externo { get; set; }

    /// <summary>mandatory</summary>
    public string data_actualizacao { get; set; }

    /// <summary>mandatory</summary>
    public byte estado_pagamento { get; set; }

    /// <summary>mandatory</summary>
    public int id_canal { get; set; }

    /// <summary>mandatory</summary>
    public string nome_utilizador_confirmacao { get; set; }
}
