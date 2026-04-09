namespace api_aggregations.Controllers;

using Microsoft.AspNetCore.Mvc;
using api_aggregations.Services;
using api_aggregations.Models;
using api_aggregations.Dtos;

[ApiController]
[Route("reserva")]
public class ReservaController : ControllerBase
{
    private readonly ReservaService _service;
    private readonly ILogger<ReservaController> _logger;

    public ReservaController(ReservaService service, ILogger<ReservaController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paginated list of reservas.
    /// </summary>
    /// <param name="query">Pagination + optional filters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>A paged result of <see cref="Reserva"/> items.</returns>
    /// <response code="200">The paged result.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Reserva>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<Reserva>>> GetAll([FromQuery] ReservaQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }
    
    /// <summary>
    /// Returns a reserva by id.
    /// </summary>
    /// <param name="id">Reserva id.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The <see cref="Reserva"/>.</returns>
    /// <response code="200">The reserva.</response>
    /// <response code="404">Not found.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Reserva), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Reserva>> GetById(int id, CancellationToken cancellationToken)
    {
        var reserva = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(reserva);
    }

    /// <summary>
    /// Creates a new reserva.
    /// </summary>
    /// <param name="reserva">The reserva to create.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The created <see cref="Reserva"/>.</returns>
    /// <response code="201">Created successfully.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Reserva), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Reserva>> Create([FromBody] Reserva reserva, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(reserva, cancellationToken);
        _logger.LogInformation("Reserva created with id {Id}", created.id);

        return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
    }

    /// <summary>
    /// Updates an existing reserva.
    /// </summary>
    /// <param name="id">Reserva id.</param>
    /// <param name="reserva">New values.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The updated <see cref="Reserva"/>.</returns>
    /// <response code="200">Updated successfully.</response>
    /// <response code="404">Not found.</response>
    /// <response code="500">Unexpected error.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Reserva), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Reserva>> Update(int id, [FromBody] Reserva reserva, CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, reserva, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a reserva by id.
    /// </summary>
    /// <param name="id">Reserva id.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
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
