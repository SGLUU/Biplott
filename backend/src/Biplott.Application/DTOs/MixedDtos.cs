using Biplott.Core.Enums;

namespace Biplott.Application.DTOs;

public class GenerateRandomSlotRequest
{
    public string GameCode { get; set; } = string.Empty;
    public int PoolIndex { get; set; } = 0;
    public RandomStrategy Strategy { get; set; } = RandomStrategy.PureRandom;
    public List<int>? ExcludedNumbers { get; set; }
}

public class GenerateRandomSlotResponse
{
    public GeneratedNumberDto Number { get; set; } = null!;
    public RandomStrategy Strategy { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public string Commentary { get; set; } = string.Empty;
}

public class GetMixedLuckyQuestionRequest
{
    public string GameCode { get; set; } = string.Empty;
    public int PoolIndex { get; set; } = 0;
    public bool IsClimaxStep { get; set; } = false;
    public List<int>? RecentQuestionIds { get; set; }
    public List<int>? RecentThemeIds { get; set; }
}

public class GetMixedLuckyQuestionResponse
{
    public QuestionDto Question { get; set; } = null!;
}

public class AnswerMixedLuckySlotRequest
{
    public string GameCode { get; set; } = string.Empty;
    public int PoolIndex { get; set; } = 0;
    public int QuestionId { get; set; }
    public int ChoiceId { get; set; }
    public List<int>? ExcludedNumbers { get; set; }
    public List<int>? PreviousNumbersInLine { get; set; }
}

public class AnswerMixedLuckySlotResponse
{
    public RevealedNumberDto RevealedNumber { get; set; } = null!;
}

public class FillRemainderRequest
{
    public string GameCode { get; set; } = string.Empty;
    public RandomStrategy Strategy { get; set; } = RandomStrategy.PureRandom;
    public List<GeneratedNumberDto> ExistingNumbers { get; set; } = new();
}

public class FillRemainderResponse
{
    public string GameCode { get; set; } = string.Empty;
    public RandomStrategy Strategy { get; set; }
    public List<GeneratedNumberDto> Numbers { get; set; } = new();
    public string Commentary { get; set; } = string.Empty;
}
