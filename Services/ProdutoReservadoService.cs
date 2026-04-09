namespace api_aggregations.Services;

using api_aggregations.Models;
using api_aggregations.Data;
using api_aggregations.Dtos;
using api_aggregations.Exceptions;
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
    
    // ############################################################################################################

    public async Task<PagedResult<ProdutoReservado>> GetAllAsync(ProdutoReservadoQuery query, CancellationToken cancellationToken)
    {
        ValidatePagination(query);

        IQueryable<ProdutoReservado> produtos = _context.ProdutoReservado.AsNoTracking();

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

        var totalCount = await produtos.CountAsync(cancellationToken);

        var skip = (query.PageNumber - 1) * query.PageSize;
        var items = await produtos
            .OrderBy(p => p.id)
            .Skip(skip)
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
