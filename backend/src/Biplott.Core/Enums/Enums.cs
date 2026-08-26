namespace Biplott.Core.Enums;

public enum NumberSource
{
    Manual = 1,
    Lucky = 2,
    Random = 3
}

public enum SlipLineStatus
{
    Empty = 0,
    Partial = 1,
    Complete = 2
}

public enum QuestionType
{
    SingleChoice = 1,
    ThisOrThat = 2,
    Scenario = 3,
    Slider = 4,
    VisualChoice = 5,
    BlindChoice = 6,
    Ranking = 7,
    QuickInstinct = 8,
    SymbolChoice = 9
}

public enum RandomStrategy
{
    PureRandom = 1,
    Balanced = 2,
    Spread = 3,
    Surprise = 4
}
