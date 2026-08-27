using Biplott.Application.DTOs;

namespace Biplott.Application.Services;

public interface IRemixService
{
    Task<GenerateLineResponse> QuickRemixAsync(StartRemixJourneyRequest request, CancellationToken cancellationToken = default);
    Task<StartJourneyResponse> StartLuckyRemixAsync(StartRemixJourneyRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<AnswerStepResponse> AnswerLuckyRemixStepAsync(string journeyId, AnswerStepRequest request, string? userId = null, CancellationToken cancellationToken = default);
}
