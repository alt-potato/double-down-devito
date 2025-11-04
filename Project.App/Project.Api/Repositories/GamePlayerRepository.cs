using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Utilities;

namespace Project.Api.Repositories;

public class GamePlayerRepository(AppDbContext context) : IGamePlayerRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Get a game player by game ID and user ID.
    /// </summary>
    public async Task<GamePlayer?> GetGamePlayerByGameIdAndUserIdAsync(Guid gameId, Guid userId)
    {
        // Validate gameId and userId
        if (gameId == Guid.Empty || userId == Guid.Empty)
        {
            throw new ArgumentException("Invalid gameId or userId");
        }

        // Retrieve the game player from the database
        GamePlayer? gamePlayer = await _context
            .GamePlayers.Include(gp => gp.Game)
            .Include(gp => gp.User)
            .FirstOrDefaultAsync(gp => gp.GameId == gameId && gp.UserId == userId);

        // return game player (can be null)
        return gamePlayer;
    }

    /// <summary>
    /// Get all game players in a game.
    /// </summary>
    public async Task<List<GamePlayer>> GetGamePlayersByGameIdAsync(Guid gameId)
    {
        // Validate gameId
        if (gameId == Guid.Empty)
        {
            throw new ArgumentException("Invalid gameId");
        }

        // Retrieve the game players from the database
        List<GamePlayer> gamePlayers = await _context
            .GamePlayers.Include(gp => gp.Game)
            .Include(gp => gp.User)
            .Where(gp => gp.GameId == gameId)
            .ToListAsync();

        // return game players (can be empty)
        return gamePlayers;
    }

    /// <summary>
    /// Get all games for a user.
    /// </summary>
    public async Task<List<GamePlayer>> GetGamePlayersByUserIdAsync(Guid userId)
    {
        // Validate userId
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Invalid userId");
        }

        // Retrieve the game players from the database
        List<GamePlayer> gamePlayers = await _context
            .GamePlayers.Include(gp => gp.Game)
            .Include(gp => gp.User)
            .Where(gp => gp.UserId == userId)
            .ToListAsync();

        // return game players (can be empty)
        return gamePlayers;
    }

    /// <summary>
    /// Create a new game player.
    /// </summary>
    public async Task<GamePlayer> CreateGamePlayerAsync(GamePlayer gamePlayer)
    {
        // Check if game player is null
        ArgumentNullException.ThrowIfNull(gamePlayer);

        // Asynchronously add the game player to the context and save changes
        await _context.GamePlayers.AddAsync(gamePlayer);
        await _context.SaveChangesAsync();

        // return game player
        return gamePlayer;
    }

    /// <summary>
    /// Update an existing game player.
    /// </summary>
    public async Task<GamePlayer> UpdateGamePlayerAsync(
        Guid gameId,
        Guid userId,
        GamePlayer gamePlayer
    )
    {
        // Check if game player exists
        var existingGamePlayer =
            await _context.GamePlayers.FirstOrDefaultAsync(gp =>
                gp.GameId == gameId && gp.UserId == userId
            )
            ?? throw new NotFoundException(
                $"Game player with GameId {gameId} and UserId {userId} not found"
            );

        // Update properties
        existingGamePlayer.Balance = gamePlayer.Balance;
        existingGamePlayer.BalanceDelta = gamePlayer.BalanceDelta;

        // Update the game player in the context and save changes
        _context.GamePlayers.Update(existingGamePlayer);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The game player you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        // return newly updated game player
        return existingGamePlayer;
    }

    /// <summary>
    /// Update game player balance.
    /// </summary>
    public async Task<GamePlayer> UpdateGamePlayerBalanceAsync(
        Guid gameId,
        Guid userId,
        long balanceDelta
    )
    {
        // Check if game player exists
        var existingGamePlayer =
            await _context.GamePlayers.FirstOrDefaultAsync(gp =>
                gp.GameId == gameId && gp.UserId == userId
            )
            ?? throw new NotFoundException(
                $"Game player with GameId {gameId} and UserId {userId} not found"
            );

        // Update balance and balance delta
        existingGamePlayer.Balance += balanceDelta;
        existingGamePlayer.BalanceDelta += balanceDelta;

        // Update the game player in the context and save changes
        _context.GamePlayers.Update(existingGamePlayer);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The game player you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        // return updated game player
        return existingGamePlayer;
    }

    /// <summary>
    /// Delete a game player.
    /// </summary>
    public async Task<GamePlayer> DeleteGamePlayerAsync(Guid gameId, Guid userId)
    {
        // Check if game player exists
        var existingGamePlayer =
            await _context.GamePlayers.FirstOrDefaultAsync(gp =>
                gp.GameId == gameId && gp.UserId == userId
            )
            ?? throw new NotFoundException(
                $"Game player with GameId {gameId} and UserId {userId} not found"
            );

        // Remove the game player from the context and save changes
        _context.GamePlayers.Remove(existingGamePlayer);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The game player you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        // Return the deleted game player
        return existingGamePlayer;
    }
}
