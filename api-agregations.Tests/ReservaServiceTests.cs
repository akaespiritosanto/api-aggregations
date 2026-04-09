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
}

