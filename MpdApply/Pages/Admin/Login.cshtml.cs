using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace MpdApply.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly IConfiguration _config;
    public LoginModel(IConfiguration config) => _config = config;

    [BindProperty, Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? Error { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        var adminPassword = _config["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            Error = "Admin access is not configured. Set the Admin:Password environment variable in Railway.";
            return Page();
        }

        if (Password != adminPassword)
        {
            Error = "Incorrect password.";
            return Page();
        }

        HttpContext.Session.SetString("AdminAuth", "true");
        return RedirectToPage("/Admin/Submissions");
    }

    public static IActionResult? RequireAuth(PageModel page)
    {
        if (page.HttpContext.Session.GetString("AdminAuth") != "true")
            return page.RedirectToPage("/Admin/Login");
        return null;
    }
}
