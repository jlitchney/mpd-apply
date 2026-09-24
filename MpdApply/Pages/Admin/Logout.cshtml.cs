using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MpdApply.Pages.Admin;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        HttpContext.Session.Remove("AdminAuth");
        return RedirectToPage("/Admin/Login");
    }
}
