using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;

namespace MpdApply.Pages.Forms;

public class ConfirmationModel(AppDbContext db) : PageModel
{
    public string? LogoBase64 { get; private set; }
    public string? AgencyName { get; private set; }

    public async Task OnGetAsync()
    {
        LogoBase64 = (await db.Settings.FindAsync("LogoBase64"))?.Value;
        AgencyName = (await db.Settings.FindAsync("AgencyName"))?.Value;
    }
}
