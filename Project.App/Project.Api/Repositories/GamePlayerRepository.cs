using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;

namespace Project.Api.Repositories;

public class GamePlayerRepository(AppDbContext context, ILogger<GamePlayerRepository> logger)
    : CompositeRepository<GamePlayer, Guid, Guid>(context, logger),
        IGamePlayerRepository
{
    /// <summary>
    /// Gets an entity by its composite key. If no entity is found, returns null.
    /// </summary>
    protected override async Task<GamePlayer?> GetAsync(
        Guid gameId,
        Guid userId,
        bool tracking = true
    ) => await GetAsync(g => g.GameId == gameId && g.UserId == userId, tracking);

    /// <summary>
    /// Check if an entity exists by its composite key. Lighter than GetAsync.
    /// </summary>
    protected override async Task<bool> ExistsAsync(Guid gameId, Guid userId) =>
        await ExistsAsync(g => g.GameId == gameId && g.UserId == userId);

    /// <summary>
    /// Get a game player by game ID and user ID.
    /// </summary>
    public Task<GamePlayer?> GetByGameIdAndUserIdAsync(Guid gameId, Guid userId) =>
        GetAsync(gameId, userId);

    /// <summary>
    /// Get all game players in a game, including the users in the game.
    /// </summary>
    public Task<IReadOnlyList<GamePlayer>> GetAllByGameIdAsync(
        Guid gameId,
        int? skip = null,
        int? take = null
    ) =>
        GetAllAsync(
            g => g.GameId == gameId,
            // expect navigation property to exist, since it's required
            include: q => q.Include(gp => gp.Game).Include(gp => gp.User!),
            skip: skip,
            take: take
        );

    /// <summary>
    /// Get all games for a user, including the users in the game.
    /// </summary>
    public Task<IReadOnlyList<GamePlayer>> GetAllByUserIdAsync(
        Guid userId,
        int? skip = null,
        int? take = null
    ) =>
        GetAllAsync(
            g => g.UserId == userId,
            // expect navigation property to exist, since it's required
            include: q => q.Include(gp => gp.Game).Include(gp => gp.User!),
            skip: skip,
            take: take
        );

    /// <summary>
    /// Create a new game player.
    /// </summary>
    public new Task<GamePlayer> CreateAsync(GamePlayer gamePlayer) => base.CreateAsync(gamePlayer);

    /// <summary>
    /// Update an existing game player.
    /// </summary>
    public new Task<GamePlayer> UpdateAsync(GamePlayer gamePlayer) => base.UpdateAsync(gamePlayer);

    /// <summary>
    /// Update game player balance.
    /// </summary>
    public Task<GamePlayer> UpdateBalanceAsync(Guid gameId, Guid userId, long balanceDelta) =>
        UpdateAsync(
            gameId,
            userId,
            g =>
            {
                g.Balance += balanceDelta;
                g.BalanceDelta += balanceDelta;
            }
        );

    /// <summary>
    /// Delete a game player.
    /// </summary>
    public new Task<GamePlayer> DeleteAsync(Guid gameId, Guid userId) =>
        base.DeleteAsync(gameId, userId);
}
