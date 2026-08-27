using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/import")]
public class AdminImportController : ControllerBase
{
    private readonly IContentImportService _importService;

    public AdminImportController(IContentImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<ImportValidationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<IActionResult> ValidateFile([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.Fail("Vui lòng tải lên một tệp tin hợp lệ (.csv, .xlsx, .json)."));
        }

        using var stream = file.OpenReadStream();
        var result = await _importService.ValidateImportFileAsync(stream, file.FileName, cancellationToken);
        return Ok(ApiResponse<ImportValidationResultDto>.Ok(result));
    }

    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ApiResponse<ImportConfirmResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmImport([FromBody] ImportConfirmRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _importService.ConfirmImportAsync(request, cancellationToken);
            return Ok(ApiResponse<ImportConfirmResponseDto>.Ok(result, result.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpGet("template")]
    public async Task<IActionResult> DownloadTemplate([FromQuery] string format = "csv", CancellationToken cancellationToken = default)
    {
        var (bytes, contentType, fileName) = await _importService.GenerateTemplateAsync(format, cancellationToken);
        return File(bytes, contentType, fileName);
    }
}