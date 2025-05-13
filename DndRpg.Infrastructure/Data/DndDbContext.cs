using DndRpg.Core;
using DndRpg.Core.Enums;

namespace DndRpg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DndRpg.Core.Models;

public class DndDbContext : DbContext
{
    public DbSet<Character> Characters { get; set; }
    public DbSet<AbilityScore> AbilityScores { get; set; }
    public DbSet<Skill> Skills { get; set; }

    public DndDbContext(DbContextOptions<DndDbContext> options) : base(options)
    {
        // Ensure database is created
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Class).IsRequired();
            entity.Property(e => e.Race).IsRequired();
        });

        modelBuilder.Entity<AbilityScore>(entity =>
        {
            entity.HasKey(e => new { e.CharacterId, e.Ability });
            
            entity.HasOne(e => e.Character)
                .WithMany(c => c.AbilityScores)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => new { e.CharacterId, e.Name });
            
            entity.HasOne(e => e.Character)
                .WithMany(c => c.Skills)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
