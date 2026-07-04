using Microsoft.EntityFrameworkCore;
using InvoiceMaker.Data;
using InvoiceMaker.Models;

namespace InvoiceMaker.Services;

// ── Interface ──────────────────────────────────────────────────

/// <summary>Business-logic layer for invoice operations.</summary>
public interface IInvoiceService
{
    Task<List<InvoiceSummaryDto>> GetAllAsync(string? status, string? search);
    Task<InvoiceResponseDto?> GetByIdAsync(int id);
    Task<InvoiceResponseDto> CreateAsync(CreateInvoiceDto dto);
    Task<InvoiceResponseDto?> UpdateAsync(int id, UpdateInvoiceDto dto);
    Task<bool> DeleteAsync(int id);
    Task<InvoiceResponseDto?> UpdateStatusAsync(int id, InvoiceStatus status);
    Task<DashboardDto> GetDashboardAsync();
    Task<string> GenerateInvoiceNumberAsync();
}

// ── Implementation ─────────────────────────────────────────────

/// <summary>Handles CRUD, status changes, and dashboard aggregation for invoices.</summary>
public class InvoiceService : IInvoiceService
{
    private readonly InvoiceMakerContext _db;
    public InvoiceService(InvoiceMakerContext db) => _db = db;

    /// <summary>Returns invoice summaries with optional status/search filters.</summary>
    public async Task<List<InvoiceSummaryDto>> GetAllAsync(string? status, string? search)
    {
        var query = _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<InvoiceStatus>(status, true, out var statusEnum))
        {
            query = query.Where(i => i.Status == statusEnum);
        }

        // Search by invoice number or customer name
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(q) ||
                i.Customer.Name.ToLower().Contains(q));
        }

        var invoices = await query
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();

        return invoices.Select(i => new InvoiceSummaryDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            CustomerName = i.Customer.Name,
            IssueDate = i.IssueDate,
            DueDate = i.DueDate,
            Status = i.Status,
            Total = i.SubTotal + (i.SubTotal * i.TaxRate / 100m) - i.DiscountAmount
        }).ToList();
    }

    /// <summary>Returns full invoice detail or null.</summary>
    public async Task<InvoiceResponseDto?> GetByIdAsync(int id)
    {
        var inv = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.LineItems.OrderBy(l => l.SortOrder))
            .FirstOrDefaultAsync(i => i.Id == id);

        return inv is null ? null : MapToResponseDto(inv);
    }

    /// <summary>Creates a new invoice with line items.</summary>
    public async Task<InvoiceResponseDto> CreateAsync(CreateInvoiceDto dto)
    {
        var invoiceNumber = await GenerateInvoiceNumberAsync();

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = dto.CustomerId,
            IssueDate = dto.IssueDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(30),
            Status = dto.Status,
            TaxRate = dto.TaxRate,
            DiscountAmount = dto.DiscountAmount,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Add line items and compute subtotal
        decimal subTotal = 0;
        foreach (var li in dto.LineItems)
        {
            var amount = li.Quantity * li.UnitPrice;
            subTotal += amount;
            invoice.LineItems.Add(new LineItem
            {
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                SortOrder = li.SortOrder,
            });
        }
        invoice.SubTotal = subTotal;

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        return (await GetByIdAsync(invoice.Id))!;
    }

    /// <summary>Updates an existing invoice and replaces its line items.</summary>
    public async Task<InvoiceResponseDto?> UpdateAsync(int id, UpdateInvoiceDto dto)
    {
        var invoice = await _db.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null) return null;

        invoice.CustomerId = dto.CustomerId;
        invoice.IssueDate = dto.IssueDate ?? invoice.IssueDate;
        invoice.DueDate = dto.DueDate ?? invoice.DueDate;
        invoice.Status = dto.Status;
        invoice.TaxRate = dto.TaxRate;
        invoice.DiscountAmount = dto.DiscountAmount;
        invoice.Notes = dto.Notes;
        invoice.UpdatedAt = DateTime.UtcNow;

        // Replace line items
        _db.LineItems.RemoveRange(invoice.LineItems);
        decimal subTotal = 0;
        foreach (var li in dto.LineItems)
        {
            var amount = li.Quantity * li.UnitPrice;
            subTotal += amount;
            invoice.LineItems.Add(new LineItem
            {
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                SortOrder = li.SortOrder,
            });
        }
        invoice.SubTotal = subTotal;

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    /// <summary>Deletes an invoice and its line items.</summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var invoice = await _db.Invoices.FindAsync(id);
        if (invoice is null) return false;

        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Updates only the status of an invoice.</summary>
    public async Task<InvoiceResponseDto?> UpdateStatusAsync(int id, InvoiceStatus status)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null) return null;

        invoice.Status = status;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return MapToResponseDto(invoice);
    }

    /// <summary>Returns aggregated dashboard statistics.</summary>
    public async Task<DashboardDto> GetDashboardAsync()
    {
        var invoices = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .ToListAsync();

        var allTotals = invoices.Select(i =>
            new { i.Status, Total = i.SubTotal + (i.SubTotal * i.TaxRate / 100m) - i.DiscountAmount }
        ).ToList();

        var recentInvoices = invoices
            .OrderByDescending(i => i.IssueDate)
            .Take(5)
            .Select(i => new InvoiceSummaryDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CustomerName = i.Customer.Name,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                Status = i.Status,
                Total = i.SubTotal + (i.SubTotal * i.TaxRate / 100m) - i.DiscountAmount
            })
            .ToList();

        return new DashboardDto
        {
            TotalInvoices = invoices.Count,
            TotalRevenue = allTotals.Where(t => t.Status == InvoiceStatus.Paid).Sum(t => t.Total),
            PaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            PendingAmount = allTotals
                .Where(t => t.Status == InvoiceStatus.Sent || t.Status == InvoiceStatus.Draft)
                .Sum(t => t.Total),
            OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            RecentInvoices = recentInvoices
        };
    }

    /// <summary>Generates the next sequential invoice number (INV-0001, INV-0002, …).</summary>
    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var last = await _db.Invoices
            .OrderByDescending(i => i.Id)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int next = 1;
        if (last is not null && last.StartsWith("INV-") &&
            int.TryParse(last.AsSpan(4), out var num))
        {
            next = num + 1;
        }

        return $"INV-{next:D4}";
    }

    // ── Mapping ────────────────────────────────────────────────

    private static InvoiceResponseDto MapToResponseDto(Invoice inv) => new()
    {
        Id = inv.Id,
        InvoiceNumber = inv.InvoiceNumber,
        CustomerId = inv.CustomerId,
        Customer = new CustomerResponseDto
        {
            Id = inv.Customer.Id,
            Name = inv.Customer.Name,
            Email = inv.Customer.Email,
            Phone = inv.Customer.Phone,
            Address = inv.Customer.Address,
            City = inv.Customer.City,
            State = inv.Customer.State,
            ZipCode = inv.Customer.ZipCode,
            Country = inv.Customer.Country,
        },
        IssueDate = inv.IssueDate,
        DueDate = inv.DueDate,
        Status = inv.Status,
        SubTotal = inv.SubTotal,
        TaxRate = inv.TaxRate,
        TaxAmount = inv.SubTotal * inv.TaxRate / 100m,
        DiscountAmount = inv.DiscountAmount,
        Total = inv.SubTotal + (inv.SubTotal * inv.TaxRate / 100m) - inv.DiscountAmount,
        Notes = inv.Notes,
        CreatedAt = inv.CreatedAt,
        UpdatedAt = inv.UpdatedAt,
        LineItems = inv.LineItems.OrderBy(l => l.SortOrder).Select(l => new LineItemDto
        {
            Id = l.Id,
            Description = l.Description,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            Amount = l.Quantity * l.UnitPrice,
            SortOrder = l.SortOrder,
        }).ToList()
    };
}
