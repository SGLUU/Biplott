using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Services;

public class LuckyDnaService : ILuckyDnaService
{
    private readonly BiplottDbContext _dbContext;

    public LuckyDnaService(BiplottDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LuckyDnaResponse> GetUserDnaAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Người dùng không tồn tại.");
        }

        var resetAt = user.DnaResetAt;

        // Count historical questions answered after DnaResetAt
        var query = _dbContext.UserQuestionHistories
            .Where(h => h.UserId == userId);

        if (resetAt.HasValue)
        {
            query = query.Where(h => h.AnsweredAt > resetAt.Value);
        }

        var totalAnswers = await query.CountAsync(cancellationToken);

        var response = new LuckyDnaResponse
        {
            TotalAnswers = totalAnswers,
            Status = GetDnaStatus(totalAnswers)
        };

        if (totalAnswers == 0)
        {
            response.Description = "DNA của bạn chưa được hình thành. Hãy chơi ít nhất một lượt Lucky Journey để bắt đầu phân tích.";
            return response;
        }

        // Fetch user profiles
        var profiles = await _dbContext.UserTraitProfiles
            .Include(p => p.Trait)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        var allTraits = await _dbContext.Traits
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);

        var traitScores = allTraits.Select(t =>
        {
            var p = profiles.FirstOrDefault(prof => prof.TraitId == t.Id);
            return new TraitScoreDto
            {
                TraitCode = t.Code,
                TraitName = t.Name,
                Score = p?.NormalizedScore ?? 0,
                SampleCount = p?.SampleCount ?? 0
            };
        }).ToList();

        response.AllTraits = traitScores;
        response.TopTraits = traitScores.OrderByDescending(t => t.Score).Take(3).ToList();
        response.UpdatedAt = profiles.Count > 0 ? profiles.Max(p => p.UpdatedAt) : user.UpdatedAt;

        var (archetype, desc) = CalculateArchetype(traitScores);
        response.Archetype = archetype;
        response.Description = response.Status == "Forming"
            ? "DNA đang hình thành... Vui lòng trả lời thêm để hoàn thiện chân dung vui tâm linh."
            : desc;

        return response;
    }

    public async Task<LuckyDnaResponse> GetGuestDnaAsync(string guestSessionToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(guestSessionToken))
        {
            return new LuckyDnaResponse
            {
                Status = "NotFormed",
                Description = "DNA của bạn chưa được hình thành. Hãy bắt đầu chơi."
            };
        }

        // For guests, we aggregate on the fly from UserQuestionHistories
        var histories = await _dbContext.UserQuestionHistories
            .Include(h => h.Choice)
                .ThenInclude(c => c.ChoiceTraits)
                    .ThenInclude(ct => ct.Trait)
            .Where(h => h.GuestSessionToken == guestSessionToken)
            .ToListAsync(cancellationToken);

        var totalAnswers = histories.Count;

        var response = new LuckyDnaResponse
        {
            TotalAnswers = totalAnswers,
            Status = GetDnaStatus(totalAnswers)
        };

        if (totalAnswers == 0)
        {
            response.Description = "DNA của bạn chưa được hình thành. Hãy chơi ít nhất một lượt Lucky Journey để bắt đầu phân tích.";
            return response;
        }

        // Aggregate in-memory for guest
        var allTraits = await _dbContext.Traits
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);

        var traitAccumulations = new Dictionary<int, (double Weight, int Count)>();
        foreach (var h in histories)
        {
            if (h.Choice?.ChoiceTraits == null) continue;
            foreach (var ct in h.Choice.ChoiceTraits)
            {
                if (!traitAccumulations.ContainsKey(ct.TraitId))
                {
                    traitAccumulations[ct.TraitId] = (0.0, 0);
                }
                var current = traitAccumulations[ct.TraitId];
                traitAccumulations[ct.TraitId] = (current.Weight + ct.Weight, current.Count + 1);
            }
        }

        var traitScores = allTraits.Select(t =>
        {
            traitAccumulations.TryGetValue(t.Id, out var acc);
            var score = acc.Count > 0 ? (int)Math.Round((acc.Weight / acc.Count) * 100.0) : 0;
            return new TraitScoreDto
            {
                TraitCode = t.Code,
                TraitName = t.Name,
                Score = score,
                SampleCount = acc.Count
            };
        }).ToList();

        response.AllTraits = traitScores;
        response.TopTraits = traitScores.OrderByDescending(t => t.Score).Take(3).ToList();
        response.UpdatedAt = histories.Count > 0 ? histories.Max(h => h.AnsweredAt) : DateTime.UtcNow;

        var (archetype, desc) = CalculateArchetype(traitScores);
        response.Archetype = archetype;
        response.Description = response.Status == "Forming"
            ? "DNA đang hình thành... Vui lòng trả lời thêm để hoàn thiện chân dung vui tâm linh."
            : desc;

        return response;
    }

    public async Task ResetUserDnaAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Người dùng không tồn tại.");
        }

        // Update reset timestamp
        user.DnaResetAt = DateTime.UtcNow;

        // Remove calculated profiles
        var profiles = await _dbContext.UserTraitProfiles
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        _dbContext.UserTraitProfiles.RemoveRange(profiles);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateDnaForAnswerAsync(
        string? userId,
        string? guestSessionToken,
        int questionId,
        int choiceId,
        string? journeyId,
        CancellationToken cancellationToken = default)
    {
        // 1. Indempotency check
        if (!string.IsNullOrWhiteSpace(journeyId))
        {
            var alreadyExists = await _dbContext.UserQuestionHistories
                .AnyAsync(h => h.JourneyId == journeyId && h.QuestionId == questionId, cancellationToken);
            if (alreadyExists) return; // Prevent double-counting
        }

        var choice = await _dbContext.QuestionChoices
            .Include(c => c.ChoiceTraits)
                .ThenInclude(ct => ct.Trait)
            .FirstOrDefaultAsync(c => c.Id == choiceId, cancellationToken);
        if (choice == null || choice.QuestionId != questionId) return;

        // 2. Write history record
        var history = new UserQuestionHistory
        {
            UserId = userId,
            GuestSessionToken = guestSessionToken,
            QuestionId = questionId,
            ChoiceId = choiceId,
            RevealedNumber = 0, // In this context we don't strictly need to link number if generated separately
            AnsweredAt = DateTime.UtcNow,
            JourneyId = journeyId
        };

        _dbContext.UserQuestionHistories.Add(history);

        // 3. For authenticated users, update Trait profiles
        if (!string.IsNullOrWhiteSpace(userId) && choice.ChoiceTraits != null)
        {
            foreach (var ct in choice.ChoiceTraits)
            {
                if (!ct.Trait.IsActive) continue;

                var profile = await _dbContext.UserTraitProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.TraitId == ct.TraitId, cancellationToken);

                if (profile == null)
                {
                    profile = new UserTraitProfile
                    {
                        UserId = userId,
                        TraitId = ct.TraitId,
                        AccumulatedWeight = ct.Weight,
                        SampleCount = 1,
                        NormalizedScore = (int)Math.Round(ct.Weight * 100.0),
                        UpdatedAt = DateTime.UtcNow
                    };
                    _dbContext.UserTraitProfiles.Add(profile);
                }
                else
                {
                    profile.AccumulatedWeight += ct.Weight;
                    profile.SampleCount += 1;
                    profile.NormalizedScore = (int)Math.Round((profile.AccumulatedWeight / profile.SampleCount) * 100.0);
                    profile.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GetDnaStatus(int totalAnswers)
    {
        if (totalAnswers == 0) return "NotFormed";
        if (totalAnswers < 5) return "Forming";
        return "Completed";
    }

    private (string Archetype, string Description) CalculateArchetype(List<TraitScoreDto> traits)
    {
        if (traits == null || traits.Count == 0)
        {
            return ("Lữ Khách Đa Nhân Cách", "DNA của bạn chưa thể xác định được phong cách chính.");
        }

        var topTrait = traits.OrderByDescending(t => t.Score).First();

        return topTrait.TraitCode switch
        {
            "RiskTolerance" => ("Chiến Thần Liều Ăn Nhiều", "Bạn sẵn sàng đặt cược tất cả vào những lựa chọn rủi ro nhất. Con số của bạn luôn mang năng lượng bùng nổ."),
            "ChaosEnergy" => ("Kẻ Phá Binh Hệ Hỗn Loạn", "Bạn thích sự bất ổn và luôn chọn những quyết định đảo lộn mọi trật tự thông thường để sinh số."),
            "Intuition" => ("Nhà Ngoại Cảm Vũ Trụ", "Bạn dựa hoàn toàn vào linh cảm tâm linh và năng lượng vũ trụ để dẫn lối cho các con số."),
            "SpiritualVibe" => ("Nhà Ngoại Cảm Vũ Trụ", "Bạn dựa hoàn toàn vào linh cảm tâm linh và năng lượng vũ trụ để dẫn lối cho các con số."),
            "CosmicKarma" => ("Nhà Ngoại Cảm Vũ Trụ", "Bạn dựa hoàn toàn vào linh cảm tâm linh và năng lượng vũ trụ để dẫn lối cho các con số."),
            "Stability" => ("Tín Đồ An Toàn Tuyệt Đối", "Bạn ưa thích sự chắc chắn, an tâm, hạn chế tối đa rủi ro. Con số của bạn mang tính ổn định cao."),
            "Order" => ("Tín Đồ An Toàn Tuyệt Đối", "Bạn ưa thích sự chắc chắn, an tâm, hạn chế tối đa rủi ro. Con số của bạn mang tính ổn định cao."),
            "Patience" => ("Tín Đồ An Toàn Tuyệt Đối", "Bạn ưa thích sự chắc chắn, an tâm, hạn chế tối đa rủi ro. Con số của bạn mang tính ổn định cao."),
            "Independence" => ("Sói Đơn Độc Thích Khám Phá", "Bạn tự đi con đường riêng, không bị ảnh hưởng bởi đám đông hay những quy tắc định sẵn."),
            "Exploration" => ("Sói Đơn Độc Thích Khám Phá", "Bạn tự đi con đường riêng, không bị ảnh hưởng bởi đám đông hay những quy tắc định sẵn."),
            "Emotion" => ("Kẻ Mơ Mộng FOMO", "Lựa chọn của bạn mang nặng yếu tố cảm xúc và hoài niệm. Bạn sợ bị bỏ lại phía sau."),
            "Nostalgia" => ("Kẻ Mơ Mộng FOMO", "Lựa chọn của bạn mang nặng yếu tố cảm xúc và hoài niệm. Bạn sợ bị bỏ lại phía sau."),
            "FOMO" => ("Kẻ Mơ Mộng FOMO", "Lựa chọn của bạn mang nặng yếu tố cảm xúc và hoài niệm. Bạn sợ bị bỏ lại phía sau."),
            "LogicVsInstinct" => ("Nhà Khảo Cổ Bản Năng", "Bạn luôn cân nhắc giữa logic và linh cảm tự nhiên, tạo ra những con số vô cùng tinh quái."),
            _ => ("Lữ Khách Đa Nhân Cách", "DNA của bạn là một tổ hợp phức tạp của nhiều luồng năng lượng trái ngược.")
        };
    }
}
