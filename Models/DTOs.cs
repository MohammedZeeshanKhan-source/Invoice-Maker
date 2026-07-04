using System.ComponentModel.DataAnnotations;

namespace InvoiceMaker.Models;

// ── Customer DTOs ──────────────────────────────────────────────

/// <summary>DTO for creating a new customer.</summary>
public class CreateCustomerDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(254), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? ZipCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}

/// <summary>DTO for updating an existing customer.</summary>
public class UpdateCustomerDto : CreateCustomerDto { }

/// <summary>DTO returned when reading customer data.</summary>
public class CustomerResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int InvoiceCount { get; set; }
}

// ── Invoice DTOs ───────────────────────────────────────────────

/// <summary>DTO for a line item within an invoice creation request.</summary>
public class CreateLineItemDto
{
    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>DTO for creating a new invoice.</summary>
public class CreateInvoiceDto
{
    [Required]
    public int CustomerId { get; set; }

    public DateTime? IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal TaxRate { get; set; }
    public decimal DiscountAmount { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public List<CreateLineItemDto> LineItems { get; set; } = new();
}

/// <summary>DTO for updating an existing invoice.</summary>
public class UpdateInvoiceDto : CreateInvoiceDto { }

/// <summary>Lightweight invoice summary for list views.</summary>
public class InvoiceSummaryDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal Total { get; set; }
}

/// <summary>Full invoice detail response.</summary>
public class InvoiceResponseDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public CustomerResponseDto Customer { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LineItemDto> LineItems { get; set; } = new();
}

/// <summary>Line item detail in a response.</summary>
public class LineItemDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

// ── Dashboard DTO ──────────────────────────────────────────────

/// <summary>Aggregated dashboard statistics.</summary>
public class DashboardDto
{
    public int TotalInvoices { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PaidInvoices { get; set; }
    public decimal PendingAmount { get; set; }
    public int OverdueInvoices { get; set; }
    public List<InvoiceSummaryDto> RecentInvoices { get; set; } = new();
}
