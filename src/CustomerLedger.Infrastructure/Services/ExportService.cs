using System.Text.Json;
using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Services;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public ExportService(ApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private int? EffectiveBranchId(int? requestedBranchId) =>
        _currentUser.IsAdministrator ? requestedBranchId : _currentUser.BranchId;

    public async Task<(byte[] Content, string FileName)> ExportCustomersCsvAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveBranchId = EffectiveBranchId(branchId);
        var query = _db.Customers.AsNoTracking().Where(c => !c.IsDeleted);
        if (effectiveBranchId.HasValue)
        {
            query = query.Where(c => c.BranchId == effectiveBranchId.Value);
        }

        var customers = await query.Include(c => c.Branch).OrderBy(c => c.CustomerCode).ToListAsync(cancellationToken);

        var lines = new List<string> { CsvUtilities.BuildRow(new[] { "CustomerCode", "FullName", "Email", "PhoneNumber", "Branch", "Status", "RegistrationDate" }) };
        lines.AddRange(customers.Select(c => CsvUtilities.BuildRow(new[]
        {
            c.CustomerCode, c.FullName, c.Email, c.PhoneNumber, c.Branch.Name, c.Status.ToString(), c.RegistrationDate.ToString("yyyy-MM-dd")
        })));

        return (CsvUtilities.BuildCsvBytes(lines), $"customers_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public async Task<(byte[] Content, string FileName)> ExportCustomersJsonAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveBranchId = EffectiveBranchId(branchId);
        var query = _db.Customers.AsNoTracking().Where(c => !c.IsDeleted);
        if (effectiveBranchId.HasValue)
        {
            query = query.Where(c => c.BranchId == effectiveBranchId.Value);
        }

        var customers = await query
            .Select(c => new { c.CustomerCode, c.FullName, c.Email, c.PhoneNumber, c.Status, c.RegistrationDate })
            .OrderBy(c => c.CustomerCode)
            .ToListAsync(cancellationToken);

        var json = JsonSerializer.SerializeToUtf8Bytes(customers, new JsonSerializerOptions { WriteIndented = true });
        return (json, $"customers_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    public async Task<(byte[] Content, string FileName)> ExportInvoicesCsvAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveBranchId = EffectiveBranchId(branchId);
        var query = _db.Invoices.AsNoTracking().Where(i => !i.IsDeleted);
        if (effectiveBranchId.HasValue)
        {
            query = query.Where(i => i.BranchId == effectiveBranchId.Value);
        }

        var invoices = await query.Include(i => i.Customer).OrderBy(i => i.InvoiceNumber).ToListAsync(cancellationToken);

        var lines = new List<string> { CsvUtilities.BuildRow(new[] { "InvoiceNumber", "Customer", "InvoiceDate", "TotalAmount", "PaidAmount", "OutstandingAmount", "PaymentStatus", "InvoiceStatus" }) };
        lines.AddRange(invoices.Select(i => CsvUtilities.BuildRow(new[]
        {
            i.InvoiceNumber, i.Customer.FullName, i.InvoiceDate.ToString("yyyy-MM-dd"),
            i.TotalAmount.ToString("F2"), i.PaidAmount.ToString("F2"), i.OutstandingAmount.ToString("F2"),
            i.PaymentStatus.ToString(), i.InvoiceStatus.ToString()
        })));

        return (CsvUtilities.BuildCsvBytes(lines), $"invoices_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public async Task<(byte[] Content, string FileName)> ExportPaymentsCsvAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveBranchId = EffectiveBranchId(branchId);
        var query = _db.Payments.AsNoTracking().AsQueryable();
        if (effectiveBranchId.HasValue)
        {
            query = query.Where(p => p.BranchId == effectiveBranchId.Value);
        }

        var payments = await query.Include(p => p.Invoice).Include(p => p.Customer).OrderBy(p => p.PaymentNumber).ToListAsync(cancellationToken);

        var lines = new List<string> { CsvUtilities.BuildRow(new[] { "PaymentNumber", "Invoice", "Customer", "PaymentDate", "Amount", "Method", "Status" }) };
        lines.AddRange(payments.Select(p => CsvUtilities.BuildRow(new[]
        {
            p.PaymentNumber, p.Invoice.InvoiceNumber, p.Customer.FullName, p.PaymentDate.ToString("yyyy-MM-dd"),
            p.Amount.ToString("F2"), p.PaymentMethod.ToString(), p.PaymentStatus.ToString()
        })));

        return (CsvUtilities.BuildCsvBytes(lines), $"payments_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    public async Task<(byte[] Content, string FileName)> ExportCustomerAccountStatementCsvAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.Include(c => c.CustomerAccount).FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken)
            ?? throw new BusinessRuleException("Customer not found.");

        if (!_currentUser.CanAccessBranch(customer.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this customer's branch.");
        }

        var invoices = await _db.Invoices.AsNoTracking()
            .Where(i => i.CustomerId == customerId && !i.IsDeleted)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync(cancellationToken);

        var payments = await _db.Payments.AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        var lines = new List<string>
        {
            CsvUtilities.BuildRow(new[] { "Account Statement", customer.FullName, customer.CustomerCode }),
            CsvUtilities.BuildRow(new[] { "Credit Limit", customer.CustomerAccount?.CreditLimit.ToString("F2") ?? "0.00" }),
            CsvUtilities.BuildRow(new[] { "Current Balance", customer.CustomerAccount?.CurrentBalance.ToString("F2") ?? "0.00" }),
            string.Empty,
            CsvUtilities.BuildRow(new[] { "Type", "Reference", "Date", "Amount", "Status" })
        };

        lines.AddRange(invoices.Select(i => CsvUtilities.BuildRow(new[]
        {
            "Invoice", i.InvoiceNumber, i.InvoiceDate.ToString("yyyy-MM-dd"), i.TotalAmount.ToString("F2"), i.InvoiceStatus.ToString()
        })));

        lines.AddRange(payments.Select(p => CsvUtilities.BuildRow(new[]
        {
            "Payment", p.PaymentNumber, p.PaymentDate.ToString("yyyy-MM-dd"), p.Amount.ToString("F2"), p.PaymentStatus.ToString()
        })));

        return (CsvUtilities.BuildCsvBytes(lines), $"statement_{customer.CustomerCode}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }
}
