using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MpdApply.Data;
using MpdApply.Models;
using MpdApply.Services;

namespace MpdApply.Pages.Admin;

public class SubmissionsModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;

    public SubmissionsModel(AppDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    public List<ApplicationSubmission> Applications { get; set; } = new();

    public async Task OnGetAsync()
    {
        Applications = await _db.Applications
            .Where(a => a.Status == "Submitted")
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostResendAsync(Guid id)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app == null) return NotFound();

        var recipient = await _db.Settings.FindAsync("RecipientEmail");
        var pdf = PdfGenerator.Generate(app);
        await _email.SendApplicationAsync(recipient?.Value ?? "jason@allstartalent.us", app.FullName, pdf);

        TempData["Message"] = $"Resent application for {app.FullName}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadAsync(Guid id)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app == null) return NotFound();

        var pdf = PdfGenerator.Generate(app);
        return File(pdf, "application/pdf", $"Application_{app.LastName}_{app.FirstName}.pdf");
    }
}
