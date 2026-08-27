using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/traits")]
public class AdminTraitsController : ControllerBase
{
    private readonly IAdminTraitService _traitService;

    public AdminTraitsController(IAdminTraitService traitService)
    {
        _traitService = traitService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AdminTraitDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTraits(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _traitService.GetTraitsPagedAsync(page, pageSize, search, isActive, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminTraitDto>>.Ok(result));
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminTraitDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActiveTraits(CancellationToken cancellationToken)
    {
        var result = await _traitService.GetAllActiveTraitsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminTraitDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminTraitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTraitById(int id, CancellationToken cancellationToken)
    {
        var result = await _traitService.GetTraitByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail($"Không tìm thấy thuộc tính có ID = {id}."));

        return Ok(ApiResponse<AdminTraitDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminTraitDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTrait([FromBody] CreateTraitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _traitService.CreateTraitAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<AdminTraitDto>.Ok(result, "Tạo thuộc tính thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminTraitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTrait(int id, [FromBody] UpdateTraitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _traitService.UpdateTraitAsync(id, request, cancellationToken);
            return Ok(ApiResponse<AdminTraitDto>.Ok(result, "Cập nhật thuộc tính thành công."));
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
    [ProducesResponseType(typeof(ApiResponse<AdminTraitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _traitService.SetTraitStatusAsync(id, request.IsActive, cancellationToken);
            return Ok(ApiResponse<AdminTraitDto>.Ok(result, $"Đã {(request.IsActive ? "kích hoạt" : "tạm dừng")} thuộc tính thành công."));
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
    public async Task<IActionResult> DeleteTrait(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _traitService.DeleteTraitAsync(id, cancellationToken);
            return Ok(ApiResponse<string>.Ok("Xóa thuộc tính thành công."));
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