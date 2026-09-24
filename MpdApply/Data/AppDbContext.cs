using Microsoft.EntityFrameworkCore;
using MpdApply.Models;

namespace MpdApply.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApplicationSubmission> Applications => Set<ApplicationSubmission>();
    public DbSet<AppSetting> Settings => Set<AppSetting>();
    public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppSetting>().HasKey(s => s.Key);
        b.Entity<FormTemplate>().HasIndex(t => t.Slug).IsUnique();
        b.Entity<FormSubmission>()
            .HasOne(s => s.FormTemplate).WithMany()
            .HasForeignKey(s => s.FormTemplateId);
    }
}
