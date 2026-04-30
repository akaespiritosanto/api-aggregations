namespace api_agregations.Tests;

using api_aggregations.Dtos;
using api_aggregations.Exceptions;
using api_aggregations.Services;
using api_aggregations.Utils;
using Microsoft.EntityFrameworkCore;
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
        r1.data_pedido = DateStringHelper.ToDateString(new DateTime(2026, 1, 1));

        var r2 = TestDataFactory.CreateReserva(id: 2, numero: "A-2", estado: 1);
        r2.id_vendedor = 1;
        r2.data_pedido = DateStringHelper.ToDateString(new DateTime(2026, 1, 2));

        var r3 = TestDataFactory.CreateReserva(id: 3, numero: "B-1", estado: 1);
        r3.id_vendedor = 2;
        r3.data_pedido = DateStringHelper.ToDateString(new DateTime(2026, 1, 1));

        var r4 = TestDataFactory.CreateReserva(id: 4, numero: "OLD", estado: 1);
        r4.id_vendedor = 1;
        r4.data_pedido = DateStringHelper.ToDateString(new DateTime(2025, 12, 31));

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

    [Fact]
    public async Task GetTotalsAsync_WhenMesIsProvided_GroupsByMonth()
    {
        await using var context = TestDbContextFactory.Create();

        var januaryFirst = TestDataFactory.CreateReserva(id: 1, numero: "JAN-1", estado: 1);
        januaryFirst.id_vendedor = 7;
        januaryFirst.data_pedido = DateStringHelper.ToDateString(new DateTime(2026, 1, 1));

        var januarySecond = TestDataFactory.CreateReserva(id: 2, numero: "JAN-2", estado: 1);
        januarySecond.id_vendedor = 7;
        januarySecond.data_pedido = DateStringHelper.ToDateString(new DateTime(2026, 1, 20));

        var february = TestDataFactory.CreateReserva(id: 3, numero: "FEB-1", estado: 1);
        february.id_vendedor = 7;
        february.data_pedido = DateStringHelper.ToDateString(new DateTime(2026, 2, 1));

        context.Reserva.AddRange(januaryFirst, januarySecond, february);
        await context.SaveChangesAsync();

        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        var result = await service.GetTotalsAsync(ano: 2026, mes: 1, dia: null, idVendedor: 7, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(2026, result[0].ano);
        Assert.Equal(1, result[0].mes);
        Assert.Null(result[0].dia);
        Assert.Equal(7, result[0].id_vendedor);
        Assert.Equal(2, result[0].quantidade);
    }

    [Fact]
    public async Task GetTotalsAsync_WhenDateIsInvalid_ThrowsBadRequest()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.GetTotalsAsync(ano: 2026, mes: 2, dia: 31, idVendedor: null, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_AddsReserva()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);
        var reserva = TestDataFactory.CreateReserva(id: 10, numero: "NEW", estado: 1);

        var created = await service.CreateAsync(reserva, CancellationToken.None);

        Assert.Equal(10, created.id);
        Assert.Equal(1, await context.Reserva.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesStoredReserva()
    {
        await using var context = TestDbContextFactory.Create();

        context.Reserva.Add(TestDataFactory.CreateReserva(id: 1, numero: "OLD", estado: 1));
        await context.SaveChangesAsync();

        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);
        var updatedReserva = TestDataFactory.CreateReserva(id: 999, numero: "NEW", estado: 2);

        var result = await service.UpdateAsync(1, updatedReserva, CancellationToken.None);

        Assert.Equal(1, result.id);
        Assert.Equal("NEW", result.numero);
        Assert.Equal((short?)2, result.estado);
    }

    [Fact]
    public async Task DeleteAsync_RemovesReserva()
    {
        await using var context = TestDbContextFactory.Create();

        context.Reserva.Add(TestDataFactory.CreateReserva(id: 1, numero: "TO-DELETE", estado: 1));
        await context.SaveChangesAsync();

        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        await service.DeleteAsync(1, CancellationToken.None);

        Assert.Empty(context.Reserva);
    }

    [Fact]
    public async Task UpdateAsync_WhenReservaDoesNotExist_ThrowsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);
        var reserva = TestDataFactory.CreateReserva(id: 1, numero: "ANY", estado: 1);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(999, reserva, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_WhenReservaDoesNotExist_ThrowsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeleteAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task GetTotalsAsync_UsesDataActualizacaoWhenDataPedidoIsNull()
    {
        await using var context = TestDbContextFactory.Create();

        var reserva = TestDataFactory.CreateReserva(id: 1, numero: "FALLBACK", estado: 1);
        reserva.id_vendedor = 3;
        reserva.data_pedido = null;
        reserva.data_actualizacao = DateStringHelper.ToDateString(new DateTime(2026, 6, 15));

        context.Reserva.Add(reserva);
        await context.SaveChangesAsync();

        var service = new ReservaService(context, NullLogger<ReservaService>.Instance);

        var result = await service.GetTotalsAsync(ano: 2026, mes: 6, dia: 15, idVendedor: 3, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(1, result[0].quantidade);
        Assert.Equal(2026, result[0].ano);
        Assert.Equal(6, result[0].mes);
        Assert.Equal(15, result[0].dia);
    }
}
