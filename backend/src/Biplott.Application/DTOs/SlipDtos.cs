using Biplott.Core.Enums;

namespace Biplott.Application.DTOs;

public class GeneratedNumberDto
{
    public int Value { get; set; }
    public string Formatted => Value.ToString("D2");
    public int PoolIndex { get; set; }
    public NumberSource Source { get; set; } = NumberSource.Random;
    public string? MetadataJson { get; set; }
}

public class GenerateLineRequest
{
    public string GameCode { get; set; } = string.Empty;
    public RandomStrategy Strategy { get; set; } = RandomStrategy.PureRandom;
    public List<int>? ExcludedNumbers { get; set; }
    public List<GeneratedNumberDto>? CurrentNumbers { get; set; }
}

public class GenerateLineResponse
{
    public RandomStrategy Strategy { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public List<GeneratedNumberDto> Numbers { get; set; } = new();
    public string Commentary { get; set; } = string.Empty;
}

public class ValidateLineRequest
{
    public string GameCode { get; set; } = string.Empty;
    public string LineLabel { get; set; } = "A";
    public List<GeneratedNumberDto> Numbers { get; set; } = new();
}

public class ValidateLineResponse
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class GenerateSlipRequest
{
    public string GameCode { get; set; } = string.Empty;
    public RandomStrategy Strategy { get; set; } = RandomStrategy.PureRandom;
    public string FillMode { get; set; } = "EmptyOnly"; // "EmptyOnly" | "All"
    public List<SlipLineDto>? ExistingLines { get; set; }
}

public class SlipLineDto
{
    public string LineLabel { get; set; } = "A";
    public SlipLineStatus Status { get; set; } = SlipLineStatus.Empty;
    public List<GeneratedNumberDto> Numbers { get; set; } = new();
    public RandomStrategy? Strategy { get; set; }
    public string? Commentary { get; set; }
}

public class GenerateSlipResponse
{
    public string GameCode { get; set; } = string.Empty;
    public RandomStrategy Strategy { get; set; }
    public List<SlipLineDto> Lines { get; set; } = new();
    public string Commentary { get; set; } = string.Empty;
}
