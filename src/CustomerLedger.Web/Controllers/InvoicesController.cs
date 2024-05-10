using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Controllers;

[Authorize]
public class InvoicesController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICustomerService _customerService;

    public InvoicesController(IInvoiceService invoiceService, ICustomerService customerService)
    {
        _invoiceService = invoiceService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(int? branchId, int? customerId, string? status, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        var result = await _invoiceService.GetPagedAsync(branchId, customerId, status, pageNumber, pageSize: 15, cancellationToken);
        ViewData["route_status"] = status;
        ViewData["status"] = status;
        return View(result);
    }

    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _invoiceService.GetByIdAsync(id, cancellationToken);
            if (invoice is null)
            {
                return NotFound();
            }

            return View(invoice);
        }
        catch (BranchAccessDeniedException)
        {
            return Forbid();
        }
    }

    public async Task<IActionResult> Create(int customerId, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        return View(new InvoiceCreateViewModel
        {
            CustomerId = customerId,
            Customer = customer,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceCreateViewModel model, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(model.CustomerId, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        if (!model.Items.Any())
        {
            ModelState.AddModelError(string.Empty, "An invoice must have at least one item.");
        }

        if (!ModelState.IsValid)
        {
            model.Customer = customer;
            return View(model);
        }

        try
        {
            var invoice = await _invoiceService.CreateDraftAsync(
                new Invoice
                {
                    CustomerId = model.CustomerId,
                    BranchId = customer.BranchId,
                    InvoiceNumber = model.InvoiceNumber,
                    InvoiceDate = model.InvoiceDate,
                    DueDate = model.DueDate
                },
                model.Items.Select(i => new InvoiceItem
                {
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount,
                    TaxAmount = i.TaxAmount
                }).ToList(),
                cancellationToken);

            TempData["StatusMessage"] = "Invoice created as Draft. Add more items or activate it to accept payments.";
            return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Customer = customer;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(long invoiceId, InvoiceItemInputModel item, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid item — check quantity, unit price, discount and tax.";
            return RedirectToAction(nameof(Details), new { id = invoiceId });
        }

        try
        {
            await _invoiceService.AddItemAsync(invoiceId, new InvoiceItem
            {
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxAmount = item.TaxAmount
            }, cancellationToken);

            TempData["StatusMessage"] = "Item added.";
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = invoiceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(long invoiceId, long invoiceItemId, CancellationToken cancellationToken)
    {
        try
        {
            await _invoiceService.RemoveItemAsync(invoiceId, invoiceItemId, cancellationToken);
            TempData["StatusMessage"] = "Item removed.";
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = invoiceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(long id, CancellationToken cancellationToken)
    {
        try
        {
            await _invoiceService.ActivateAsync(id, cancellationToken);
            TempData["StatusMessage"] = "Invoice activated — it can now accept payments.";
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id, CancellationToken cancellationToken)
    {
        try
        {
            await _invoiceService.CancelAsync(id, cancellationToken);
            TempData["StatusMessage"] = "Invoice cancelled.";
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
