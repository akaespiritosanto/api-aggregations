namespace api_aggregations.Controllers;

using Microsoft.AspNetCore.Mvc;
using api_aggregations.Services;
using api_aggregations.Models;
using api_aggregations.Dtos;

[ApiController]
[Route("produtoreservado")]
public class ProdutoReservadoController : ControllerBase
{
    private readonly ProdutoReservadoService _service;
    private readonly ILogger<ProdutoReservadoController> _logger;

    public ProdutoReservadoController(ProdutoReservadoService service, ILogger<ProdutoReservadoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Returns totals of reserved products, grouped by ano/mes/dia and id_entidade.
    /// </summary>
    /// <param name="ano">optional</param>
    /// <param name="mes">optional</param>
    /// <param name="dia">optional</param>
    /// <param name="id_entidade">optional</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>A list of grouped totals.</returns>
    [HttpGet("totais")]
    [ProducesResponseType(typeof(List<ProdutoReservadoTotalsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ProdutoReservadoTotalsDto>>> GetTotals(
        [FromQuery] int? ano,
        [FromQuery] int? mes,
        [FromQuery] int? dia,
        [FromQuery] int? id_entidade,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetTotalsAsync(ano, mes, dia, id_entidade, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns a paginated list of reserved products.
    /// </summary>
    /// <param name="query">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>A paged result of <see cref="ProdutoReservado"/> items.</returns>
    /// <response code="200">The paged result.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProdutoReservado>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<ProdutoReservado>>> GetAll([FromQuery] ProdutoReservadoQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns a reserved product by id.
    /// </summary>
    /// <param name="id">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>The <see cref="ProdutoReservado"/>.</returns>
    /// <response code="200">The reserved product.</response>
    /// <response code="404">Not found.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdutoReservado), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProdutoReservado>> GetById(int id, CancellationToken cancellationToken)
    {
        var produto = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(produto);
    }

    /// <summary>
    /// Creates a new reserved product.
    /// </summary>
    /// <param name="produtoReservado">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>The created <see cref="ProdutoReservado"/>.</returns>
    /// <response code="201">Created successfully.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoReservado), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProdutoReservado>> Create([FromBody] ProdutoReservado produtoReservado, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(produtoReservado, cancellationToken);
        _logger.LogInformation("ProdutoReservado created with id {Id}", created.id);

        return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
    }

    /// <summary>
    /// Updates an existing reserved product.
    /// </summary>
    /// <param name="id">mandatory</param>
    /// <param name="produtoReservado">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>The updated <see cref="ProdutoReservado"/>.</returns>
    /// <response code="200">Updated successfully.</response>
    /// <response code="404">Not found.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProdutoReservado), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProdutoReservado>> Update(int id, [FromBody] ProdutoReservado produtoReservado, CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, produtoReservado, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a reserved product by id.
    /// </summary>
    /// <param name="id">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>No content.</returns>
    /// <response code="204">Deleted successfully.</response>
    /// <response code="404">Not found.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
    
}
