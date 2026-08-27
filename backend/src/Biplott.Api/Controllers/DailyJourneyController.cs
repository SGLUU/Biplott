using System.Security.Claims;
using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/daily-journeys")]
[Route("api/daily-journeys")]
public class DailyJourneyController : ControllerBase
{
    private readonly IDailyJourneyService _dailyService;
    private readonly ILogger<DailyJourneyController> _logger;

    public DailyJourneyController(IDailyJourneyService dailyService, ILogger<DailyJourneyController> logger)
    {
        _dailyService = dailyService;
        _logger = logger;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Lấy thông tin hành trình Daily Journey hôm nay
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DailyJourneyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTodayDailyJourney(
        [FromQuery] string gameCode,
        [FromQuery] string? guestSessionToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi (gameCode) không được để trống."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var journey = await _dailyService.GetTodayDailyJourneyAsync(gameCode, userId, guestSessionToken, cancellationToken);
            if (journey == null)
            {
                return NotFound(ApiResponse<string>.Fail("Hôm nay chưa bắt đầu hành trình nào cho game này."));
            }
            return Ok(ApiResponse<DailyJourneyDto>.Ok(journey));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin Daily Journey hôm nay.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể tải thông tin hành trình hôm nay."));
        }
    }

    /// <summary>
    /// Bắt đầu hoặc tiếp tục hành trình Daily Journey hôm nay
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(ApiResponse<StartJourneyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartDailyJourney(
        [FromBody] StartJourneyRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi (GameCode) không được để trống."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var response = await _dailyService.StartDailyJourneyAsync(request, userId, cancellationToken);
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
            _logger.LogError(ex, "Lỗi khi bắt đầu Daily Journey.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể khởi tạo hành trình hôm nay."));
        }
    }

    /// <summary>
    /// Gửi lựa chọn đáp án bước hiện tại cho Daily Journey
    /// </summary>
    [HttpPost("{journeyId}/answer")]
    [ProducesResponseType(typeof(ApiResponse<AnswerStepResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnswerDailyStep(
        [FromRoute] Guid journeyId,
        [FromBody] AnswerStepRequest request,
        CancellationToken cancellationToken)
    {
        if (request.QuestionId <= 0 || request.ChoiceId <= 0)
        {
            return BadRequest(ApiResponse<string>.Fail("QuestionId và ChoiceId không hợp lệ."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var response = await _dailyService.AnswerDailyStepAsync(journeyId, request, userId, cancellationToken);
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
            _logger.LogError(ex, "Lỗi khi trả lời bước Daily Journey {JourneyId}.", journeyId);
            return StatusCode(500, ApiResponse<string>.Fail("Đã xảy ra lỗi khi ghi nhận đáp án."));
        }
    }
}
