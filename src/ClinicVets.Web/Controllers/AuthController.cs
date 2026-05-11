using ClinicVets.Application.Services;
using ClinicVets.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicVets.Web.Controllers;

public class AuthController : Controller
{
    private readonly EmployeeAuthenticationService _employeeAuthenticationService;

    public AuthController(EmployeeAuthenticationService employeeAuthenticationService)
    {
        _employeeAuthenticationService = employeeAuthenticationService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please enter a valid email and password.";
            return View(model);
        }

        var result = await _employeeAuthenticationService.LoginAsync(model.Email, model.Password);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        TempData["SuccessMessage"] = $"Welcome, {result.Employee!.FullName}.";
        return RedirectToAction("Dashboard", "Home");
    }
}
