using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biplott.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Route("api/[controller]")]
[Produces("application/json")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ các Game đang hoạt động cùng cấu hình Pools.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GameDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGames(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching active games list");
        var games = await _gameService.GetActiveGamesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GameDto>>.Ok(games, "Lấy danh sách game thành công"));
    }

    /// <summary>
    /// Lấy thông tin chi tiết của Game theo mã Code (POWER_655, MEGA_645, LOTTO_535).
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ApiResponse<GameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGameByCode(string code, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching game by code: {Code}", code);
        var game = await _gameService.GetByCodeAsync(code, cancellationToken);
        if (game == null)
        {
            return NotFound(new ProblemDetails
            {
                Type = "https://biplot.vn/errors/game-not-found",
                Title = "Không tìm thấy trò chơi",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Không tìm thấy thông tin trò chơi với mã '{code}'.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(ApiResponse<GameDto>.Ok(game, $"Lấy thông tin game {game.Name} thành công"));
    }
}
