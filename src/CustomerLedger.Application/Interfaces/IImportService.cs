using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Validates a customer CSV before ever writing to the database. Expected header:
/// CustomerCode,FullName,Email,PhoneNumber,CNIC,Address,City,InitialCreditLimit — extra
/// columns are ignored, missing required columns reject every row with a clear reason.
/// </summary>
public interface IImportService
{
    /// <summary>Validates every row without persisting anything — always returns WasCommitted = false.</summary>
    Task<CustomerImportResult> PreviewCustomerImportAsync(int branchId, Stream csvStream, CancellationToken cancellationToken = default);

    /// <summary>Re-validates (never trusts a client-supplied "already validated" flag) and writes only the accepted rows inside one transaction.</summary>
    Task<CustomerImportResult> ImportCustomersAsync(int branchId, Stream csvStream, CancellationToken cancellationToken = default);
}
