using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;

namespace MpdApply.Pages.Apply;

public class Step4Model : PageModel
{
    private readonly AppDbContext _db;
    public Step4Model(AppDbContext db) => _db = db;

    [BindProperty] public string? InitCriminal1 { get; set; }
    [BindProperty] public string? InitCriminal2 { get; set; }
    [BindProperty] public string? InitCriminal3 { get; set; }
    [BindProperty] public string? InitCriminal4 { get; set; }
    [BindProperty] public string? InitCriminal5 { get; set; }
    [BindProperty] public string? InitCriminal6 { get; set; }
    [BindProperty] public string? InitDrug1 { get; set; }
    [BindProperty] public string? InitDrug2 { get; set; }
    [BindProperty] public string? InitDrug3 { get; set; }
    [BindProperty] public string? InitDrug4 { get; set; }
    [BindProperty] public string? InitDrug5 { get; set; }
    [BindProperty] public string? InitDrug6 { get; set; }
    [BindProperty] public string? InitDrug7 { get; set; }
    [BindProperty] public string? InitDrug8 { get; set; }
    [BindProperty] public string? InitDriving1 { get; set; }
    [BindProperty] public string? InitDriving2 { get; set; }
    [BindProperty] public string? InitDriving3 { get; set; }
    [BindProperty] public string? InitDriving4 { get; set; }
    [BindProperty] public string? InitDriving5 { get; set; }
    [BindProperty] public string? InitDriving6 { get; set; }
    [BindProperty] public string? InitDriving7 { get; set; }
    [BindProperty] public string? InitDriving8 { get; set; }
    [BindProperty] public string? InitDriving9 { get; set; }
    [BindProperty] public string? InitAck1 { get; set; }
    [BindProperty] public string? InitAck2 { get; set; }
    [BindProperty] public string? InitAck3 { get; set; }

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

        app.InitCriminal1 = InitCriminal1; app.InitCriminal2 = InitCriminal2;
        app.InitCriminal3 = InitCriminal3; app.InitCriminal4 = InitCriminal4;
        app.InitCriminal5 = InitCriminal5; app.InitCriminal6 = InitCriminal6;
        app.InitDrug1 = InitDrug1; app.InitDrug2 = InitDrug2; app.InitDrug3 = InitDrug3;
        app.InitDrug4 = InitDrug4; app.InitDrug5 = InitDrug5; app.InitDrug6 = InitDrug6;
        app.InitDrug7 = InitDrug7; app.InitDrug8 = InitDrug8;
        app.InitDriving1 = InitDriving1; app.InitDriving2 = InitDriving2;
        app.InitDriving3 = InitDriving3; app.InitDriving4 = InitDriving4;
        app.InitDriving5 = InitDriving5; app.InitDriving6 = InitDriving6;
        app.InitDriving7 = InitDriving7; app.InitDriving8 = InitDriving8;
        app.InitDriving9 = InitDriving9;
        app.InitAck1 = InitAck1; app.InitAck2 = InitAck2; app.InitAck3 = InitAck3;

        await _db.SaveChangesAsync();
        return RedirectToPage("Step5");
    }
}
