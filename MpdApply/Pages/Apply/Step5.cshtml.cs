using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MpdApply.Data;
using MpdApply.Services;

namespace MpdApply.Pages.Apply;

public class Step5Model : PageModel
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;

    public Step5Model(AppDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    [BindProperty] public string? Gender { get; set; }
    [BindProperty] public string? EeoGroup { get; set; }
    [BindProperty] public bool VeteranProtectedCategory { get; set; }
    [BindProperty] public bool VeteranNotProtected { get; set; }
    [BindProperty] public string? BackgroundSignatureData { get; set; }
    [BindProperty] public string? VeteranSignatureData { get; set; }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("AppId") == null) return RedirectToPage("Step1");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var idStr = HttpContext.Session.GetString("AppId");
        if (idStr == null) return RedirectToPage("Step1");
        var app = await _db.Applications.FindAsync(Guid.Parse(idStr));
        if (app == null) return RedirectToPage("Step1");

        if (string.IsNullOrWhiteSpace(BackgroundSignatureData) || BackgroundSignatureData == "data:,")
        {
            TempData["Error"] = "A signature is required for the background check authorization.";
            return Page();
        }

        app.Gender = Gender;
        app.EeoGroup = EeoGroup;
        app.VeteranProtectedCategory = VeteranProtectedCategory;
        app.VeteranNotProtected = VeteranNotProtected;
        app.BackgroundSignatureData = BackgroundSignatureData;
        app.BackgroundSignedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(VeteranSignatureData) && VeteranSignatureData != "data:,")
        {
            app.VeteranSignatureData = VeteranSignatureData;
            app.VeteranSignedAt = DateTime.UtcNow;
        }
        app.Status = "Submitted";
        app.SubmittedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Generate PDF and email
        try
        {
            var recipient = await _db.Settings.FindAsync("RecipientEmail");
            var recipientEmail = recipient?.Value ?? "jason@allstartalent.us";
            var pdf = PdfGenerator.Generate(app);
            await _email.SendApplicationAsync(recipientEmail, app.FullName, pdf);
        }
        catch (Exception ex)
        {
            // Log but don't fail — submission is saved
            HttpContext.RequestServices.GetRequiredService<ILogger<Step5Model>>()
                .LogError(ex, "Failed to send application email for {Id}", app.Id);
        }

        HttpContext.Session.Remove("AppId");
        return RedirectToPage("Confirmation", new { name = app.FirstName });
    }
}
