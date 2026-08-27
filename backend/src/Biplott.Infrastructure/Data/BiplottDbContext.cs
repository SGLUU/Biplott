using Biplott.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Data;

public class BiplottDbContext : IdentityDbContext<ApplicationUser>
{
    public BiplottDbContext(DbContextOptions<BiplottDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<GamePool> GamePools => Set<GamePool>();
    public DbSet<Slip> Slips => Set<Slip>();
    public DbSet<SlipLine> SlipLines => Set<SlipLine>();
    public DbSet<SlipLineNumber> SlipLineNumbers => Set<SlipLineNumber>();
    public DbSet<Theme> Themes => Set<Theme>();
    public DbSet<Trait> Traits => Set<Trait>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionChoice> QuestionChoices => Set<QuestionChoice>();
    public DbSet<ChoiceTrait> ChoiceTraits => Set<ChoiceTrait>();
    public DbSet<UserQuestionHistory> UserQuestionHistories => Set<UserQuestionHistory>();
    public DbSet<UserActivityHistory> UserActivityHistories => Set<UserActivityHistory>();
    public DbSet<EngineConfig> EngineConfigs => Set<EngineConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.HasIndex(e => e.RefreshToken);
        });

        // Games
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Tagline).HasMaxLength(255);

            entity.HasMany(e => e.Pools)
                  .WithOne(p => p.Game)
                  .HasForeignKey(p => p.GameId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // GamePools
        modelBuilder.Entity<GamePool>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.BadgeColor).HasMaxLength(20);
        });

        // Slips
        modelBuilder.Entity<Slip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SlipCode).IsUnique();
            entity.HasIndex(e => e.GuestSessionToken);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsFavorite });
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.Property(e => e.SlipCode).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(e => e.Game)
                  .WithMany(g => g.Slips)
                  .HasForeignKey(e => e.GameId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Lines)
                  .WithOne(l => l.Slip)
                  .HasForeignKey(l => l.SlipId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ApplicationUser>()
                  .WithMany(u => u.Slips)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SlipLines
        modelBuilder.Entity<SlipLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LineLabel).HasMaxLength(5).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasMany(e => e.Numbers)
                  .WithOne(n => n.SlipLine)
                  .HasForeignKey(n => n.SlipLineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SlipLineNumbers
        modelBuilder.Entity<SlipLineNumber>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Source).HasConversion<string>().HasMaxLength(20);
        });

        // Themes
        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.SortOrder });
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        // Traits
        modelBuilder.Entity<Trait>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.Code });
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(50);
        });

        // Questions
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ThemeId, e.IsActive });
            entity.HasIndex(e => new { e.QuestionType, e.IsActive });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UpdatedAt);
            entity.Property(e => e.QuestionType).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Content).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Subtitle).HasMaxLength(500);
            entity.Property(e => e.MediaUrl).HasMaxLength(500);

            entity.HasOne(e => e.Theme)
                  .WithMany(t => t.Questions)
                  .HasForeignKey(e => e.ThemeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Choices)
                  .WithOne(c => c.Question)
                  .HasForeignKey(c => c.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // QuestionChoices
        modelBuilder.Entity<QuestionChoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.QuestionId, e.IsActive });
            entity.Property(e => e.Content).HasMaxLength(500).IsRequired();
            entity.Property(e => e.SubContent).HasMaxLength(255);
            entity.Property(e => e.MediaUrl).HasMaxLength(500);
        });

        // ChoiceTraits
        modelBuilder.Entity<ChoiceTrait>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.QuestionChoice)
                  .WithMany(qc => qc.ChoiceTraits)
                  .HasForeignKey(e => e.QuestionChoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Trait)
                  .WithMany(t => t.ChoiceTraits)
                  .HasForeignKey(e => e.TraitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserQuestionHistories
        modelBuilder.Entity<UserQuestionHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.AnsweredAt });
            entity.HasIndex(e => new { e.GuestSessionToken, e.AnsweredAt });

            entity.HasOne(e => e.Question)
                  .WithMany(q => q.Histories)
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Choice)
                  .WithMany()
                  .HasForeignKey(e => e.ChoiceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // UserActivityHistories
        modelBuilder.Entity<UserActivityHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.Property(e => e.ActivityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Summary).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.ActivityHistories)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Game)
                  .WithMany()
                  .HasForeignKey(e => e.GameId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // EngineConfigs
        modelBuilder.Entity<EngineConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });
    }
}
