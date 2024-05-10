using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Controllers;

[Authorize]
public class InstallmentPlansController : Controller
{
    private readonly IInstallmentPlanService _installmentPlanService;
    private readonly IInvoiceService _invoiceService;
    private readonly IInstallmentScheduleService _installmentScheduleService;

    public InstallmentPlansController(
        IInstallmentPlanService installmentPlanService,
        IInvoiceService invoiceService,
        IInstallmentScheduleService installmentScheduleService)
    {
        _installmentPlanService = installmentPlanService;
        _invoiceService = invoiceService;
        _installmentScheduleService = installmentScheduleService;
    }

    /// <summary>No dedicated paged list yet in Index scope — plans are reached from their invoice; this redirects staff who land here directly to Invoices.</summary>
    public IActionResult Index() => RedirectToAction("Index", "Invoices");

    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        var plan = await _installmentPlanService.GetByIdAsync(id, cancellationToken);
        if (plan is null)
        {
            return NotFound();
        }

        return View(plan);
    }

    public async Task<IActionResult> Create(long invoiceId, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _invoiceService.GetByIdAsync(invoiceId, cancellationToken);
            if (invoice is null)
            {
                return NotFound();
            }

            return View(new InstallmentPlanCreateViewModel
            {
                InvoiceId = invoiceId,
                Invoice = invoice
            });
        }
        catch (BranchAccessDeniedException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InstallmentPlanCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Invoice = await _invoiceService.GetByIdAsync(model.InvoiceId, cancellationToken);
            return View(model);
        }

        try
        {
            var plan = await _installmentPlanService.CreateAsync(new InstallmentPlan
            {
                InvoiceId = model.InvoiceId,
                NumberOfInstallments = model.NumberOfInstallments,
                DownPayment = model.DownPayment,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Frequency = model.Frequency
            }, cancellationToken);

            TempData["StatusMessage"] = "Installment plan created and pending approval.";
            return RedirectToAction(nameof(Details), new { id = plan.InstallmentPlanId });
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Invoice = await _invoiceService.GetByIdAsync(model.InvoiceId, cancellationToken);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrator + "," + Roles.BranchManager)]
    public async Task<IActionResult> Approve(long id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        await _installmentPlanService.ApproveAsync(id, userId, cancellationToken);
        TempData["StatusMessage"] = "Installment plan approved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id, CancellationToken cancellationToken)
    {
        await _installmentPlanService.CancelAsync(id, cancellationToken);
        TempData["StatusMessage"] = "Installment plan cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayInstallment(long planId, long scheduleId, decimal amount, PaymentMethod paymentMethod, string? transactionReference, CancellationToken cancellationToken)
    {
        try
        {
            await _installmentScheduleService.PayInstallmentAsync(scheduleId, amount, paymentMethod, transactionReference, cancellationToken);
            TempData["StatusMessage"] = "Installment payment recorded.";
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = planId });
    }
}
