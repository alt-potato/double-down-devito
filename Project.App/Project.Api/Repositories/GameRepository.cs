using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;

namespace Project.Api.Repositories;

public class GameRepository(AppDbContext context, ILogger<GameRepository> logger)
    : Repository<Game>(context, logger),
        IGameRepository
{
    /// <summary>
    /// Get a game by its ID.
    /// </summary>
    public async Task<Game?> GetByIdAsync(Guid id) => await GetAsync(id);

    /// <summary>
    /// Get all games with optional filtering parameters.
    /// </summary>
    public async Task<IReadOnlyList<Game>> GetAllAsync(
        string? gameMode = null,
        DateTimeOffset? createdBefore = null,
        DateTimeOffset? createdAfter = null,
        DateTimeOffset? startedBefore = null,
        DateTimeOffset? startedAfter = null,
        bool? hasEnded = null,
        int? skip = null,
        int? take = null
    ) =>
        await GetAllAsync(
            g =>
                (gameMode == null || g.GameMode == gameMode)
                && (createdBefore == null || g.CreatedAt < createdBefore)
                && (createdAfter == null || g.CreatedAt > createdAfter)
                && (startedBefore == null || g.StartedAt < startedBefore)
                && (startedAfter == null || g.StartedAt > startedAfter)
                && (hasEnded == null || g.EndedAt != null),
            q => q.OrderBy(g => g.CreatedAt),
            q => q.Include(g => g.GamePlayers).ThenInclude(gp => gp.User!), // user should never be null
            skip,
            take
        );

    /// <summary>
    /// Create a new game.
    /// </summary>
    public new async Task<Game> CreateAsync(Game game) => await base.CreateAsync(game);

    /// <summary>
    /// Update an existing game.
    /// </summary>
    public new async Task<Game> UpdateAsync(Game game) => await base.UpdateAsync(game);

    /// <summary>
    /// Delete a game by its ID.
    /// </summary>
    public new async Task<Game> DeleteAsync(Guid id) => await base.DeleteAsync(id);

    /// <summary>
    /// Check if a game exists by its ID.
    /// </summary>
    public new async Task<bool> ExistsAsync(Guid id) => await base.ExistsAsync(id);

    /// <summary>
    /// Update game state.
    /// </summary>
    public async Task<Game> UpdateGamestateAsync(Guid id, string gameState) =>
        await UpdateAsync(id, g => g.GameState = gameState);

    /// <summary>
    /// Update game round.
    /// </summary>
    public async Task<Game> UpdateRoundAsync(Guid id, int round) =>
        await UpdateAsync(id, g => g.Round = round);

    /// <summary>
    /// Update game deck ID.
    /// </summary>
    public async Task<Game> UpdateDeckIdAsync(Guid id, string deckId) =>
        await UpdateAsync(id, g => g.DeckId = deckId);
}
