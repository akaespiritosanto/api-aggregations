namespace api_aggregations.Services;

using System.Globalization;
using api_aggregations.Data;
using api_aggregations.Dtos;
using api_aggregations.Models;
using api_aggregations.Utils;
using Microsoft.EntityFrameworkCore;

public class RelatorioValoresEDuracaoReservasService
{
    private static readonly CultureInfo PtPtCulture = new("pt-PT");

    private readonly AppDbContext _context;

    public RelatorioValoresEDuracaoReservasService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TotaisProdutoDto> GetTotaisLugarAsync(
        TotaisQuery query,
        CancellationToken cancellationToken)
    {
        var linhas = await GetFilteredRowsAsync(query, cancellationToken);
        var meses = BuildProductMonths(linhas);
        var totaisValorProdutoAno = BuildProductYearTotals(linhas, useDuration: false);
        var totaisDuracaoProdutoAno = BuildProductYearTotals(linhas, useDuration: true);

        meses = RemoveZeroProductMonthItems(meses);
        totaisValorProdutoAno = RemoveZeroProductYearItems(totaisValorProdutoAno);
        totaisDuracaoProdutoAno = RemoveZeroProductYearItems(totaisDuracaoProdutoAno);

        return new TotaisProdutoDto
        {
            meses = meses,
            totaisValorProdutoAno = totaisValorProdutoAno,
            totaisValorAno = totaisValorProdutoAno.Sum(x => x.valor),
            totaisDuracaoProdutoAno = totaisDuracaoProdutoAno,
            totalDuracaoAno = totaisDuracaoProdutoAno.Sum(x => x.valor)
        };
    }

    public async Task<TotaisLugarDto> GetTotaisLugarPorLugarAsync(
        TotaisQuery query,
        CancellationToken cancellationToken)
    {
        var linhas = await GetFilteredRowsAsync(query, cancellationToken);
        var meses = BuildPlaceMonths(linhas);
        var totaisValorLugarAno = BuildPlaceYearTotals(linhas, useDuration: false);
        var totaisDuracaoLugarAno = BuildPlaceYearTotals(linhas, useDuration: true);

        meses = RemoveZeroPlaceMonthItems(meses);
        totaisValorLugarAno = RemoveZeroPlaceYearItems(totaisValorLugarAno);
        totaisDuracaoLugarAno = RemoveZeroPlaceYearItems(totaisDuracaoLugarAno);

        return new TotaisLugarDto
        {
            meses = meses,
            totaisValorLugarAno = totaisValorLugarAno,
            totalValorAno = totaisValorLugarAno.Sum(x => x.valor),
            totaisDuracaoLugarAno = totaisDuracaoLugarAno,
            totalDuracaoAno = totaisDuracaoLugarAno.Sum(x => x.valor)
        };
    }

    public async Task<List<ListDisponibilidadesBaseDto>> GetDisponibilidadesBaseAsync(
        ListDisponibilidadesBaseQuery query,
        CancellationToken cancellationToken)
    {
        return await _context.DispBase
            .AsNoTracking()
            .Where(d => d.idServico == query.idServico)
            .OrderBy(d => d.id)
            .Select(d => new ListDisponibilidadesBaseDto
            {
                id = d.id,
                referencias = d.referencia ?? string.Empty
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<RelatorioLinha>> GetFilteredRowsAsync(
        TotaisQuery query,
        CancellationToken cancellationToken)
    {
        var dataInicio = DateStringHelper.ParseDateOrNull(query.dataInicio);
        var dataFim = DateStringHelper.ParseDateOrNull(query.dataFim);

        var relatorio = GetRelatorioQuery()
            .AsNoTracking();

        if (query.idServico.HasValue)
        {
            relatorio = relatorio.Where(r => r.IdServico == query.idServico.Value);
        }

        if (query.idDispBase.HasValue && query.idDispBase.Value != 0)
        {
            relatorio = relatorio.Where(r => r.IdDispBase == query.idDispBase.Value);
        }

        var rows = await relatorio
            .Select(r => new
            {
                r.DataInicio,
                r.DataFim,
                r.IdProduto,
                r.AbreviaturaProduto,
                r.RefDispBase,
                r.Lugar,
                r.Duracao,
                r.Valor
            })
            .ToListAsync(cancellationToken);

        var parsedRows = rows.Select(r => new RelatorioLinha
        {
            DataInicio = DateStringHelper.ParseDate(r.DataInicio),
            DataFim = DateStringHelper.ParseDate(r.DataFim),
            IdProduto = r.IdProduto,
            AbreviaturaProduto = r.AbreviaturaProduto,
            RefDispBase = r.RefDispBase,
            Lugar = r.Lugar,
            Duracao = r.Duracao,
            Valor = r.Valor
        });

        if (dataInicio.HasValue)
        {
            parsedRows = parsedRows.Where(r => r.DataFim >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            parsedRows = parsedRows.Where(r => r.DataInicio <= dataFim.Value);
        }

        return parsedRows
            .OrderBy(r => r.DataInicio)
            .ThenBy(r => r.Lugar)
            .ToList();
    }

    private static List<RelatorioMesTotaisLugarDto> RemoveZeroProductMonthItems(
        List<RelatorioMesTotaisLugarDto> meses)
    {
        var filteredMonths = new List<RelatorioMesTotaisLugarDto>();

        foreach (var mes in meses)
        {
            var produtos = RemoveZeroProductItems(mes.produtos);

            if (mes.ano == 0 ||
                mes.mes == 0 ||
                mes.totalValorMes == 0 ||
                mes.totalDuracaoMes == 0 ||
                produtos.Count == 0)
            {
                continue;
            }

            filteredMonths.Add(new RelatorioMesTotaisLugarDto
            {
                ano = mes.ano,
                mes = mes.mes,
                nome = mes.nome,
                produtos = produtos,
                totalValorMes = mes.totalValorMes,
                totalDuracaoMes = mes.totalDuracaoMes
            });
        }

        return filteredMonths;
    }

    private static List<RelatorioProdutoMesDto> RemoveZeroProductItems(
        List<RelatorioProdutoMesDto> produtos)
    {
        var filteredProducts = new List<RelatorioProdutoMesDto>();

        foreach (var produto in produtos)
        {
            if (produto.id == "0" ||
                produto.valor == 0 ||
                produto.duracao == 0)
            {
                continue;
            }

            filteredProducts.Add(produto);
        }

        return filteredProducts;
    }

    private static List<RelatorioTotalProdutoAnoDto> RemoveZeroProductYearItems(
        List<RelatorioTotalProdutoAnoDto> totais)
    {
        var filteredTotals = new List<RelatorioTotalProdutoAnoDto>();

        foreach (var total in totais)
        {
            if (total.id == "0" || total.valor == 0)
            {
                continue;
            }

            filteredTotals.Add(total);
        }

        return filteredTotals;
    }

    private static List<TotaisMesLugarDto> RemoveZeroPlaceMonthItems(
        List<TotaisMesLugarDto> meses)
    {
        var filteredMonths = new List<TotaisMesLugarDto>();

        foreach (var mes in meses)
        {
            var lugares = RemoveZeroPlaceItems(mes.lugares);

            if (mes.ano == 0 ||
                mes.mes == 0 ||
                mes.totalValorMes == 0 ||
                mes.totalDuracaoMes == 0 ||
                lugares.Count == 0)
            {
                continue;
            }

            filteredMonths.Add(new TotaisMesLugarDto
            {
                ano = mes.ano,
                mes = mes.mes,
                nome = mes.nome,
                lugares = lugares,
                totalValorMes = mes.totalValorMes,
                totalDuracaoMes = mes.totalDuracaoMes
            });
        }

        return filteredMonths;
    }

    private static List<TotaisLugarItemDto> RemoveZeroPlaceItems(
        List<TotaisLugarItemDto> lugares)
    {
        var filteredPlaces = new List<TotaisLugarItemDto>();

        foreach (var lugar in lugares)
        {
            if (lugar.valor == 0 || lugar.duracao == 0)
            {
                continue;
            }

            filteredPlaces.Add(lugar);
        }

        return filteredPlaces;
    }

    private static List<TotaisTotalLugarAnoDto> RemoveZeroPlaceYearItems(
        List<TotaisTotalLugarAnoDto> totais)
    {
        var filteredTotals = new List<TotaisTotalLugarAnoDto>();

        foreach (var total in totais)
        {
            if (total.valor == 0)
            {
                continue;
            }

            filteredTotals.Add(total);
        }

        return filteredTotals;
    }

    private IQueryable<RelatorioValoresEDuracaoReservas> GetRelatorioQuery()
    {
        if (!_context.Database.IsSqlServer())
        {
            return _context.RelatorioValoresEDuracaoReservas;
        }

        return _context.RelatorioValoresEDuracaoReservas.FromSqlRaw(
            """
            SELECT
                [DataInicio],
                [DataFim],
                [IdServico],
                [IdProduto],
                [AbreviaturaProduto],
                [IdDispBase],
                [RefDispBase],
                [Lugar],
                [Quantidade],
                [Duracao],
                CAST([Valor] AS decimal(18, 4)) AS [Valor]
            FROM [RelatorioValoresEDuracaoReservas]
            """);
    }

    private static List<RelatorioMesTotaisLugarDto> BuildProductMonths(List<RelatorioLinha> linhas)
    {
        return GroupByMonth(linhas)
            .Select(group => new RelatorioMesTotaisLugarDto
            {
                ano = group.Key.Ano,
                mes = group.Key.Mes,
                nome = GetMonthName(group.Key.Mes),
                produtos = group
                    .GroupBy(x => new { x.IdProduto, Nome = x.AbreviaturaProduto ?? string.Empty })
                    .OrderBy(x => x.Key.IdProduto)
                    .ThenBy(x => x.Key.Nome)
                    .Select(x => new RelatorioProdutoMesDto
                    {
                        id = x.Key.IdProduto.ToString(),
                        nome = x.Key.Nome,
                        valor = x.Sum(y => y.Valor),
                        duracao = x.Sum(y => (decimal)y.Duracao)
                    })
                    .ToList(),
                totalValorMes = group.Sum(x => x.Valor),
                totalDuracaoMes = group.Sum(x => (decimal)x.Duracao)
            })
            .ToList();
    }

    private static List<RelatorioTotalProdutoAnoDto> BuildProductYearTotals(List<RelatorioLinha> linhas, bool useDuration)
    {
        return linhas
            .GroupBy(r => new { r.IdProduto, Nome = r.AbreviaturaProduto ?? string.Empty })
            .OrderBy(g => g.Key.IdProduto)
            .ThenBy(g => g.Key.Nome)
            .Select(g => new RelatorioTotalProdutoAnoDto
            {
                id = g.Key.IdProduto.ToString(),
                nome = g.Key.Nome,
                valor = useDuration ? g.Sum(x => (decimal)x.Duracao) : g.Sum(x => x.Valor)
            })
            .ToList();
    }

    private static List<TotaisMesLugarDto> BuildPlaceMonths(List<RelatorioLinha> linhas)
    {
        return GroupByMonth(linhas)
            .Select(group => new TotaisMesLugarDto
            {
                ano = group.Key.Ano,
                mes = group.Key.Mes,
                nome = GetMonthName(group.Key.Mes),
                lugares = group
                    .GroupBy(x => new { Nome = x.Lugar ?? string.Empty, RefDispBase = x.RefDispBase ?? string.Empty })
                    .OrderBy(x => x.Key.Nome)
                    .ThenBy(x => x.Key.RefDispBase)
                    .Select(x => new TotaisLugarItemDto
                    {
                        nome = x.Key.Nome,
                        refDispBase = x.Key.RefDispBase,
                        valor = x.Sum(y => y.Valor),
                        duracao = x.Sum(y => (decimal)y.Duracao)
                    })
                    .ToList(),
                totalValorMes = group.Sum(x => x.Valor),
                totalDuracaoMes = group.Sum(x => (decimal)x.Duracao)
            })
            .ToList();
    }

    private static List<TotaisTotalLugarAnoDto> BuildPlaceYearTotals(List<RelatorioLinha> linhas, bool useDuration)
    {
        return linhas
            .GroupBy(r => new { Nome = r.Lugar ?? string.Empty, RefDispBase = r.RefDispBase ?? string.Empty })
            .OrderBy(g => g.Key.Nome)
            .ThenBy(g => g.Key.RefDispBase)
            .Select(g => new TotaisTotalLugarAnoDto
            {
                nome = g.Key.Nome,
                refDispBase = g.Key.RefDispBase,
                valor = useDuration ? g.Sum(x => (decimal)x.Duracao) : g.Sum(x => x.Valor)
            })
            .ToList();
    }

    private static IOrderedEnumerable<IGrouping<MonthKey, RelatorioLinha>> GroupByMonth(List<RelatorioLinha> linhas)
    {
        return linhas
            .GroupBy(r => new MonthKey(r.DataInicio.Year, r.DataInicio.Month))
            .OrderBy(g => g.Key.Ano)
            .ThenBy(g => g.Key.Mes);
    }

    private static string GetMonthName(int month)
    {
        return PtPtCulture.DateTimeFormat.GetMonthName(month).ToLower();
    }

    private sealed class RelatorioLinha
    {
        public DateTime DataInicio { get; init; }
        public DateTime DataFim { get; init; }
        public int IdProduto { get; init; }
        public string? AbreviaturaProduto { get; init; }
        public string? RefDispBase { get; init; }
        public string? Lugar { get; init; }
        public int Duracao { get; init; }
        public decimal Valor { get; init; }
    }

    private sealed record MonthKey(int Ano, int Mes);
}
