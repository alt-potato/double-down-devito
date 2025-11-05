using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;

namespace Project.Api.Repositories;

public class GamePlayerRepository(AppDbContext context, ILogger<GamePlayerRepository> logger)
    : CompositeRepository<GamePlayer, Guid, Guid>(context, logger),
        IGamePlayerRepository
{
    /// <summary>
    /// Get a game player by game ID and user ID.
    /// </summary>
    public Task<GamePlayer?> GetByGameIdAndUserIdAsync(Guid gameId, Guid userId) =>
        GetAsync(gameId, userId);

    /// <summary>
    /// Get all game players in a game.
    /// </summary>
    public Task<IReadOnlyList<GamePlayer>> GetAllByGameIdAsync(
        Guid gameId,
        int? skip = null,
        int? take = null
    ) => GetAllAsync(g => g.GameId == gameId, skip: skip, take: take);

    /// <summary>
    /// Get all games for a user.
    /// </summary>
    public Task<IReadOnlyList<GamePlayer>> GetAllByUserIdAsync(
        Guid userId,
        int? skip = null,
        int? take = null
    ) => GetAllAsync(g => g.UserId == userId, skip: skip, take: take);

    /// <summary>
    /// Create a new game player.
    /// </summary>
    public new Task<GamePlayer> CreateAsync(GamePlayer gamePlayer) => CreateAsync(gamePlayer);

    /// <summary>
    /// Update an existing game player.
    /// </summary>
    public new Task<GamePlayer> UpdateAsync(GamePlayer gamePlayer) => UpdateAsync(gamePlayer);

    /// <summary>
    /// Update game player balance.
    /// </summary>
    public Task<GamePlayer> UpdateBalanceAsync(Guid gameId, Guid userId, long balanceDelta) =>
        UpdateAsync(gameId, userId, g => g.BalanceDelta += balanceDelta);

    /// <summary>
    /// Delete a game player.
    /// </summary>
    public new Task<GamePlayer> DeleteAsync(Guid gameId, Guid userId) =>
        DeleteAsync(gameId, userId);
}
