namespace api_aggregations.Models;

public class ProdutoReservado
{
    public int id { get; set; }

    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    public int id_reserva { get; set; }
    public short? estado { get; set; }

    public int id_tarifa { get; set; }
    public int id_produto { get; set; }

    public string? referencia { get; set; }
    public string? numero { get; set; }

    public byte? agregado { get; set; }
    public int? id_produto_reservado { get; set; }

    public string? hora_inicio { get; set; }
    public string? hora_fim { get; set; }

    public int? id_entidade { get; set; }

    public int? n_pessoas { get; set; }
    public int quantidade { get; set; }

    public double? desconto { get; set; }
    public double valor_tarifa { get; set; }
    public double valor_comissao { get; set; }
    public double valor_taxas { get; set; }

    public string ref_taxas { get; set; }

    public int id_factura { get; set; }

    public double valor_tarifa_origem { get; set; }

    public short? tipo_tarifa { get; set; }

    public DateTime data_cancelamento { get; set; }

    public double cancel_fee_aplicado { get; set; }

    public DateTime data_actualizacao { get; set; }
    public DateTime data_criacao { get; set; }

    public DateTime? checkin_hora { get; set; }

    public double total_imp_sob_taxas_tarifa { get; set; }
    public double total_imp_sob_supl { get; set; }
    public double total_impostos { get; set; }
    public double total_suplementos { get; set; }

    public string id_externo { get; set; }
    public int sistema_externo { get; set; }
    public string refExterno { get; set; }

    public int id_ponto_partida { get; set; }
    public int id_ponto_consumo { get; set; }
    public int id_ponto_destino { get; set; }

    public string nome_produto { get; set; }

    public string n_factura { get; set; }
    public string obs_factura { get; set; }

    public int id_disp_base { get; set; }

    public string id_lugar { get; set; }

    public decimal valor_comissao_suplementos { get; set; }

    public string? UserConfirmacao { get; set; }
    public string? UserCheckIn { get; set; }
    public string? UserCancelamento { get; set; }

    public int? IdUserConfirmacao { get; set; }
    public int? IdUserCheckIn { get; set; }
    public int? IdUserCancelamento { get; set; }

    public DateTime? DataCheckIn { get; set; }
    public DateTime? DataConfirmacao { get; set; }

    public int id_canal { get; set; }
    public int id_codigoDesconto { get; set; }

    public DateTime data_embarque { get; set; }
    public DateTime data_desembarque { get; set; }

    public double descontoAutomatico { get; set; }
}
