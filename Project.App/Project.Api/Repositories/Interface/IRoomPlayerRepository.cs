using Project.Api.Models;
using Project.Api.Utilities.Enums;

namespace Project.Api.Repositories.Interface;

public interface IRoomPlayerRepository
{
    /// <summary>
    /// Get a room player by room ID and user ID.
    /// </summary>
    Task<RoomPlayer?> GetRoomPlayerByRoomIdAndUserIdAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Get all room players.
    /// </summary>
    Task<List<RoomPlayer>> GetAllRoomPlayersAsync();

    /// <summary>
    /// Get all room players in a room.
    /// </summary>
    Task<List<RoomPlayer>> GetRoomPlayersByRoomIdAsync(Guid roomId);

    /// <summary>
    /// Get all room players for a user.
    /// </summary>
    Task<List<RoomPlayer>> GetRoomPlayersByUserIdAsync(Guid userId);

    /// <summary>
    /// Get players in a room by status.
    /// </summary>
    Task<List<RoomPlayer>> GetPlayersInRoomByStatusAsync(Guid roomId, params Status[] statuses);

    /// <summary>
    /// Create a new room player.
    /// </summary>
    Task<RoomPlayer> CreateRoomPlayerAsync(RoomPlayer roomPlayer);

    /// <summary>
    /// Update an existing room player.
    /// </summary>
    Task<RoomPlayer> UpdateRoomPlayerAsync(RoomPlayer roomPlayer);

    /// <summary>
    /// Delete a room player by room ID and user ID.
    /// </summary>
    Task<RoomPlayer> DeleteRoomPlayerAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Check if a player is in a room.
    /// </summary>
    Task<bool> IsPlayerInRoomAsync(Guid roomId, Guid userId);

    /// <summary>
    /// Get the player count in a room.
    /// </summary>
    Task<int> GetPlayerCountInRoomAsync(Guid roomId);

    /// <summary>
    /// Update player status.
    /// </summary>
    Task<RoomPlayer> UpdatePlayerStatusAsync(Guid roomId, Guid userId, Status status);
}
