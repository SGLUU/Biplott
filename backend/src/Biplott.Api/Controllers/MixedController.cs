using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/mixed")]
[Route("api/mixed")]
public class MixedController : ControllerBase
{
    private readonly IMixedService _mixedService;
    private readonly ILogger<MixedController> _logger;

    public MixedController(
        IMixedService mixedService,
        ILogger<MixedController> logger)
    {
        _mixedService = mixedService;
        _logger = logger;
    }

    /// <summary>
    /// Sinh 1 số ngẫu nhiên theo chiến lược Thần Tài cho 1 ô số cụ thể
    /// </summary>
    [HttpPost("generate-random-slot")]
    [ProducesResponseType(typeof(ApiResponse<GenerateRandomSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateRandomSlot(
        [FromBody] GenerateRandomSlotRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi không được để trống."));
        }

        try
        {
            var response = await _mixedService.GenerateRandomSlotAsync(request, cancellationToken);
            return Ok(ApiResponse<GenerateRandomSlotResponse>.Ok(response));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi sinh số ngẫu nhiên cho ô số Mixed Mode.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể sinh số ngẫu nhiên lúc này."));
        }
    }

    /// <summary>
    /// Lấy 1 câu hỏi Lucky duy nhất cho 1 ô số trong Mixed Mode
    /// </summary>
    [HttpPost("lucky-question")]
    [ProducesResponseType(typeof(ApiResponse<GetMixedLuckyQuestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMixedLuckyQuestion(
        [FromBody] GetMixedLuckyQuestionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi không được để trống."));
        }

        try
        {
            var response = await _mixedService.GetMixedLuckyQuestionAsync(request, cancellationToken);
            return Ok(ApiResponse<GetMixedLuckyQuestionResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy câu hỏi Lucky cho Mixed Mode.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể lấy câu hỏi lúc này."));
        }
    }

    /// <summary>
    /// Xử lý đáp án câu hỏi Lucky cho 1 ô số trong Mixed Mode và mở 1 con số
    /// </summary>
    [HttpPost("lucky-answer")]
    [ProducesResponseType(typeof(ApiResponse<AnswerMixedLuckySlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnswerMixedLuckySlot(
        [FromBody] AnswerMixedLuckySlotRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi không được để trống."));
        }

        if (request.QuestionId <= 0 || request.ChoiceId <= 0)
        {
            return BadRequest(ApiResponse<string>.Fail("QuestionId và ChoiceId không hợp lệ."));
        }

        try
        {
            var response = await _mixedService.AnswerMixedLuckySlotAsync(request, cancellationToken);
            return Ok(ApiResponse<AnswerMixedLuckySlotResponse>.Ok(response));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý đáp án Lucky cho Mixed Mode.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể xử lý đáp án lúc này."));
        }
    }

    /// <summary>
    /// Thần Tài điền các ô còn trống cho dòng Mixed Mode (giữ nguyên các ô đã chọn)
    /// </summary>
    [HttpPost("fill-remainder")]
    [ProducesResponseType(typeof(ApiResponse<FillRemainderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FillRemainder(
        [FromBody] FillRemainderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(ApiResponse<string>.Fail("Mã trò chơi không được để trống."));
        }

        try
        {
            var response = await _mixedService.FillRemainderAsync(request, cancellationToken);
            return Ok(ApiResponse<FillRemainderResponse>.Ok(response));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi điền số còn trống cho Mixed Mode.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể điền số lúc này."));
        }
    }
}
