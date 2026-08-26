using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/slips")]
[Route("api/slips")]
public class SlipsController : ControllerBase
{
    private readonly ISlipService _slipService;

    public SlipsController(ISlipService slipService)
    {
        _slipService = slipService;
    }

    [HttpPost("validate-line")]
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<ValidateLineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateLine([FromBody] ValidateLineRequest request, CancellationToken cancellationToken)
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

        var result = await _slipService.ValidateLineAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            return Ok(ApiResponse<ValidateLineResponse>.Ok(result, "Kiểm tra hợp lệ thất bại"));
        }

        return Ok(ApiResponse<ValidateLineResponse>.Ok(result, "Dòng số hợp lệ"));
    }
}
