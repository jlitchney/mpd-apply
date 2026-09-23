using Microsoft.EntityFrameworkCore;
using MpdApply.Models;

namespace MpdApply.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApplicationSubmission> Applications => Set<ApplicationSubmission>();
    public DbSet<AppSetting> Settings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppSetting>().HasKey(s => s.Key);
    }
}
