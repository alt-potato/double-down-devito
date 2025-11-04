using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IGamePlayerRepository
{
    /// <summary>
    /// Get a game player by game ID and user ID.
    /// </summary>
    Task<GamePlayer?> GetGamePlayerByGameIdAndUserIdAsync(Guid gameId, Guid userId);

    /// <summary>
    /// Get all game players in a game.
    /// </summary>
    Task<List<GamePlayer>> GetGamePlayersByGameIdAsync(Guid gameId);

    /// <summary>
    /// Get all games for a user.
    /// </summary>
    Task<List<GamePlayer>> GetGamePlayersByUserIdAsync(Guid userId);

    /// <summary>
    /// Create a new game player.
    /// </summary>
    Task<GamePlayer> CreateGamePlayerAsync(GamePlayer gamePlayer);

    /// <summary>
    /// Update an existing game player.
    /// </summary>
    Task<GamePlayer> UpdateGamePlayerAsync(Guid gameId, Guid userId, GamePlayer gamePlayer);

    /// <summary>
    /// Update game player balance.
    /// </summary>
    Task<GamePlayer> UpdateGamePlayerBalanceAsync(Guid gameId, Guid userId, long balanceDelta);

    /// <summary>
    /// Delete a game player.
    /// </summary>
    Task<GamePlayer> DeleteGamePlayerAsync(Guid gameId, Guid userId);
}
