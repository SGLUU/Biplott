using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/questions")]
public class AdminQuestionsController : ControllerBase
{
    private readonly IAdminQuestionService _questionService;

    public AdminQuestionsController(IAdminQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AdminQuestionListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestions([FromQuery] QuestionFilterParams filter, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetQuestionsPagedAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminQuestionListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminQuestionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionById(int id, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetQuestionByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse<string>.Fail($"Không tìm thấy câu hỏi có ID = {id}."));

        return Ok(ApiResponse<AdminQuestionDetailDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminQuestionDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _questionService.CreateQuestionAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<AdminQuestionDetailDto>.Ok(result, "Tạo câu hỏi thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AdminQuestionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _questionService.UpdateQuestionAsync(id, request, cancellationToken);
            return Ok(ApiResponse<AdminQuestionDetailDto>.Ok(result, "Cập nhật câu hỏi thành công."));
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

    [HttpPost("{id:int}/duplicate")]
    [ProducesResponseType(typeof(ApiResponse<AdminQuestionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DuplicateQuestion(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _questionService.DuplicateQuestionAsync(id, cancellationToken);
            return Ok(ApiResponse<AdminQuestionDetailDto>.Ok(result, "Nhân bản câu hỏi thành công (trạng thái Bản nháp)."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<AdminQuestionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _questionService.SetQuestionStatusAsync(id, request.IsActive, cancellationToken);
            return Ok(ApiResponse<AdminQuestionDetailDto>.Ok(result, $"Đã {(request.IsActive ? "kích hoạt" : "tạm dừng")} câu hỏi thành công."));
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

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _questionService.DeleteQuestionAsync(id, cancellationToken);
            return Ok(ApiResponse<string>.Ok("Xử lý xóa câu hỏi thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
    }
}