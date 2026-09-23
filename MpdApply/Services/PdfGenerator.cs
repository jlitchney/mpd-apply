using MpdApply.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MpdApply.Services;

public static class PdfGenerator
{
    public static byte[] Generate(ApplicationSubmission a)
    {
        byte[]? initImg = null;
        if (a.InitialsMode == "drawn" && !string.IsNullOrWhiteSpace(a.InitialsImageData))
        {
            var raw = a.InitialsImageData.Replace("data:image/png;base64,", "");
            initImg = Convert.FromBase64String(raw);
        }

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(0.75f, Unit.Inch);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(h =>
                {
                    h.Item().AlignCenter().Text("Town of Middletown Police Department").FontSize(14).Bold();
                    h.Item().AlignCenter().Text("Employment Application").FontSize(12).Bold();
                    h.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    // ── Section 1: Personal Info ──
                    Section(col, "Personal Information");
                    TwoCol(col,
                        $"Date of Application: {a.ApplicationDate:MM/dd/yyyy}",
                        $"Position Applied For: {a.PositionAppliedFor}");
                    Field(col, "How Did You Hear About Us", a.HowDidYouHear + (string.IsNullOrEmpty(a.HowDidYouHearDetail) ? "" : $" — {a.HowDidYouHearDetail}"));
                    TwoCol(col,
                        $"Last Name: {a.LastName}",
                        $"First Name: {a.FirstName}");
                    TwoCol(col,
                        $"Middle Initial: {a.MiddleInitial}",
                        $"Nickname: {a.Nickname}   Maiden: {a.MaidenName}");
                    Field(col, "Residence Street", a.StreetAddress);
                    TwoCol(col,
                        $"City: {a.City}   State: {a.State}   Zip: {a.Zip}",
                        $"County: {a.County}");
                    if (a.MailingDifferent)
                    {
                        Field(col, "Mailing Address", $"{a.MailingStreet}, {a.MailingCity}, {a.MailingState} {a.MailingZip}");
                    }
                    TwoCol(col,
                        $"Date of Birth: {a.DateOfBirth:MM/dd/yyyy}",
                        $"SSN: {a.SSN}");
                    TwoCol(col,
                        $"Driver's License State: {a.DlState}   Number: {a.DlNumber}",
                        $"Home Phone: {a.HomePhone}");
                    TwoCol(col,
                        $"Cell Phone: {a.CellPhone}",
                        $"Work Phone: {a.WorkPhone}");
                    Field(col, "Email Address", a.Email);

                    // ── Section 2: Employment ──
                    Section(col, "Employment & Experience");
                    TwoCol(col,
                        $"Currently Employed: {(a.EmployedNow == true ? "Yes" : "No")}",
                        $"May Contact Employer: {(a.MayContactEmployer == true ? "Yes" : "No")}");
                    TwoCol(col,
                        $"Current Employer: {a.CurrentEmployer}",
                        $"Position: {a.CurrentPosition}");
                    TwoCol(col,
                        $"Available Date: {a.AvailableDate}",
                        $"Previously Applied: {(a.PreviouslyApplied == true ? "Yes" : "No")}");
                    if (a.PreviouslyApplied == true)
                        Field(col, "Previous Application Details", a.PreviousApplicationDetails);
                    Field(col, "Police Experience", a.PoliceExperience);
                    if (a.PoliceExperience != "None")
                    {
                        TwoCol(col, $"Department: {a.PoliceDepartment}", $"Length: {a.PoliceLength}");
                        TwoCol(col, $"Position/Rank: {a.PoliceRank}", $"Reason for Leaving: {a.PoliceReasonLeaving}");
                    }
                    TwoCol(col,
                        $"Armed Forces: {(a.ArmedForces ? "Yes" : "No")}   Active Duty: {(a.ActiveDuty ? "Yes" : "No")}   Reserve: {(a.Reserve ? "Yes" : "No")}",
                        $"Branch: {a.ArmedForcesBranch}   Length: {a.ArmedForcesLength}");
                    Field(col, "Honorable Discharge", a.HonorableDischarge ?? "N/A");
                    Field(col, "Professional Licenses / Certifications / Skills", a.Licenses);

                    // ── Section 3: COPT Standards ──
                    Section(col, "COPT Eligibility Standards (Initials)");
                    InitialRow(col, initImg, a.InitCoptCitizenship, "United States Citizenship (native or naturalized)");
                    InitialRow(col, initImg, a.InitCopt18, "18 years or older for Public Safety Aide");
                    InitialRow(col, initImg, a.InitCopt21, "21 years or older for Police Officer");
                    InitialRow(col, initImg, a.InitCoptSenses, "No impediment of the senses");
                    InitialRow(col, initImg, a.InitCoptVision, "Acuity of vision not more than 20/200 corrected to 20/20 in each eye");
                    InitialRow(col, initImg, a.InitCoptColorVision, "Ability to distinguish between colors red, green, and amber; no pathology; acceptable depth perception");
                    InitialRow(col, initImg, a.InitCoptHearing, "Possess normal hearing in both ears per current standard");
                    InitialRow(col, initImg, a.InitCoptNoCommunicable, "Have no communicable diseases");
                    InitialRow(col, initImg, a.InitCoptNoDeformity, "Have no physical deformities detrimental to proper performance of police duties");
                    InitialRow(col, initImg, a.InitCoptDrugScreen, "Must pass a drug-screening test prior to appointment or attendance of a Police training academy");
                    InitialRow(col, initImg, a.InitCoptWeight, "Weight must be proportionate to height and build or body fat percentage");
                    InitialRow(col, initImg, a.InitCoptMilitary, "Honorable discharge or positive conduct during military service");
                    InitialRow(col, initImg, a.InitCoptNoFelony, "No Felony or Misdemeanor conviction prohibiting the possession of a firearm");
                    InitialRow(col, initImg, a.InitCoptDl, "Valid Driver's License for Police Officer");

                    // ── Education ──
                    Section(col, "Education / Work History Standard");
                    InitialRow(col, initImg, a.InitEduStandard, a.EduStandardMet ?? "");

                    // ── Section 4: Disqualification ──
                    Section(col, "Criminal Record Disqualification Acknowledgements (Initials)");
                    InitialRow(col, initImg, a.InitCriminal1, "Any felony or domestic violence conviction is an automatic disqualification.");
                    InitialRow(col, initImg, a.InitCriminal2, "Any arrest or conviction indicating a pattern of disregard for the law may result in disqualification.");
                    InitialRow(col, initImg, a.InitCriminal3, "Any commitments for a mental disorder preventing possession of a firearm is a disqualification.");
                    InitialRow(col, initImg, a.InitCriminal4, "Arrest or conviction for all other crimes are subject to review at the time of application.");
                    InitialRow(col, initImg, a.InitCriminal5, "Arrest for any offense must be expunged, and conviction must be pardoned, prior to submitting an application.");
                    InitialRow(col, initImg, a.InitCriminal6, "Any criminal activity that would be considered a Felony under Federal Law is a disqualification.");

                    Section(col, "Drug Usage Disqualification Acknowledgements (Initials)");
                    InitialRow(col, initImg, a.InitDrug1, "Any use of mind-altering hallucinogenic drug (LSD, PCP, etc.), heroin or derivatives is an automatic disqualification.");
                    InitialRow(col, initImg, a.InitDrug2, "Any use of an illegal drug within two (2) years prior to application is an automatic disqualification.");
                    InitialRow(col, initImg, a.InitDrug3, "More than 50 experimental uses of Marijuana and/or more than 2 uses of Cocaine may result in disqualification.");
                    InitialRow(col, initImg, a.InitDrug4, "The sale or delivery of any controlled substance after age 21 will be automatic disqualification.");
                    InitialRow(col, initImg, a.InitDrug5, "Any use of all other illegal drugs will be subject to review at the time of application.");
                    InitialRow(col, initImg, a.InitDrug6, "All other drug use, including illegally using prescribed drugs, is subject to review.");
                    InitialRow(col, initImg, a.InitDrug7, "Any use of a controlled substance after having filed an application for employment as a Police Officer.");
                    InitialRow(col, initImg, a.InitDrug8, "Any history or pattern of extensive use or abuse of a controlled substance or alcohol without evidence of rehabilitation.");

                    Section(col, "Driving History Disqualification Acknowledgements (Initials)");
                    InitialRow(col, initImg, a.InitDriving1, "Must possess a current and valid driver's license and at least one year of driving experience.");
                    InitialRow(col, initImg, a.InitDriving2, "A DUI conviction within previous 5 years is an automatic disqualification.");
                    InitialRow(col, initImg, a.InitDriving3, "Any driving record/history indicating poor, dangerous, or undesirable driving habits may result in disqualification.");
                    InitialRow(col, initImg, a.InitDriving4, "Any driving record/history indicating that operation of an automobile by the applicant might endanger the public.");
                    InitialRow(col, initImg, a.InitDriving5, "Any driving record/history indicating the applicant has used narcotics or alcohol to impair their ability to drive.");
                    InitialRow(col, initImg, a.InitDriving6, "Any license suspension or revocation within three years of the closing date is an automatic disqualification.");
                    InitialRow(col, initImg, a.InitDriving7, "Any alcohol-related driving arrests and overall driving history will be subject to review.");
                    InitialRow(col, initImg, a.InitDriving8, "Motor vehicle conviction for Failing to Stop, Leaving the Scene, Criminal Negligence, or False Statements on license application.");
                    InitialRow(col, initImg, a.InitDriving9, "Any driving-related automatic disqualification as listed above.");

                    Section(col, "Employment Acknowledgements (Initials)");
                    InitialRow(col, initImg, a.InitAck1, "The Police Department is a 24 hour/7 day a week operation. Officers work rotating day and night shifts and holidays.");
                    InitialRow(col, initImg, a.InitAck2, "The Police Department is a para-military organization. Officers wear an authorized uniform and maintain grooming standards.");
                    InitialRow(col, initImg, a.InitAck3, "Candidate must be able to achieve minimum standard during fitness testing: Sit-Ups (22), Push-Ups (12), 1.5 Mile Run (16:31).");

                    // ── Background Check Signature ──
                    Section(col, "Background Check Authorization");
                    col.Item().PaddingBottom(4).Text("I hereby grant the police permission to conduct a background check on me for the purpose of determining eligibility for participation in the Hiring Process for the Middletown Police Department.").Italic();

                    if (!string.IsNullOrEmpty(a.BackgroundSignatureData))
                    {
                        var sigBytes = Convert.FromBase64String(a.BackgroundSignatureData.Replace("data:image/png;base64,", ""));
                        col.Item().Row(r =>
                        {
                            r.RelativeItem(2).Column(sc =>
                            {
                                sc.Item().Text("Signature:").Bold();
                                sc.Item().Height(50).Image(sigBytes);
                            });
                            r.RelativeItem().Column(dc =>
                            {
                                dc.Item().Text("Date:").Bold();
                                dc.Item().Text(a.BackgroundSignedAt?.ToLocalTime().ToString("MM/dd/yyyy") ?? "");
                            });
                        });
                    }

                    // ── EEO ──
                    Section(col, "Voluntary EEO & Veteran Status Data");
                    TwoCol(col, $"Gender: {a.Gender}", $"EEO Group: {a.EeoGroup}");
                    TwoCol(col,
                        $"Protected Veteran Category: {(a.VeteranProtectedCategory ? "Yes" : "No")}",
                        $"Not a Protected Veteran: {(a.VeteranNotProtected ? "Yes" : "No")}");

                    if (!string.IsNullOrEmpty(a.VeteranSignatureData))
                    {
                        var sigBytes = Convert.FromBase64String(a.VeteranSignatureData.Replace("data:image/png;base64,", ""));
                        col.Item().Row(r =>
                        {
                            r.RelativeItem(2).Column(sc =>
                            {
                                sc.Item().Text("Applicant Signature (Veteran Status):").Bold();
                                sc.Item().Height(50).Image(sigBytes);
                            });
                            r.RelativeItem().Column(dc =>
                            {
                                dc.Item().Text("Date:").Bold();
                                dc.Item().Text(a.VeteranSignedAt?.ToLocalTime().ToString("MM/dd/yyyy") ?? "");
                            });
                        });
                    }

                    // Footer meta
                    col.Item().PaddingTop(12).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(6)
                        .Text($"Submitted: {a.SubmittedAt?.ToLocalTime():MM/dd/yyyy h:mm tt}   IP: {a.IpAddress}")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Town of Middletown Police Department — Employment Application   ").FontColor(Colors.Grey.Darken1);
                    t.Span("Page ").FontColor(Colors.Grey.Darken1);
                    t.CurrentPageNumber().FontColor(Colors.Grey.Darken1);
                    t.Span(" of ").FontColor(Colors.Grey.Darken1);
                    t.TotalPages().FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }

    private static void Section(ColumnDescriptor col, string title)
    {
        col.Item().PaddingTop(10).PaddingBottom(3)
            .BorderBottom(1).BorderColor(Colors.Blue.Darken3)
            .Text(title).FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
    }

    private static void Field(ColumnDescriptor col, string label, string? value)
    {
        col.Item().PaddingTop(3).Row(r =>
        {
            r.AutoItem().Text($"{label}: ").Bold();
            r.RelativeItem().Text(value ?? "");
        });
    }

    private static void TwoCol(ColumnDescriptor col, string left, string right)
    {
        col.Item().PaddingTop(3).Row(r =>
        {
            r.RelativeItem().Text(left);
            r.RelativeItem().Text(right);
        });
    }

    private static void InitialRow(ColumnDescriptor col, byte[]? initialsImage, string? initials, string text)
    {
        col.Item().PaddingTop(2).Row(r =>
        {
            var box = r.ConstantItem(40).Border(0.5f).BorderColor(Colors.Grey.Lighten2).AlignCenter().AlignMiddle();
            if (initialsImage != null)
                box.Height(20).Image(initialsImage).FitArea();
            else
                box.Text(initials ?? "").Bold().FontSize(8);
            r.RelativeItem().PaddingLeft(6).Text(text).FontSize(8);
        });
    }
}
