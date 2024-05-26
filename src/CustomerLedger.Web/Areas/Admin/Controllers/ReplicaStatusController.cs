using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Administrator)]
public class ReplicaStatusController : Controller
{
    private readonly IReplicaHealthService _replicaHealth;
    private readonly IReplicaAwareReportingService _reportingService;

    public ReplicaStatusController(IReplicaHealthService replicaHealth, IReplicaAwareReportingService reportingService)
    {
        _replicaHealth = replicaHealth;
        _reportingService = reportingService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.IsReplicaHealthy = await _replicaHealth.IsReplicaHealthyAsync(cancellationToken);
        var result = await _reportingService.GetBranchRevenueSummaryAsync(cancellationToken);
        return View(result);
    }
}
