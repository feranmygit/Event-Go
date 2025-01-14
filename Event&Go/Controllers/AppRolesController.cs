using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Event_Go.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AppRolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppRolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // List All Roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        // Create Role (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Create Role (POST)
        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            // Avoid duplicate Roles
            if (!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }

            TempData["success"] = "New role created successfully.";
            return RedirectToAction("index");
        }

        // Edit Role (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return View("Error");

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return View("Error");

            return View(role);
        }

        // Edit Role (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(string id, string roleName)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(roleName))
                return View("Error");

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return View("Error");

            role.Name = roleName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                TempData["success"] = "Role updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error updating role.");
                return View(role);
            }
        }

        // Delete Role
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["success"] = "Role deleted successfully.";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Error deleting role.";
            return RedirectToAction("Index");
        }

    }
}
