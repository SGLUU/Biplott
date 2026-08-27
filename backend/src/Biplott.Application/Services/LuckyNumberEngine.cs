using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;

namespace Biplott.Application.Services;

public interface ILuckyNumberEngine
{
    RevealedNumberDto GenerateLuckyNumber(
        GamePool pool,
        QuestionChoice choice,
        HashSet<int> excludedNumbersInPool,
        List<int> previousNumbersInLine,
        IRandomSource? randomSource = null);
}

public class LuckyNumberEngine : ILuckyNumberEngine
{
    private readonly IRandomSource _defaultRandomSource;
    private readonly IEngineConfigService? _configService;

    private static readonly HashSet<int> Primes = new()
    {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61
    };

    public LuckyNumberEngine(IRandomSource randomSource, IEngineConfigService? configService = null)
    {
        _defaultRandomSource = randomSource;
        _configService = configService;
    }

    public RevealedNumberDto GenerateLuckyNumber(
        GamePool pool,
        QuestionChoice choice,
        HashSet<int> excludedNumbersInPool,
        List<int> previousNumbersInLine,
        IRandomSource? randomSource = null)
    {
        var rng = randomSource ?? _defaultRandomSource;
        var config = _configService?.GetCurrentLuckyConfig() ?? new LuckyEngineConfigDto();

        // 1. Determine valid candidate numbers in pool
        var candidates = Enumerable.Range(pool.MinNumber, pool.MaxNumber - pool.MinNumber + 1)
            .Where(n => pool.AllowDuplicates || !excludedNumbersInPool.Contains(n))
            .ToList();

        // Fallback if all numbers were excluded
        if (candidates.Count == 0)
        {
            candidates = Enumerable.Range(pool.MinNumber, pool.MaxNumber - pool.MinNumber + 1).ToList();
        }

        // 2. Extract traits from choice
        var choiceTraits = choice.ChoiceTraits?.ToList() ?? new List<ChoiceTrait>();
        var dominantTrait = choiceTraits
            .OrderByDescending(ct => Math.Abs(ct.Weight))
            .FirstOrDefault()?.Trait?.Name ?? "Trực giác";

        // 3. Score every candidate
        var scoredCandidates = new List<(int Number, double Weight)>();

        foreach (var num in candidates)
        {
            double affinity = CalculateTotalAffinity(num, pool, choiceTraits, previousNumbersInLine);
            double noise = (rng.NextDouble() * 2.0 - 1.0) * config.NoiseMagnitude;

            // W(n) = max(minWeight, BaseWeight + affinityMultiplier * Affinity + Noise)
            double finalWeight = Math.Max(config.MinWeight, config.BaseWeight + (config.TraitAffinityMultiplier * affinity) + noise);
            scoredCandidates.Add((num, finalWeight));
        }

        // 4. Weighted Random Sampling (Roulette Wheel Selection)
        double totalWeight = scoredCandidates.Sum(c => c.Weight);
        double roll = rng.NextDouble() * totalWeight;
        double cumulative = 0.0;
        int selectedNumber = scoredCandidates.Last().Number;

        foreach (var (candidateNum, weight) in scoredCandidates)
        {
            cumulative += weight;
            if (roll <= cumulative)
            {
                selectedNumber = candidateNum;
                break;
            }
        }

        // 5. Generate commentary & explanation
        string explanation = GenerateExplanation(selectedNumber, choice, dominantTrait);

        return new RevealedNumberDto
        {
            Value = selectedNumber,
            PoolIndex = pool.PoolIndex,
            Source = NumberSource.Lucky,
            Explanation = explanation,
            DominantTrait = dominantTrait,
            ThemeName = choice.Question?.Theme?.Name,
            QuestionText = choice.Question?.Content,
            ChoiceText = choice.Content,
            MetadataJson = $"{{\"questionId\":{choice.QuestionId},\"choiceId\":{choice.Id},\"dominantTrait\":\"{dominantTrait}\"}}"
        };
    }

    private static double CalculateTotalAffinity(
        int num,
        GamePool pool,
        List<ChoiceTrait> choiceTraits,
        List<int> previousNumbers)
    {
        if (choiceTraits.Count == 0) return 0.0;

        double totalAffinity = 0.0;
        double normMagnitude = (double)(num - pool.MinNumber) / Math.Max(1, pool.MaxNumber - pool.MinNumber);
        bool isEven = num % 2 == 0;
        bool isPrime = Primes.Contains(num);
        int digitRoot = 1 + ((num - 1) % 9);
        int digitSum = (num / 10) + (num % 10);
        double minDistance = previousNumbers.Count > 0
            ? previousNumbers.Min(p => Math.Abs(num - p))
            : (pool.MaxNumber - pool.MinNumber) / 3.0;

        foreach (var ct in choiceTraits)
        {
            string traitCode = ct.Trait?.Code ?? string.Empty;
            double w = ct.Weight;

            double traitAffinity = traitCode switch
            {
                "RiskTolerance" => (normMagnitude > 0.65 ? 1.0 : -0.5) + (isPrime ? 0.7 : 0.0),
                "ChaosEnergy" => (isPrime ? 1.0 : 0.0) + (normMagnitude > 0.85 || normMagnitude < 0.15 ? 0.8 : -0.3),
                "Intuition" or "SpiritualVibe" or "CosmicKarma" => (digitRoot is 3 or 7 or 9 ? 1.0 : -0.2) + (isPrime ? 0.5 : 0.0),
                "Stability" or "Order" or "Patience" => (isEven ? 0.8 : -0.4) + (num % 5 == 0 ? 0.7 : 0.0) + (num % 11 == 0 ? 0.9 : 0.0),
                "Independence" or "Exploration" => (minDistance >= 6 ? 1.0 : -0.6) + (normMagnitude > 0.5 ? 0.5 : 0.0),
                "Emotion" or "Nostalgia" or "FOMO" => (normMagnitude is >= 0.3 and <= 0.7 ? 0.9 : -0.3) + (digitRoot is 2 or 4 or 8 ? 0.6 : 0.0),
                "LogicVsInstinct" => (isEven && !isPrime ? 0.8 : -0.5),
                _ => (isEven ? 0.3 : -0.3)
            };

            totalAffinity += w * traitAffinity;
        }

        return Math.Clamp(totalAffinity, -3.0, 3.0);
    }

    private static string GenerateExplanation(int number, QuestionChoice choice, string dominantTrait)
    {
        var cleanChoice = choice.Content.Trim();
        var numStr = number.ToString("D2");

        var templates = new[]
        {
            $"Lựa chọn \"{cleanChoice}\" mang thiên hướng {dominantTrait}, kéo con số {numStr} bước ra ánh sáng!",
            $"Con số {numStr} cộng hưởng mạnh mẽ với năng lượng {dominantTrait} từ quyết định của bạn.",
            $"Số {numStr} xuất hiện: Một nét chấm phá đậm chất {dominantTrait} và tự do.",
            $"Từ lựa chọn \"{cleanChoice}\", vũ trụ Bịp lót gửi gắm con số {numStr} đầy bất ngờ."
        };

        int idx = Math.Abs(number ^ cleanChoice.Length) % templates.Length;
        return templates[idx];
    }
}
