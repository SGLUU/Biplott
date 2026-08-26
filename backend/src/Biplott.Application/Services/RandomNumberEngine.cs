using System.Security.Cryptography;
using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Enums;

namespace Biplott.Application.Services;

public interface IRandomNumberEngine
{
    GenerateLineResponse GenerateLine(Game game, RandomStrategy strategy, List<int>? excludedNumbers = null, List<GeneratedNumberDto>? currentNumbers = null);
    List<GeneratedNumberDto> GeneratePoolNumbers(GamePool pool, RandomStrategy strategy, HashSet<int>? excludedNumbers = null);
    List<GeneratedNumberDto> GeneratePoolNumbers(GamePool pool, int count, RandomStrategy strategy, HashSet<int>? excludedNumbers = null);
}

public class RandomNumberEngine : IRandomNumberEngine
{
    public GenerateLineResponse GenerateLine(Game game, RandomStrategy strategy, List<int>? excludedNumbers = null, List<GeneratedNumberDto>? currentNumbers = null)
    {
        var resultNumbers = new List<GeneratedNumberDto>();
        var pools = game.Pools.OrderBy(p => p.PoolIndex).ToList();
        var globalExcluded = excludedNumbers != null ? new HashSet<int>(excludedNumbers) : new HashSet<int>();

        // If current numbers are provided, preserve them
        var currentByPool = currentNumbers?
            .GroupBy(n => n.PoolIndex)
            .ToDictionary(g => g.Key, g => g.ToList()) ?? new Dictionary<int, List<GeneratedNumberDto>>();

        foreach (var pool in pools)
        {
            currentByPool.TryGetValue(pool.PoolIndex, out var existingInPool);
            existingInPool ??= new List<GeneratedNumberDto>();

            var poolExcluded = new HashSet<int>(globalExcluded);
            foreach (var existing in existingInPool)
            {
                poolExcluded.Add(existing.Value);
            }

            var neededCount = pool.PickCount - existingInPool.Count;
            if (neededCount > 0)
            {
                var generatedForPool = GeneratePoolNumbersWithCount(pool, strategy, poolExcluded, neededCount);
                resultNumbers.AddRange(existingInPool);
                resultNumbers.AddRange(generatedForPool);
            }
            else
            {
                resultNumbers.AddRange(existingInPool);
            }
        }

        // Sort numbers within each pool ascending for presentation
        var orderedNumbers = resultNumbers
            .OrderBy(n => n.PoolIndex)
            .ThenBy(n => n.Value)
            .ToList();

        var (strategyName, commentary) = GetStrategyMetadata(strategy);

        return new GenerateLineResponse
        {
            Strategy = strategy,
            StrategyName = strategyName,
            Numbers = orderedNumbers,
            Commentary = commentary
        };
    }

    public List<GeneratedNumberDto> GeneratePoolNumbers(GamePool pool, RandomStrategy strategy, HashSet<int>? excludedNumbers = null)
    {
        return GeneratePoolNumbersWithCount(pool, strategy, excludedNumbers ?? new HashSet<int>(), pool.PickCount);
    }

    public List<GeneratedNumberDto> GeneratePoolNumbers(GamePool pool, int count, RandomStrategy strategy, HashSet<int>? excludedNumbers = null)
    {
        return GeneratePoolNumbersWithCount(pool, strategy, excludedNumbers ?? new HashSet<int>(), count);
    }

    private List<GeneratedNumberDto> GeneratePoolNumbersWithCount(GamePool pool, RandomStrategy strategy, HashSet<int> excludedNumbers, int count)
    {
        if (count <= 0) return new List<GeneratedNumberDto>();

        return strategy switch
        {
            RandomStrategy.Balanced => GenerateBalanced(pool, excludedNumbers, count),
            RandomStrategy.Spread => GenerateSpread(pool, excludedNumbers, count),
            RandomStrategy.Surprise => GenerateSurprise(pool, excludedNumbers, count),
            _ => GeneratePureRandom(pool, excludedNumbers, count)
        };
    }

    private List<GeneratedNumberDto> GeneratePureRandom(GamePool pool, HashSet<int> excludedNumbers, int count)
    {
        var available = Enumerable.Range(pool.MinNumber, pool.MaxNumber - pool.MinNumber + 1)
            .Where(n => pool.AllowDuplicates || !excludedNumbers.Contains(n))
            .ToList();

        if (available.Count < count)
        {
            // If excluded list left too few, reset exclusions for pool
            available = Enumerable.Range(pool.MinNumber, pool.MaxNumber - pool.MinNumber + 1).ToList();
        }

        var selected = new List<int>();
        for (int i = 0; i < count; i++)
        {
            if (available.Count == 0) break;
            int idx = RandomNumberGenerator.GetInt32(0, available.Count);
            selected.Add(available[idx]);
            if (!pool.AllowDuplicates)
            {
                available.RemoveAt(idx);
            }
        }

        return selected.Select(val => new GeneratedNumberDto
        {
            Value = val,
            PoolIndex = pool.PoolIndex,
            Source = NumberSource.Random
        }).ToList();
    }

    private List<GeneratedNumberDto> GenerateBalanced(GamePool pool, HashSet<int> excludedNumbers, int count)
    {
        if (count <= 1 || pool.MaxNumber - pool.MinNumber < 4)
        {
            return GeneratePureRandom(pool, excludedNumbers, count);
        }

        int midPoint = pool.MinNumber + (pool.MaxNumber - pool.MinNumber) / 2;
        var evens = new List<int>();
        var odds = new List<int>();

        for (int i = pool.MinNumber; i <= pool.MaxNumber; i++)
        {
            if (!pool.AllowDuplicates && excludedNumbers.Contains(i)) continue;
            if (i % 2 == 0) evens.Add(i);
            else odds.Add(i);
        }

        int targetEvens = count / 2;
        int targetOdds = count - targetEvens;

        // If random coin toss, occasionally switch target
        if (RandomNumberGenerator.GetInt32(0, 2) == 1 && count % 2 != 0)
        {
            (targetEvens, targetOdds) = (targetOdds, targetEvens);
        }

        var selected = new List<int>();

        // Pick Evens
        for (int i = 0; i < targetEvens && evens.Count > 0; i++)
        {
            int idx = RandomNumberGenerator.GetInt32(0, evens.Count);
            selected.Add(evens[idx]);
            evens.RemoveAt(idx);
        }

        // Pick Odds
        for (int i = 0; i < targetOdds && odds.Count > 0; i++)
        {
            int idx = RandomNumberGenerator.GetInt32(0, odds.Count);
            selected.Add(odds[idx]);
            odds.RemoveAt(idx);
        }

        // If not enough numbers, fill with pure random
        if (selected.Count < count)
        {
            var remainingExcluded = new HashSet<int>(excludedNumbers);
            foreach (var s in selected) remainingExcluded.Add(s);
            var remaining = GeneratePureRandom(pool, remainingExcluded, count - selected.Count);
            selected.AddRange(remaining.Select(r => r.Value));
        }

        return selected.Take(count).Select(val => new GeneratedNumberDto
        {
            Value = val,
            PoolIndex = pool.PoolIndex,
            Source = NumberSource.Random
        }).ToList();
    }

    private List<GeneratedNumberDto> GenerateSpread(GamePool pool, HashSet<int> excludedNumbers, int count)
    {
        if (count <= 1 || (pool.MaxNumber - pool.MinNumber + 1) < count)
        {
            return GeneratePureRandom(pool, excludedNumbers, count);
        }

        int totalRange = pool.MaxNumber - pool.MinNumber + 1;
        double bucketSize = (double)totalRange / count;
        var selected = new List<int>();

        for (int i = 0; i < count; i++)
        {
            int bucketStart = pool.MinNumber + (int)Math.Floor(i * bucketSize);
            int bucketEnd = (i == count - 1)
                ? pool.MaxNumber
                : pool.MinNumber + (int)Math.Floor((i + 1) * bucketSize) - 1;

            if (bucketEnd < bucketStart) bucketEnd = bucketStart;

            var bucketNumbers = Enumerable.Range(bucketStart, bucketEnd - bucketStart + 1)
                .Where(n => pool.AllowDuplicates || (!excludedNumbers.Contains(n) && !selected.Contains(n)))
                .ToList();

            if (bucketNumbers.Count > 0)
            {
                int idx = RandomNumberGenerator.GetInt32(0, bucketNumbers.Count);
                selected.Add(bucketNumbers[idx]);
            }
        }

        // If any bucket was empty, fill remaining
        if (selected.Count < count)
        {
            var remainingExcluded = new HashSet<int>(excludedNumbers);
            foreach (var s in selected) remainingExcluded.Add(s);
            var remaining = GeneratePureRandom(pool, remainingExcluded, count - selected.Count);
            selected.AddRange(remaining.Select(r => r.Value));
        }

        return selected.Take(count).Select(val => new GeneratedNumberDto
        {
            Value = val,
            PoolIndex = pool.PoolIndex,
            Source = NumberSource.Random
        }).ToList();
    }

    private List<GeneratedNumberDto> GenerateSurprise(GamePool pool, HashSet<int> excludedNumbers, int count)
    {
        if (count <= 1 || (pool.MaxNumber - pool.MinNumber + 1) < count)
        {
            return GeneratePureRandom(pool, excludedNumbers, count);
        }

        var selected = new List<int>();
        int patternType = RandomNumberGenerator.GetInt32(0, 3);

        if (patternType == 0 && count >= 3)
        {
            // Pattern: Consecutive Cluster (e.g. 2 consecutive numbers + spread remainder)
            int maxStart = pool.MaxNumber - 1;
            int start = RandomNumberGenerator.GetInt32(pool.MinNumber, Math.Max(pool.MinNumber + 1, maxStart));
            if (!excludedNumbers.Contains(start) && !excludedNumbers.Contains(start + 1))
            {
                selected.Add(start);
                selected.Add(start + 1);
            }
        }
        else if (patternType == 1 && count >= 2)
        {
            // Pattern: Same ending digit (e.g. 07, 27)
            int lastDigit = RandomNumberGenerator.GetInt32(0, 10);
            var matching = Enumerable.Range(pool.MinNumber, pool.MaxNumber - pool.MinNumber + 1)
                .Where(n => n % 10 == lastDigit && !excludedNumbers.Contains(n))
                .ToList();

            if (matching.Count >= 2)
            {
                int i1 = RandomNumberGenerator.GetInt32(0, matching.Count);
                selected.Add(matching[i1]);
                matching.RemoveAt(i1);
                int i2 = RandomNumberGenerator.GetInt32(0, matching.Count);
                selected.Add(matching[i2]);
            }
        }

        // Fill the rest with Spread random
        var currentExcluded = new HashSet<int>(excludedNumbers);
        foreach (var s in selected) currentExcluded.Add(s);

        int needed = count - selected.Count;
        if (needed > 0)
        {
            var remainder = GenerateSpread(pool, currentExcluded, needed);
            selected.AddRange(remainder.Select(r => r.Value));
        }

        return selected.Take(count).Select(val => new GeneratedNumberDto
        {
            Value = val,
            PoolIndex = pool.PoolIndex,
            Source = NumberSource.Random
        }).ToList();
    }

    private static (string Name, string Commentary) GetStrategyMetadata(RandomStrategy strategy)
    {
        return strategy switch
        {
            RandomStrategy.PureRandom => (
                "Pure Random",
                "Thần Tài ngẫu nhiên thuần khiết — Vạn sự tùy duyên, con số nào cũng có cơ hội bước ra ánh sáng!"
            ),
            RandomStrategy.Balanced => (
                "Balanced",
                "Thần Tài cân bằng âm dương — Chẵn lẻ hài hòa, cao thấp vuông tròn cho tâm hồn thanh thản!"
            ),
            RandomStrategy.Spread => (
                "Spread",
                "Thần Tài phân tán may mắn — Trải đều dải số khắp các cung bậc, không co cụm một nơi!"
            ),
            RandomStrategy.Surprise => (
                "Surprise",
                "Thần Tài bất ngờ — Dãy số với cấu trúc độc lạ, phá vỡ mọi quy chuẩn suy nghĩ thông thường!"
            ),
            _ => ("Random", "Thần Tài ban số — Vui là chính, nát là chuyện bình thường!")
        };
    }
}
