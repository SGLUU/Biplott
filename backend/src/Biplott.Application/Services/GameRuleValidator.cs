using Biplott.Core.Entities;

namespace Biplott.Application.Services;

public interface IGameRuleValidator
{
    ValidationResult ValidateNumbers(Game game, IReadOnlyList<(int Value, int PoolIndex)> numbers);
}

public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Fail(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
}

public class GameRuleValidator : IGameRuleValidator
{
    public ValidationResult ValidateNumbers(Game game, IReadOnlyList<(int Value, int PoolIndex)> numbers)
    {
        var errors = new List<string>();

        if (game.Pools == null || game.Pools.Count == 0)
        {
            return ValidationResult.Fail($"Trò chơi {game.Name} chưa được cấu hình tập số (Pools).");
        }

        // Group selected numbers by PoolIndex
        var groupedNumbers = numbers.GroupBy(n => n.PoolIndex).ToDictionary(g => g.Key, g => g.Select(x => x.Value).ToList());

        foreach (var pool in game.Pools)
        {
            groupedNumbers.TryGetValue(pool.PoolIndex, out var poolNumbers);
            poolNumbers ??= new List<int>();

            // 1. Check PickCount
            if (poolNumbers.Count != pool.PickCount)
            {
                errors.Add($"Tập số '{pool.Name}' yêu cầu chọn đúng {pool.PickCount} số (hiện có: {poolNumbers.Count}).");
            }

            // 2. Check Range
            foreach (var num in poolNumbers)
            {
                if (num < pool.MinNumber || num > pool.MaxNumber)
                {
                    errors.Add($"Số {num} không hợp lệ trong tập số '{pool.Name}' (dải số hợp lệ: {pool.MinNumber:D2} - {pool.MaxNumber:D2}).");
                }
            }

            // 3. Check Duplicate Constraint within Pool
            if (!pool.AllowDuplicates)
            {
                var duplicates = poolNumbers.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicates.Count > 0)
                {
                    errors.Add($"Tập số '{pool.Name}' không cho phép trùng lặp số: {string.Join(", ", duplicates.Select(d => d.ToString("D2")))}.");
                }
            }
        }

        // 4. Check for invalid PoolIndexes not belonging to this Game
        var validPoolIndices = game.Pools.Select(p => p.PoolIndex).ToHashSet();
        foreach (var poolIndex in groupedNumbers.Keys)
        {
            if (!validPoolIndices.Contains(poolIndex))
            {
                errors.Add($"Tập số có index {poolIndex} không tồn tại trong trò chơi {game.Name}.");
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : new ValidationResult { IsValid = false, Errors = errors };
    }
}
