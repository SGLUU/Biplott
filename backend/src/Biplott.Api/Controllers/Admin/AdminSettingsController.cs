using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/settings")]
public class AdminSettingsController : ControllerBase
{
    private readonly IEngineConfigService _configService;

    public AdminSettingsController(IEngineConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AdminSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _configService.GetSettingsAsync(cancellationToken);
        return Ok(ApiResponse<AdminSettingsDto>.Ok(result));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<AdminSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings([FromBody] AdminSettingsDto settings, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _configService.UpdateSettingsAsync(settings, cancellationToken);
            return Ok(ApiResponse<AdminSettingsDto>.Ok(result, "Cập nhật cấu hình hệ thống thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPost("reset")]
    [ProducesResponseType(typeof(ApiResponse<AdminSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetToDefaults(CancellationToken cancellationToken)
    {
        var result = await _configService.ResetToDefaultsAsync(cancellationToken);
        return Ok(ApiResponse<AdminSettingsDto>.Ok(result, "Đã khôi phục cấu hình hệ thống về mặc định."));
    }
}