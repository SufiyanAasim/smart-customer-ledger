using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Web.Areas.Admin.Controllers;

/// <summary>
/// User (employee) management. There is no public self-registration — every account is
/// created here by an Administrator and assigned exactly one role and (unless
/// Administrator) exactly one branch.
/// </summary>
[Area("Admin")]
[Authorize(Roles = Roles.Administrator)]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBranchService _branchService;

    public UsersController(UserManager<ApplicationUser> userManager, IBranchService branchService)
    {
        _userManager = userManager;
        _branchService = branchService;
    }

    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                u.FullName.Contains(search) ||
                u.Email!.Contains(search) ||
                u.EmployeeCode.Contains(search));
        }

        var users = await query.OrderBy(u => u.FullName).ToListAsync(cancellationToken);
        ViewData["search"] = search;
        return View(users);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new UserFormViewModel
        {
            AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken),
            AvailableRoles = Roles.All
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "A password is required for a new account.");
        }

        if (model.Role != Roles.Administrator && model.BranchId is null)
        {
            ModelState.AddModelError(nameof(model.BranchId), "A branch is required for non-administrator accounts.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken);
            model.AvailableRoles = Roles.All;
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.FullName,
            EmployeeCode = model.EmployeeCode,
            BranchId = model.Role == Roles.Administrator ? null : model.BranchId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken);
            model.AvailableRoles = Roles.All;
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);

        TempData["StatusMessage"] = "User account created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            EmployeeCode = user.EmployeeCode,
            BranchId = user.BranchId,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive,
            AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken),
            AvailableRoles = Roles.All
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormViewModel model, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (model.Role != Roles.Administrator && model.BranchId is null)
        {
            ModelState.AddModelError(nameof(model.BranchId), "A branch is required for non-administrator accounts.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableBranches = await _branchService.GetAllActiveAsync(cancellationToken);
            model.AvailableRoles = Roles.All;
            return View(model);
        }

        user.FullName = model.FullName;
        user.EmployeeCode = model.EmployeeCode;
        user.BranchId = model.Role == Roles.Administrator ? null : model.BranchId;
        user.IsActive = model.IsActive;

        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(model.Role))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, model.Password);
        }

        TempData["StatusMessage"] = "User account updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is not null)
        {
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
        }

        TempData["StatusMessage"] = "User account deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is not null)
        {
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
        }

        TempData["StatusMessage"] = "User account reactivated.";
        return RedirectToAction(nameof(Index));
    }
}
