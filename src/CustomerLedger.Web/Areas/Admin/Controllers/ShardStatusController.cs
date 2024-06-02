using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Administrator)]
public class ShardStatusController : Controller
{
    private readonly IShardResolver _shardResolver;
    private readonly ICrossShardReportingService _crossShardReportingService;

    public ShardStatusController(IShardResolver shardResolver, ICrossShardReportingService crossShardReportingService)
    {
        _shardResolver = shardResolver;
        _crossShardReportingService = crossShardReportingService;
    }

    public IActionResult Index()
    {
        ViewBag.Shards = _shardResolver.GetAllShards();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunCrossShardReport(CancellationToken cancellationToken)
    {
        var result = await _crossShardReportingService.GetBranchRevenueSummaryAcrossShardsAsync(cancellationToken);
        ViewBag.Shards = _shardResolver.GetAllShards();
        return View("Index", result);
    }
}
