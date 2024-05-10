using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Administrator)]
public class BranchesController : Controller
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    public async Task<IActionResult> Index(string? search, bool includeInactive, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        var result = await _branchService.GetPagedAsync(search, includeInactive, pageNumber, pageSize: 15, cancellationToken);
        ViewData["route_search"] = search;
        ViewData["route_includeInactive"] = includeInactive;
        ViewData["search"] = search;
        ViewData["includeInactive"] = includeInactive;
        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetByIdAsync(id, cancellationToken);
        if (branch is null)
        {
            return NotFound();
        }

        return View(branch);
    }

    public IActionResult Create() => View(new BranchFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BranchFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _branchService.CreateAsync(new Branch
            {
                BranchCode = model.BranchCode,
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City
            }, cancellationToken);

            TempData["StatusMessage"] = "Branch created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetByIdAsync(id, cancellationToken);
        if (branch is null)
        {
            return NotFound();
        }

        return View(new BranchFormViewModel
        {
            BranchId = branch.BranchId,
            BranchCode = branch.BranchCode,
            Name = branch.Name,
            Email = branch.Email,
            PhoneNumber = branch.PhoneNumber,
            Address = branch.Address,
            City = branch.City
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BranchFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.BranchId || !ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _branchService.UpdateAsync(new Branch
            {
                BranchId = model.BranchId,
                BranchCode = model.BranchCode,
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City
            }, cancellationToken);

            TempData["StatusMessage"] = "Branch updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _branchService.DeactivateAsync(id, cancellationToken);
        TempData["StatusMessage"] = "Branch deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(int id, CancellationToken cancellationToken)
    {
        await _branchService.ReactivateAsync(id, cancellationToken);
        TempData["StatusMessage"] = "Branch reactivated.";
        return RedirectToAction(nameof(Index));
    }
}
