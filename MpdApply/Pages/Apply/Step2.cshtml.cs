using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MpdApply.Data;

namespace MpdApply.Pages.Apply;

public class Step2Model : PageModel
{
    private readonly AppDbContext _db;
    public Step2Model(AppDbContext db) => _db = db;

    [BindProperty] public bool? EmployedNow { get; set; }
    [BindProperty] public bool? MayContactEmployer { get; set; }
    [BindProperty] public string? CurrentEmployer { get; set; }
    [BindProperty] public string? CurrentPosition { get; set; }
    [BindProperty] public string? AvailableDate { get; set; }
    [BindProperty] public bool? PreviouslyApplied { get; set; }
    [BindProperty] public string? PreviousApplicationDetails { get; set; }
    [BindProperty] public string PoliceExperience { get; set; } = "None";
    [BindProperty] public string? PoliceDepartment { get; set; }
    [BindProperty] public string? PoliceLength { get; set; }
    [BindProperty] public string? PoliceRank { get; set; }
    [BindProperty] public string? PoliceReasonLeaving { get; set; }
    [BindProperty] public bool ArmedForces { get; set; }
    [BindProperty] public bool ActiveDuty { get; set; }
    [BindProperty] public bool Reserve { get; set; }
    [BindProperty] public string? ArmedForcesLength { get; set; }
    [BindProperty] public string? ArmedForcesBranch { get; set; }
    [BindProperty] public string? HonorableDischarge { get; set; }
    [BindProperty] public string? Licenses { get; set; }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("AppId") == null)
            return RedirectToPage("Step1");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var idStr = HttpContext.Session.GetString("AppId");
        if (idStr == null) return RedirectToPage("Step1");
        var app = await _db.Applications.FindAsync(Guid.Parse(idStr));
        if (app == null) return RedirectToPage("Step1");

        app.EmployedNow = EmployedNow;
        app.MayContactEmployer = MayContactEmployer;
        app.CurrentEmployer = CurrentEmployer;
        app.CurrentPosition = CurrentPosition;
        app.AvailableDate = AvailableDate;
        app.PreviouslyApplied = PreviouslyApplied;
        app.PreviousApplicationDetails = PreviousApplicationDetails;
        app.PoliceExperience = PoliceExperience;
        app.PoliceDepartment = PoliceDepartment;
        app.PoliceLength = PoliceLength;
        app.PoliceRank = PoliceRank;
        app.PoliceReasonLeaving = PoliceReasonLeaving;
        app.ArmedForces = ArmedForces;
        app.ActiveDuty = ActiveDuty;
        app.Reserve = Reserve;
        app.ArmedForcesLength = ArmedForcesLength;
        app.ArmedForcesBranch = ArmedForcesBranch;
        app.HonorableDischarge = HonorableDischarge;
        app.Licenses = Licenses;

        await _db.SaveChangesAsync();
        return RedirectToPage("Step3");
    }
}
