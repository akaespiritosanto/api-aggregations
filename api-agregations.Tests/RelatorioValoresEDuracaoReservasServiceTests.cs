namespace api_agregations.Tests;

using api_aggregations.Dtos;
using api_aggregations.Models;
using api_aggregations.Services;
using api_aggregations.Utils;

public class RelatorioValoresEDuracaoReservasServiceTests
{
    [Fact]
    public async Task GetTotaisLugarAsync_ReturnsTotalsGroupedByMonthAndLugar()
    {
        await using var context = TestDbContextFactory.Create();

        context.RelatorioValoresEDuracaoReservas.AddRange(
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 2)),
                IdServico = 5,
                IdProduto = 2415,
                AbreviaturaProduto = "aaa",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "aaa",
                Quantidade = 1,
                Duracao = 2,
                Valor = 100m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 10)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 11)),
                IdServico = 5,
                IdProduto = 2415,
                AbreviaturaProduto = "aaa",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "aaa",
                Quantidade = 1,
                Duracao = 3,
                Valor = 50m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 15)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 16)),
                IdServico = 5,
                IdProduto = 2416,
                AbreviaturaProduto = "bbb",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "bbb",
                Quantidade = 1,
                Duracao = 4,
                Valor = 25m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 5, 3)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 5, 4)),
                IdServico = 5,
                IdProduto = 2415,
                AbreviaturaProduto = "aaa",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "aaa",
                Quantidade = 1,
                Duracao = 5,
                Valor = 70m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 2)),
                IdServico = 99,
                IdProduto = 2415,
                AbreviaturaProduto = "aaa",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "zzz",
                Quantidade = 1,
                Duracao = 100,
                Valor = 999m
            });

        await context.SaveChangesAsync();

        var service = new RelatorioValoresEDuracaoReservasService(context);

        var result = await service.GetTotaisLugarAsync(
            new TotaisQuery
            {
                idServico = 5,
                dataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                dataFim = DateStringHelper.ToDateString(new DateTime(2026, 5, 31)),
                idDispBase = 10
            },
            CancellationToken.None);

        Assert.Equal(2, result.meses.Count);

        var abril = result.meses[0];
        Assert.Equal(2026, abril.ano);
        Assert.Equal(4, abril.mes);
        Assert.Equal("abril", abril.nome);
        Assert.Equal(175m, abril.totalValorMes);
        Assert.Equal(9m, abril.totalDuracaoMes);
        Assert.Equal(2, abril.produtos.Count);
        Assert.Equal("2415", abril.produtos[0].id);
        Assert.Equal("aaa", abril.produtos[0].nome);
        Assert.Equal(150m, abril.produtos[0].valor);
        Assert.Equal(5m, abril.produtos[0].duracao);
        Assert.Equal("2416", abril.produtos[1].id);
        Assert.Equal("bbb", abril.produtos[1].nome);
        Assert.Equal(25m, abril.produtos[1].valor);
        Assert.Equal(4m, abril.produtos[1].duracao);

        Assert.Equal(245m, result.totaisValorAno);
        Assert.Equal(14m, result.totalDuracaoAno);
        Assert.Equal(2, result.totaisValorProdutoAno.Count);
        Assert.Equal("2415", result.totaisValorProdutoAno[0].id);
        Assert.Equal("aaa", result.totaisValorProdutoAno[0].nome);
        Assert.Equal(220m, result.totaisValorProdutoAno[0].valor);
        Assert.Equal("2416", result.totaisValorProdutoAno[1].id);
        Assert.Equal("bbb", result.totaisValorProdutoAno[1].nome);
        Assert.Equal(25m, result.totaisValorProdutoAno[1].valor);
        Assert.Equal("2415", result.totaisDuracaoProdutoAno[0].id);
        Assert.Equal("aaa", result.totaisDuracaoProdutoAno[0].nome);
        Assert.Equal(10m, result.totaisDuracaoProdutoAno[0].valor);
        Assert.Equal("2416", result.totaisDuracaoProdutoAno[1].id);
        Assert.Equal("bbb", result.totaisDuracaoProdutoAno[1].nome);
        Assert.Equal(4m, result.totaisDuracaoProdutoAno[1].valor);
    }

    [Fact]
    public async Task GetTotaisLugarAsync_WithoutFilters_ReturnsAllRows()
    {
        await using var context = TestDbContextFactory.Create();

        context.RelatorioValoresEDuracaoReservas.Add(
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 2)),
                IdServico = 5,
                IdProduto = 2415,
                AbreviaturaProduto = "aaa",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "aaa",
                Quantidade = 1,
                Duracao = 2,
                Valor = 100m
            });

        await context.SaveChangesAsync();

        var service = new RelatorioValoresEDuracaoReservasService(context);

        var result = await service.GetTotaisLugarAsync(new TotaisQuery(), CancellationToken.None);

        Assert.Single(result.meses);
        Assert.Equal(100m, result.totaisValorAno);
        Assert.Equal(2m, result.totalDuracaoAno);
    }

    [Fact]
    public async Task GetTotaisLugarPorLugarAsync_ReturnsTotalsGroupedByMonthAndLugar()
    {
        await using var context = TestDbContextFactory.Create();

        context.RelatorioValoresEDuracaoReservas.AddRange(
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 2)),
                IdServico = 5,
                IdProduto = 2415,
                AbreviaturaProduto = "aaa",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "aaa",
                Quantidade = 1,
                Duracao = 2,
                Valor = 100m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 10)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 11)),
                IdServico = 5,
                IdProduto = 2415,
                AbreviaturaProduto = "aaa",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "aaa",
                Quantidade = 1,
                Duracao = 3,
                Valor = 50m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 15)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 16)),
                IdServico = 5,
                IdProduto = 2416,
                AbreviaturaProduto = "bbb",
                IdDispBase = 10,
                RefDispBase = "DB-10",
                Lugar = "bbb",
                Quantidade = 1,
                Duracao = 4,
                Valor = 25m
            });

        await context.SaveChangesAsync();

        var service = new RelatorioValoresEDuracaoReservasService(context);

        var result = await service.GetTotaisLugarPorLugarAsync(
            new TotaisQuery
            {
                idServico = 5,
                dataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                dataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 30)),
                idDispBase = 10
            },
            CancellationToken.None);

        Assert.Single(result.meses);
        var abril = result.meses[0];
        Assert.Equal(2026, abril.ano);
        Assert.Equal(4, abril.mes);
        Assert.Equal("abril", abril.nome);
        Assert.Equal(2, abril.lugares.Count);
        Assert.Equal("aaa", abril.lugares[0].nome);
        Assert.Equal("DB-10", abril.lugares[0].refDispBase);
        Assert.Equal(150m, abril.lugares[0].valor);
        Assert.Equal(5m, abril.lugares[0].duracao);
        Assert.Equal("bbb", abril.lugares[1].nome);
        Assert.Equal("DB-10", abril.lugares[1].refDispBase);
        Assert.Equal(25m, abril.lugares[1].valor);
        Assert.Equal(4m, abril.lugares[1].duracao);

        Assert.Equal(175m, result.totalValorAno);
        Assert.Equal(9m, result.totalDuracaoAno);
        Assert.Equal("aaa", result.totaisValorLugarAno[0].nome);
        Assert.Equal("DB-10", result.totaisValorLugarAno[0].refDispBase);
        Assert.Equal(150m, result.totaisValorLugarAno[0].valor);
        Assert.Equal("bbb", result.totaisValorLugarAno[1].nome);
        Assert.Equal("DB-10", result.totaisValorLugarAno[1].refDispBase);
        Assert.Equal(25m, result.totaisValorLugarAno[1].valor);
    }

    [Fact]
    public async Task GetDisponibilidadesBaseAsync_ReturnsDispBaseRowsForService()
    {
        await using var context = TestDbContextFactory.Create();

        context.DispBase.AddRange(
            new DispBase { id = 1, referencia = "aaa", idServico = 10 },
            new DispBase { id = 2, referencia = "bbb", idServico = 10 },
            new DispBase { id = 3, referencia = "ccc", idServico = 20 });

        await context.SaveChangesAsync();

        var service = new RelatorioValoresEDuracaoReservasService(context);

        var result = await service.GetDisponibilidadesBaseAsync(
            new ListDisponibilidadesBaseQuery { idServico = 10 },
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].id);
        Assert.Equal("aaa", result[0].referencias);
        Assert.Equal(2, result[1].id);
        Assert.Equal("bbb", result[1].referencias);
    }

    [Fact]
    public async Task GetTotaisLugarPorLugarAsync_UsesDateOverlapFilters()
    {
        await using var context = TestDbContextFactory.Create();

        context.RelatorioValoresEDuracaoReservas.AddRange(
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 5)),
                IdServico = 5,
                IdProduto = 100,
                AbreviaturaProduto = "A",
                IdDispBase = 10,
                Lugar = "alpha",
                Quantidade = 1,
                Duracao = 4,
                Valor = 10m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 3, 30)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 2)),
                IdServico = 5,
                IdProduto = 101,
                AbreviaturaProduto = "B",
                IdDispBase = 10,
                Lugar = "beta",
                Quantidade = 1,
                Duracao = 2,
                Valor = 20m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 5, 1)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 5, 2)),
                IdServico = 5,
                IdProduto = 102,
                AbreviaturaProduto = "C",
                IdDispBase = 10,
                Lugar = "gamma",
                Quantidade = 1,
                Duracao = 1,
                Valor = 30m
            });

        await context.SaveChangesAsync();

        var service = new RelatorioValoresEDuracaoReservasService(context);

        var result = await service.GetTotaisLugarPorLugarAsync(
            new TotaisQuery
            {
                idServico = 5,
                idDispBase = 10,
                dataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                dataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 30))
            },
            CancellationToken.None);

        Assert.Equal(30m, result.totalValorAno);
        Assert.Equal(6m, result.totalDuracaoAno);
        Assert.Equal(2, result.totaisValorLugarAno.Count);
        Assert.Equal("alpha", result.totaisValorLugarAno[0].nome);
        Assert.Equal("beta", result.totaisValorLugarAno[1].nome);
    }

    [Fact]
    public async Task GetTotaisLugarAsync_WhenThereAreNoRows_ReturnsEmptyTotals()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RelatorioValoresEDuracaoReservasService(context);

        var result = await service.GetTotaisLugarAsync(new TotaisQuery(), CancellationToken.None);

        Assert.Empty(result.meses);
        Assert.Empty(result.totaisValorProdutoAno);
        Assert.Empty(result.totaisDuracaoProdutoAno);
        Assert.Equal(0m, result.totaisValorAno);
        Assert.Equal(0m, result.totalDuracaoAno);
    }

    [Fact]
    public async Task GetTotaisLugarAsync_WhenIdDispBaseIsZero_IgnoresDispBaseFilter()
    {
        await using var context = TestDbContextFactory.Create();

        context.RelatorioValoresEDuracaoReservas.AddRange(
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 2)),
                IdServico = 5,
                IdProduto = 100,
                AbreviaturaProduto = "A",
                IdDispBase = 10,
                Lugar = "alpha",
                Quantidade = 1,
                Duracao = 2,
                Valor = 10m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 3)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 4)),
                IdServico = 5,
                IdProduto = 101,
                AbreviaturaProduto = "B",
                IdDispBase = 99,
                Lugar = "beta",
                Quantidade = 1,
                Duracao = 3,
                Valor = 20m
            });

        await context.SaveChangesAsync();

        var service = new RelatorioValoresEDuracaoReservasService(context);

        var result = await service.GetTotaisLugarAsync(
            new TotaisQuery
            {
                idServico = 5,
                idDispBase = 0
            },
            CancellationToken.None);

        Assert.Equal(30m, result.totaisValorAno);
        Assert.Equal(5m, result.totalDuracaoAno);
        Assert.Equal(2, result.totaisValorProdutoAno.Count);
    }

    [Fact]
    public async Task GetTotaisLugarPorLugarAsync_WhenIdDispBaseIsZero_IgnoresDispBaseFilter()
    {
        await using var context = TestDbContextFactory.Create();

        context.RelatorioValoresEDuracaoReservas.AddRange(
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 1)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 2)),
                IdServico = 5,
                IdProduto = 100,
                AbreviaturaProduto = "A",
                IdDispBase = 10,
                Lugar = "alpha",
                Quantidade = 1,
                Duracao = 2,
                Valor = 10m
            },
            new RelatorioValoresEDuracaoReservas
            {
                DataInicio = DateStringHelper.ToDateString(new DateTime(2026, 4, 3)),
                DataFim = DateStringHelper.ToDateString(new DateTime(2026, 4, 4)),
                IdServico = 5,
                IdProduto = 101,
                AbreviaturaProduto = "B",
                IdDispBase = 99,
                Lugar = "beta",
                Quantidade = 1,
                Duracao = 3,
                Valor = 20m
            });

        await context.SaveChangesAsync();

        var service = new RelatorioValoresEDuracaoReservasService(context);

        var result = await service.GetTotaisLugarPorLugarAsync(
            new TotaisQuery
            {
                idServico = 5,
                idDispBase = 0
            },
            CancellationToken.None);

        Assert.Equal(30m, result.totalValorAno);
        Assert.Equal(5m, result.totalDuracaoAno);
        Assert.Equal(2, result.totaisValorLugarAno.Count);
    }
}
