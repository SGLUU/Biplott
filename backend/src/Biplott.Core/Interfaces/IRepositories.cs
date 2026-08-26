using Biplott.Core.Entities;

namespace Biplott.Core.Interfaces;

public interface IGameRepository
{
    Task<IReadOnlyList<Game>> GetActiveGamesAsync(CancellationToken cancellationToken = default);
    Task<Game?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public interface ISlipRepository
{
    Task<Slip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Slip?> GetByCodeAsync(string slipCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Slip>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Slip> Items, int TotalCount)> GetUserSlipsPagedAsync(string userId, int page, int pageSize, bool isFavoriteOnly, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Slip>> GetByGuestSessionAsync(string guestSessionToken, CancellationToken cancellationToken = default);
    Task AddAsync(Slip slip, CancellationToken cancellationToken = default);
    Task UpdateAsync(Slip slip, CancellationToken cancellationToken = default);
    Task DeleteAsync(Slip slip, CancellationToken cancellationToken = default);
}

public interface IQuestionRepository
{
    Task<IReadOnlyList<Question>> GetAllActiveQuestionsAsync(CancellationToken cancellationToken = default);
    Task<QuestionChoice?> GetChoiceWithDetailsAsync(int choiceId, CancellationToken cancellationToken = default);
}

public interface IUserActivityRepository
{
    Task AddAsync(UserActivityHistory activity, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserActivityHistory> Items, int TotalCount)> GetUserHistoryPagedAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
