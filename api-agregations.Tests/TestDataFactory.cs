namespace api_agregations.Tests;

using api_aggregations.Models;
using api_aggregations.Utils;

public static class TestDataFactory
{
    public static Reserva CreateReserva(int id, string numero, short estado)
    {
        return new Reserva
        {
            id = id,
            numero = numero,
            estado = estado,
            tipo = 1,
            data_pedido = DateStringHelper.ToDateString(DateTime.UtcNow.Date),
            data_anulacao = null,
            id_vendedor = 1,
            referencia = "REF",
            observacoes = "OBS",
            id_externo = $"EXT-{id}",
            sistema_externo = 1,
            data_actualizacao = DateStringHelper.ToDateString(DateTime.UtcNow),
            estado_pagamento = 1,
            id_canal = 1,
            nome_utilizador_confirmacao = "tester"
        };
    }

    public static ProdutoReservado CreateProdutoReservado(int id, int idReserva, string? referencia = null)
    {
        var now = DateTime.UtcNow;

        return new ProdutoReservado
        {
            id = id,
            DataInicio = DateStringHelper.ToDateString(now.Date),
            DataFim = DateStringHelper.ToDateString(now.Date.AddDays(1)),
            id_reserva = idReserva,
            estado = 1,
            id_tarifa = 1,
            id_produto = 10,
            referencia = referencia ?? $"REF-{id}",
            numero = $"NUM-{id}",
            agregado = 0,
            id_produto_reservado = null,
            hora_inicio = "10:00",
            hora_fim = "11:00",
            id_entidade = 1,
            n_pessoas = 2,
            quantidade = 1,
            desconto = 0,
            valor_tarifa = 100,
            valor_comissao = 0,
            valor_taxas = 0,
            ref_taxas = "TAX",
            id_factura = 1,
            valor_tarifa_origem = 100,
            tipo_tarifa = 1,
            data_cancelamento = DateStringHelper.ToDateString(now),
            cancel_fee_aplicado = 0,
            data_actualizacao = DateStringHelper.ToDateString(now),
            data_criacao = DateStringHelper.ToDateString(now),
            checkin_hora = null,
            total_imp_sob_taxas_tarifa = 0,
            total_imp_sob_supl = 0,
            total_impostos = 0,
            total_suplementos = 0,
            id_externo = $"EXT-PR-{id}",
            sistema_externo = 1,
            refExterno = $"REFEXT-{id}",
            id_ponto_partida = 1,
            id_ponto_consumo = 1,
            id_ponto_destino = 1,
            nome_produto = "Produto",
            n_factura = $"FAT-{id}",
            obs_factura = "OK",
            id_disp_base = 1,
            id_lugar = "LUGAR",
            valor_comissao_suplementos = 0,
            UserConfirmacao = null,
            UserCheckIn = null,
            UserCancelamento = null,
            IdUserConfirmacao = null,
            IdUserCheckIn = null,
            IdUserCancelamento = null,
            DataCheckIn = null,
            DataConfirmacao = null,
            id_canal = 1,
            id_codigoDesconto = 0,
            data_embarque = DateStringHelper.ToDateString(now),
            data_desembarque = DateStringHelper.ToDateString(now.AddHours(1)),
            descontoAutomatico = 0
        };
    }
}

