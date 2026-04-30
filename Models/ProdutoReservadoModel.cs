namespace api_aggregations.Models;

public class ProdutoReservado
{
    /// <summary>mandatory</summary>
    public int id { get; set; }

    /// <summary>optional</summary>
    public string? DataInicio { get; set; }
    /// <summary>optional</summary>
    public string? DataFim { get; set; }

    /// <summary>mandatory</summary>
    public int id_reserva { get; set; }
    /// <summary>optional</summary>
    public short? estado { get; set; }

    /// <summary>mandatory</summary>
    public int id_tarifa { get; set; }
    /// <summary>mandatory</summary>
    public int id_produto { get; set; }

    /// <summary>optional</summary>
    public string? referencia { get; set; }
    /// <summary>optional</summary>
    public string? numero { get; set; }

    /// <summary>optional</summary>
    public byte? agregado { get; set; }
    /// <summary>optional</summary>
    public int? id_produto_reservado { get; set; }

    /// <summary>optional</summary>
    public string? hora_inicio { get; set; }
    /// <summary>optional</summary>
    public string? hora_fim { get; set; }

    /// <summary>optional</summary>
    public int? id_entidade { get; set; }

    /// <summary>optional</summary>
    public int? n_pessoas { get; set; }
    /// <summary>mandatory</summary>
    public int quantidade { get; set; }

    /// <summary>optional</summary>
    public double? desconto { get; set; }
    /// <summary>mandatory</summary>
    public double valor_tarifa { get; set; }
    /// <summary>mandatory</summary>
    public double valor_comissao { get; set; }
    /// <summary>mandatory</summary>
    public double valor_taxas { get; set; }

    /// <summary>mandatory</summary>
    public string ref_taxas { get; set; }

    /// <summary>mandatory</summary>
    public int id_factura { get; set; }

    /// <summary>mandatory</summary>
    public double valor_tarifa_origem { get; set; }

    /// <summary>optional</summary>
    public short? tipo_tarifa { get; set; }

    /// <summary>mandatory</summary>
    public string data_cancelamento { get; set; }

    /// <summary>mandatory</summary>
    public double cancel_fee_aplicado { get; set; }

    /// <summary>mandatory</summary>
    public string data_actualizacao { get; set; }
    /// <summary>mandatory</summary>
    public string data_criacao { get; set; }

    /// <summary>optional</summary>
    public string? checkin_hora { get; set; }

    /// <summary>mandatory</summary>
    public double total_imp_sob_taxas_tarifa { get; set; }
    /// <summary>mandatory</summary>
    public double total_imp_sob_supl { get; set; }
    /// <summary>mandatory</summary>
    public double total_impostos { get; set; }
    /// <summary>mandatory</summary>
    public double total_suplementos { get; set; }

    /// <summary>mandatory</summary>
    public string id_externo { get; set; }
    /// <summary>mandatory</summary>
    public int sistema_externo { get; set; }
    /// <summary>mandatory</summary>
    public string refExterno { get; set; }

    /// <summary>mandatory</summary>
    public int id_ponto_partida { get; set; }
    /// <summary>mandatory</summary>
    public int id_ponto_consumo { get; set; }
    /// <summary>mandatory</summary>
    public int id_ponto_destino { get; set; }

    /// <summary>mandatory</summary>
    public string nome_produto { get; set; }

    /// <summary>mandatory</summary>
    public string n_factura { get; set; }
    /// <summary>mandatory</summary>
    public string obs_factura { get; set; }

    /// <summary>mandatory</summary>
    public int id_disp_base { get; set; }

    /// <summary>mandatory</summary>
    public string id_lugar { get; set; }

    /// <summary>mandatory</summary>
    public decimal valor_comissao_suplementos { get; set; }

    /// <summary>optional</summary>
    public string? UserConfirmacao { get; set; }
    /// <summary>optional</summary>
    public string? UserCheckIn { get; set; }
    /// <summary>optional</summary>
    public string? UserCancelamento { get; set; }

    /// <summary>optional</summary>
    public int? IdUserConfirmacao { get; set; }
    /// <summary>optional</summary>
    public int? IdUserCheckIn { get; set; }
    /// <summary>optional</summary>
    public int? IdUserCancelamento { get; set; }

    /// <summary>optional</summary>
    public string? DataCheckIn { get; set; }
    /// <summary>optional</summary>
    public string? DataConfirmacao { get; set; }

    /// <summary>mandatory</summary>
    public int id_canal { get; set; }
    /// <summary>mandatory</summary>
    public int id_codigoDesconto { get; set; }

    /// <summary>mandatory</summary>
    public string data_embarque { get; set; }
    /// <summary>mandatory</summary>
    public string data_desembarque { get; set; }

    /// <summary>mandatory</summary>
    public double descontoAutomatico { get; set; }
}
