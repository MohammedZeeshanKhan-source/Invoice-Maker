using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceMaker.Models;

/// <summary>
/// Represents an invoice issued to a customer.
/// </summary>
public class Invoice
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Unique, auto-generated invoice number (e.g. INV-0001).</summary>
    [Required, MaxLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>Foreign key to the billed customer.</summary>
    public int CustomerId { get; set; }

    /// <summary>Date the invoice was issued.</summary>
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    /// <summary>Date payment is due.</summary>
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

    /// <summary>Current lifecycle status.</summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    /// <summary>Sum of all line-item amounts.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    /// <summary>Tax percentage applied (0–100).</summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; }

    /// <summary>Computed tax amount (SubTotal × TaxRate / 100).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount => SubTotal * TaxRate / 100m;

    /// <summary>Flat discount subtracted from the total.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    /// <summary>Computed grand total (SubTotal + TaxAmount − DiscountAmount).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total => SubTotal + TaxAmount - DiscountAmount;

    /// <summary>Optional notes or terms.</summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>When this record was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When this record was last updated (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Customer who is billed.</summary>
    public Customer Customer { get; set; } = null!;

    /// <summary>Line items on this invoice.</summary>
    public ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();
}
