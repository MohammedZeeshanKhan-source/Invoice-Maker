using Microsoft.EntityFrameworkCore;
using InvoiceMaker.Data;
using InvoiceMaker.Models;

namespace InvoiceMaker.Services;

// ── Interface ──────────────────────────────────────────────────

/// <summary>Business-logic layer for customer operations.</summary>
public interface ICustomerService
{
    Task<List<CustomerResponseDto>> GetAllAsync();
    Task<CustomerResponseDto?> GetByIdAsync(int id);
    Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerResponseDto?> UpdateAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<CustomerResponseDto>> SearchAsync(string query);
}

// ── Implementation ─────────────────────────────────────────────

/// <summary>Handles CRUD and search for <see cref="Customer"/> entities.</summary>
public class CustomerService : ICustomerService
{
    private readonly InvoiceMakerContext _db;
    public CustomerService(InvoiceMakerContext db) => _db = db;

    /// <summary>Returns every customer with their invoice count.</summary>
    public async Task<List<CustomerResponseDto>> GetAllAsync()
    {
        return await _db.Customers
            .Include(c => c.Invoices)
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    /// <summary>Returns a single customer or null.</summary>
    public async Task<CustomerResponseDto?> GetByIdAsync(int id)
    {
        var c = await _db.Customers
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == id);
        return c is null ? null : MapToDto(c);
    }

    /// <summary>Creates a new customer.</summary>
    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return MapToDto(customer);
    }

    /// <summary>Updates an existing customer.</summary>
    public async Task<CustomerResponseDto?> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _db.Customers.Include(c => c.Invoices).FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return null;

        customer.Name = dto.Name;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.City = dto.City;
        customer.State = dto.State;
        customer.ZipCode = dto.ZipCode;
        customer.Country = dto.Country;
        customer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(customer);
    }

    /// <summary>Deletes a customer if they have no invoices.</summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _db.Customers.Include(c => c.Invoices).FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return false;
        if (customer.Invoices.Any())
            throw new InvalidOperationException("Cannot delete a customer with existing invoices.");

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Searches customers by name or email.</summary>
    public async Task<List<CustomerResponseDto>> SearchAsync(string query)
    {
        var q = query.ToLower();
        return await _db.Customers
            .Include(c => c.Invoices)
            .Where(c => c.Name.ToLower().Contains(q) || c.Email.ToLower().Contains(q))
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    // ── Mapping ────────────────────────────────────────────────
    private static CustomerResponseDto MapToDto(Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Email = c.Email,
        Phone = c.Phone,
        Address = c.Address,
        City = c.City,
        State = c.State,
        ZipCode = c.ZipCode,
        Country = c.Country,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        InvoiceCount = c.Invoices?.Count ?? 0
    };
}
