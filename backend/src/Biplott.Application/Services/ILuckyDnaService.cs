using Biplott.Application.DTOs;

namespace Biplott.Application.Services;

public interface ILuckyDnaService
{
    Task<LuckyDnaResponse> GetUserDnaAsync(string userId, CancellationToken cancellationToken = default);
    Task<LuckyDnaResponse> GetGuestDnaAsync(string guestSessionToken, CancellationToken cancellationToken = default);
    Task ResetUserDnaAsync(string userId, CancellationToken cancellationToken = default);
    Task UpdateDnaForAnswerAsync(string? userId, string? guestSessionToken, int questionId, int choiceId, string? journeyId, CancellationToken cancellationToken = default);
}
