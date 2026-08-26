using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Xunit;

namespace Biplott.Tests;

public class NoveltyEngineTests
{
    private static List<Question> CreateMockQuestions()
    {
        var theme1 = new Theme { Id = 1, Code = "THEME_PERSONALITY", Name = "Tính cách", Icon = "🧠", IsActive = true };
        var theme2 = new Theme { Id = 2, Code = "THEME_LOVE", Name = "Tình cảm", Icon = "❤️", IsActive = true };
        var themeDestiny = new Theme { Id = 10, Code = "THEME_DESTINY", Name = "Định mệnh", Icon = "🔮", IsActive = true };

        return new List<Question>
        {
            new() { Id = 1, ThemeId = 1, Theme = theme1, QuestionType = QuestionType.SingleChoice, Content = "Q1 Tính cách", IsActive = true },
            new() { Id = 2, ThemeId = 1, Theme = theme1, QuestionType = QuestionType.ThisOrThat, Content = "Q2 Tính cách", IsActive = true },
            new() { Id = 3, ThemeId = 2, Theme = theme2, QuestionType = QuestionType.SingleChoice, Content = "Q3 Tình cảm", IsActive = true },
            new() { Id = 4, ThemeId = 2, Theme = theme2, QuestionType = QuestionType.Scenario, Content = "Q4 Tình cảm", IsActive = true },
            new() { Id = 5, ThemeId = 10, Theme = themeDestiny, QuestionType = QuestionType.QuickInstinct, Content = "Q5 Định mệnh", IsActive = true },
            new() { Id = 99, ThemeId = 1, Theme = theme1, QuestionType = QuestionType.SingleChoice, Content = "Q99 Inactive", IsActive = false }
        };
    }

    [Fact]
    public void SelectNextQuestion_ShouldNeverSelect_InactiveQuestions()
    {
        var rng = new DeterministicRandomSource(1);
        var engine = new NoveltyEngine(rng);
        var questions = CreateMockQuestions();

        var context = new NoveltyContext();

        for (int i = 0; i < 20; i++)
        {
            var result = engine.SelectNextQuestion(questions, context, isClimaxStep: false, randomSource: new DeterministicRandomSource(i));
            Assert.NotEqual(99, result.Id);
        }
    }

    [Fact]
    public void SelectNextQuestion_ShouldNeverRepeat_QuestionAlreadyAnsweredInJourney()
    {
        var rng = new DeterministicRandomSource(1);
        var engine = new NoveltyEngine(rng);
        var questions = CreateMockQuestions();

        var context = new NoveltyContext
        {
            AnsweredQuestionIdsInJourney = new List<int> { 1, 2, 3 }
        };

        for (int i = 0; i < 20; i++)
        {
            var result = engine.SelectNextQuestion(questions, context, isClimaxStep: false, randomSource: new DeterministicRandomSource(i * 10));
            Assert.DoesNotContain(result.Id, new[] { 1, 2, 3 });
            Assert.Contains(result.Id, new[] { 4, 5 });
        }
    }

    [Fact]
    public void SelectNextQuestion_WhenClimaxStep_ShouldPrioritizeDestinyTheme()
    {
        var engine = new NoveltyEngine(new DeterministicRandomSource());
        var questions = CreateMockQuestions();

        var context = new NoveltyContext();

        // When isClimaxStep is true, question 5 (THEME_DESTINY) has heavily boosted weight (e.g. +200)
        int pickedQ5Count = 0;
        for (int i = 0; i < 30; i++)
        {
            var result = engine.SelectNextQuestion(questions, context, isClimaxStep: true, randomSource: new DeterministicRandomSource(i * 3));
            if (result.Id == 5) pickedQ5Count++;
        }

        Assert.True(pickedQ5Count > 15, "Climax step must strongly favor Destiny theme.");
    }
}
