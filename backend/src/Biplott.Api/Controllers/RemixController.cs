using System.Security.Claims;
using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/remix")]
[Route("api/remix")]
public class RemixController : ControllerBase
{
    private readonly IRemixService _remixService;
    private readonly ILogger<RemixController> _logger;

    public RemixController(IRemixService remixService, ILogger<RemixController> logger)
    {
        _remixService = remixService;
        _logger = logger;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Thực hiện Quick Remix (ngẫu nhiên nhanh) làm mới các số chưa khóa
    /// </summary>
    [HttpPost("quick")]
    [ProducesResponseType(typeof(ApiResponse<GenerateLineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> QuickRemix(
        [FromBody] StartRemixJourneyRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi (GameCode) không được để trống."));
        }

        try
        {
            var response = await _remixService.QuickRemixAsync(request, cancellationToken);
            return Ok(ApiResponse<GenerateLineResponse>.Ok(response));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thực hiện Quick Remix.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể thực hiện Quick Remix lúc này."));
        }
    }

    /// <summary>
    /// Khởi động hành trình Lucky Remix cho các ô chưa khóa
    /// </summary>
    [HttpPost("lucky/start")]
    [ProducesResponseType(typeof(ApiResponse<StartJourneyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartLuckyRemix(
        [FromBody] StartRemixJourneyRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi (GameCode) không được để trống."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var response = await _remixService.StartLuckyRemixAsync(request, userId, cancellationToken);
            return Ok(ApiResponse<StartJourneyResponse>.Ok(response));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi bắt đầu Lucky Remix.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể khởi động Lucky Remix lúc này."));
        }
    }

    /// <summary>
    /// Trả lời câu hỏi cho Lucky Remix step và mở số ngẫu nhiên tiếp theo
    /// </summary>
    [HttpPost("lucky/{journeyId}/answer")]
    [ProducesResponseType(typeof(ApiResponse<AnswerStepResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnswerLuckyRemixStep(
        [FromRoute] string journeyId,
        [FromBody] AnswerStepRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(journeyId))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã hành trình (JourneyId) không hợp lệ."));
        }

        if (request.QuestionId <= 0 || request.ChoiceId <= 0)
        {
            return BadRequest(ApiResponse<string>.Fail("QuestionId và ChoiceId không hợp lệ."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var response = await _remixService.AnswerLuckyRemixStepAsync(journeyId, request, userId, cancellationToken);
            return Ok(ApiResponse<AnswerStepResponse>.Ok(response));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý đáp án Lucky Remix {JourneyId}.", journeyId);
            return StatusCode(500, ApiResponse<string>.Fail("Lỗi khi lưu đáp án Lucky Remix."));
        }
    }
}
