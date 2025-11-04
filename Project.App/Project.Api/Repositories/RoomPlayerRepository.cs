using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Utilities;
using Project.Api.Utilities.Enums;

namespace Project.Api.Repositories;

public class RoomPlayerRepository(AppDbContext context) : IRoomPlayerRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Get a room player by room ID and user ID.
    /// </summary>
    public async Task<RoomPlayer?> GetRoomPlayerByIdAsync(Guid roomId, Guid userId)
    {
        return await _context
            .RoomPlayers.Include(rp => rp.Room)
            .Include(rp => rp.User)
            .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userId);
    }

    /// <summary>
    /// Get all room players.
    /// </summary>
    public async Task<List<RoomPlayer>> GetAllRoomPlayersAsync()
    {
        return await _context
            .RoomPlayers.Include(rp => rp.Room)
            .Include(rp => rp.User)
            .ToListAsync();
    }

    /// <summary>
    /// Get all room players in a room.
    /// </summary>
    public async Task<List<RoomPlayer>> GetRoomPlayersByRoomIdAsync(Guid roomId)
    {
        return await _context
            .RoomPlayers.Include(rp => rp.Room)
            .Include(rp => rp.User)
            .Where(rp => rp.RoomId == roomId)
            .ToListAsync();
    }

    /// <summary>
    /// Get all room players for a user.
    /// </summary>
    public async Task<List<RoomPlayer>> GetRoomPlayersByUserIdAsync(Guid userId)
    {
        return await _context
            .RoomPlayers.Include(rp => rp.Room)
            .Include(rp => rp.User)
            .Where(rp => rp.UserId == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Get a room player by room ID and user ID.
    /// </summary>
    public async Task<RoomPlayer?> GetRoomPlayerByRoomIdAndUserIdAsync(Guid roomId, Guid userId)
    {
        return await _context
            .RoomPlayers.Include(rp => rp.Room)
            .Include(rp => rp.User)
            .FirstOrDefaultAsync(rp => rp.RoomId == roomId && rp.UserId == userId);
    }

    /// <summary>
    /// Get players in a room by status.
    /// </summary>
    public async Task<List<RoomPlayer>> GetPlayersInRoomByStatusAsync(
        Guid roomId,
        params Status[] statuses
    )
    {
        return await _context
            .RoomPlayers.Include(rp => rp.Room)
            .Include(rp => rp.User)
            .Where(rp => rp.RoomId == roomId && statuses.Contains(rp.Status))
            .ToListAsync();
    }

    /// <summary>
    /// Create a new room player.
    /// </summary>
    public async Task<RoomPlayer> CreateRoomPlayerAsync(RoomPlayer roomPlayer)
    {
        _context.RoomPlayers.Add(roomPlayer);
        await _context.SaveChangesAsync();
        return roomPlayer;
    }

    /// <summary>
    /// Update an existing room player.
    /// </summary>
    public async Task<RoomPlayer> UpdateRoomPlayerAsync(RoomPlayer roomPlayer)
    {
        var existingRoomPlayer =
            await _context.RoomPlayers.FirstOrDefaultAsync(rp =>
                rp.RoomId == roomPlayer.RoomId && rp.UserId == roomPlayer.UserId
            )
            ?? throw new NotFoundException(
                $"Room player with RoomId {roomPlayer.RoomId} and UserId {roomPlayer.UserId} not found"
            );

        _context.Entry(existingRoomPlayer).CurrentValues.SetValues(roomPlayer);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The room player you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingRoomPlayer;
    }

    /// <summary>
    /// Delete a room player by room ID and user ID.
    /// </summary>
    public async Task<RoomPlayer> DeleteRoomPlayerAsync(Guid roomId, Guid userId)
    {
        var existingRoomPlayer =
            await _context.RoomPlayers.FirstOrDefaultAsync(rp =>
                rp.RoomId == roomId && rp.UserId == userId
            )
            ?? throw new NotFoundException(
                $"Room player with RoomId {roomId} and UserId {userId} not found"
            );

        _context.RoomPlayers.Remove(existingRoomPlayer);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The room player you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingRoomPlayer;
    }

    /// <summary>
    /// Check if a player is in a room.
    /// </summary>
    public async Task<bool> IsPlayerInRoomAsync(Guid roomId, Guid userId)
    {
        return await _context.RoomPlayers.AnyAsync(rp =>
            rp.RoomId == roomId && rp.UserId == userId
        );
    }

    /// <summary>
    /// Get the player count in a room.
    /// </summary>
    public async Task<int> GetPlayerCountInRoomAsync(Guid roomId)
    {
        return await _context.RoomPlayers.CountAsync(rp => rp.RoomId == roomId);
    }

    /// <summary>
    /// Update player status.
    /// </summary>
    public async Task<RoomPlayer> UpdatePlayerStatusAsync(Guid roomId, Guid userId, Status status)
    {
        var existingRoomPlayer =
            await _context.RoomPlayers.FirstOrDefaultAsync(rp =>
                rp.RoomId == roomId && rp.UserId == userId
            )
            ?? throw new NotFoundException(
                $"Room player with RoomId {roomId} and UserId {userId} not found"
            );

        existingRoomPlayer.Status = status;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The room player you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingRoomPlayer;
    }
}
