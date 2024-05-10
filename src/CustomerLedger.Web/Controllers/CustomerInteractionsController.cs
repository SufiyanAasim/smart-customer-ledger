using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Controllers;

[Authorize]
public class CustomerInteractionsController : Controller
{
    private readonly ICustomerInteractionService _interactionService;
    private readonly ICustomerService _customerService;

    public CustomerInteractionsController(ICustomerInteractionService interactionService, ICustomerService customerService)
    {
        _interactionService = interactionService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(int? branchId, int? customerId, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        var result = await _interactionService.GetPagedAsync(branchId, customerId, pageNumber, pageSize: 15, cancellationToken);
        return View(result);
    }

    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        var interaction = await _interactionService.GetByIdAsync(id, cancellationToken);
        if (interaction is null)
        {
            return NotFound();
        }

        return View(interaction);
    }

    public async Task<IActionResult> Create(int customerId, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        return View(new CustomerInteractionFormViewModel { CustomerId = customerId, Customer = customer });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerInteractionFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Customer = await _customerService.GetByIdAsync(model.CustomerId, cancellationToken);
            return View(model);
        }

        try
        {
            var interaction = await _interactionService.CreateAsync(new CustomerInteraction
            {
                CustomerId = model.CustomerId,
                InteractionType = model.InteractionType,
                Subject = model.Subject,
                Description = model.Description,
                InteractionDate = model.InteractionDate,
                FollowUpDate = model.FollowUpDate,
                Status = model.Status
            }, cancellationToken);

            TempData["StatusMessage"] = "Interaction recorded.";
            return RedirectToAction(nameof(Details), new { id = interaction.CustomerInteractionId });
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Customer = await _customerService.GetByIdAsync(model.CustomerId, cancellationToken);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var interaction = await _interactionService.GetByIdAsync(id, cancellationToken);
        if (interaction is null)
        {
            return NotFound();
        }

        return View(new CustomerInteractionFormViewModel
        {
            CustomerInteractionId = interaction.CustomerInteractionId,
            CustomerId = interaction.CustomerId,
            Customer = interaction.Customer,
            InteractionType = interaction.InteractionType,
            Subject = interaction.Subject,
            Description = interaction.Description,
            InteractionDate = interaction.InteractionDate,
            FollowUpDate = interaction.FollowUpDate,
            Status = interaction.Status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, CustomerInteractionFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.CustomerInteractionId || !ModelState.IsValid)
        {
            model.Customer = await _customerService.GetByIdAsync(model.CustomerId, cancellationToken);
            return View(model);
        }

        try
        {
            await _interactionService.UpdateAsync(new CustomerInteraction
            {
                CustomerInteractionId = model.CustomerInteractionId,
                InteractionType = model.InteractionType,
                Subject = model.Subject,
                Description = model.Description,
                InteractionDate = model.InteractionDate,
                FollowUpDate = model.FollowUpDate,
                Status = model.Status
            }, cancellationToken);

            TempData["StatusMessage"] = "Interaction updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Customer = await _customerService.GetByIdAsync(model.CustomerId, cancellationToken);
            return View(model);
        }
    }
}
