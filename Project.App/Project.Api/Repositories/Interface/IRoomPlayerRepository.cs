using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IRoomPlayerRepository
{
    /// <summary>
    /// Get a room player by room ID and user ID.
    /// </summary>
    Task<RoomPlayer?> GetByRoomIdAndUserIdAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Get all room players.
    /// </summary>
    Task<IReadOnlyList<RoomPlayer>> GetAllAsync(int? skip = null, int? take = null);

    /// <summary>
    /// Get all room players in a room.
    /// </summary>
    Task<IReadOnlyList<RoomPlayer>> GetAllByRoomIdAsync(
        Guid roomId,
        int? skip = null,
        int? take = null
    );

    /// <summary>
    /// Get all room players for a user.
    /// </summary>
    Task<IReadOnlyList<RoomPlayer>> GetAllByUserIdAsync(
        Guid userId,
        int? skip = null,
        int? take = null
    );

    /// <summary>
    /// Add a player to a room.
    /// </summary>
    Task<RoomPlayer> AddAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Update an existing room player.
    /// </summary>
    Task<RoomPlayer> UpdateAsync(RoomPlayer roomPlayer);

    /// <summary>
    /// Delete a room player by room ID and user ID.
    /// </summary>
    Task<RoomPlayer> DeleteAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Check if a player is in a room.
    /// </summary>
    Task<bool> RoomHasPlayerAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Get the player count in a room.
    /// </summary>
    Task<int> GetPlayerCountAsync(Guid roomId);
}
