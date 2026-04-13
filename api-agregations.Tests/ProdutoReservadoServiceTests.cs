namespace api_agregations.Tests;

using api_aggregations.Dtos;
using api_aggregations.Exceptions;
using api_aggregations.Services;
using Microsoft.Extensions.Logging.Abstractions;

public class ProdutoReservadoServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        await using var context = TestDbContextFactory.Create();

        context.ProdutoReservado.Add(TestDataFactory.CreateProdutoReservado(id: 1, idReserva: 10));
        context.ProdutoReservado.Add(TestDataFactory.CreateProdutoReservado(id: 2, idReserva: 10));
        context.ProdutoReservado.Add(TestDataFactory.CreateProdutoReservado(id: 3, idReserva: 20));
        await context.SaveChangesAsync();

        var service = new ProdutoReservadoService(context, NullLogger<ProdutoReservadoService>.Instance);

        var result = await service.GetAllAsync(new ProdutoReservadoQuery { PageNumber = 1, PageSize = 2 }, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetAllAsync_WithFilters_FiltersCorrectly()
    {
        await using var context = TestDbContextFactory.Create();

        context.ProdutoReservado.Add(TestDataFactory.CreateProdutoReservado(id: 1, idReserva: 10, referencia: "ABC"));
        context.ProdutoReservado.Add(TestDataFactory.CreateProdutoReservado(id: 2, idReserva: 11, referencia: "DEF"));
        await context.SaveChangesAsync();

        var service = new ProdutoReservadoService(context, NullLogger<ProdutoReservadoService>.Instance);

        var result = await service.GetAllAsync(new ProdutoReservadoQuery { id_reserva = 10, referencia = "A" }, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new ProdutoReservadoService(context, NullLogger<ProdutoReservadoService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(id: 123, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_WhenInvalidPagination_ThrowsBadRequest()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new ProdutoReservadoService(context, NullLogger<ProdutoReservadoService>.Instance);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.GetAllAsync(new ProdutoReservadoQuery { PageNumber = 0, PageSize = 10 }, CancellationToken.None));
    }

    [Fact]
    public async Task GetTotalsAsync_FiltersAndSumsCorrectly_ByDayAndIdEntidade()
    {
        await using var context = TestDbContextFactory.Create();

        var p1 = TestDataFactory.CreateProdutoReservado(id: 1, idReserva: 10);
        p1.id_entidade = 1;
        p1.data_criacao = new DateTime(2026, 1, 1);
        p1.quantidade = 2;
        p1.valor_tarifa = 100;
        p1.valor_comissao = 10;
        p1.valor_taxas = 5;
        p1.desconto = 10;
        p1.descontoAutomatico = 5;

        var p2 = TestDataFactory.CreateProdutoReservado(id: 2, idReserva: 10);
        p2.id_entidade = 1;
        p2.data_criacao = new DateTime(2026, 1, 1);
        p2.quantidade = 1;
        p2.valor_tarifa = 50;
        p2.valor_comissao = 3;
        p2.valor_taxas = 2;
        p2.desconto = null;
        p2.descontoAutomatico = 0;

        var p3 = TestDataFactory.CreateProdutoReservado(id: 3, idReserva: 10);
        p3.id_entidade = 2;
        p3.data_criacao = new DateTime(2026, 1, 2);
        p3.quantidade = 99;
        p3.valor_tarifa = 999;
        p3.valor_comissao = 999;
        p3.valor_taxas = 999;
        p3.desconto = 999;
        p3.descontoAutomatico = 999;

        context.ProdutoReservado.AddRange(p1, p2, p3);
        await context.SaveChangesAsync();

        var service = new ProdutoReservadoService(context, NullLogger<ProdutoReservadoService>.Instance);

        var result = await service.GetTotalsAsync(ano: 2026, mes: 1, dia: 1, idEntidade: 1, CancellationToken.None);

        Assert.Single(result);
        var totals = result.Single();

        Assert.Equal(2026, totals.ano);
        Assert.Equal(1, totals.mes);
        Assert.Equal(1, totals.dia);
        Assert.Equal(1, totals.id_entidade);

        Assert.Equal(3, totals.quantidade);
        Assert.Equal(150, totals.valor_tarifa);
        Assert.Equal(13, totals.valor_comissao);
        Assert.Equal(7, totals.valor_taxas);
        Assert.Equal(15, totals.valor_descontos);
    }
}
