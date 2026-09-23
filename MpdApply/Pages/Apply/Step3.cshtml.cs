using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;

namespace MpdApply.Pages.Apply;

public class Step3Model : PageModel
{
    private readonly AppDbContext _db;
    public Step3Model(AppDbContext db) => _db = db;

    [BindProperty] public string? InitCoptCitizenship { get; set; }
    [BindProperty] public string? InitCopt18 { get; set; }
    [BindProperty] public string? InitCopt21 { get; set; }
    [BindProperty] public string? InitCoptSenses { get; set; }
    [BindProperty] public string? InitCoptVision { get; set; }
    [BindProperty] public string? InitCoptColorVision { get; set; }
    [BindProperty] public string? InitCoptHearing { get; set; }
    [BindProperty] public string? InitCoptNoCommunicable { get; set; }
    [BindProperty] public string? InitCoptNoDeformity { get; set; }
    [BindProperty] public string? InitCoptDrugScreen { get; set; }
    [BindProperty] public string? InitCoptWeight { get; set; }
    [BindProperty] public string? InitCoptMilitary { get; set; }
    [BindProperty] public string? InitCoptNoFelony { get; set; }
    [BindProperty] public string? InitCoptDl { get; set; }
    [BindProperty] public string? InitEduStandard { get; set; }
    [BindProperty] public string? EduStandardMet { get; set; }

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

        app.InitCoptCitizenship = InitCoptCitizenship;
        app.InitCopt18 = InitCopt18;
        app.InitCopt21 = InitCopt21;
        app.InitCoptSenses = InitCoptSenses;
        app.InitCoptVision = InitCoptVision;
        app.InitCoptColorVision = InitCoptColorVision;
        app.InitCoptHearing = InitCoptHearing;
        app.InitCoptNoCommunicable = InitCoptNoCommunicable;
        app.InitCoptNoDeformity = InitCoptNoDeformity;
        app.InitCoptDrugScreen = InitCoptDrugScreen;
        app.InitCoptWeight = InitCoptWeight;
        app.InitCoptMilitary = InitCoptMilitary;
        app.InitCoptNoFelony = InitCoptNoFelony;
        app.InitCoptDl = InitCoptDl;
        app.InitEduStandard = InitEduStandard;
        app.EduStandardMet = EduStandardMet;

        await _db.SaveChangesAsync();
        return RedirectToPage("Step4");
    }
}
