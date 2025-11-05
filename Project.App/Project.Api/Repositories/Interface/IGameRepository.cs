using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IGameRepository
{
    /// <summary>
    /// Get a game by its ID.
    /// </summary>
    Task<Game?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all games with optional filtering parameters.
    /// </summary>
    Task<IReadOnlyList<Game>> GetAllAsync(
        string? gameMode = null,
        DateTimeOffset? createdBefore = null,
        DateTimeOffset? createdAfter = null,
        DateTimeOffset? startedBefore = null,
        DateTimeOffset? startedAfter = null,
        bool? hasEnded = null,
        int? skip = null,
        int? take = null
    );

    /// <summary>
    /// Create a new game.
    /// </summary>
    Task<Game> CreateAsync(Game game);

    /// <summary>
    /// Update an existing game.
    /// </summary>
    Task<Game> UpdateAsync(Game game);

    /// <summary>
    /// Delete a game by its ID.
    /// </summary>
    Task<Game> DeleteAsync(Guid id);

    /// <summary>
    /// Check if a game exists by its ID.
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Update game state.
    /// </summary>
    Task<Game> UpdateGamestateAsync(Guid id, string gameState);

    /// <summary>
    /// Update game round.
    /// </summary>
    Task<Game> UpdateRoundAsync(Guid id, int round);

    /// <summary>
    /// Update game deck ID.
    /// </summary>
    Task<Game> UpdateDeckIdAsync(Guid id, string deckId);
}
