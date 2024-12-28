using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class AccountsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountsController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> BecomeOrganizer()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Remove the "User" role if it exists
        if (await _userManager.IsInRoleAsync(user, "User"))
        {
            await _userManager.RemoveFromRoleAsync(user, "User");
        }

        // Add the "Organizer" role
        if (!await _roleManager.RoleExistsAsync("Organizer"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Organizer"));
        }

        await _userManager.AddToRoleAsync(user, "Organizer");

        TempData["success"] = "You are now an Organizer, Kindly logout and login again!";
        return RedirectToAction("Index", "Home");
    }
}
