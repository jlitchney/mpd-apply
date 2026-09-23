namespace MpdApply.Models;

public class ApplicationSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Status { get; set; } = "Draft"; // Draft, Submitted
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public string? IpAddress { get; set; }

    // Step 1 – Personal Information
    public DateTime ApplicationDate { get; set; } = DateTime.Today;
    public string PositionAppliedFor { get; set; } = "";
    public string HowDidYouHear { get; set; } = "";
    public string? HowDidYouHearDetail { get; set; }
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? MiddleInitial { get; set; }
    public string? Nickname { get; set; }
    public string? MaidenName { get; set; }
    public string StreetAddress { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
    public string? County { get; set; }
    public bool MailingDifferent { get; set; }
    public string? MailingStreet { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingZip { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? SSN { get; set; }
    public string? DlState { get; set; }
    public string? DlNumber { get; set; }
    public string? HomePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? CellPhone { get; set; }
    public string Email { get; set; } = "";

    // Step 2 – Employment & Experience
    public bool? EmployedNow { get; set; }
    public bool? MayContactEmployer { get; set; }
    public string? CurrentEmployer { get; set; }
    public string? CurrentPosition { get; set; }
    public string? AvailableDate { get; set; }
    public bool? PreviouslyApplied { get; set; }
    public string? PreviousApplicationDetails { get; set; }
    public string PoliceExperience { get; set; } = "None";
    public string? PoliceDepartment { get; set; }
    public string? PoliceLength { get; set; }
    public string? PoliceRank { get; set; }
    public string? PoliceReasonLeaving { get; set; }
    public bool ArmedForces { get; set; }
    public bool ActiveDuty { get; set; }
    public bool Reserve { get; set; }
    public string? ArmedForcesLength { get; set; }
    public string? ArmedForcesBranch { get; set; }
    public string? HonorableDischarge { get; set; }
    public string? Licenses { get; set; }

    // Step 3 – COPT Standards (initials per item)
    public string? InitCoptCitizenship { get; set; }
    public string? InitCopt18 { get; set; }
    public string? InitCopt21 { get; set; }
    public string? InitCoptSenses { get; set; }
    public string? InitCoptVision { get; set; }
    public string? InitCoptColorVision { get; set; }
    public string? InitCoptHearing { get; set; }
    public string? InitCoptNoCommunicable { get; set; }
    public string? InitCoptNoDeformity { get; set; }
    public string? InitCoptDrugScreen { get; set; }
    public string? InitCoptWeight { get; set; }
    public string? InitCoptMilitary { get; set; }
    public string? InitCoptNoFelony { get; set; }
    public string? InitCoptDl { get; set; }
    // Education standard
    public string? InitEduStandard { get; set; }
    public string? EduStandardMet { get; set; }

    // Step 4 – Disqualification Acknowledgements (initials per group)
    public string? InitCriminal1 { get; set; }
    public string? InitCriminal2 { get; set; }
    public string? InitCriminal3 { get; set; }
    public string? InitCriminal4 { get; set; }
    public string? InitCriminal5 { get; set; }
    public string? InitCriminal6 { get; set; }
    public string? InitDrug1 { get; set; }
    public string? InitDrug2 { get; set; }
    public string? InitDrug3 { get; set; }
    public string? InitDrug4 { get; set; }
    public string? InitDrug5 { get; set; }
    public string? InitDrug6 { get; set; }
    public string? InitDrug7 { get; set; }
    public string? InitDrug8 { get; set; }
    public string? InitDriving1 { get; set; }
    public string? InitDriving2 { get; set; }
    public string? InitDriving3 { get; set; }
    public string? InitDriving4 { get; set; }
    public string? InitDriving5 { get; set; }
    public string? InitDriving6 { get; set; }
    public string? InitDriving7 { get; set; }
    public string? InitDriving8 { get; set; }
    public string? InitDriving9 { get; set; }
    public string? InitAck1 { get; set; }
    public string? InitAck2 { get; set; }
    public string? InitAck3 { get; set; }
    // Background check signature
    public string? BackgroundSignatureData { get; set; }
    public DateTime? BackgroundSignedAt { get; set; }

    // Step 5 – EEO & Veteran Status (voluntary)
    public string? Gender { get; set; }
    public string? EeoGroup { get; set; }
    public bool VeteranProtectedCategory { get; set; }
    public bool VeteranNotProtected { get; set; }
    public string? VeteranSignatureData { get; set; }
    public DateTime? VeteranSignedAt { get; set; }

    public string FullName => $"{FirstName} {MiddleInitial} {LastName}".Replace("  ", " ").Trim();
}
