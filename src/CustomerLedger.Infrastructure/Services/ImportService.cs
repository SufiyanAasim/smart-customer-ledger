using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Services;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

/// <summary>
/// Validates a customer CSV row-by-row before any database write. Every accepted row still
/// gets re-validated inside ImportAsync's transaction — a caller cannot skip validation by
/// claiming a file was "already previewed".
/// </summary>
public class ImportService : IImportService
{
    private static readonly string[] RequiredHeaders = { "CustomerCode", "FullName", "PhoneNumber", "Address", "City" };
    private const int MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB — generous for a customer CSV, small enough to reject an obviously wrong upload.

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public ImportService(ApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CustomerImportResult> PreviewCustomerImportAsync(int branchId, Stream csvStream, CancellationToken cancellationToken = default)
    {
        var (rows, _) = await ValidateAsync(branchId, csvStream, cancellationToken);
        return new CustomerImportResult { Rows = rows, WasCommitted = false };
    }

    public async Task<CustomerImportResult> ImportCustomersAsync(int branchId, Stream csvStream, CancellationToken cancellationToken = default)
    {
        var (rows, parsedCustomers) = await ValidateAsync(branchId, csvStream, cancellationToken);

        var accepted = rows.Where(r => r.Accepted).ToList();
        if (accepted.Count == 0)
        {
            return new CustomerImportResult { Rows = rows, WasCommitted = false };
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        foreach (var rowNumber in accepted.Select(r => r.RowNumber))
        {
            var customer = parsedCustomers[rowNumber];
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync(cancellationToken);

            _db.CustomerAccounts.Add(new CustomerAccount
            {
                CustomerId = customer.CustomerId,
                CreditLimit = 0,
                CurrentBalance = 0,
                TotalBilled = 0,
                TotalPaid = 0,
                AccountStatus = AccountStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CustomerImportResult { Rows = rows, WasCommitted = true };
    }

    private async Task<(List<CustomerImportRowResult> Results, Dictionary<int, Customer> Parsed)> ValidateAsync(int branchId, Stream csvStream, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessBranch(branchId))
        {
            throw new BranchAccessDeniedException("You cannot import customers into a different branch.");
        }

        using var memoryStream = new MemoryStream();
        await csvStream.CopyToAsync(memoryStream, cancellationToken);
        if (memoryStream.Length > MaxFileSizeBytes)
        {
            throw new BusinessRuleException($"File exceeds the {MaxFileSizeBytes / 1024 / 1024} MB import limit.");
        }

        var content = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        var csvRows = CsvUtilities.ParseCsv(content);

        if (csvRows.Count == 0)
        {
            throw new BusinessRuleException("The file is empty.");
        }

        var header = csvRows[0];
        var columnIndex = header.Select((name, idx) => (name: name.Trim(), idx)).ToDictionary(x => x.name, x => x.idx, StringComparer.OrdinalIgnoreCase);

        var missingHeaders = RequiredHeaders.Where(h => !columnIndex.ContainsKey(h)).ToList();
        if (missingHeaders.Count > 0)
        {
            throw new BusinessRuleException($"Missing required column(s): {string.Join(", ", missingHeaders)}.");
        }

        var existingCodes = new HashSet<string>(
            await _db.Customers.Where(c => c.BranchId == branchId).Select(c => c.CustomerCode).ToListAsync(cancellationToken),
            StringComparer.OrdinalIgnoreCase);
        var codesSeenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var results = new List<CustomerImportRowResult>();
        var parsed = new Dictionary<int, Customer>();

        for (var i = 1; i < csvRows.Count; i++)
        {
            var row = csvRows[i];
            string Field(string name) => columnIndex.TryGetValue(name, out var idx) && idx < row.Count ? row[idx].Trim() : string.Empty;

            var code = Field("CustomerCode");
            var fullName = Field("FullName");
            var phone = Field("PhoneNumber");
            var address = Field("Address");
            var city = Field("City");
            var email = Field("Email");
            var cnic = Field("CNIC");

            string? rejectionReason = null;

            if (string.IsNullOrWhiteSpace(code)) rejectionReason = "CustomerCode is required.";
            else if (string.IsNullOrWhiteSpace(fullName)) rejectionReason = "FullName is required.";
            else if (string.IsNullOrWhiteSpace(phone)) rejectionReason = "PhoneNumber is required.";
            else if (string.IsNullOrWhiteSpace(address)) rejectionReason = "Address is required.";
            else if (string.IsNullOrWhiteSpace(city)) rejectionReason = "City is required.";
            else if (existingCodes.Contains(code)) rejectionReason = $"CustomerCode '{code}' already exists in this branch.";
            else if (!codesSeenInFile.Add(code)) rejectionReason = $"Duplicate CustomerCode '{code}' within this file.";

            var accepted = rejectionReason is null;

            results.Add(new CustomerImportRowResult
            {
                RowNumber = i,
                CustomerCode = code,
                FullName = fullName,
                Accepted = accepted,
                RejectionReason = rejectionReason
            });

            if (accepted)
            {
                parsed[i] = new Customer
                {
                    BranchId = branchId,
                    CustomerCode = code,
                    FullName = fullName,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email,
                    PhoneNumber = phone,
                    CNIC = string.IsNullOrWhiteSpace(cnic) ? null : cnic,
                    Address = address,
                    City = city,
                    RegistrationDate = DateTime.UtcNow,
                    Status = CustomerStatus.Active,
                    IsDeleted = false,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }
        }

        return (results, parsed);
    }
}
