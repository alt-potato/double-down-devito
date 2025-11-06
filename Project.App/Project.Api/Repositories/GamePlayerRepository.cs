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
    ) => await base.GetAsync(g => g.GameId == gameId && g.UserId == userId, tracking);

    /// <summary>
    /// Check if an entity exists by its composite key. Lighter than GetAsync.
    /// </summary>
    protected override async Task<bool> ExistsAsync(Guid gameId, Guid userId) =>
        await base.ExistsAsync(g => g.GameId == gameId && g.UserId == userId);

    /// <summary>
    /// Get a game player by game ID and user ID.
    /// </summary>
    public async Task<GamePlayer?> GetByGameIdAndUserIdAsync(Guid gameId, Guid userId) =>
        await GetAsync(gameId, userId);

    /// <summary>
    /// Get all game players in a game, including the users in the game.
    /// </summary>
    public Task<IReadOnlyList<GamePlayer>> GetAllByGameIdAsync(
        Guid gameId,
        int? skip = null,
        int? take = null
    ) =>
        base.GetAllAsync(
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
        base.GetAllAsync(
            g => g.UserId == userId,
            // expect navigation property to exist, since it's required
            include: q => q.Include(gp => gp.Game).Include(gp => gp.User!),
            skip: skip,
            take: take
        );

    /// <summary>
    /// Get players in a game by status.
    /// </summary>
    public Task<IReadOnlyList<GamePlayer>> GetAllInRoomByStatusAsync(
        Guid roomId,
        params GamePlayer.PlayerStatus[] statuses
    ) => base.GetAllAsync(gp => gp.GameId == roomId && statuses.Contains(gp.Status));

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
        base.UpdateAsync(
            gameId,
            userId,
            g =>
            {
                g.Balance += balanceDelta;
                g.BalanceDelta += balanceDelta;
            }
        );

    /// <summary>
    /// Update player status.
    /// </summary>
    public Task<GamePlayer> UpdatePlayerStatusAsync(
        Guid gameId,
        Guid userId,
        GamePlayer.PlayerStatus status
    ) => base.UpdateAsync(gameId, userId, r => r.Status = status);

    /// <summary>
    /// Delete a game player.
    /// </summary>
    public new Task<GamePlayer> DeleteAsync(Guid gameId, Guid userId) =>
        base.DeleteAsync(gameId, userId);
}
