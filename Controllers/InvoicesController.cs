using Microsoft.AspNetCore.Mvc;
using InvoiceMaker.Models;
using InvoiceMaker.Services;

namespace InvoiceMaker.Controllers;

/// <summary>API controller for invoice management.</summary>
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _svc;
    public InvoicesController(IInvoiceService svc) => _svc = svc;

    /// <summary>Returns aggregated dashboard statistics.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
        => Ok(await _svc.GetDashboardAsync());

    /// <summary>Lists invoices with optional status and search filters.</summary>
    [HttpGet]
    public async Task<ActionResult<List<InvoiceSummaryDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? search)
        => Ok(await _svc.GetAllAsync(status, search));

    /// <summary>Gets a single invoice with full detail.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceResponseDto>> GetById(int id)
    {
        var result = await _svc.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new invoice.</summary>
    [HttpPost]
    public async Task<ActionResult<InvoiceResponseDto>> Create([FromBody] CreateInvoiceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Updates an existing invoice (replaces line items).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<InvoiceResponseDto>> Update(int id, [FromBody] UpdateInvoiceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _svc.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Deletes an invoice.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _svc.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Updates only the status of an invoice.</summary>
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<InvoiceResponseDto>> UpdateStatus(int id, [FromBody] InvoiceStatus status)
    {
        var result = await _svc.UpdateStatusAsync(id, status);
        return result is null ? NotFound() : Ok(result);
    }
}
