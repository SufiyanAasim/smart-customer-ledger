using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerLedger.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IBranchService _branchService;
    private readonly ICurrentUserContext _currentUser;
    private readonly IImportService _importService;

    public CustomersController(ICustomerService customerService, IBranchService branchService, ICurrentUserContext currentUser, IImportService importService)
    {
        _importService = importService;
        _customerService = customerService;
        _branchService = branchService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(int? branchId, string? search, string? status, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetPagedAsync(branchId, search, status, pageNumber, pageSize: 15, cancellationToken);
        ViewData["route_search"] = search;
        ViewData["route_status"] = status;
        ViewData["search"] = search;
        ViewData["status"] = status;
        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id, cancellationToken);
            if (customer is null)
            {
                return NotFound();
            }

            return View(customer);
        }
        catch (BranchAccessDeniedException)
        {
            return Forbid();
        }
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CustomerFormViewModel
        {
            AvailableBranches = _currentUser.IsAdministrator
                ? await _branchService.GetAllActiveAsync(cancellationToken)
                : (await _branchService.GetAllActiveAsync(cancellationToken)).Where(b => b.BranchId == _currentUser.BranchId).ToList(),
            BranchId = _currentUser.BranchId ?? 0
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken);
            return View(model);
        }

        try
        {
            var customer = await _customerService.CreateAsync(new Customer
            {
                BranchId = model.BranchId,
                CustomerCode = model.CustomerCode,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                CNIC = model.CNIC,
                Address = model.Address,
                City = model.City
            }, model.InitialCreditLimit, cancellationToken);

            TempData["StatusMessage"] = "Customer registered successfully.";
            return RedirectToAction(nameof(Details), new { id = customer.CustomerId });
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id, cancellationToken);
            if (customer is null)
            {
                return NotFound();
            }

            return View(new CustomerFormViewModel
            {
                CustomerId = customer.CustomerId,
                BranchId = customer.BranchId,
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                CNIC = customer.CNIC,
                Address = customer.Address,
                City = customer.City,
                AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken)
            });
        }
        catch (BranchAccessDeniedException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.CustomerId || !ModelState.IsValid)
        {
            model.AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken);
            return View(model);
        }

        try
        {
            await _customerService.UpdateAsync(new Customer
            {
                CustomerId = model.CustomerId,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                CNIC = model.CNIC,
                Address = model.Address,
                City = model.City
            }, cancellationToken);

            TempData["StatusMessage"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.DeactivateAsync(id, cancellationToken);
            TempData["StatusMessage"] = "Customer deactivated.";
        }
        catch (BranchAccessDeniedException)
        {
            return Forbid();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Import(CancellationToken cancellationToken)
    {
        ViewBag.Branches = _currentUser.IsAdministrator
            ? await _branchService.GetAllActiveAsync(cancellationToken)
            : (await _branchService.GetAllActiveAsync(cancellationToken)).Where(b => b.BranchId == _currentUser.BranchId).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(int branchId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Choose a CSV file to preview.";
            return RedirectToAction(nameof(Import));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _importService.PreviewCustomerImportAsync(branchId, stream, cancellationToken);
            ViewBag.BranchId = branchId;
            return View("ImportResult", result);
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Import));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmImport(int branchId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Choose the same CSV file again to confirm the import.";
            return RedirectToAction(nameof(Import));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _importService.ImportCustomersAsync(branchId, stream, cancellationToken);
            ViewBag.BranchId = branchId;
            return View("ImportResult", result);
        }
        catch (Exception ex) when (ex is BusinessRuleException or BranchAccessDeniedException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Import));
        }
    }
}
