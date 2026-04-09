namespace api_aggregations.Services;

using api_aggregations.Models;
using api_aggregations.Data;
using api_aggregations.Dtos;
using api_aggregations.Exceptions;
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
    
    public async Task<PagedResult<Reserva>> GetAllAsync(ReservaQuery query, CancellationToken cancellationToken)
    {
        ValidatePagination(query);

        IQueryable<Reserva> reservas = _context.Reserva.AsNoTracking();

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

        var totalCount = await reservas.CountAsync(cancellationToken);

        var skip = (query.PageNumber - 1) * query.PageSize;
        var items = await reservas
            .OrderBy(r => r.id)
            .Skip(skip)
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
}
