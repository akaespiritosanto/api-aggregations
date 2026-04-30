namespace api_aggregations.Services;

using api_aggregations.Models;
using api_aggregations.Data;
using api_aggregations.Dtos;
using api_aggregations.Exceptions;
using api_aggregations.Utils;
using Microsoft.EntityFrameworkCore;

public class ProdutoReservadoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProdutoReservadoService> _logger;

    public ProdutoReservadoService(AppDbContext context, ILogger<ProdutoReservadoService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<List<ProdutoReservadoTotalsDto>> GetTotalsAsync(int? ano, int? mes, int? dia, int? idEntidade, CancellationToken cancellationToken)
    {
        ValidateDateParts(ano, mes, dia);

        var produtos = await BuildTotalsRowsAsync(idEntidade, cancellationToken);
        var filteredProdutos = FilterTotalsRows(produtos, ano, mes, dia);

        if (mes is null)
        {
            return filteredProdutos
                .GroupBy(p => new ProdutoYearKey(p.Ano, p.id_entidade))
                .Select(CreateYearTotals)
                .OrderBy(x => x.ano)
                .ThenBy(x => x.id_entidade)
                .ToList();
        }

        if (dia is null)
        {
            return filteredProdutos
                .GroupBy(p => new ProdutoMonthKey(p.Ano, p.Mes, p.id_entidade))
                .Select(CreateMonthTotals)
                .OrderBy(x => x.ano)
                .ThenBy(x => x.mes)
                .ThenBy(x => x.id_entidade)
                .ToList();
        }

        return filteredProdutos
            .GroupBy(p => new ProdutoDayKey(p.Ano, p.Mes, p.Dia, p.id_entidade))
            .Select(CreateDayTotals)
            .OrderBy(x => x.ano)
            .ThenBy(x => x.mes)
            .ThenBy(x => x.dia)
            .ThenBy(x => x.id_entidade)
            .ToList();
    }

    public async Task<PagedResult<ProdutoReservado>> GetAllAsync(ProdutoReservadoQuery query, CancellationToken cancellationToken)
    {
        ValidatePagination(query);

        var produtos = ApplyListFilters(query);

        var totalCount = await produtos.CountAsync(cancellationToken);
        var items = await produtos
            .OrderBy(p => p.id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProdutoReservado>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProdutoReservado> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var produto = await _context.ProdutoReservado
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.id == id, cancellationToken);

        if (produto is null)
        {
            throw new NotFoundException($"ProdutoReservado with id '{id}' was not found.");
        }

        return produto;
    }
    
    public async Task<ProdutoReservado> CreateAsync(ProdutoReservado produtoReservado, CancellationToken cancellationToken)
    {
        _context.ProdutoReservado.Add(produtoReservado);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created ProdutoReservado with id {Id}", produtoReservado.id);
        return produtoReservado;
    }

    public async Task<ProdutoReservado> UpdateAsync(int id, ProdutoReservado produtoReservado, CancellationToken cancellationToken)
    {
        var existing = await _context.ProdutoReservado
            .FirstOrDefaultAsync(p => p.id == id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"ProdutoReservado with id '{id}' was not found.");
        }

        produtoReservado.id = existing.id;
        _context.Entry(existing).CurrentValues.SetValues(produtoReservado);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated ProdutoReservado with id {Id}", id);
        return existing;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var existing = await _context.ProdutoReservado
            .FirstOrDefaultAsync(p => p.id == id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"ProdutoReservado with id '{id}' was not found.");
        }

        _context.ProdutoReservado.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted ProdutoReservado with id {Id}", id);
    }

    private IQueryable<ProdutoReservado> ApplyListFilters(ProdutoReservadoQuery query)
    {
        var produtos = _context.ProdutoReservado.AsNoTracking().AsQueryable();

        if (query.id_reserva is not null)
        {
            produtos = produtos.Where(p => p.id_reserva == query.id_reserva.Value);
        }

        if (query.id_produto is not null)
        {
            produtos = produtos.Where(p => p.id_produto == query.id_produto.Value);
        }

        if (query.estado is not null)
        {
            produtos = produtos.Where(p => p.estado == query.estado.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.referencia))
        {
            produtos = produtos.Where(p => p.referencia != null && p.referencia.Contains(query.referencia));
        }

        if (query.agregado is not null)
        {
            produtos = produtos.Where(p => p.agregado == query.agregado.Value);
        }

        return produtos;
    }

    private async Task<List<ProdutoReservadoTotalRow>> BuildTotalsRowsAsync(int? idEntidade, CancellationToken cancellationToken)
    {
        var produtos = _context.ProdutoReservado.AsNoTracking().AsQueryable();

        if (idEntidade is not null)
        {
            produtos = produtos.Where(p => p.id_entidade == idEntidade.Value);
        }

        var rows = await produtos
            .Select(p => new
            {
                p.data_criacao,
                p.id_entidade,
                p.quantidade,
                p.valor_tarifa,
                p.valor_comissao,
                p.valor_taxas,
                p.desconto,
                p.descontoAutomatico
            })
            .ToListAsync(cancellationToken);

        return rows.Select(p =>
        {
            var dataCriacao = GetCreationDate(p.data_criacao);

            return new ProdutoReservadoTotalRow
            {
                Ano = dataCriacao.Year,
                Mes = dataCriacao.Month,
                Dia = dataCriacao.Day,
                id_entidade = p.id_entidade,
                quantidade = p.quantidade,
                valor_tarifa = p.valor_tarifa,
                valor_comissao = p.valor_comissao,
                valor_taxas = p.valor_taxas,
                desconto = p.desconto,
                descontoAutomatico = p.descontoAutomatico
            };
        }).ToList();
    }

    private static List<ProdutoReservadoTotalRow> FilterTotalsRows(List<ProdutoReservadoTotalRow> rows, int? ano, int? mes, int? dia)
    {
        var filteredRows = rows;

        if (ano is not null)
        {
            filteredRows = filteredRows.Where(p => p.Ano == ano.Value).ToList();
        }

        if (mes is not null)
        {
            filteredRows = filteredRows.Where(p => p.Mes == mes.Value).ToList();
        }

        if (dia is not null)
        {
            filteredRows = filteredRows.Where(p => p.Dia == dia.Value).ToList();
        }

        return filteredRows;
    }

    private static DateTime GetCreationDate(string dataCriacao)
    {
        return DateStringHelper.ParseDate(dataCriacao);
    }

    private static ProdutoReservadoTotalsDto CreateYearTotals(IGrouping<ProdutoYearKey, ProdutoReservadoTotalRow> group)
    {
        return CreateTotals(group, group.Key.Ano, null, null, group.Key.id_entidade);
    }

    private static ProdutoReservadoTotalsDto CreateMonthTotals(IGrouping<ProdutoMonthKey, ProdutoReservadoTotalRow> group)
    {
        return CreateTotals(group, group.Key.Ano, group.Key.Mes, null, group.Key.id_entidade);
    }

    private static ProdutoReservadoTotalsDto CreateDayTotals(IGrouping<ProdutoDayKey, ProdutoReservadoTotalRow> group)
    {
        return CreateTotals(group, group.Key.Ano, group.Key.Mes, group.Key.Dia, group.Key.id_entidade);
    }

    private static ProdutoReservadoTotalsDto CreateTotals(
        IEnumerable<ProdutoReservadoTotalRow> group,
        int ano,
        int? mes,
        int? dia,
        int? idEntidade)
    {
        return new ProdutoReservadoTotalsDto
        {
            ano = ano,
            mes = mes,
            dia = dia,
            id_entidade = idEntidade,
            quantidade = group.Sum(x => x.quantidade),
            valor_tarifa = group.Sum(x => x.valor_tarifa),
            valor_comissao = group.Sum(x => x.valor_comissao),
            valor_taxas = group.Sum(x => x.valor_taxas),
            valor_descontos = group.Sum(x => (x.desconto ?? 0) + x.descontoAutomatico)
        };
    }

    private static void ValidatePagination(PaginationQuery query)
    {
        if (query.PageNumber < 1)
        {
            throw new BadRequestException("PageNumber must be greater than or equal to 1.");
        }

        if (query.PageSize < 1)
        {
            throw new BadRequestException("PageSize must be greater than or equal to 1.");
        }

        if (query.PageSize > 100)
        {
            throw new BadRequestException("PageSize must be less than or equal to 100.");
        }
    }

    private static void ValidateDateParts(int? ano, int? mes, int? dia)
    {
        if (ano is not null && ano < 1)
        {
            throw new BadRequestException("Parameter 'ano' must be a valid year.");
        }

        if (mes is not null && (mes < 1 || mes > 12))
        {
            throw new BadRequestException("Parameter 'mes' must be between 1 and 12.");
        }

        if (dia is not null && (dia < 1 || dia > 31))
        {
            throw new BadRequestException("Parameter 'dia' must be between 1 and 31.");
        }

        if (mes is not null && ano is null)
        {
            throw new BadRequestException("When using 'mes', you must also provide 'ano'.");
        }

        if (dia is not null && (ano is null || mes is null))
        {
            throw new BadRequestException("When using 'dia', you must also provide 'ano' and 'mes'.");
        }

        if (ano is not null && mes is not null && dia is not null)
        {
            var maxDay = DateTime.DaysInMonth(ano.Value, mes.Value);
            if (dia.Value > maxDay)
            {
                throw new BadRequestException($"Parameter 'dia' must be between 1 and {maxDay} for ano={ano} and mes={mes}.");
            }
        }
    }

    private sealed class ProdutoReservadoTotalRow
    {
        public int Ano { get; init; }
        public int Mes { get; init; }
        public int Dia { get; init; }
        public int? id_entidade { get; init; }
        public int quantidade { get; init; }
        public double valor_tarifa { get; init; }
        public double valor_comissao { get; init; }
        public double valor_taxas { get; init; }
        public double? desconto { get; init; }
        public double descontoAutomatico { get; init; }
    }

    private sealed record ProdutoYearKey(int Ano, int? id_entidade);
    private sealed record ProdutoMonthKey(int Ano, int Mes, int? id_entidade);
    private sealed record ProdutoDayKey(int Ano, int Mes, int Dia, int? id_entidade);
}
