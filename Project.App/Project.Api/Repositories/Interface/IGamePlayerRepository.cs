using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IGamePlayerRepository
{
    /// <summary>
    /// Get a game player by game ID and user ID.
    /// </summary>
    Task<GamePlayer?> GetByGameIdAndUserIdAsync(Guid gameId, Guid userId);

    /// <summary>
    /// Get all game players in a game.
    /// </summary>
    Task<IReadOnlyList<GamePlayer>> GetAllByGameIdAsync(
        Guid gameId,
        int? skip = null,
        int? take = null
    );

    /// <summary>
    /// Get all games for a user.
    /// </summary>
    Task<IReadOnlyList<GamePlayer>> GetAllByUserIdAsync(
        Guid userId,
        int? skip = null,
        int? take = null
    );

    /// <summary>
    /// Create a new game player.
    /// </summary>
    Task<GamePlayer> CreateAsync(GamePlayer gamePlayer);

    /// <summary>
    /// Update an existing game player.
    /// </summary>
    Task<GamePlayer> UpdateAsync(GamePlayer gamePlayer);

    /// <summary>
    /// Update game player balance.
    /// </summary>
    Task<GamePlayer> UpdateBalanceAsync(Guid gameId, Guid userId, long balanceDelta);

    /// <summary>
    /// Delete a game player.
    /// </summary>
    Task<GamePlayer> DeleteAsync(Guid gameId, Guid userId);
}
