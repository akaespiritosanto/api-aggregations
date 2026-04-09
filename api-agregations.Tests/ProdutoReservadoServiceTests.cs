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
}

