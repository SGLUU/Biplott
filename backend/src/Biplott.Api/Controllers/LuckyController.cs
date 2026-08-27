using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/lucky")]
[Route("api/lucky")]
public class LuckyController : ControllerBase
{
    private readonly ILuckyJourneySessionService _journeyService;
    private readonly ILogger<LuckyController> _logger;

    public LuckyController(
        ILuckyJourneySessionService journeyService,
        ILogger<LuckyController> logger)
    {
        _journeyService = journeyService;
        _logger = logger;
    }

    /// <summary>
    /// Bắt đầu một hành trình Lucky Journey mới cho một dòng vé
    /// </summary>
    [HttpPost("journeys/start")]
    [ProducesResponseType(typeof(ApiResponse<StartJourneyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartJourney(
        [FromBody] StartJourneyRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi (GameCode) không được để trống."));
        }

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            var response = await _journeyService.StartJourneyAsync(request, userId, cancellationToken);
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
            _logger.LogError(ex, "Lỗi không xác định khi bắt đầu Lucky Journey.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể khởi tạo hành trình lúc này."));
        }
    }

    /// <summary>
    /// Gửi đáp án cho bước hiện tại, mở ngay 1 con số may mắn và nhận câu hỏi tiếp theo
    /// </summary>
    [HttpPost("journeys/{journeyId}/answer")]
    [ProducesResponseType(typeof(ApiResponse<AnswerStepResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnswerStep(
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
            return BadRequest(ApiResponse<string>.Fail("QuestionId và ChoiceId phải là số nguyên dương hợp lệ."));
        }

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            var response = await _journeyService.AnswerStepAsync(journeyId, request, userId, cancellationToken);
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
            _logger.LogError(ex, "Lỗi khi xử lý đáp án Lucky Journey {JourneyId}", journeyId);
            return StatusCode(500, ApiResponse<string>.Fail("Đã có lỗi xảy ra khi xử lý lựa chọn."));
        }
    }

    /// <summary>
    /// Hủy bỏ phiên Lucky Journey
    /// </summary>
    [HttpPost("journeys/{journeyId}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelJourney([FromRoute] string journeyId)
    {
        await _journeyService.CancelJourneyAsync(journeyId);
        return Ok(ApiResponse<bool>.Ok(true, "Đã hủy hành trình thành công."));
    }
}
