using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;

namespace Biplott.Application.Services;

public interface INoveltyEngine
{
    QuestionDto SelectNextQuestion(
        IReadOnlyList<Question> allActiveQuestions,
        NoveltyContext context,
        bool isClimaxStep = false,
        IRandomSource? randomSource = null);
}

public class NoveltyEngine : INoveltyEngine
{
    private readonly IRandomSource _defaultRandomSource;

    public NoveltyEngine(IRandomSource randomSource)
    {
        _defaultRandomSource = randomSource;
    }

    public QuestionDto SelectNextQuestion(
        IReadOnlyList<Question> allActiveQuestions,
        NoveltyContext context,
        bool isClimaxStep = false,
        IRandomSource? randomSource = null)
    {
        var rng = randomSource ?? _defaultRandomSource;

        if (allActiveQuestions == null || allActiveQuestions.Count == 0)
        {
            throw new InvalidOperationException("Ngân hàng câu hỏi hiện đang trống.");
        }

        // 1. Filter out questions already answered in this journey
        var answeredInJourney = new HashSet<int>(context.AnsweredQuestionIdsInJourney);
        var candidates = allActiveQuestions
            .Where(q => q.IsActive && !answeredInJourney.Contains(q.Id))
            .ToList();

        // Fallback if all questions have been answered
        if (candidates.Count == 0)
        {
            candidates = allActiveQuestions.Where(q => q.IsActive).ToList();
        }

        // 2. Score candidate questions
        var recentQuestions = new HashSet<int>(context.RecentQuestionIds);
        var recentThemes = new HashSet<int>(context.RecentThemeIds);
        var themesInJourney = new HashSet<int>(context.ThemesUsedInJourney);
        var lastQuestionType = context.QuestionTypesUsedInJourney.LastOrDefault();

        var scored = new List<(Question Question, double Weight)>();

        foreach (var q in candidates)
        {
            double weight = 100.0;

            // Never seen vs recently seen
            if (recentQuestions.Contains(q.Id))
            {
                weight -= 70.0; // RecentlySeenPenalty
            }
            else
            {
                weight += 50.0; // NeverSeenBonus
            }

            // Theme diversity within current journey
            if (themesInJourney.Contains(q.ThemeId))
            {
                weight -= 60.0; // RepeatedThemePenalty
            }

            // Recent theme penalty from previous sessions
            if (recentThemes.Contains(q.ThemeId))
            {
                weight -= 30.0;
            }

            // Question type diversity bonus
            if (context.QuestionTypesUsedInJourney.Count > 0 && q.QuestionType != lastQuestionType)
            {
                weight += 25.0; // QuestionTypeDiversityBonus
            }

            // Climax step boost (for Lotto special pool)
            if (isClimaxStep)
            {
                if (q.Theme?.Code == "THEME_DESTINY" || q.Theme?.Name.Contains("Định mệnh") == true)
                {
                    weight += 500.0;
                }
                else if (q.QuestionType == QuestionType.QuickInstinct)
                {
                    weight += 100.0;
                }
            }

            scored.Add((q, Math.Max(1.0, weight)));
        }

        // 3. Weighted Random Selection
        double totalWeight = scored.Sum(s => s.Weight);
        double roll = rng.NextDouble() * totalWeight;
        double cumulative = 0.0;
        Question selected = scored.Last().Question;

        foreach (var (question, weight) in scored)
        {
            cumulative += weight;
            if (roll <= cumulative)
            {
                selected = question;
                break;
            }
        }

        // 4. Map to QuestionDto
        return MapToDto(selected);
    }

    private static QuestionDto MapToDto(Question q)
    {
        return new QuestionDto
        {
            Id = q.Id,
            ThemeId = q.ThemeId,
            ThemeCode = q.Theme?.Code ?? string.Empty,
            ThemeName = q.Theme?.Name ?? "Chủ đề",
            ThemeIcon = q.Theme?.Icon ?? "🎲",
            QuestionType = q.QuestionType,
            Content = q.Content,
            Subtitle = q.Subtitle,
            MediaUrl = q.MediaUrl,
            Choices = q.Choices
                .Where(c => c.IsActive)
                .OrderBy(c => c.OrderIndex)
                .Select(c => new ChoiceDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    SubContent = c.SubContent,
                    MediaUrl = c.MediaUrl,
                    OrderIndex = c.OrderIndex
                })
                .ToList()
        };
    }
}
