using Microsoft.AspNetCore.Mvc;
using InvoiceMaker.Models;
using InvoiceMaker.Services;

namespace InvoiceMaker.Controllers;

/// <summary>API controller for customer management.</summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _svc;
    public CustomersController(ICustomerService svc) => _svc = svc;

    /// <summary>Lists all customers.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CustomerResponseDto>>> GetAll()
        => Ok(await _svc.GetAllAsync());

    /// <summary>Searches customers by name or email.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<CustomerResponseDto>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(await _svc.GetAllAsync());
        return Ok(await _svc.SearchAsync(query));
    }

    /// <summary>Gets a single customer by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
    {
        var result = await _svc.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new customer.</summary>
    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing customer.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponseDto>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _svc.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Deletes a customer.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _svc.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
