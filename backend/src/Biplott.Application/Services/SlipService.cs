using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;

namespace Biplott.Application.Services;

public interface ISlipService
{
    Task<ValidateLineResponse> ValidateLineAsync(ValidateLineRequest request, CancellationToken cancellationToken = default);
    Task<GenerateLineResponse> GenerateLineAsync(GenerateLineRequest request, CancellationToken cancellationToken = default);
    Task<GenerateSlipResponse> GenerateSlipAsync(GenerateSlipRequest request, CancellationToken cancellationToken = default);
}

public class SlipService : ISlipService
{
    private static readonly string[] StandardLineLabels = { "A", "B", "C", "D", "E", "F" };
    private readonly IGameRepository _gameRepository;
    private readonly IGameRuleValidator _ruleValidator;
    private readonly IRandomNumberEngine _randomEngine;

    public SlipService(
        IGameRepository gameRepository,
        IGameRuleValidator ruleValidator,
        IRandomNumberEngine randomEngine)
    {
        _gameRepository = gameRepository;
        _ruleValidator = ruleValidator;
        _randomEngine = randomEngine;
    }

    public async Task<ValidateLineResponse> ValidateLineAsync(ValidateLineRequest request, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByCodeAsync(request.GameCode, cancellationToken);
        if (game == null)
        {
            return new ValidateLineResponse
            {
                IsValid = false,
                Errors = new List<string> { $"Không tìm thấy trò chơi có mã '{request.GameCode}'." }
            };
        }

        var numberTuples = request.Numbers.Select(n => (n.Value, n.PoolIndex)).ToList();
        var validation = _ruleValidator.ValidateNumbers(game, numberTuples);

        return new ValidateLineResponse
        {
            IsValid = validation.IsValid,
            Errors = validation.Errors
        };
    }

    public async Task<GenerateLineResponse> GenerateLineAsync(GenerateLineRequest request, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByCodeAsync(request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi có mã '{request.GameCode}'.");
        }

        return _randomEngine.GenerateLine(
            game,
            request.Strategy,
            request.ExcludedNumbers,
            request.CurrentNumbers);
    }

    public async Task<GenerateSlipResponse> GenerateSlipAsync(GenerateSlipRequest request, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByCodeAsync(request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi có mã '{request.GameCode}'.");
        }

        var fillAll = string.Equals(request.FillMode, "All", StringComparison.OrdinalIgnoreCase);
        var existingDict = request.ExistingLines?
            .ToDictionary(l => l.LineLabel, l => l, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, SlipLineDto>();

        var resultLines = new List<SlipLineDto>();
        var generatedSignatures = new HashSet<string>();

        // Collect signatures of existing lines to avoid duplicates
        if (!fillAll)
        {
            foreach (var line in existingDict.Values)
            {
                if (line.Status == SlipLineStatus.Complete && line.Numbers.Count > 0)
                {
                    generatedSignatures.Add(GetLineSignature(line.Numbers));
                }
            }
        }

        foreach (var label in StandardLineLabels)
        {
            existingDict.TryGetValue(label, out var existingLine);

            if (!fillAll && existingLine != null && existingLine.Status == SlipLineStatus.Complete && existingLine.Numbers.Count > 0)
            {
                // Preserve complete line
                resultLines.Add(existingLine);
                continue;
            }

            // Generate new line, ensuring uniqueness across the slip
            GenerateLineResponse? newLine = null;
            int attempts = 0;

            while (attempts < 10)
            {
                attempts++;
                var candidate = _randomEngine.GenerateLine(game, request.Strategy);
                var sig = GetLineSignature(candidate.Numbers);
                if (!generatedSignatures.Contains(sig))
                {
                    generatedSignatures.Add(sig);
                    newLine = candidate;
                    break;
                }
                if (attempts == 10)
                {
                    newLine = candidate; // Fallback if saturated
                }
            }

            resultLines.Add(new SlipLineDto
            {
                LineLabel = label,
                Status = SlipLineStatus.Complete,
                Numbers = newLine!.Numbers.OrderBy(n => n.PoolIndex).ThenBy(n => n.Value).ToList(),
                Strategy = request.Strategy,
                Commentary = newLine.Commentary
            });
        }

        return new GenerateSlipResponse
        {
            GameCode = game.Code,
            Strategy = request.Strategy,
            Lines = resultLines,
            Commentary = $"Đã tạo ngẫu nhiên toàn bộ phiếu theo chiến lược '{request.Strategy}'."
        };
    }

    private static string GetLineSignature(IEnumerable<GeneratedNumberDto> numbers)
    {
        return string.Join("|", numbers.OrderBy(n => n.PoolIndex).ThenBy(n => n.Value).Select(n => $"{n.PoolIndex}:{n.Value}"));
    }
}
