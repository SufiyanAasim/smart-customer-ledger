using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Controllers;

[Authorize]
public class ExportsController : Controller
{
    private readonly IExportService _exportService;

    public ExportsController(IExportService exportService)
    {
        _exportService = exportService;
    }

    public async Task<IActionResult> CustomersCsv(int? branchId, CancellationToken cancellationToken)
    {
        var (content, fileName) = await _exportService.ExportCustomersCsvAsync(branchId, cancellationToken);
        return File(content, "text/csv", fileName);
    }

    public async Task<IActionResult> CustomersJson(int? branchId, CancellationToken cancellationToken)
    {
        var (content, fileName) = await _exportService.ExportCustomersJsonAsync(branchId, cancellationToken);
        return File(content, "application/json", fileName);
    }

    public async Task<IActionResult> InvoicesCsv(int? branchId, CancellationToken cancellationToken)
    {
        var (content, fileName) = await _exportService.ExportInvoicesCsvAsync(branchId, cancellationToken);
        return File(content, "text/csv", fileName);
    }

    public async Task<IActionResult> PaymentsCsv(int? branchId, CancellationToken cancellationToken)
    {
        var (content, fileName) = await _exportService.ExportPaymentsCsvAsync(branchId, cancellationToken);
        return File(content, "text/csv", fileName);
    }

    public async Task<IActionResult> CustomerStatement(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var (content, fileName) = await _exportService.ExportCustomerAccountStatementCsvAsync(customerId, cancellationToken);
            return File(content, "text/csv", fileName);
        }
        catch (BranchAccessDeniedException)
        {
            return Forbid();
        }
    }
}
