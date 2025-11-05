using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IRoomRepository
{
    /// <summary>
    /// Get a room by its ID.
    /// </summary>
    Task<Room?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all rooms.
    /// </summary>
    Task<IReadOnlyList<Room>> GetAllAsync(int? skip = null, int? take = null);

    /// <summary>
    /// Get all active rooms.
    /// </summary>
    Task<IReadOnlyList<Room>> GetAllActiveAsync();

    /// <summary>
    /// Get all public rooms.
    /// </summary>
    Task<IReadOnlyList<Room>> GetAllPublicAsync();

    /// <summary>
    /// Get a room by host ID.
    /// </summary>
    Task<Room?> GetByHostIdAsync(Guid hostId);

    /// <summary>
    /// Create a new room.
    /// </summary>
    Task<Room> CreateAsync(Room room);

    /// <summary>
    /// Update an existing room.
    /// </summary>
    Task<Room> UpdateAsync(Room room);

    /// <summary>
    /// Delete a room by its ID.
    /// </summary>
    Task<Room> DeleteAsync(Guid id);

    /// <summary>
    /// Check if a room exists by its ID.
    /// </summary>
    Task<bool> ExistsAsync(Guid id);
}
