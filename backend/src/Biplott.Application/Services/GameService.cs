using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Core.Interfaces;

namespace Biplott.Application.Services;

public interface IGameService
{
    Task<IReadOnlyList<GameDto>> GetActiveGamesAsync(CancellationToken cancellationToken = default);
    Task<GameDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<IReadOnlyList<GameDto>> GetActiveGamesAsync(CancellationToken cancellationToken = default)
    {
        var games = await _gameRepository.GetActiveGamesAsync(cancellationToken);
        return games.Select(MapToDto).ToList();
    }

    public async Task<GameDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByCodeAsync(code, cancellationToken);
        return game == null ? null : MapToDto(game);
    }

    private static GameDto MapToDto(Game game)
    {
        return new GameDto
        {
            Id = game.Id,
            Code = game.Code,
            Name = game.Name,
            Description = game.Description,
            Tagline = game.Tagline,
            IconUrl = game.IconUrl,
            IsActive = game.IsActive,
            SortOrder = game.SortOrder,
            Pools = game.Pools.OrderBy(p => p.PoolIndex).Select(p => new GamePoolDto
            {
                Id = p.Id,
                PoolIndex = p.PoolIndex,
                Name = p.Name,
                MinNumber = p.MinNumber,
                MaxNumber = p.MaxNumber,
                PickCount = p.PickCount,
                AllowDuplicates = p.AllowDuplicates,
                BadgeColor = p.BadgeColor
            }).ToList()
        };
    }
}
