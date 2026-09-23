using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;
using MpdApply.Models;
using System.ComponentModel.DataAnnotations;

namespace MpdApply.Pages.Apply;

public class Step1Model : PageModel
{
    private readonly AppDbContext _db;
    public Step1Model(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string PositionAppliedFor { get; set; } = "";
        [Required] public string HowDidYouHear { get; set; } = "";
        public string? HowDidYouHearDetail { get; set; }
        [Required] public string LastName { get; set; } = "";
        [Required] public string FirstName { get; set; } = "";
        public string? MiddleInitial { get; set; }
        public string? Nickname { get; set; }
        public string? MaidenName { get; set; }
        [Required] public string StreetAddress { get; set; } = "";
        [Required] public string City { get; set; } = "";
        [Required] public string State { get; set; } = "";
        [Required] public string Zip { get; set; } = "";
        public string? County { get; set; }
        public bool MailingDifferent { get; set; }
        public string? MailingStreet { get; set; }
        public string? MailingCity { get; set; }
        public string? MailingState { get; set; }
        public string? MailingZip { get; set; }
        [Required] public string DateOfBirth { get; set; } = "";
        [Required] public string SSN { get; set; } = "";
        public string? DlState { get; set; }
        public string? DlNumber { get; set; }
        public string? HomePhone { get; set; }
        public string? WorkPhone { get; set; }
        public string? CellPhone { get; set; }
        [Required, EmailAddress] public string Email { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var app = new ApplicationSubmission
        {
            ApplicationDate = DateTime.Today,
            PositionAppliedFor = Input.PositionAppliedFor,
            HowDidYouHear = Input.HowDidYouHear,
            HowDidYouHearDetail = Input.HowDidYouHearDetail,
            LastName = Input.LastName,
            FirstName = Input.FirstName,
            MiddleInitial = Input.MiddleInitial,
            Nickname = Input.Nickname,
            MaidenName = Input.MaidenName,
            StreetAddress = Input.StreetAddress,
            City = Input.City,
            State = Input.State,
            Zip = Input.Zip,
            County = Input.County,
            MailingDifferent = Input.MailingDifferent,
            MailingStreet = Input.MailingStreet,
            MailingCity = Input.MailingCity,
            MailingState = Input.MailingState,
            MailingZip = Input.MailingZip,
            DateOfBirth = DateTime.TryParse(Input.DateOfBirth, out var dob) ? dob : null,
            SSN = Input.SSN,
            DlState = Input.DlState,
            DlNumber = Input.DlNumber,
            HomePhone = Input.HomePhone,
            WorkPhone = Input.WorkPhone,
            CellPhone = Input.CellPhone,
            Email = Input.Email,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        _db.Applications.Add(app);
        await _db.SaveChangesAsync();

        HttpContext.Session.SetString("AppId", app.Id.ToString());
        return RedirectToPage("Step2");
    }
}
