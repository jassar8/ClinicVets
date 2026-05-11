using ClinicVets.Application.Services;
using ClinicVets.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicVets.Web.Controllers;

public class EmployeesController : Controller
{
    private readonly EmployeeRegistrationService _employeeRegistrationService;

    public EmployeesController(EmployeeRegistrationService employeeRegistrationService)
    {
        _employeeRegistrationService = employeeRegistrationService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterEmployeeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please complete all required fields correctly.";
            return View(model);
        }

        var result = await _employeeRegistrationService.RegisterAsync(
            model.FullName,
            model.Email,
            model.Password,
            model.Role);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Register");
    }
}
