namespace CustomerLedger.Application.Interfaces;

/// <summary>Every export returns raw file bytes plus a suggested file name — branch isolation is applied the same way list screens apply it (ICurrentUserContext), never trusting a caller-supplied branch id for non-administrators.</summary>
public interface IExportService
{
    Task<(byte[] Content, string FileName)> ExportCustomersCsvAsync(int? branchId, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string FileName)> ExportCustomersJsonAsync(int? branchId, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string FileName)> ExportInvoicesCsvAsync(int? branchId, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string FileName)> ExportPaymentsCsvAsync(int? branchId, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string FileName)> ExportCustomerAccountStatementCsvAsync(int customerId, CancellationToken cancellationToken = default);
}
