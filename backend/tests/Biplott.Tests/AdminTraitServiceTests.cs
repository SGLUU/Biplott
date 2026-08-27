using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Biplott.Tests;

public class AdminTraitServiceTests
{
    private static (BiplottDbContext db, AdminTraitService service) CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<BiplottDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new BiplottDbContext(options);
        var service = new AdminTraitService(db);
        return (db, service);
    }

    [Fact]
    public async Task CreateTrait_Valid_ShouldPersistTrait()
    {
        var (db, service) = CreateTestContext();
        var req = new CreateTraitRequest
        {
            Code = "Discipline",
            Name = "Kỷ luật",
            Description = "Mức độ kiên trì",
            Category = "Personality",
            IsActive = true
        };

        var result = await service.CreateTraitAsync(req);

        Assert.NotNull(result);
        Assert.Equal("Discipline", result.Code);
        Assert.Equal("Kỷ luật", result.Name);

        var saved = await db.Traits.FirstOrDefaultAsync(t => t.Code == "Discipline");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task CreateTrait_DuplicateCode_ShouldThrowArgumentException()
    {
        var (db, service) = CreateTestContext();
        db.Traits.Add(new Trait { Code = "Intuition", Name = "Trực giác" });
        await db.SaveChangesAsync();

        var req = new CreateTraitRequest
        {
            Code = "Intuition",
            Name = "Trực giác mới"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateTraitAsync(req));
    }

    [Fact]
    public async Task DeleteTrait_WhenUsedByChoices_ShouldThrowInvalidOperationException()
    {
        var (db, service) = CreateTestContext();
        var trait = new Trait { Code = "RiskTolerance", Name = "Liều lĩnh" };
        var choiceTrait = new ChoiceTrait { Trait = trait, Weight = 0.8 };
        trait.ChoiceTraits.Add(choiceTrait);
        db.Traits.Add(trait);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteTraitAsync(trait.Id));
    }
}