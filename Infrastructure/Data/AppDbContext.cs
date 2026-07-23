using Microsoft.EntityFrameworkCore;
using Ecoa.Core.Entities;

namespace Ecoa.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<EnvironmentalAction> EnvironmentalActions => Set<EnvironmentalAction>();
    public DbSet<ActionEvidence> ActionEvidences => Set<ActionEvidence>();
    public DbSet<Validation> Validations => Set<Validation>();
    public DbSet<IncentivePool> IncentivePools => Set<IncentivePool>();
    public DbSet<TokenTransaction> TokenTransactions => Set<TokenTransaction>();
    public DbSet<Ride> Rides => Set<Ride>();
    public DbSet<RidePoint> RidePoints => Set<RidePoint>();
    public DbSet<RideImuSample> RideImuSamples => Set<RideImuSample>();
    public DbSet<CyclePath> CyclePaths => Set<CyclePath>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Cpf).IsUnique();
        });

        modelBuilder.Entity<EnvironmentalAction>(e =>
        {
            e.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId);
            e.HasMany(a => a.Evidences).WithOne(ev => ev.Action).HasForeignKey(ev => ev.ActionId);
            e.HasMany(a => a.Validations).WithOne(v => v.Action).HasForeignKey(v => v.ActionId);
        });

        modelBuilder.Entity<ActionEvidence>(e =>
        {
            e.HasOne(ev => ev.Action).WithMany(a => a.Evidences).HasForeignKey(ev => ev.ActionId);
        });

        modelBuilder.Entity<Validation>(e =>
        {
            e.HasOne(v => v.Action).WithMany(a => a.Validations).HasForeignKey(v => v.ActionId);
            e.HasOne(v => v.Validator).WithMany().HasForeignKey(v => v.ValidatorId);
        });

        modelBuilder.Entity<TokenTransaction>(e =>
        {
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId);
            e.HasOne(t => t.Action).WithMany().HasForeignKey(t => t.ActionId).IsRequired(false);
        });

        modelBuilder.Entity<Ride>(e =>
        {
            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
            e.HasMany(r => r.Points).WithOne(p => p.Ride).HasForeignKey(p => p.RideId);
            e.HasMany(r => r.ImuSamples).WithOne(s => s.Ride).HasForeignKey(s => s.RideId);
        });

        modelBuilder.Entity<RidePoint>(e =>
        {
            e.HasOne(p => p.Ride).WithMany(r => r.Points).HasForeignKey(p => p.RideId);
        });

        modelBuilder.Entity<RideImuSample>(e =>
        {
            e.HasOne(s => s.Ride).WithMany(r => r.ImuSamples).HasForeignKey(s => s.RideId);
            e.HasIndex(s => new { s.RideId, s.RecordedAt });
        });
    }
}
