using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Administrator + "," + Roles.BranchManager)]
public class ReconciliationController : Controller
{
    private readonly IReconciliationService _reconciliationService;
    private readonly IBranchService _branchService;

    public ReconciliationController(IReconciliationService reconciliationService, IBranchService branchService)
    {
        _reconciliationService = reconciliationService;
        _branchService = branchService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Branches = await _branchService.GetAllActiveAsync(cancellationToken);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(int branchId, CancellationToken cancellationToken)
    {
        var reports = await _reconciliationService.ReconcileBranchAsync(branchId, cancellationToken);
        return View("Results", reports);
    }
}
