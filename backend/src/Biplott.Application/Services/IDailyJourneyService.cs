using Biplott.Application.DTOs;

namespace Biplott.Application.Services;

public interface IDailyJourneyService
{
    Task<StartJourneyResponse> StartDailyJourneyAsync(StartJourneyRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<AnswerStepResponse> AnswerDailyStepAsync(Guid journeyId, AnswerStepRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<DailyJourneyDto?> GetTodayDailyJourneyAsync(string gameCode, string? userId = null, string? guestSessionToken = null, CancellationToken cancellationToken = default);
}

public class DailyJourneyDto
{
    public Guid JourneyId { get; set; }
    public string GameCode { get; set; } = string.Empty;
    public string DailyDate { get; set; } = string.Empty;
    public string Status { get; set; } = "InProgress"; // InProgress | Completed
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public List<RevealedNumberDto> Numbers { get; set; } = new();
    public List<DailyJourneyAnswerDto> Answers { get; set; } = new();
    public QuestionDto? ActiveQuestion { get; set; }
}

public class DailyJourneyAnswerDto
{
    public int QuestionId { get; set; }
    public int ChoiceId { get; set; }
    public int StepIndex { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public string ChoiceContent { get; set; } = string.Empty;
    public string ThemeName { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
}
