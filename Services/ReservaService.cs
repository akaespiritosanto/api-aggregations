namespace api_aggregations.Services;

using api_aggregations.Models;
using api_aggregations.Data;
using api_aggregations.Dtos;
using api_aggregations.Exceptions;
using api_aggregations.Utils;
using Microsoft.EntityFrameworkCore;

public class ReservaService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReservaService> _logger;

    public ReservaService(AppDbContext context, ILogger<ReservaService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<ReservaTotalsDto>> GetTotalsAsync(int? ano, int? mes, int? dia, int? idVendedor, CancellationToken cancellationToken)
    {
        ValidateDateParts(ano, mes, dia);

        var reservas = await BuildTotalsRowsAsync(idVendedor, cancellationToken);
        var filteredReservas = FilterTotalsRows(reservas, ano, mes, dia);

        if (mes is null)
        {
            return filteredReservas
                .GroupBy(r => new ReservaYearKey(r.Ano, r.id_vendedor))
                .Select(CreateYearTotals)
                .OrderBy(x => x.ano)
                .ThenBy(x => x.id_vendedor)
                .ToList();
        }

        if (dia is null)
        {
            return filteredReservas
                .GroupBy(r => new ReservaMonthKey(r.Ano, r.Mes, r.id_vendedor))
                .Select(CreateMonthTotals)
                .OrderBy(x => x.ano)
                .ThenBy(x => x.mes)
                .ThenBy(x => x.id_vendedor)
                .ToList();
        }

        return filteredReservas
            .GroupBy(r => new ReservaDayKey(r.Ano, r.Mes, r.Dia, r.id_vendedor))
            .Select(CreateDayTotals)
            .OrderBy(x => x.ano)
            .ThenBy(x => x.mes)
            .ThenBy(x => x.dia)
            .ThenBy(x => x.id_vendedor)
            .ToList();
    }

    public async Task<PagedResult<Reserva>> GetAllAsync(ReservaQuery query, CancellationToken cancellationToken)
    {
        ValidatePagination(query);

        var reservas = ApplyListFilters(query);

        var totalCount = await reservas.CountAsync(cancellationToken);
        var items = await reservas
            .OrderBy(r => r.id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Reserva>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Reserva> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var reserva = await _context.Reserva
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.id == id, cancellationToken);

        if (reserva is null)
        {
            throw new NotFoundException($"Reserva with id '{id}' was not found.");
        }

        return reserva;
    }

    public async Task<Reserva> CreateAsync(Reserva reserva, CancellationToken cancellationToken)
    {
        _context.Reserva.Add(reserva);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created Reserva with id {Id}", reserva.id);
        return reserva;
    }

    public async Task<Reserva> UpdateAsync(int id, Reserva reserva, CancellationToken cancellationToken)
    {
        var existing = await _context.Reserva
            .FirstOrDefaultAsync(r => r.id == id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Reserva with id '{id}' was not found.");
        }

        reserva.id = existing.id;
        _context.Entry(existing).CurrentValues.SetValues(reserva);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated Reserva with id {Id}", id);
        return existing;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var existing = await _context.Reserva
            .FirstOrDefaultAsync(r => r.id == id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Reserva with id '{id}' was not found.");
        }

        _context.Reserva.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted Reserva with id {Id}", id);
    }

    private IQueryable<Reserva> ApplyListFilters(ReservaQuery query)
    {
        var reservas = _context.Reserva.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.numero))
        {
            reservas = reservas.Where(r => r.numero != null && r.numero.Contains(query.numero));
        }

        if (query.tipo is not null)
        {
            reservas = reservas.Where(r => r.tipo == query.tipo.Value);
        }

        if (query.estado is not null)
        {
            reservas = reservas.Where(r => r.estado == query.estado.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.id_externo))
        {
            reservas = reservas.Where(r => r.id_externo.Contains(query.id_externo));
        }

        return reservas;
    }

    private async Task<List<ReservaTotalRow>> BuildTotalsRowsAsync(int? idVendedor, CancellationToken cancellationToken)
    {
        var reservas = _context.Reserva.AsNoTracking().AsQueryable();

        if (idVendedor is not null)
        {
            reservas = reservas.Where(r => r.id_vendedor == idVendedor.Value);
        }

        var rows = await reservas
            .Select(r => new
            {
                r.data_pedido,
                r.data_actualizacao,
                r.id_vendedor
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r =>
        {
            var data = GetReservaDate(r.data_pedido, r.data_actualizacao);

            return new ReservaTotalRow
            {
                Ano = data.Year,
                Mes = data.Month,
                Dia = data.Day,
                id_vendedor = r.id_vendedor
            };
        }).ToList();
    }

    private static List<ReservaTotalRow> FilterTotalsRows(List<ReservaTotalRow> rows, int? ano, int? mes, int? dia)
    {
        var filteredRows = rows;

        if (ano is not null)
        {
            filteredRows = filteredRows.Where(r => r.Ano == ano.Value).ToList();
        }

        if (mes is not null)
        {
            filteredRows = filteredRows.Where(r => r.Mes == mes.Value).ToList();
        }

        if (dia is not null)
        {
            filteredRows = filteredRows.Where(r => r.Dia == dia.Value).ToList();
        }

        return filteredRows;
    }

    private static DateTime GetReservaDate(string? dataPedido, string dataActualizacao)
    {
        return DateStringHelper.ParseDateOrNull(dataPedido)
            ?? DateStringHelper.ParseDate(dataActualizacao);
    }

    private static ReservaTotalsDto CreateYearTotals(IGrouping<ReservaYearKey, ReservaTotalRow> group)
    {
        return new ReservaTotalsDto
        {
            ano = group.Key.Ano,
            mes = null,
            dia = null,
            id_vendedor = group.Key.id_vendedor,
            quantidade = group.Count()
        };
    }

    private static ReservaTotalsDto CreateMonthTotals(IGrouping<ReservaMonthKey, ReservaTotalRow> group)
    {
        return new ReservaTotalsDto
        {
            ano = group.Key.Ano,
            mes = group.Key.Mes,
            dia = null,
            id_vendedor = group.Key.id_vendedor,
            quantidade = group.Count()
        };
    }

    private static ReservaTotalsDto CreateDayTotals(IGrouping<ReservaDayKey, ReservaTotalRow> group)
    {
        return new ReservaTotalsDto
        {
            ano = group.Key.Ano,
            mes = group.Key.Mes,
            dia = group.Key.Dia,
            id_vendedor = group.Key.id_vendedor,
            quantidade = group.Count()
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

    private sealed class ReservaTotalRow
    {
        public int Ano { get; init; }
        public int Mes { get; init; }
        public int Dia { get; init; }
        public int id_vendedor { get; init; }
    }

    private sealed record ReservaYearKey(int Ano, int id_vendedor);
    private sealed record ReservaMonthKey(int Ano, int Mes, int id_vendedor);
    private sealed record ReservaDayKey(int Ano, int Mes, int Dia, int id_vendedor);
}
