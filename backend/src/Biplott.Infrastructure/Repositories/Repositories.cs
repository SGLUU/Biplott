using Biplott.Core.Entities;
using Biplott.Core.Interfaces;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Biplott.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly BiplottDbContext _dbContext;

    public GameRepository(BiplottDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Game>> GetActiveGamesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Games
            .Include(g => g.Pools)
            .Where(g => g.IsActive)
            .OrderBy(g => g.SortOrder)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Game?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Games
            .Include(g => g.Pools)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Code.ToUpper() == code.ToUpper(), cancellationToken);
    }

    public async Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Games
            .Include(g => g.Pools)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }
}

public class SlipRepository : ISlipRepository
{
    private readonly BiplottDbContext _dbContext;

    public SlipRepository(BiplottDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Slip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Slips
            .Include(s => s.Game).ThenInclude(g => g.Pools)
            .Include(s => s.Lines).ThenInclude(l => l.Numbers)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Slip?> GetByCodeAsync(string slipCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Slips
            .Include(s => s.Game).ThenInclude(g => g.Pools)
            .Include(s => s.Lines).ThenInclude(l => l.Numbers)
            .FirstOrDefaultAsync(s => s.SlipCode.ToUpper() == slipCode.ToUpper(), cancellationToken);
    }

    public async Task<IReadOnlyList<Slip>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Slips
            .Include(s => s.Game)
            .Include(s => s.Lines).ThenInclude(l => l.Numbers)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Slip> Items, int TotalCount)> GetUserSlipsPagedAsync(
        string userId,
        int page,
        int pageSize,
        bool isFavoriteOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Slips
            .Include(s => s.Game)
            .Include(s => s.Lines).ThenInclude(l => l.Numbers)
            .Where(s => s.UserId == userId);

        if (isFavoriteOnly)
        {
            query = query.Where(s => s.IsFavorite);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Slip>> GetByGuestSessionAsync(string guestSessionToken, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Slips
            .Include(s => s.Game)
            .Include(s => s.Lines).ThenInclude(l => l.Numbers)
            .Where(s => s.GuestSessionToken == guestSessionToken)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Slip slip, CancellationToken cancellationToken = default)
    {
        await _dbContext.Slips.AddAsync(slip, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Slip slip, CancellationToken cancellationToken = default)
    {
        _dbContext.Slips.Update(slip);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Slip slip, CancellationToken cancellationToken = default)
    {
        _dbContext.Slips.Remove(slip);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class UserActivityRepository : IUserActivityRepository
{
    private readonly BiplottDbContext _dbContext;

    public UserActivityRepository(BiplottDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserActivityHistory activity, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserActivityHistories.AddAsync(activity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<UserActivityHistory> Items, int TotalCount)> GetUserHistoryPagedAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserActivityHistories
            .Include(a => a.Game)
            .Where(a => a.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

public class QuestionRepository : IQuestionRepository
{
    private readonly BiplottDbContext _dbContext;

    public QuestionRepository(BiplottDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Question>> GetAllActiveQuestionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Questions
            .Include(q => q.Theme)
            .Include(q => q.Choices)
                .ThenInclude(c => c.ChoiceTraits)
                    .ThenInclude(ct => ct.Trait)
            .Where(q => q.IsActive && q.Theme.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<QuestionChoice?> GetChoiceWithDetailsAsync(int choiceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.QuestionChoices
            .Include(c => c.Question).ThenInclude(q => q.Theme)
            .Include(c => c.ChoiceTraits).ThenInclude(ct => ct.Trait)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == choiceId, cancellationToken);
    }
}

