using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Utilities.Enums;

namespace Project.Api.Repositories;

public class RoomPlayerRepository(AppDbContext context, ILogger<RoomPlayerRepository> logger)
    : CompositeRepository<RoomPlayer, Guid, Guid>(context, logger),
        IRoomPlayerRepository
{
    /// <summary>
    /// Gets an entity by its composite key. If no entity is found, returns null.
    /// </summary>
    protected override async Task<RoomPlayer?> GetAsync(
        Guid roomId,
        Guid userId,
        bool tracking = true
    ) => await GetAsync(r => r.RoomId == roomId && r.UserId == userId, tracking);

    /// <summary>
    /// Check if an entity exists by its composite key. Lighter than GetAsync.
    /// </summary>
    protected override async Task<bool> ExistsAsync(Guid roomId, Guid userId) =>
        await ExistsAsync(r => r.RoomId == roomId && r.UserId == userId);

    /// <summary>
    /// Get a room player by room ID and user ID.
    /// </summary>
    public Task<RoomPlayer?> GetByRoomIdAndUserIdAsync(Guid roomId, Guid userId) =>
        GetAsync(roomId, userId);

    /// <summary>
    /// Get all room players.
    /// </summary>
    public Task<IReadOnlyList<RoomPlayer>> GetAllAsync(int? skip = null, int? take = null) =>
        base.GetAllAsync(skip: skip, take: take);

    /// <summary>
    /// Get all room players in a room.
    /// </summary>
    public Task<IReadOnlyList<RoomPlayer>> GetAllByRoomIdAsync(
        Guid roomId,
        int? skip = null,
        int? take = null
    ) => GetAllAsync(r => r.RoomId == roomId, skip: skip, take: take);

    /// <summary>
    /// Get all room players for a user.
    /// </summary>
    public Task<IReadOnlyList<RoomPlayer>> GetAllByUserIdAsync(
        Guid userId,
        int? skip = null,
        int? take = null
    ) => GetAllAsync(r => r.UserId == userId, skip: skip, take: take);

    /// <summary>
    /// Get players in a room by status.
    /// </summary>
    public Task<IReadOnlyList<RoomPlayer>> GetAllInRoomByStatusAsync(
        Guid roomId,
        params Status[] statuses
    ) => GetAllAsync(r => r.RoomId == roomId && statuses.Contains(r.Status));

    /// <summary>
    /// Create a new room player.
    /// </summary>
    public new Task<RoomPlayer> CreateAsync(RoomPlayer roomPlayer) => base.CreateAsync(roomPlayer);

    /// <summary>
    /// Update an existing room player.
    /// </summary>
    public new Task<RoomPlayer> UpdateAsync(RoomPlayer roomPlayer) => base.UpdateAsync(roomPlayer);

    /// <summary>
    /// Delete a room player by room ID and user ID.
    /// </summary>
    public new Task<RoomPlayer> DeleteAsync(Guid roomId, Guid userId) =>
        base.DeleteAsync(roomId, userId);

    /// <summary>
    /// Check if a player is in a room.
    /// </summary>
    public Task<bool> RoomHasPlayerAsync(Guid roomId, Guid userId) => ExistsAsync(roomId, userId);

    /// <summary>
    /// Get the player count in a room.
    /// </summary>
    public Task<int> GetPlayerCountAsync(Guid roomId) => CountAsync(r => r.RoomId == roomId);

    /// <summary>
    /// Update player status.
    /// </summary>
    public Task<RoomPlayer> UpdatePlayerStatusAsync(Guid roomId, Guid userId, Status status) =>
        UpdateAsync(roomId, userId, r => r.Status = status);
}
