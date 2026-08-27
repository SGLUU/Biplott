using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Biplott.Tests;

public class AdminThemeServiceTests
{
    private static (BiplottDbContext db, AdminThemeService service) CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<BiplottDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new BiplottDbContext(options);
        var service = new AdminThemeService(db);
        return (db, service);
    }

    [Fact]
    public async Task CreateTheme_Valid_ShouldPersistTheme()
    {
        var (db, service) = CreateTestContext();
        var req = new CreateThemeRequest
        {
            Code = "THEME_TECH",
            Name = "Công nghệ",
            Description = "Chủ đề coder",
            SortOrder = 1,
            IsActive = true
        };

        var result = await service.CreateThemeAsync(req);

        Assert.NotNull(result);
        Assert.Equal("THEME_TECH", result.Code);
        Assert.Equal("Công nghệ", result.Name);
        Assert.True(result.IsActive);

        var saved = await db.Themes.FirstOrDefaultAsync(t => t.Code == "THEME_TECH");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task CreateTheme_DuplicateCode_ShouldThrowArgumentException()
    {
        var (db, service) = CreateTestContext();
        db.Themes.Add(new Theme { Code = "THEME_CAREER", Name = "Sự nghiệp" });
        await db.SaveChangesAsync();

        var req = new CreateThemeRequest
        {
            Code = "THEME_CAREER",
            Name = "Sự nghiệp 2"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateThemeAsync(req));
    }

    [Fact]
    public async Task UpdateTheme_Valid_ShouldUpdateProperties()
    {
        var (db, service) = CreateTestContext();
        var theme = new Theme { Code = "THEME_LOVE", Name = "Tình duyên cũ", SortOrder = 5 };
        db.Themes.Add(theme);
        await db.SaveChangesAsync();

        var req = new UpdateThemeRequest
        {
            Name = "Tình yêu mới",
            Description = "Cập nhật",
            SortOrder = 1,
            IsActive = false
        };

        var updated = await service.UpdateThemeAsync(theme.Id, req);

        Assert.Equal("Tình yêu mới", updated.Name);
        Assert.Equal("Cập nhật", updated.Description);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task DeleteTheme_WhenHasQuestions_ShouldThrowInvalidOperationException()
    {
        var (db, service) = CreateTestContext();
        var theme = new Theme { Code = "THEME_USED", Name = "Có câu hỏi" };
        theme.Questions.Add(new Question { Content = "Câu hỏi 1", QuestionType = Biplott.Core.Enums.QuestionType.SingleChoice });
        db.Themes.Add(theme);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteThemeAsync(theme.Id));
    }

    [Fact]
    public async Task DeleteTheme_WhenNoQuestions_ShouldDeleteSuccessfully()
    {
        var (db, service) = CreateTestContext();
        var theme = new Theme { Code = "THEME_EMPTY", Name = "Rỗng" };
        db.Themes.Add(theme);
        await db.SaveChangesAsync();

        await service.DeleteThemeAsync(theme.Id);

        var exists = await db.Themes.AnyAsync(t => t.Id == theme.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task GetThemesPaged_SearchAndActiveFilter_ShouldReturnCorrectResult()
    {
        var (db, service) = CreateTestContext();
        db.Themes.AddRange(
            new Theme { Code = "T1", Name = "Tình yêu", IsActive = true, SortOrder = 1 },
            new Theme { Code = "T2", Name = "Tiền tài", IsActive = true, SortOrder = 2 },
            new Theme { Code = "T3", Name = "Tình bạn ẩn", IsActive = false, SortOrder = 3 }
        );
        await db.SaveChangesAsync();

        var result = await service.GetThemesPagedAsync(1, 10, "Tình", true);

        Assert.Single(result.Items);
        Assert.Equal("T1", result.Items[0].Code);
    }
}