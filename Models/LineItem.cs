using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceMaker.Models;

/// <summary>
/// A single billable line on an invoice.
/// </summary>
public class LineItem
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to the parent invoice.</summary>
    public int InvoiceId { get; set; }

    /// <summary>Description of the product or service.</summary>
    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Quantity of units.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    /// <summary>Price per unit.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>Computed line total (Quantity × UnitPrice).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount => Quantity * UnitPrice;

    /// <summary>Display order on the invoice.</summary>
    public int SortOrder { get; set; }

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Parent invoice.</summary>
    public Invoice Invoice { get; set; } = null!;
}
