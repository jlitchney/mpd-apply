using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;
using MpdApply.Models;
using System.ComponentModel.DataAnnotations;

namespace MpdApply.Pages.Settings;

public class IndexModel(AppDbContext db) : PageModel
{
    [BindProperty, Required, EmailAddress] public string RecipientEmail { get; set; } = "";
    [BindProperty, Required]              public string AgencyName { get; set; } = "";
    [BindProperty]                        public IFormFile? LogoFile { get; set; }

    public string? CurrentLogoBase64 { get; set; }
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        RecipientEmail = (await db.Settings.FindAsync("RecipientEmail"))?.Value ?? "";
        AgencyName     = (await db.Settings.FindAsync("AgencyName"))?.Value ?? "";
        CurrentLogoBase64 = (await db.Settings.FindAsync("LogoBase64"))?.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        if (!ModelState.IsValid)
        {
            CurrentLogoBase64 = (await db.Settings.FindAsync("LogoBase64"))?.Value;
            return Page();
        }

        await Upsert("RecipientEmail", RecipientEmail);
        await Upsert("AgencyName", AgencyName);

        if (LogoFile != null && LogoFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await LogoFile.CopyToAsync(ms);
            var b64 = Convert.ToBase64String(ms.ToArray());
            var mime = LogoFile.ContentType;
            await Upsert("LogoBase64", $"data:{mime};base64,{b64}");
        }

        await db.SaveChangesAsync();
        Message = "Settings saved.";
        CurrentLogoBase64 = (await db.Settings.FindAsync("LogoBase64"))?.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostClearLogoAsync()
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;
        var setting = await db.Settings.FindAsync("LogoBase64");
        if (setting != null) db.Settings.Remove(setting);
        await db.SaveChangesAsync();
        Message = "Logo removed.";
        return RedirectToPage();
    }

    private async Task Upsert(string key, string value)
    {
        var s = await db.Settings.FindAsync(key);
        if (s == null) db.Settings.Add(new AppSetting { Key = key, Value = value });
        else s.Value = value;
    }
}
