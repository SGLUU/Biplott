using System.Security.Claims;
using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Đăng ký tài khoản người dùng mới
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            SetRefreshTokenCookie(response.RefreshToken);
            return Ok(ApiResponse<AuthResponse>.Ok(response, "Đăng ký tài khoản thành công!"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng ký tài khoản.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể hoàn tất đăng ký lúc này."));
        }
    }

    /// <summary>
    /// Đăng nhập tài khoản
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            SetRefreshTokenCookie(response.RefreshToken);
            return Ok(ApiResponse<AuthResponse>.Ok(response, "Đăng nhập thành công!"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể đăng nhập lúc này."));
        }
    }

    /// <summary>
    /// Làm mới Access Token thông qua Refresh Token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request, CancellationToken cancellationToken)
    {
        var token = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            Request.Cookies.TryGetValue("refreshToken", out token);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(ApiResponse<string>.Fail("Không tìm thấy Refresh Token. Vui lòng đăng nhập lại."));
        }

        try
        {
            var response = await _authService.RefreshTokenAsync(token, cancellationToken);
            SetRefreshTokenCookie(response.RefreshToken);
            return Ok(ApiResponse<AuthResponse>.Ok(response));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi làm mới token.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể làm mới token lúc này."));
        }
    }

    /// <summary>
    /// Đăng xuất tài khoản và thu hồi Refresh Token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await _authService.LogoutAsync(userId, cancellationToken);
        }

        Response.Cookies.Delete("refreshToken");
        return Ok(ApiResponse<string>.Ok("Đăng xuất thành công!"));
    }

    /// <summary>
    /// Lấy thông tin người dùng hiện tại
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Chưa đăng nhập."));
        }

        try
        {
            var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
            return Ok(ApiResponse<UserDto>.Ok(user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(14)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
