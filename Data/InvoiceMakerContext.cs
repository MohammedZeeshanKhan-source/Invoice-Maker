using Microsoft.EntityFrameworkCore;
using InvoiceMaker.Models;

namespace InvoiceMaker.Data;

/// <summary>
/// Entity Framework Core database context for the Invoice Maker application.
/// </summary>
public class InvoiceMakerContext : DbContext
{
    public InvoiceMakerContext(DbContextOptions<InvoiceMakerContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<LineItem> LineItems => Set<LineItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Customer ───────────────────────────────────────────
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(c => c.Email).IsUnique();
            e.HasMany(c => c.Invoices)
             .WithOne(i => i.Customer)
             .HasForeignKey(i => i.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Invoice ────────────────────────────────────────────
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
            e.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

            // Ignore computed properties so EF doesn't try to map them to columns
            e.Ignore(i => i.TaxAmount);
            e.Ignore(i => i.Total);

            e.HasMany(i => i.LineItems)
             .WithOne(li => li.Invoice)
             .HasForeignKey(li => li.InvoiceId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── LineItem ───────────────────────────────────────────
        modelBuilder.Entity<LineItem>(e =>
        {
            // Ignore computed Amount property
            e.Ignore(li => li.Amount);
        });

        // ── Seed Data ──────────────────────────────────────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Customers
        var customers = new[]
        {
            new Customer { Id = 1, Name = "Acme Corporation", Email = "billing@acme.com", Phone = "+1-555-0101", Address = "123 Business Ave", City = "San Francisco", State = "CA", ZipCode = "94102", Country = "USA", CreatedAt = now, UpdatedAt = now },
            new Customer { Id = 2, Name = "TechStart Inc.", Email = "accounts@techstart.io", Phone = "+1-555-0202", Address = "456 Innovation Blvd", City = "Austin", State = "TX", ZipCode = "73301", Country = "USA", CreatedAt = now, UpdatedAt = now },
            new Customer { Id = 3, Name = "Global Design Studio", Email = "finance@globaldesign.co", Phone = "+44-20-7946-0958", Address = "78 Creative Lane", City = "London", State = "England", ZipCode = "EC1A 1BB", Country = "UK", CreatedAt = now, UpdatedAt = now },
        };
        modelBuilder.Entity<Customer>().HasData(customers);

        // Invoices
        var invoices = new[]
        {
            new { Id = 1, InvoiceNumber = "INV-0001", CustomerId = 1, IssueDate = now, DueDate = now.AddDays(30), Status = InvoiceStatus.Paid, SubTotal = 5500.00m, TaxRate = 8.5m, DiscountAmount = 200.00m, Notes = "Thank you for your business!", CreatedAt = now, UpdatedAt = now },
            new { Id = 2, InvoiceNumber = "INV-0002", CustomerId = 2, IssueDate = now.AddDays(10), DueDate = now.AddDays(40), Status = InvoiceStatus.Sent, SubTotal = 3200.00m, TaxRate = 7.0m, DiscountAmount = 0m, Notes = "Net 30 payment terms.", CreatedAt = now, UpdatedAt = now },
            new { Id = 3, InvoiceNumber = "INV-0003", CustomerId = 3, IssueDate = now.AddDays(-45), DueDate = now.AddDays(-15), Status = InvoiceStatus.Overdue, SubTotal = 8750.00m, TaxRate = 20.0m, DiscountAmount = 500.00m, Notes = "Overdue – please remit immediately.", CreatedAt = now, UpdatedAt = now },
        };
        modelBuilder.Entity<Invoice>().HasData(invoices);

        // Line Items
        var lineItems = new[]
        {
            // Invoice 1
            new { Id = 1, InvoiceId = 1, Description = "Web Application Development", Quantity = 40m, UnitPrice = 125.00m, SortOrder = 1 },
            new { Id = 2, InvoiceId = 1, Description = "UI/UX Design Consultation", Quantity = 4m, UnitPrice = 50.00m, SortOrder = 2 },
            // Invoice 2
            new { Id = 3, InvoiceId = 2, Description = "Monthly Cloud Hosting (Pro)", Quantity = 1m, UnitPrice = 800.00m, SortOrder = 1 },
            new { Id = 4, InvoiceId = 2, Description = "SSL Certificate Renewal", Quantity = 2m, UnitPrice = 200.00m, SortOrder = 2 },
            new { Id = 5, InvoiceId = 2, Description = "Database Administration", Quantity = 12m, UnitPrice = 100.00m, SortOrder = 3 },
            // Invoice 3
            new { Id = 6, InvoiceId = 3, Description = "Brand Identity Package", Quantity = 1m, UnitPrice = 4500.00m, SortOrder = 1 },
            new { Id = 7, InvoiceId = 3, Description = "Marketing Collateral Design", Quantity = 5m, UnitPrice = 350.00m, SortOrder = 2 },
            new { Id = 8, InvoiceId = 3, Description = "Print Production Management", Quantity = 10m, UnitPrice = 250.00m, SortOrder = 3 },
        };
        modelBuilder.Entity<LineItem>().HasData(lineItems);
    }
}
