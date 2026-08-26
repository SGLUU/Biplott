using System.Security.Claims;
using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/user/slips")]
[Route("api/user/slips")]
public class UserSlipsController : ControllerBase
{
    private readonly IUserSlipService _userSlipService;
    private readonly ILogger<UserSlipsController> _logger;

    public UserSlipsController(
        IUserSlipService userSlipService,
        ILogger<UserSlipsController> logger)
    {
        _userSlipService = userSlipService;
        _logger = logger;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");
    }

    /// <summary>
    /// Lưu phiếu số vào tài khoản cá nhân
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SavedSlipSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveSlip(
        [FromBody] SaveSlipRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userSlipService.SaveSlipAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<SavedSlipSummaryDto>.Ok(result, "Đã lưu phiếu vào danh sách thành công!"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu phiếu số.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể lưu phiếu số lúc này."));
        }
    }

    /// <summary>
    /// Lấy danh sách các phiếu số đã lưu của người dùng hiện tại (hỗ trợ phân trang và lọc yêu thích)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SavedSlipSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavedSlips(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool isFavorite = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userSlipService.GetUserSlipsAsync(userId, page, pageSize, isFavorite, cancellationToken);
            return Ok(ApiResponse<PagedResult<SavedSlipSummaryDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu số.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể tải danh sách phiếu lúc này."));
        }
    }

    /// <summary>
    /// Xem chi tiết một phiếu số đã lưu kèm Lucky Story
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SavedSlipDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSlipDetail(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userSlipService.GetSlipDetailAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<SavedSlipDetailDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết phiếu {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail("Không thể xem chi tiết phiếu lúc này."));
        }
    }

    /// <summary>
    /// Bật/tắt trạng thái yêu thích của phiếu số
    /// </summary>
    [HttpPut("{id:guid}/favorite")]
    [ProducesResponseType(typeof(ApiResponse<ToggleFavoriteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFavorite(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userSlipService.ToggleFavoriteAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<ToggleFavoriteResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đổi trạng thái yêu thích phiếu {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail("Không thể thao tác lúc này."));
        }
    }

    /// <summary>
    /// Xóa phiếu số thuộc quyền sở hữu của người dùng hiện tại
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSlip(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userSlipService.DeleteSlipAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<string>.Ok("Đã xóa phiếu số thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa phiếu {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail("Không thể xóa phiếu lúc này."));
        }
    }

    /// <summary>
    /// Lấy lịch sử hoạt động tạo số của người dùng
    /// </summary>
    [HttpGet("/api/v1/user/history")]
    [HttpGet("/api/user/history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserActivityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userSlipService.GetUserHistoryAsync(userId, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<UserActivityDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử hoạt động.");
            return StatusCode(500, ApiResponse<string>.Fail("Không thể tải lịch sử lúc này."));
        }
    }
}
