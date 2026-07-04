namespace InvoiceMaker.Models;

/// <summary>
/// Represents the lifecycle status of an invoice.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>Invoice is being drafted and has not been sent.</summary>
    Draft = 0,

    /// <summary>Invoice has been sent to the customer.</summary>
    Sent = 1,

    /// <summary>Invoice has been paid in full.</summary>
    Paid = 2,

    /// <summary>Invoice is past its due date and unpaid.</summary>
    Overdue = 3,

    /// <summary>Invoice has been cancelled.</summary>
    Cancelled = 4
}
