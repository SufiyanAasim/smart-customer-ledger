using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Administrator)]
public class BackupHistoriesController : Controller
{
    private readonly IBackupHistoryService _backupHistoryService;
    private readonly IBackupService _backupService;
    private readonly IRestoreService _restoreService;

    public BackupHistoriesController(IBackupHistoryService backupHistoryService, IBackupService backupService, IRestoreService restoreService)
    {
        _backupHistoryService = backupHistoryService;
        _backupService = backupService;
        _restoreService = restoreService;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        var result = await _backupHistoryService.GetPagedAsync(pageNumber, pageSize: 20, cancellationToken);
        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BackupType backupType, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var history = await _backupService.CreateBackupAsync(backupType, userId, cancellationToken);

        TempData[history.Status == BackupStatus.Completed ? "StatusMessage" : "ErrorMessage"] =
            history.Status == BackupStatus.Completed
                ? $"Backup '{history.FileName}' completed successfully ({history.FileSize:N0} bytes)."
                : $"Backup failed: {history.ErrorMessage}";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Destructive: overwrites the current database with the chosen backup. The
    /// confirmation is in the view (a typed "RESTORE" prompt), not just a JS confirm().
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(long id, string confirmationText, CancellationToken cancellationToken)
    {
        if (!string.Equals(confirmationText, "RESTORE", StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "Restore cancelled — confirmation text did not match.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var succeeded = await _restoreService.RestoreAsync(id, cancellationToken);
            TempData[succeeded ? "StatusMessage" : "ErrorMessage"] =
                succeeded ? "Database restored successfully." : "Restore failed — check server logs.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
