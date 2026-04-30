namespace api_aggregations.Controllers;

using api_aggregations.Dtos;
using api_aggregations.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("relatoriovaloreseduracaoreservas")]
public class RelatorioValoresEDuracaoReservasController : ControllerBase
{
    private readonly RelatorioValoresEDuracaoReservasService _service;

    public RelatorioValoresEDuracaoReservasController(RelatorioValoresEDuracaoReservasService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns totals grouped by month and place.
    /// </summary>
    /// <param name="query">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>Totals grouped by month and by place.</returns>
    [HttpGet("totaisProduto")]
    [ProducesResponseType(typeof(TotaisProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TotaisProdutoDto>> GetTotaisProduto(
        [FromQuery] TotaisQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetTotaisLugarAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns totals grouped by month and place.
    /// </summary>
    /// <param name="query">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>Totals grouped by month and by place.</returns>
    [HttpGet("totaisLugar")]
    [ProducesResponseType(typeof(TotaisLugarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TotaisLugarDto>> GetTotaisLugar(
        [FromQuery] TotaisQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetTotaisLugarPorLugarAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the available DispBase rows for a service.
    /// </summary>
    /// <param name="query">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>A list of DispBase items.</returns>
    [HttpGet("listDisponibilidadesBase")]
    [ProducesResponseType(typeof(List<ListDisponibilidadesBaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ListDisponibilidadesBaseDto>>> ListDisponibilidadesBase(
        [FromQuery] ListDisponibilidadesBaseQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetDisponibilidadesBaseAsync(query, cancellationToken);
        return Ok(result);
    }
}
