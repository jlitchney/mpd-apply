using MpdApply.Models;

namespace MpdApply.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!db.Settings.Any())
        {
            db.Settings.AddRange(
                new AppSetting { Key = "RecipientEmail", Value = "jason@allstartalent.us" },
                new AppSetting { Key = "AgencyName", Value = "Town of Middletown Police Department" }
            );
            await db.SaveChangesAsync();
        }

        if (!db.FormTemplates.Any(t => t.Slug == "ride-along"))
        {
            db.FormTemplates.Add(new FormTemplate
            {
                Name = "Ride-Along Application",
                Slug = "ride-along",
                Description = "Application to participate in the Middletown Police Department Ride-Along Program.",
                IsPublished = true,
                SchemaJson = RideAlongSchema
            });
            await db.SaveChangesAsync();
        }
    }

    private const string RideAlongSchema = """
    {
      "pages": [
        {
          "title": "Ride-Along Application",
          "intro": "Please fill out the information below. You must present your license/ID upon return of this form for verification and documentation purposes. Ride-Alongs are approved on a case by case basis by the Chief of Police; there are restrictions that will apply.",
          "fields": [
            {"key":"date","type":"date","label":"Date","required":true,"width":"third"},
            {"key":"name","type":"text","label":"Full Name","required":true,"width":"full"},
            {"key":"email","type":"email","label":"Email Address","required":true,"width":"half"},
            {"key":"phone","type":"tel","label":"Phone Number","required":true,"width":"half"},
            {"key":"address","type":"textarea","label":"Address","required":true,"width":"full","rows":2},
            {"key":"age_birthday","type":"text","label":"Age and Birthday","required":true,"width":"half"},
            {"key":"license","type":"text","label":"License State and Number","required":true,"width":"half"},
            {"key":"emergency_contact","type":"text","label":"Emergency Contact Name and Number","required":true,"width":"full"},
            {"key":"availability","type":"text","label":"Availability for Ride-Along","required":true,"width":"full"},
            {"key":"sec_academic","type":"section_header","label":"Academic Requirements (if applicable)"},
            {"key":"school","type":"text","label":"School","width":"full"},
            {"key":"year_of_study","type":"text","label":"Year of Study","width":"half"},
            {"key":"field_of_study","type":"text","label":"Field of Study","width":"half"},
            {"key":"professor","type":"text","label":"Professor","width":"half"},
            {"key":"purpose","type":"text","label":"Purpose (e.g. Research paper, exam, required experience)","width":"full"},
            {"key":"class_required","type":"yesno","label":"Is a Police Ride-Along required for your class?","width":"half"},
            {"key":"sec_career","type":"section_header","label":"Interest in Law Enforcement Career"},
            {"key":"law_area","type":"textarea","label":"What area of Law Enforcement is of interest to you? (e.g. Federal, State, Local, Police, Lawyer, Investigative, etc.)","width":"full","rows":2},
            {"key":"departments","type":"textarea","label":"Have you applied to any Police Departments? If so, tell us which ones.","width":"full","rows":2},
            {"key":"reason","type":"textarea","label":"Why have you chosen to participate in the Middletown Police Department’s Ride-Along Program?","required":true,"width":"full","rows":3}
          ]
        },
        {
          "title": "Waiver & Signature",
          "fields": [
            {"key":"waiver_intro","type":"paragraph","content":"Please read the information below and sign. You must present your license/ID upon signing this form for verification and documentation purposes."},
            {"key":"sec_waiver","type":"section_header","label":"Ride-Along Program Waiver of Civil Liability and Indemnification Agreement"},
            {"key":"waiver_text","type":"paragraph","content":"I, the undersigned, desire to participate in the Middletown Police Department, Police Ride-Along Program in order to obtain knowledge of the necessary skills and duties of a police officer.\n\nIn consideration of the Middletown Police Department granting me permission to accompany a member or members of the police department as observed in the Ride-Along Program, and recognizing that this activity involves certain inherent dangers, I do hereby agree to assume all risks attendant to such activity and I do for myself, my heirs and assigns, hereby waive any and all rights and claims of liability for damages, losses, personal injuries (including exposure to contagious or infectious disease) or death which I might suffer or sustain against the Police Department, Middletown its elected officials, officer agents or employees, in both their public and private capacities, which are in anyway related to or are a result of my voluntary participation in the Ride-Along Program; and I hereby hold harmless such persons and entities.\n\nI further agree to comply with all rules and regulations of the Ride-Along Program and any instructions or orders issued by members of the police, supervisory and administrative personnel and to not interfere in any way with the performance of their duties.\n\nI further agree to maintain all suspect/victim confidentiality and I will not disclose any information.\n\nI hereby grant the police permission to conduct a background check on me for the purpose of determining eligibility for participation in the Ride-Along Program.\n\nIn the event that a demand or claim is made against the entities or persons set forth herein, I agree to indemnify those persons and/or entities for all damages, attorney fees and costs incurred in defending said demand or claim.\n\nAs evidenced by my signature below, I certify that I have read and fully understand this waiver and its consequences and affirm that I am eighteen (18) years of age or older."},
            {"key":"print_name","type":"text","label":"Print Name","required":true,"width":"half"},
            {"key":"signature","type":"signature","label":"Signature","required":true,"width":"full"}
          ]
        }
      ]
    }
    """;
}
