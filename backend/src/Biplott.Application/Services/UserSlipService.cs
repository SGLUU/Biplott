using System.Text.Json;
using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Core.Interfaces;

namespace Biplott.Application.Services;

public interface IUserSlipService
{
    Task<SavedSlipSummaryDto> SaveSlipAsync(string userId, SaveSlipRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<SavedSlipSummaryDto>> GetUserSlipsAsync(string userId, int page = 1, int pageSize = 10, bool isFavoriteOnly = false, CancellationToken cancellationToken = default);
    Task<SavedSlipDetailDto> GetSlipDetailAsync(string userId, Guid slipId, CancellationToken cancellationToken = default);
    Task<ToggleFavoriteResponse> ToggleFavoriteAsync(string userId, Guid slipId, CancellationToken cancellationToken = default);
    Task DeleteSlipAsync(string userId, Guid slipId, CancellationToken cancellationToken = default);
    Task<PagedResult<UserActivityDto>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task LogActivityAsync(string userId, int gameId, string activityType, string title, string summary, string? numbersJson = null, CancellationToken cancellationToken = default);
}

public class UserSlipService : IUserSlipService
{
    private readonly ISlipRepository _slipRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IUserActivityRepository _activityRepository;
    private readonly IGameRuleValidator _validator;

    public UserSlipService(
        ISlipRepository slipRepository,
        IGameRepository gameRepository,
        IUserActivityRepository activityRepository,
        IGameRuleValidator validator)
    {
        _slipRepository = slipRepository;
        _gameRepository = gameRepository;
        _activityRepository = activityRepository;
        _validator = validator;
    }

    public async Task<SavedSlipSummaryDto> SaveSlipAsync(
        string userId,
        SaveSlipRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực.");
        }

        var game = await _gameRepository.GetByCodeAsync(request.GameCode, cancellationToken);
        if (game == null)
        {
            throw new ArgumentException($"Không tìm thấy trò chơi '{request.GameCode}'.");
        }

        var completedLines = request.Lines
            .Where(l => l.Numbers != null && l.Numbers.Count > 0)
            .ToList();

        if (completedLines.Count == 0)
        {
            throw new ArgumentException("Phiếu không có dòng nào hoàn chỉnh để lưu.");
        }

        // Validate all completed lines against game rules
        foreach (var line in completedLines)
        {
            var tuples = line.Numbers.Select(n => (n.Value, n.PoolIndex)).ToList();
            var validation = _validator.ValidateNumbers(game, tuples);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors);
                throw new ArgumentException($"Dòng {line.LineLabel} không hợp lệ: {errors}");
            }
        }

        var slipCode = !string.IsNullOrWhiteSpace(request.SlipCode)
            ? request.SlipCode.Trim()
            : $"BIP-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var slip = new Slip
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GameId = game.Id,
            SlipCode = slipCode,
            Title = !string.IsNullOrWhiteSpace(request.Title)
                ? request.Title.Trim()
                : $"Vé {game.Name} - {DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm}",
            IsFavorite = request.IsFavorite,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var lineReq in completedLines)
        {
            var slipLine = new SlipLine
            {
                Id = Guid.NewGuid(),
                SlipId = slip.Id,
                LineLabel = lineReq.LineLabel,
                Status = SlipLineStatus.Complete,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var sortedNumbers = lineReq.Numbers
                .OrderBy(n => n.PoolIndex)
                .ThenBy(n => n.Value)
                .ToList();

            int order = 0;
            foreach (var numReq in sortedNumbers)
            {
                slipLine.Numbers.Add(new SlipLineNumber
                {
                    Id = Guid.NewGuid(),
                    SlipLineId = slipLine.Id,
                    Value = numReq.Value,
                    PoolIndex = numReq.PoolIndex,
                    Source = numReq.Source,
                    OrderIndex = order++,
                    MetadataJson = numReq.MetadataJson
                });
            }

            slip.Lines.Add(slipLine);
        }

        await _slipRepository.AddAsync(slip, cancellationToken);

        // Log user activity
        await LogActivityAsync(
            userId,
            game.Id,
            "SavedSlip",
            $"Đã lưu vé {game.Name} ({slip.Lines.Count} dòng)",
            $"Mã vé: {slip.SlipCode} • Đã lưu vào danh sách của tôi",
            null,
            cancellationToken);

        return MapToSummary(slip, game);
    }

    public async Task<PagedResult<SavedSlipSummaryDto>> GetUserSlipsAsync(
        string userId,
        int page = 1,
        int pageSize = 10,
        bool isFavoriteOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var (items, totalCount) = await _slipRepository.GetUserSlipsPagedAsync(
            userId,
            page,
            pageSize,
            isFavoriteOnly,
            cancellationToken);

        var dtos = items.Select(s => MapToSummary(s, s.Game)).ToList();

        return new PagedResult<SavedSlipSummaryDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SavedSlipDetailDto> GetSlipDetailAsync(
        string userId,
        Guid slipId,
        CancellationToken cancellationToken = default)
    {
        var slip = await _slipRepository.GetByIdAsync(slipId, cancellationToken);
        if (slip == null || slip.UserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy vé số hoặc bạn không có quyền xem vé này.");
        }

        var lines = new List<SavedSlipLineDetailDto>();
        var luckyStories = new List<LuckyStoryDto>();

        foreach (var line in slip.Lines.OrderBy(l => l.LineLabel))
        {
            var numbers = line.Numbers
                .OrderBy(n => n.PoolIndex)
                .ThenBy(n => n.Value)
                .Select(n => new GeneratedNumberDto
                {
                    Value = n.Value,
                    PoolIndex = n.PoolIndex,
                    Source = n.Source,
                    MetadataJson = n.MetadataJson
                })
                .ToList();

            var uniqueSources = numbers.Select(n => n.Source).Distinct().ToList();
            var derivedMode = uniqueSources.Count > 1
                ? "Mixed"
                : uniqueSources.FirstOrDefault().ToString();

            lines.Add(new SavedSlipLineDetailDto
            {
                Id = line.Id,
                LineLabel = line.LineLabel,
                Status = line.Status,
                Numbers = numbers,
                DerivedMode = derivedMode
            });

            // Parse Lucky Stories from metadata
            foreach (var num in line.Numbers.Where(n => n.Source == NumberSource.Lucky && !string.IsNullOrWhiteSpace(n.MetadataJson)).OrderBy(n => n.PoolIndex).ThenBy(n => n.Value))
            {
                try
                {
                    using var doc = JsonDocument.Parse(num.MetadataJson!);
                    var root = doc.RootElement;

                    var themeName = root.TryGetProperty("themeName", out var tn) ? tn.GetString() ?? "" : "";
                    var qText = root.TryGetProperty("questionText", out var qt) ? qt.GetString() ?? "" : "";
                    var cText = root.TryGetProperty("choiceText", out var ct) ? ct.GetString() ?? "" : "";
                    var exp = root.TryGetProperty("explanation", out var ex) ? ex.GetString() ?? "" : "";
                    var trait = root.TryGetProperty("dominantTrait", out var dt) ? dt.GetString() : null;

                    luckyStories.Add(new LuckyStoryDto
                    {
                        LineLabel = line.LineLabel,
                        NumberValue = num.Value,
                        PoolIndex = num.PoolIndex,
                        ThemeName = themeName,
                        QuestionText = qText,
                        ChoiceText = cText,
                        Explanation = exp,
                        DominantTrait = trait
                    });
                }
                catch
                {
                    // Ignore parsing error for corrupted metadata
                }
            }
        }

        return new SavedSlipDetailDto
        {
            Id = slip.Id,
            GameCode = slip.Game.Code,
            GameName = slip.Game.Name,
            SlipCode = slip.SlipCode,
            Title = slip.Title,
            IsFavorite = slip.IsFavorite,
            CreatedAt = slip.CreatedAt,
            UpdatedAt = slip.UpdatedAt,
            Lines = lines,
            LuckyStories = luckyStories
        };
    }

    public async Task<ToggleFavoriteResponse> ToggleFavoriteAsync(
        string userId,
        Guid slipId,
        CancellationToken cancellationToken = default)
    {
        var slip = await _slipRepository.GetByIdAsync(slipId, cancellationToken);
        if (slip == null || slip.UserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy vé số hoặc bạn không có quyền thao tác.");
        }

        slip.IsFavorite = !slip.IsFavorite;
        slip.UpdatedAt = DateTime.UtcNow;
        await _slipRepository.UpdateAsync(slip, cancellationToken);

        return new ToggleFavoriteResponse
        {
            SlipId = slip.Id,
            IsFavorite = slip.IsFavorite
        };
    }

    public async Task DeleteSlipAsync(
        string userId,
        Guid slipId,
        CancellationToken cancellationToken = default)
    {
        var slip = await _slipRepository.GetByIdAsync(slipId, cancellationToken);
        if (slip == null || slip.UserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy vé số hoặc bạn không có quyền xóa vé này.");
        }

        await _slipRepository.DeleteAsync(slip, cancellationToken);
    }

    public async Task<PagedResult<UserActivityDto>> GetUserHistoryAsync(
        string userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var (items, totalCount) = await _activityRepository.GetUserHistoryPagedAsync(
            userId,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(a => new UserActivityDto
        {
            Id = a.Id,
            GameCode = a.Game.Code,
            GameName = a.Game.Name,
            ActivityType = a.ActivityType,
            Title = a.Title,
            Summary = a.Summary,
            NumbersJson = a.NumbersJson,
            CreatedAt = a.CreatedAt
        }).ToList();

        return new PagedResult<UserActivityDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task LogActivityAsync(
        string userId,
        int gameId,
        string activityType,
        string title,
        string summary,
        string? numbersJson = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;

        var activity = new UserActivityHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GameId = gameId,
            ActivityType = activityType,
            Title = title,
            Summary = summary,
            NumbersJson = numbersJson,
            CreatedAt = DateTime.UtcNow
        };

        await _activityRepository.AddAsync(activity, cancellationToken);
    }

    private static SavedSlipSummaryDto MapToSummary(Slip slip, Game game)
    {
        var lines = slip.Lines.OrderBy(l => l.LineLabel).Select(l =>
        {
            var numbers = l.Numbers
                .OrderBy(n => n.PoolIndex)
                .ThenBy(n => n.Value)
                .Select(n => new GeneratedNumberDto
                {
                    Value = n.Value,
                    PoolIndex = n.PoolIndex,
                    Source = n.Source,
                    MetadataJson = n.MetadataJson
                })
                .ToList();

            var uniqueSources = numbers.Select(n => n.Source).Distinct().ToList();
            var derivedMode = uniqueSources.Count > 1
                ? "Mixed"
                : uniqueSources.FirstOrDefault().ToString();

            return new SavedSlipLineSummaryDto
            {
                LineLabel = l.LineLabel,
                Numbers = numbers,
                DerivedMode = derivedMode
            };
        }).ToList();

        return new SavedSlipSummaryDto
        {
            Id = slip.Id,
            GameCode = game?.Code ?? "",
            GameName = game?.Name ?? "",
            SlipCode = slip.SlipCode,
            Title = slip.Title,
            IsFavorite = slip.IsFavorite,
            CompletedLineCount = slip.Lines.Count,
            CreatedAt = slip.CreatedAt,
            Lines = lines
        };
    }
}
