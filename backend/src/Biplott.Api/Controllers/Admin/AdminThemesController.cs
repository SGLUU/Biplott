using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/themes")]
public class AdminThemesController : ControllerBase
{
    private readonly IAdminThemeService _themeService;

    public AdminThemesController(IAdminThemeService themeService)
    {
        _themeService = themeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AdminThemeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThemes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _themeService.GetThemesPagedAsync(page, pageSize, search, isActive, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminThemeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminThemeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetThemeById(int id, CancellationToken cancellationToken)
    {
        var result = await _themeService.GetThemeByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail($"Không tìm thấy chủ đề có ID = {id}."));

        return Ok(ApiResponse<AdminThemeDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminThemeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTheme([FromBody] CreateThemeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _themeService.CreateThemeAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<AdminThemeDto>.Ok(result, "Tạo chủ đề thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminThemeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTheme(int id, [FromBody] UpdateThemeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _themeService.UpdateThemeAsync(id, request, cancellationToken);
            return Ok(ApiResponse<AdminThemeDto>.Ok(result, "Cập nhật chủ đề thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<AdminThemeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _themeService.SetThemeStatusAsync(id, request.IsActive, cancellationToken);
            return Ok(ApiResponse<AdminThemeDto>.Ok(result, $"Đã {(request.IsActive ? "kích hoạt" : "tạm dừng")} chủ đề thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTheme(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _themeService.DeleteThemeAsync(id, cancellationToken);
            return Ok(ApiResponse<string>.Ok("Xóa chủ đề thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }
}