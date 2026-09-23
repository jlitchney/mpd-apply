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
    }
}
