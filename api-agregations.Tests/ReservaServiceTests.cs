namespace api_agregations.Tests;

using api_aggregations.Dtos;
using api_aggregations.Exceptions;
using api_aggregations.Services;
using Microsoft.Extensions.Logging.Abstractions;

public class ReservaServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        await using var context = TestDbContextFactory.Create();

        context.Reserva.Add(TestDataFactory.CreateReserva(id: 1, numero: "A-1", estado: 1));
        context.Reserva.Add(TestDataFactory.CreateReserva(id: 2, numero: "A-2", estado: 1));
        context.Reserva.Add(TestDataFactory.CreateReserva(id: 3, numero: "B-1", estado: 2));
        await context.SaveChangesAsync();

        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        var result = await service.GetAllAsync(new ReservaQuery { PageNumber = 1, PageSize = 2 }, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithFilters_FiltersCorrectly()
    {
        await using var context = TestDbContextFactory.Create();

        context.Reserva.Add(TestDataFactory.CreateReserva(id: 1, numero: "ABC", estado: 1));
        context.Reserva.Add(TestDataFactory.CreateReserva(id: 2, numero: "DEF", estado: 1));
        await context.SaveChangesAsync();

        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        var result = await service.GetAllAsync(new ReservaQuery { numero = "A" }, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(id: 999, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_WhenInvalidPagination_ThrowsBadRequest()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.GetAllAsync(new ReservaQuery { PageNumber = 1, PageSize = 0 }, CancellationToken.None));
    }

    [Fact]
    public async Task GetTotalsAsync_GroupsByYear_WhenMesIsNull()
    {
        await using var context = TestDbContextFactory.Create();

        var r1 = TestDataFactory.CreateReserva(id: 1, numero: "A-1", estado: 1);
        r1.id_vendedor = 1;
        r1.data_pedido = new DateTime(2026, 1, 1);

        var r2 = TestDataFactory.CreateReserva(id: 2, numero: "A-2", estado: 1);
        r2.id_vendedor = 1;
        r2.data_pedido = new DateTime(2026, 1, 2);

        var r3 = TestDataFactory.CreateReserva(id: 3, numero: "B-1", estado: 1);
        r3.id_vendedor = 2;
        r3.data_pedido = new DateTime(2026, 1, 1);

        var r4 = TestDataFactory.CreateReserva(id: 4, numero: "OLD", estado: 1);
        r4.id_vendedor = 1;
        r4.data_pedido = new DateTime(2025, 12, 31);

        context.Reserva.AddRange(r1, r2, r3, r4);
        await context.SaveChangesAsync();

        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        var result = await service.GetTotalsAsync(ano: 2026, mes: null, dia: null, idVendedor: null, CancellationToken.None);

        Assert.Equal(2, result.Count);

        var vendedor1 = result.Single(x => x.id_vendedor == 1);
        Assert.Equal(2, vendedor1.quantidade);
        Assert.Equal(2026, vendedor1.ano);
        Assert.Null(vendedor1.mes);
        Assert.Null(vendedor1.dia);

        var vendedor2 = result.Single(x => x.id_vendedor == 2);
        Assert.Equal(1, vendedor2.quantidade);
    }
}
