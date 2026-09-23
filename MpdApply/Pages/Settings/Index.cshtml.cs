using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;
using MpdApply.Models;
using System.ComponentModel.DataAnnotations;

namespace MpdApply.Pages.Settings;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty, Required, EmailAddress]
    public string RecipientEmail { get; set; } = "";

    [BindProperty, Required]
    public string AgencyName { get; set; } = "";

    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        var email = await _db.Settings.FindAsync("RecipientEmail");
        var agency = await _db.Settings.FindAsync("AgencyName");
        RecipientEmail = email?.Value ?? "";
        AgencyName = agency?.Value ?? "";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await Upsert("RecipientEmail", RecipientEmail);
        await Upsert("AgencyName", AgencyName);
        await _db.SaveChangesAsync();
        Message = "Settings saved.";
        return Page();
    }

    private async Task Upsert(string key, string value)
    {
        var setting = await _db.Settings.FindAsync(key);
        if (setting == null)
            _db.Settings.Add(new AppSetting { Key = key, Value = value });
        else
            setting.Value = value;
    }
}
