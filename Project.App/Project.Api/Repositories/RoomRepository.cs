using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Utilities;

namespace Project.Api.Repositories;

public class RoomRepository(AppDbContext context, ILogger<RoomRepository> logger)
    : Repository<Room>(context, logger),
        IRoomRepository
{
    /// <summary>
    /// Get a room by its ID.
    /// </summary>
    public async Task<Room?> GetByIdAsync(Guid id) => await GetAsync(id);

    /// <summary>
    /// Get all rooms.
    /// </summary>
    public async Task<IReadOnlyList<Room>> GetAllAsync(int? skip = null, int? take = null) =>
        await GetAllAsync(skip: skip, take: take);

    /// <summary>
    /// Get all active rooms.
    /// </summary>
    public async Task<IReadOnlyList<Room>> GetAllActiveAsync() =>
        await GetAllAsync(r => r.IsActive);

    /// <summary>
    /// Get all public rooms.
    /// </summary>
    public async Task<IReadOnlyList<Room>> GetAllPublicAsync() =>
        await GetAllAsync(r => r.IsPublic);

    /// <summary>
    /// Get a room by host ID.
    /// </summary>
    public async Task<Room?> GetByHostIdAsync(Guid hostId) =>
        await GetAsync(r => r.HostId == hostId);

    /// <summary>
    /// Create a new room.
    /// </summary>
    public new async Task<Room> CreateAsync(Room room) => await CreateAsync(room);

    /// <summary>
    /// Update an existing room.
    /// </summary>
    public new async Task<Room> UpdateAsync(Room room) => await UpdateAsync(room);

    /// <summary>
    /// Delete a room by its ID.
    /// </summary>
    public new async Task<Room> DeleteAsync(Guid id) => await DeleteAsync(id);

    /// <summary>
    /// Check if a room exists by its ID.
    /// </summary>
    public new async Task<bool> ExistsAsync(Guid id) => await ExistsAsync(id);
}
