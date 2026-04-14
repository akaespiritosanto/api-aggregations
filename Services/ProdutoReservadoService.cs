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

    public async Task<List<ProdutoReservadoTotalsDto>> GetTotalsAsync(int? ano, int? mes, int? dia, int? idEntidade, CancellationToken cancellationToken)
    {
        ValidateDateParts(ano, mes, dia);

        IQueryable<ProdutoReservado> produtos = _context.ProdutoReservado.AsNoTracking();

        if (idEntidade is not null)
        {
            produtos = produtos.Where(p => p.id_entidade == idEntidade.Value);
        }

        var produtosComData = produtos.Select(p => new
        {
            Data = p.data_criacao,
            p.id_entidade,
            p.quantidade,
            p.valor_tarifa,
            p.valor_comissao,
            p.valor_taxas,
            p.desconto,
            p.descontoAutomatico
        });

        var produtosComPartesData = produtosComData.Select(p => new
        {
            Ano = p.Data.Year,
            Mes = p.Data.Month,
            Dia = p.Data.Day,
            p.id_entidade,
            p.quantidade,
            p.valor_tarifa,
            p.valor_comissao,
            p.valor_taxas,
            p.desconto,
            p.descontoAutomatico
        });

        if (ano is not null) produtosComPartesData = produtosComPartesData.Where(p => p.Ano == ano.Value);
        if (mes is not null) produtosComPartesData = produtosComPartesData.Where(p => p.Mes == mes.Value);
        if (dia is not null) produtosComPartesData = produtosComPartesData.Where(p => p.Dia == dia.Value);

        if (mes is null)
        {
            return await produtosComPartesData
                .GroupBy(p => new { p.Ano, p.id_entidade })
                .Select(g => new ProdutoReservadoTotalsDto
                {
                    ano = g.Key.Ano,
                    mes = null,
                    dia = null,
                    id_entidade = g.Key.id_entidade,
                    quantidade = g.Sum(x => x.quantidade),
                    valor_tarifa = g.Sum(x => x.valor_tarifa),
                    valor_comissao = g.Sum(x => x.valor_comissao),
                    valor_taxas = g.Sum(x => x.valor_taxas),
                    valor_descontos = g.Sum(x => (x.desconto ?? 0) + x.descontoAutomatico)
                })
                .OrderBy(x => x.ano)
                .ThenBy(x => x.id_entidade)
                .ToListAsync(cancellationToken);
        }

        if (dia is null)
        {
            return await produtosComPartesData
                .GroupBy(p => new { p.Ano, p.Mes, p.id_entidade })
                .Select(g => new ProdutoReservadoTotalsDto
                {
                    ano = g.Key.Ano,
                    mes = g.Key.Mes,
                    dia = null,
                    id_entidade = g.Key.id_entidade,
                    quantidade = g.Sum(x => x.quantidade),
                    valor_tarifa = g.Sum(x => x.valor_tarifa),
                    valor_comissao = g.Sum(x => x.valor_comissao),
                    valor_taxas = g.Sum(x => x.valor_taxas),
                    valor_descontos = g.Sum(x => (x.desconto ?? 0) + x.descontoAutomatico)
                })
                .OrderBy(x => x.ano)
                .ThenBy(x => x.mes)
                .ThenBy(x => x.id_entidade)
                .ToListAsync(cancellationToken);
        }

        return await produtosComPartesData
            .GroupBy(p => new { p.Ano, p.Mes, p.Dia, p.id_entidade })
            .Select(g => new ProdutoReservadoTotalsDto
            {
                ano = g.Key.Ano,
                mes = g.Key.Mes,
                dia = g.Key.Dia,
                id_entidade = g.Key.id_entidade,
                quantidade = g.Sum(x => x.quantidade),
                valor_tarifa = g.Sum(x => x.valor_tarifa),
                valor_comissao = g.Sum(x => x.valor_comissao),
                valor_taxas = g.Sum(x => x.valor_taxas),
                valor_descontos = g.Sum(x => (x.desconto ?? 0) + x.descontoAutomatico)
            })
            .OrderBy(x => x.ano)
            .ThenBy(x => x.mes)
            .ThenBy(x => x.dia)
            .ThenBy(x => x.id_entidade)
            .ToListAsync(cancellationToken);
    }

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
        var deleted = await _context.ProdutoReservado
            .Where(p => p.id == id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
        {
            throw new NotFoundException($"ProdutoReservado with id '{id}' was not found.");
        }

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

    private static void ValidateDateParts(int? ano, int? mes, int? dia)
    {
        if (ano is not null && ano < 1)
        {
            throw new BadRequestException("ano must be a valid year.");
        }

        if (mes is not null && (mes < 1 || mes > 12))
        {
            throw new BadRequestException("mes must be between 1 and 12.");
        }

        if (dia is not null && (dia < 1 || dia > 31))
        {
            throw new BadRequestException("dia must be between 1 and 31.");
        }

        if (mes is not null && ano is null)
        {
            throw new BadRequestException("When using mes, you must also provide ano.");
        }

        if (dia is not null && (ano is null || mes is null))
        {
            throw new BadRequestException("When using dia, you must also provide ano and mes.");
        }

        if (ano is not null && mes is not null && dia is not null)
        {
            var maxDay = DateTime.DaysInMonth(ano.Value, mes.Value);
            if (dia.Value > maxDay)
            {
                throw new BadRequestException($"dia must be between 1 and {maxDay} for ano={ano} and mes={mes}.");
            }
        }
    }
}
