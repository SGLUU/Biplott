using System.Security.Claims;
using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/lucky-dna")]
[Route("api/lucky-dna")]
public class LuckyDnaController : ControllerBase
{
    private readonly ILuckyDnaService _dnaService;
    private readonly ILogger<LuckyDnaController> _logger;

    public LuckyDnaController(ILuckyDnaService dnaService, ILogger<LuckyDnaController> logger)
    {
        _dnaService = dnaService;
        _logger = logger;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Lấy hồ sơ Lucky DNA của người dùng hiện tại hoặc khách
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<LuckyDnaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLuckyDna(
        [FromQuery] string? guestSessionToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                var dna = await _dnaService.GetUserDnaAsync(userId, cancellationToken);
                return Ok(ApiResponse<LuckyDnaResponse>.Ok(dna));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(guestSessionToken))
                {
                    return BadRequest(ApiResponse<string>.Fail("Yêu cầu mã định danh khách (guestSessionToken) đối với khách chưa đăng nhập."));
                }
                var dna = await _dnaService.GetGuestDnaAsync(guestSessionToken, cancellationToken);
                return Ok(ApiResponse<LuckyDnaResponse>.Ok(dna));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin Lucky DNA.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể lấy thông tin Lucky DNA lúc này."));
        }
    }

    /// <summary>
    /// Reset Lucky DNA của người dùng
    /// </summary>
    [Authorize]
    [HttpPost("reset")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetLuckyDna(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<string>.Fail("Người dùng chưa được xác thực."));
            }

            await _dnaService.ResetUserDnaAsync(userId, cancellationToken);
            return Ok(ApiResponse<bool>.Ok(true, "Đã reset profile Lucky DNA thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi reset profile Lucky DNA.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể reset profile lúc này."));
        }
    }
}
