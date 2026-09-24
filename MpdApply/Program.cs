using Microsoft.EntityFrameworkCore;
using MpdApply.Data;
using MpdApply.Services;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Railway injects PORT — bind to it
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromHours(2); o.Cookie.HttpOnly = true; });

var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "mpd-apply.db";
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ClaudeService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Schema migrations for columns added after initial deploy
    var migrations = new[]
    {
        "ALTER TABLE Applications ADD COLUMN InitialsMode TEXT",
        "ALTER TABLE Applications ADD COLUMN InitialsImageData TEXT",
    };
    foreach (var sql in migrations)
        try { db.Database.ExecuteSqlRaw(sql); } catch { }

    await DbSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
