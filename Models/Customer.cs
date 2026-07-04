using System.ComponentModel.DataAnnotations;

namespace InvoiceMaker.Models;

/// <summary>
/// Represents a customer who can be billed via invoices.
/// </summary>
public class Customer
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Full name of the customer or business.</summary>
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Contact email address.</summary>
    [Required, MaxLength(254), EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Contact phone number (optional).</summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>Street address (optional).</summary>
    [MaxLength(300)]
    public string? Address { get; set; }

    /// <summary>City (optional).</summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>State or province (optional).</summary>
    [MaxLength(100)]
    public string? State { get; set; }

    /// <summary>ZIP or postal code (optional).</summary>
    [MaxLength(20)]
    public string? ZipCode { get; set; }

    /// <summary>Country (optional).</summary>
    [MaxLength(100)]
    public string? Country { get; set; }

    /// <summary>When this record was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When this record was last updated (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Invoices associated with this customer.</summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
