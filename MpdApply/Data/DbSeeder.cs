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

        if (!db.FormTemplates.Any(t => t.Slug == "employment-application"))
        {
            db.FormTemplates.Add(new FormTemplate
            {
                Name = "Employment Application",
                Slug = "employment-application",
                Description = "Application for employment with the Town of Middletown Police Department.",
                IsPublished = true,
                SchemaJson = EmploymentApplicationSchema
            });
            await db.SaveChangesAsync();
        }

        if (!db.FormTemplates.Any(t => t.Slug == "voluntary-applicant-data"))
        {
            db.FormTemplates.Add(new FormTemplate
            {
                Name = "Voluntary Applicant Data",
                Slug = "voluntary-applicant-data",
                Description = "Voluntary affirmative action and veteran status data form. Completion is strictly voluntary and has no bearing on your application.",
                IsPublished = true,
                SchemaJson = VoluntaryApplicantDataSchema
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
            {"key":"reason","type":"textarea","label":"Why have you chosen to participate in the Middletown Police Department's Ride-Along Program?","required":true,"width":"full","rows":3}
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

    private const string EmploymentApplicationSchema = """
    {
      "pages": [
        {
          "title": "Personal Information",
          "intro": "Applicants for all positions are considered without regard to race, color, religion, sex, national origin, age, marital status, or the presence of disabilities.",
          "fields": [
            {"key":"app_date","type":"date","label":"Date of Application","required":true,"width":"third"},
            {"key":"position","type":"text","label":"Position Applied For","required":true,"width":"full"},
            {"key":"how_heard","type":"select","label":"How did you hear about us?","required":true,"width":"full","options":["Employee of Town/MPD","Town/MPD Website","Newspaper/Publication","Online Search Engine","College/University","Job Fair/Recruitment Fair","Other"]},
            {"key":"sec_personal","type":"section_header","label":"Personal Information"},
            {"key":"last_name","type":"text","label":"Last Name","required":true,"width":"third"},
            {"key":"first_name","type":"text","label":"First Name","required":true,"width":"third"},
            {"key":"middle_initial","type":"text","label":"Middle Initial","width":"third"},
            {"key":"address","type":"textarea","label":"Street Address","required":true,"width":"full","rows":2},
            {"key":"city","type":"text","label":"City","required":true,"width":"third"},
            {"key":"state","type":"text","label":"State","required":true,"width":"third"},
            {"key":"zip","type":"text","label":"Zip Code","required":true,"width":"third"},
            {"key":"dob","type":"date","label":"Date of Birth","required":true,"width":"third"},
            {"key":"ssn","type":"text","label":"Social Security Number","required":true,"width":"third","placeholder":"XXX-XX-XXXX"},
            {"key":"dl_state","type":"text","label":"Driver's License State","required":true,"width":"third"},
            {"key":"dl_number","type":"text","label":"Driver's License Number","required":true,"width":"third"},
            {"key":"home_phone","type":"tel","label":"Home Phone","width":"third"},
            {"key":"work_phone","type":"tel","label":"Work Phone","width":"third"},
            {"key":"cell_phone","type":"tel","label":"Cell Phone","required":true,"width":"third"},
            {"key":"email","type":"email","label":"Email Address","required":true,"width":"full"}
          ]
        },
        {
          "title": "Employment Background",
          "fields": [
            {"key":"sec_current_emp","type":"section_header","label":"Current Employment"},
            {"key":"employed_now","type":"yesno","label":"Are you currently employed?","required":true,"width":"half"},
            {"key":"may_contact","type":"yesno","label":"May we contact your current employer?","width":"half"},
            {"key":"current_employer","type":"text","label":"Current Employer(s)","width":"half"},
            {"key":"current_position","type":"text","label":"Current Position","width":"half"},
            {"key":"available_date","type":"date","label":"Date available for work","required":true,"width":"third"},
            {"key":"applied_before","type":"yesno","label":"Have you ever applied to the Middletown Police Department?","required":true,"width":"half"},
            {"key":"applied_before_details","type":"text","label":"If yes, when and what position?","width":"full"},
            {"key":"sec_police_exp","type":"section_header","label":"Police Experience"},
            {"key":"police_exp","type":"radio","label":"Police experience","required":true,"width":"full","options":["None","Full-time","Part-time","Seasonal","Military Police"]},
            {"key":"police_dept","type":"text","label":"Department","width":"half"},
            {"key":"police_length","type":"text","label":"Length of Employment","width":"quarter"},
            {"key":"police_rank","type":"text","label":"Position / Rank","width":"quarter"},
            {"key":"police_reason","type":"text","label":"Reason for Leaving","width":"full"},
            {"key":"sec_armed_forces","type":"section_header","label":"Armed Forces Service"},
            {"key":"armed_forces","type":"yesno","label":"Have you served in the Armed Forces?","required":true,"width":"half"},
            {"key":"armed_forces_type","type":"radio","label":"Duty Type","width":"half","options":["Active Duty","Reserve","N/A"]},
            {"key":"armed_branch","type":"text","label":"Branch","width":"third"},
            {"key":"armed_length","type":"text","label":"Length of Service","width":"third"},
            {"key":"armed_discharge","type":"radio","label":"Honorable Discharge","width":"third","options":["Yes","No","N/A"]},
            {"key":"licenses","type":"textarea","label":"Professional Licenses, Certifications, and/or Skills","width":"full","rows":3}
          ]
        },
        {
          "title": "Eligibility Standards",
          "intro": "The Middletown Police will accept applications throughout the year. The Application and the Eligibility Standards Guide must be completed and submitted in order to be considered as an Applicant. Please initial next to each standard that you meet.",
          "fields": [
            {"key":"initials_setup","type":"initials_panel"},
            {"key":"sec_copt","type":"section_header","label":"Delaware Council on Police Training (COPT) Standards"},
            {"key":"init_citizenship","type":"initials","label":"United States Citizenship (native or naturalized)","required":true},
            {"key":"init_age","type":"initials","label":"21 years or older for Police Officer (must be 21 upon successful completion of academy training)","required":true},
            {"key":"init_vision","type":"initials","label":"Acuity of vision not more than 20/200 corrected to 20/20 in each eye; ability to distinguish colors red, green, and amber; acceptable depth perception","required":true},
            {"key":"init_hearing","type":"initials","label":"Possess normal hearing in both ears per current standard","required":true},
            {"key":"init_health","type":"initials","label":"No communicable diseases; no physical deformities detrimental to proper performance of police duties","required":true},
            {"key":"init_drug_screen","type":"initials","label":"Must pass a drug-screening test prior to appointment or attendance at a police training academy","required":true},
            {"key":"init_weight","type":"initials","label":"Weight must be proportionate to height and build or body fat percentage","required":true},
            {"key":"init_military_discharge","type":"initials","label":"Honorable discharge or positive conduct during military service (if applicable)","required":true},
            {"key":"init_no_felony","type":"initials","label":"No felony or misdemeanor conviction prohibiting the possession of a firearm","required":true},
            {"key":"init_valid_dl","type":"initials","label":"Valid driver's license for Police Officer","required":true},
            {"key":"sec_edu","type":"section_header","label":"Education OR Work History Requirement"},
            {"key":"edu_paragraph","type":"paragraph","content":"Initial next to the Education OR Work History standard that you meet:\n\n• Completion of a Bachelor's Degree\n• Completion of an Associate's Degree or 60 college credit hours\n• 30 college credit hours PLUS 2 years active duty military service, current satisfactory employment with the Town of Middletown, or 2 years of work experience\n• 2 years of experience as a sworn Police Officer\n• 4 years of full active military duty with an Honorable Discharge by date of hire\n• 3 years of satisfactory work performance demonstrating the required knowledge, skills, and abilities of the position"},
            {"key":"init_edu_requirement","type":"initials","label":"I meet one of the Education or Work History requirements listed above","required":true}
          ]
        },
        {
          "title": "Disqualification Standards",
          "intro": "Automatic Disqualification will occur for the below areas. Please initial next to each standard to signify that you understand them as written.",
          "fields": [
            {"key":"initials_reminder_1","type":"initials_reminder"},
            {"key":"sec_criminal","type":"section_header","label":"Criminal Record and Activity"},
            {"key":"init_disq_felony","type":"initials","label":"Any felony or domestic violence conviction is an automatic disqualification","required":true},
            {"key":"init_disq_pattern","type":"initials","label":"Any arrest or conviction indicating a pattern of disregard for the law or frequent undesirable behavior may result in disqualification","required":true},
            {"key":"init_disq_mental","type":"initials","label":"Any commitment for a mental disorder that would prevent possession of a firearm under Delaware Criminal Code Section 1448 is a disqualification","required":true},
            {"key":"init_disq_other_crimes","type":"initials","label":"All other arrests and convictions are subject to review. Any offense must be expunged or pardoned prior to submitting an application","required":true},
            {"key":"init_disq_federal_felony","type":"initials","label":"Any criminal activity that would be considered a felony under federal law or the law of the state in which it occurred is a disqualification","required":true},
            {"key":"sec_drug","type":"section_header","label":"Drug Usage and Activity"},
            {"key":"init_drug_hallucinogen","type":"initials","label":"Any use of mind-altering hallucinogenic drugs (LSD, PCP, etc.), heroin, or any of its derivatives is an automatic disqualification","required":true},
            {"key":"init_drug_two_years","type":"initials","label":"Any use of an illegal drug within two (2) years prior to application is an automatic disqualification (marijuana: must not have been used within 6 months of application)","required":true},
            {"key":"init_drug_marijuana","type":"initials","label":"More than 50 experimental uses of Marijuana and/or more than 2 uses of Cocaine may result in disqualification","required":true},
            {"key":"init_drug_sale","type":"initials","label":"The sale or delivery of any controlled substance after age 21 is an automatic disqualification","required":true},
            {"key":"init_drug_other","type":"initials","label":"All other drug use including illegally using prescribed drugs, and any use of a controlled substance after filing an application for Police Officer, is subject to review","required":true}
          ]
        },
        {
          "title": "Acknowledgements & Certification",
          "fields": [
            {"key":"initials_reminder_2","type":"initials_reminder"},
            {"key":"sec_driving","type":"section_header","label":"Driving History and Activity"},
            {"key":"init_drive_valid_dl","type":"initials","label":"Must possess a current valid driver's license and at least one year of driving experience","required":true},
            {"key":"init_drive_dui","type":"initials","label":"A DUI conviction within the previous 5 years is an automatic disqualification","required":true},
            {"key":"init_drive_habits","type":"initials","label":"Any driving record indicating poor, dangerous, or undesirable driving habits may result in disqualification","required":true},
            {"key":"init_drive_suspension","type":"initials","label":"Any license suspension or revocation within three years of the job announcement closing date is an automatic disqualification","required":true},
            {"key":"init_drive_serious","type":"initials","label":"Convictions for Failing to Stop at a Police command, Leaving the scene of a Personal Injury Accident, Criminal Negligence, or Making False Statements on a driver's license application result in automatic disqualification","required":true},
            {"key":"sec_emp_ack","type":"section_header","label":"Employment Acknowledgements"},
            {"key":"init_ack_shifts","type":"initials","label":"The Police Department is a 24-hour/7-day-a-week operation. Officers are expected to work rotating day and night shifts and holidays","required":true},
            {"key":"init_ack_uniform","type":"initials","label":"The Police Department is a para-military organization. Officers are expected to wear an authorized uniform and maintain the Department's grooming standards","required":true},
            {"key":"init_ack_fitness","type":"initials","label":"Candidate must achieve minimum fitness testing standards: Sit-Ups (22), Push-Ups (12), 1.5 Mile Run (16:31)","required":true},
            {"key":"sec_certification","type":"section_header","label":"Background Check Certification"},
            {"key":"cert_text","type":"paragraph","content":"I hereby grant the police permission to conduct a background check on me for the purpose of determining eligibility for participation in the Hiring Process for the Middletown Police Department.\n\nIf you fail to meet any of the above criteria you will be notified that you are no longer being considered for employment with the Middletown Police Department. You may reapply once you have met the above criteria during the next hiring process."},
            {"key":"print_name","type":"text","label":"Print Name","required":true,"width":"half"},
            {"key":"signature","type":"signature","label":"Signature","required":true,"width":"full"}
          ]
        }
      ]
    }
    """;

    private const string VoluntaryApplicantDataSchema = """
    {
      "pages": [
        {
          "title": "Voluntary Applicant Data",
          "intro": "Completion of this form is STRICTLY VOLUNTARY. We consider all applicants without regard to race, color, religion, sex, national origin, citizenship, age, mental or physical disabilities, veteran status, or any other similarly protected status. Not providing this information will not subject you to any negative personnel decision or action.",
          "fields": [
            {"key":"sec_applicant","type":"section_header","label":"Applicant Information"},
            {"key":"last_name","type":"text","label":"Last Name","required":true,"width":"third"},
            {"key":"first_name","type":"text","label":"First Name","required":true,"width":"third"},
            {"key":"middle_name","type":"text","label":"Middle Name","width":"third"},
            {"key":"phone","type":"tel","label":"Phone","width":"half"},
            {"key":"address","type":"textarea","label":"Street Address","width":"full","rows":2},
            {"key":"city","type":"text","label":"City","width":"third"},
            {"key":"state","type":"text","label":"State","width":"third"},
            {"key":"zip","type":"text","label":"Zip Code","width":"third"},
            {"key":"gender","type":"radio","label":"Gender","width":"half","options":["Male","Female","Prefer not to disclose"]},
            {"key":"position","type":"text","label":"Position Applied For","width":"half"},
            {"key":"app_date","type":"date","label":"Date","width":"third"},
            {"key":"sec_referral","type":"section_header","label":"Referral Source"},
            {"key":"referral_source","type":"radio","label":"How did you hear about this position?","width":"full","options":["Government employment agency","Private employment agency","Current employee","Walk-in","School","Relative","Advertisement","Other"]},
            {"key":"referral_person","type":"text","label":"Person who referred you (if applicable)","width":"full"},
            {"key":"sec_eeo","type":"section_header","label":"Equal Employment Opportunity Identification"},
            {"key":"eeo_group","type":"radio","label":"Please select one EEO identification group (optional)","width":"full","options":["Hispanic or Latino","White (not Hispanic or Latino)","Asian (not Hispanic or Latino)","Native Hawaiian/Other Pacific Islander (not Hispanic or Latino)","Black/African American (not Hispanic or Latino)","American Indian/Alaskan Native (not Hispanic or Latino)","Two or more races (not Hispanic or Latino)","Prefer not to disclose"]},
            {"key":"sec_veteran","type":"section_header","label":"Veteran Status"},
            {"key":"vet_text","type":"paragraph","content":"This employer is a government contractor subject to the Vietnam Era Veterans' Readjustment Assistance Act of 1974, as amended by the Jobs for Veterans Act of 2002, 38 U.S.C. 4212 (VEVRAA). Protected veterans include: (1) disabled veterans; (2) recently separated veterans (within 3 years of discharge); (3) active-duty wartime or campaign-badge veterans; and (4) Armed Forces service medal veterans."},
            {"key":"veteran_status","type":"radio","label":"Veteran Status (optional)","width":"full","options":["I identify as a protected veteran","I am not a protected veteran","Prefer not to disclose"]},
            {"key":"sec_signature","type":"section_header","label":"Certification"},
            {"key":"print_name","type":"text","label":"Print Name","width":"half"},
            {"key":"sig_date","type":"date","label":"Date","width":"half"},
            {"key":"signature","type":"signature","label":"Signature","width":"full"}
          ]
        }
      ]
    }
    """;
}
