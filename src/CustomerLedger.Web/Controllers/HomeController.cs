using System.Diagnostics;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(int? branchId, CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(branchId, cancellationToken);
        return View(summary);
    }

    [AllowAnonymous]
    public IActionResult Credits()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return RedirectToAction(nameof(Credits));
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
