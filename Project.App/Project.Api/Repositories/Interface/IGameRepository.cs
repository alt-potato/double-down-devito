using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IGameRepository
{
    /// <summary>
    /// Get a game by its ID.
    /// </summary>
    Task<Game?> GetGameByIdAsync(Guid id);

    /// <summary>
    /// Get all games with optional filtering parameters.
    /// </summary>
    Task<List<Game>> GetGamesAsync(
        string? gameMode = null,
        bool? isActive = null,
        int? minPlayers = null,
        int? maxPlayers = null,
        DateTimeOffset? createdAfter = null,
        DateTimeOffset? createdBefore = null,
        string? search = null
    );

    /// <summary>
    /// Create a new game.
    /// </summary>
    Task<Game> CreateGameAsync(Game game);

    /// <summary>
    /// Update an existing game.
    /// </summary>
    Task<Game> UpdateGameAsync(Game game);

    /// <summary>
    /// Delete a game by its ID.
    /// </summary>
    Task<Game> DeleteGameAsync(Guid id);

    /// <summary>
    /// Check if a game exists by its ID.
    /// </summary>
    Task<bool> GameExistsAsync(Guid id);

    /// <summary>
    /// Update game state.
    /// </summary>
    Task<Game> UpdateGameStateAsync(Guid id, string gameState);

    /// <summary>
    /// Update game round.
    /// </summary>
    Task<Game> UpdateGameRoundAsync(Guid id, int round);

    /// <summary>
    /// Update game deck ID.
    /// </summary>
    Task<Game> UpdateGameDeckIdAsync(Guid id, string deckId);
}
