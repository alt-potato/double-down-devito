using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Utilities;

namespace Project.Api.Repositories;

public class RoomRepository(AppDbContext context) : IRoomRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Get a room by its ID.
    /// </summary>
    public async Task<Room?> GetRoomByIdAsync(Guid id)
    {
        return await _context
            .Rooms.Include(r => r.Host)
            .Include(r => r.RoomPlayers)
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Get all rooms.
    /// </summary>
    public async Task<List<Room>> GetAllRoomsAsync()
    {
        return await _context
            .Rooms.Include(r => r.Host)
            .Include(r => r.RoomPlayers)
            .Include(r => r.Game)
            .ToListAsync();
    }

    /// <summary>
    /// Get all active rooms.
    /// </summary>
    public async Task<List<Room>> GetActiveRoomsAsync()
    {
        return await _context
            .Rooms.Include(r => r.Host)
            .Include(r => r.RoomPlayers)
            .Include(r => r.Game)
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Get all public rooms.
    /// </summary>
    public async Task<List<Room>> GetPublicRoomsAsync()
    {
        return await _context
            .Rooms.Include(r => r.Host)
            .Include(r => r.RoomPlayers)
            .Include(r => r.Game)
            .Where(r => r.IsPublic && r.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Get a room by host ID.
    /// </summary>
    public async Task<Room?> GetRoomByHostIdAsync(Guid hostId)
    {
        return await _context
            .Rooms.Include(r => r.Host)
            .Include(r => r.RoomPlayers)
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.HostId == hostId);
    }

    /// <summary>
    /// Create a new room.
    /// </summary>
    public async Task<Room> CreateRoomAsync(Room room)
    {
        _context.Rooms.Add(room);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The room you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return room;
    }

    /// <summary>
    /// Update an existing room.
    /// </summary>
    public async Task<Room> UpdateRoomAsync(Room room)
    {
        var existingRoom =
            await _context.Rooms.FindAsync(room.Id)
            ?? throw new NotFoundException($"Room with ID {room.Id} not found");

        _context.Entry(existingRoom).CurrentValues.SetValues(room);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The room you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingRoom;
    }

    /// <summary>
    /// Delete a room by its ID.
    /// </summary>
    public async Task<Room> DeleteRoomAsync(Guid id)
    {
        var room =
            await _context.Rooms.FindAsync(id)
            ?? throw new NotFoundException($"Room with ID {id} not found");

        _context.Rooms.Remove(room);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The room you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return room;
    }

    /// <summary>
    /// Check if a room exists by its ID.
    /// </summary>
    public async Task<bool> RoomExistsAsync(Guid id)
    {
        return await _context.Rooms.AnyAsync(r => r.Id == id);
    }
}
