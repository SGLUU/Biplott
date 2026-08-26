using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/than-tai")]
[Route("api/than-tai")]
public class ThanTaiController : ControllerBase
{
    private readonly ISlipService _slipService;

    public ThanTaiController(ISlipService slipService)
    {
        _slipService = slipService;
    }

    [HttpPost("generate-line")]
    [ProducesResponseType(typeof(ApiResponse<GenerateLineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateLine([FromBody] GenerateLineRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Yêu cầu không hợp lệ",
                Detail = "Mã trò chơi (GameCode) không được để trống.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        try
        {
            var result = await _slipService.GenerateLineAsync(request, cancellationToken);
            return Ok(ApiResponse<GenerateLineResponse>.Ok(result, "Sinh số Thần Tài thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Trò chơi không tồn tại",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
    }

    [HttpPost("generate-slip")]
    [ProducesResponseType(typeof(ApiResponse<GenerateSlipResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateSlip([FromBody] GenerateSlipRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Yêu cầu không hợp lệ",
                Detail = "Mã trò chơi (GameCode) không được để trống.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        try
        {
            var result = await _slipService.GenerateSlipAsync(request, cancellationToken);
            return Ok(ApiResponse<GenerateSlipResponse>.Ok(result, "Sinh cả phiếu Thần Tài thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Trò chơi không tồn tại",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
    }
}
