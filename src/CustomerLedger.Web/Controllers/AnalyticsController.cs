using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Controllers;

/// <summary>
/// v7.0.0 — Capital: customer payment-risk scoring (logistic regression) and RFM
/// segmentation (data mining). See docs/releases/v7.0.0-Capital.md for methodology and
/// limitations. Manager/Admin only — this is an operational decision-support view, not a
/// day-to-day cashier workflow.
/// </summary>
[Authorize(Roles = Roles.Administrator + "," + Roles.BranchManager)]
public class AnalyticsController : Controller
{
    private readonly ICustomerRiskScoringService _riskScoringService;
    private readonly ICustomerSegmentationService _segmentationService;

    public AnalyticsController(ICustomerRiskScoringService riskScoringService, ICustomerSegmentationService segmentationService)
    {
        _riskScoringService = riskScoringService;
        _segmentationService = segmentationService;
    }

    public async Task<IActionResult> Index(int? branchId, CancellationToken cancellationToken)
    {
        var riskScores = await _riskScoringService.ScoreCustomersAsync(branchId, cancellationToken);
        var segments = await _segmentationService.SegmentCustomersAsync(branchId, cancellationToken);

        ViewBag.Segments = segments;
        return View(riskScores);
    }
}
