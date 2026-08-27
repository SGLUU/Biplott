using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Biplott.Tests;

public class AdminQuestionServiceTests
{
    private static (BiplottDbContext db, AdminQuestionService service, Theme theme, Trait trait) CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<BiplottDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new BiplottDbContext(options);
        var theme = new Theme { Code = "THEME_CAREER", Name = "Sự nghiệp", IsActive = true };
        var trait = new Trait { Code = "RiskTolerance", Name = "Liều lĩnh", IsActive = true };
        db.Themes.Add(theme);
        db.Traits.Add(trait);
        db.SaveChanges();

        var service = new AdminQuestionService(db);
        return (db, service, theme, trait);
    }

    [Fact]
    public async Task CreateQuestion_Valid_ShouldPersistWithChoicesAndTraits()
    {
        var (db, service, theme, trait) = CreateTestContext();
        var req = new CreateQuestionRequest
        {
            ThemeId = theme.Id,
            QuestionType = QuestionType.SingleChoice,
            Content = "Bạn làm gì khi trúng số độc đắc?",
            Subtitle = "Hãy thành thật",
            IsActive = true,
            Choices = new List<CreateChoiceRequest>
            {
                new()
                {
                    Content = "Nghỉ việc ngay lập tức",
                    IsActive = true,
                    ChoiceTraits = new List<CreateChoiceTraitRequest>
                    {
                        new() { TraitId = trait.Id, Weight = 0.9 }
                    }
                },
                new()
                {
                    Content = "Vẫn đi làm bình thường",
                    IsActive = true,
                    ChoiceTraits = new List<CreateChoiceTraitRequest>()
                }
            }
        };

        var result = await service.CreateQuestionAsync(req);

        Assert.NotNull(result);
        Assert.Equal("Bạn làm gì khi trúng số độc đắc?", result.Content);
        Assert.Equal(2, result.Choices.Count);
        Assert.Single(result.Choices[0].ChoiceTraits);
        Assert.Equal(0.9, result.Choices[0].ChoiceTraits[0].Weight);
    }

    [Fact]
    public async Task CreateQuestion_ActiveWithLessThan2Choices_ShouldThrowArgumentException()
    {
        var (_, service, theme, _) = CreateTestContext();
        var req = new CreateQuestionRequest
        {
            ThemeId = theme.Id,
            Content = "Câu hỏi 1 lựa chọn",
            IsActive = true,
            Choices = new List<CreateChoiceRequest>
            {
                new() { Content = "Lựa chọn duy nhất", IsActive = true }
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateQuestionAsync(req));
    }

    [Fact]
    public async Task CreateQuestion_InvalidTraitWeight_ShouldThrowArgumentException()
    {
        var (_, service, theme, trait) = CreateTestContext();
        var req = new CreateQuestionRequest
        {
            ThemeId = theme.Id,
            Content = "Câu hỏi trọng số sai",
            IsActive = true,
            Choices = new List<CreateChoiceRequest>
            {
                new()
                {
                    Content = "Lựa chọn A",
                    IsActive = true,
                    ChoiceTraits = new List<CreateChoiceTraitRequest>
                    {
                        new() { TraitId = trait.Id, Weight = 1.5 } // Invalid > 1.0
                    }
                },
                new() { Content = "Lựa chọn B", IsActive = true }
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateQuestionAsync(req));
    }

    [Fact]
    public async Task DuplicateQuestion_ShouldCreateInactiveDraftCopyWithSameChoicesAndTraits()
    {
        var (db, service, theme, trait) = CreateTestContext();
        var q = new Question
        {
            ThemeId = theme.Id,
            QuestionType = QuestionType.Scenario,
            Content = "Tình huống gốc",
            IsActive = true,
            ViewCount = 100,
            Choices = new List<QuestionChoice>
            {
                new()
                {
                    Content = "Phương án 1",
                    OrderIndex = 0,
                    IsActive = true,
                    ChoiceTraits = new List<ChoiceTrait>
                    {
                        new() { TraitId = trait.Id, Weight = 0.7 }
                    }
                },
                new() { Content = "Phương án 2", OrderIndex = 1, IsActive = true }
            }
        };
        db.Questions.Add(q);
        await db.SaveChangesAsync();

        var duplicated = await service.DuplicateQuestionAsync(q.Id);

        Assert.NotEqual(q.Id, duplicated.Id);
        Assert.Equal("Tình huống gốc (Bản sao)", duplicated.Content);
        Assert.False(duplicated.IsActive); // Draft by default
        Assert.Equal(0, duplicated.ViewCount); // View count reset
        Assert.Equal(2, duplicated.Choices.Count);
        Assert.Single(duplicated.Choices[0].ChoiceTraits);
        Assert.Equal(0.7, duplicated.Choices[0].ChoiceTraits[0].Weight);
    }

    [Fact]
    public async Task SetQuestionStatus_ActivatingWithLessThan2ActiveChoices_ShouldThrowInvalidOperation()
    {
        var (db, service, theme, _) = CreateTestContext();
        var q = new Question
        {
            ThemeId = theme.Id,
            Content = "Câu hỏi bị vô hiệu lựa chọn",
            IsActive = false,
            Choices = new List<QuestionChoice>
            {
                new() { Content = "Lựa chọn 1", IsActive = true },
                new() { Content = "Lựa chọn 2", IsActive = false }
            }
        };
        db.Questions.Add(q);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SetQuestionStatusAsync(q.Id, true));
    }

    [Fact]
    public async Task DeleteQuestion_WhenHasUserHistory_ShouldDeactivateRatherThanHardDelete()
    {
        var (db, service, theme, _) = CreateTestContext();
        var q = new Question
        {
            ThemeId = theme.Id,
            Content = "Câu hỏi có lịch sử",
            IsActive = true
        };
        q.Histories.Add(new UserQuestionHistory { UserId = "user-1", AnsweredAt = DateTime.UtcNow });
        db.Questions.Add(q);
        await db.SaveChangesAsync();

        await service.DeleteQuestionAsync(q.Id);

        var existing = await db.Questions.FirstOrDefaultAsync(item => item.Id == q.Id);
        Assert.NotNull(existing);
        Assert.False(existing.IsActive);
    }
}