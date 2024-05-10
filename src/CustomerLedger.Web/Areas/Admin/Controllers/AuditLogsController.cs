using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Administrator)]
public class AuditLogsController : Controller
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(int? branchId, string? tableName, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        var result = await _auditLogService.GetPagedAsync(branchId, tableName, pageNumber, pageSize: 25, cancellationToken);
        ViewData["route_tableName"] = tableName;
        ViewData["tableName"] = tableName;
        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(long id, string? adminNote, CancellationToken cancellationToken)
    {
        await _auditLogService.SetReviewStatusAsync(id, AuditReviewStatus.Reviewed, adminNote, cancellationToken);
        TempData["StatusMessage"] = "Audit log entry marked as reviewed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(long id, CancellationToken cancellationToken)
    {
        await _auditLogService.ArchiveAsync(id, cancellationToken);
        TempData["StatusMessage"] = "Audit log entry archived.";
        return RedirectToAction(nameof(Index));
    }
}
