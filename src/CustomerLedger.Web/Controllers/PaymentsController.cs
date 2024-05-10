using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IInvoiceService _invoiceService;

    public PaymentsController(IPaymentService paymentService, IInvoiceService invoiceService)
    {
        _paymentService = paymentService;
        _invoiceService = invoiceService;
    }

    public async Task<IActionResult> Index(int? branchId, long? invoiceId, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetPagedAsync(branchId, invoiceId, pageNumber, pageSize: 15, cancellationToken);
        return View(result);
    }

    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentService.GetByIdAsync(id, cancellationToken);
            if (payment is null)
            {
                return NotFound();
            }

            return View(payment);
        }
        catch (BranchAccessDeniedException)
        {
            return Forbid();
        }
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

            return View(new PaymentCreateViewModel
            {
                InvoiceId = invoiceId,
                Invoice = invoice,
                PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Amount = invoice.OutstandingAmount
            });
        }
        catch (BranchAccessDeniedException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Invoice = await _invoiceService.GetByIdAsync(model.InvoiceId, cancellationToken);
            return View(model);
        }

        try
        {
            var payment = await _paymentService.RecordPaymentAsync(new Payment
            {
                InvoiceId = model.InvoiceId,
                PaymentNumber = model.PaymentNumber,
                PaymentDate = model.PaymentDate,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                TransactionReference = model.TransactionReference,
                Notes = model.Notes
            }, cancellationToken);

            TempData["StatusMessage"] = "Payment recorded successfully.";
            return RedirectToAction(nameof(Details), new { id = payment.PaymentId });
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
    public async Task<IActionResult> Reverse(long id, string reversalReason, CancellationToken cancellationToken)
    {
        try
        {
            var reversal = await _paymentService.ReverseAsync(id, reversalReason, cancellationToken);
            TempData["StatusMessage"] = "Payment reversed successfully.";
            return RedirectToAction(nameof(Details), new { id = reversal.ReversedPaymentId });
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
