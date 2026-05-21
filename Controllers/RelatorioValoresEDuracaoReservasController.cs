namespace api_aggregations.Controllers;

using api_aggregations.Dtos;
using api_aggregations.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("relatoriovaloreseduracaoreservas")]
public class RelatorioValoresEDuracaoReservasController : ControllerBase
{
    private readonly RelatorioValoresEDuracaoReservasService _service;
    private readonly RelatorioExcelExportService _excelExportService;

    public RelatorioValoresEDuracaoReservasController(
        RelatorioValoresEDuracaoReservasService service,
        RelatorioExcelExportService excelExportService)
    {
        _service = service;
        _excelExportService = excelExportService;
    }

    /// <summary>
    /// Returns totals grouped by month and product.
    /// </summary>
    /// <param name="query">mandatory</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>Totals grouped by month and by product.</returns>
    [HttpGet("totaisProduto")]
    [ProducesResponseType(typeof(TotaisProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TotaisProdutoDto>> GetTotaisProduto(
        [FromQuery] TotaisQuery query,
        CancellationToken cancellationToken)
    {
        // Returns JSON totals. Use /exportar when the same information is needed
        // as a downloadable Excel file.
        var result = await _service.GetTotaisProdutoAsync(query, cancellationToken);
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

    /// <summary>
    /// Exports one totals report to an Excel file.
    /// </summary>
    /// <param name="query">Use agruparPor=produto/lugar and mostrar=valor/duracao.</param>
    /// <param name="cancellationToken">mandatory</param>
    /// <returns>An Excel file with the selected totals.</returns>
    [HttpGet("exportar")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Exportar(
        [FromQuery] ExportarTotaisQuery query,
        CancellationToken cancellationToken)
    {
        // The Excel service validates agruparPor/mostrar and returns valid .xlsx bytes.
        var excelBytes = await _excelExportService.CriarExcelAsync(query, cancellationToken);
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_pesquisa.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
