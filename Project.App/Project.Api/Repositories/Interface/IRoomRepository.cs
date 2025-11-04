using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IRoomRepository
{
    /// <summary>
    /// Get a room by its ID.
    /// </summary>
    Task<Room?> GetRoomByIdAsync(Guid id);

    /// <summary>
    /// Get all rooms.
    /// </summary>
    Task<List<Room>> GetAllRoomsAsync();

    /// <summary>
    /// Get all active rooms.
    /// </summary>
    Task<List<Room>> GetActiveRoomsAsync();

    /// <summary>
    /// Get all public rooms.
    /// </summary>
    Task<List<Room>> GetPublicRoomsAsync();

    /// <summary>
    /// Get a room by host ID.
    /// </summary>
    Task<Room?> GetRoomByHostIdAsync(Guid hostId);

    /// <summary>
    /// Create a new room.
    /// </summary>
    Task<Room> CreateRoomAsync(Room room);

    /// <summary>
    /// Update an existing room.
    /// </summary>
    Task<Room> UpdateRoomAsync(Room room);

    /// <summary>
    /// Delete a room by its ID.
    /// </summary>
    Task<Room> DeleteRoomAsync(Guid id);

    /// <summary>
    /// Check if a room exists by its ID.
    /// </summary>
    Task<bool> RoomExistsAsync(Guid id);
}
