using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Utilities;

namespace Project.Api.Repositories;

public class GameRepository(AppDbContext context) : IGameRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Get a game by its ID.
    /// </summary>
    public async Task<Game?> GetGameByIdAsync(Guid id)
    {
        return await _context
            .Games.Include(g => g.GamePlayers)
            .ThenInclude(gp => gp.User)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <summary>
    /// Get all games with optional filtering parameters.
    /// </summary>
    public async Task<List<Game>> GetGamesAsync(
        string? gameMode = null,
        bool? isActive = null,
        int? minPlayers = null,
        int? maxPlayers = null,
        DateTimeOffset? createdAfter = null,
        DateTimeOffset? createdBefore = null,
        string? search = null
    )
    {
        var query = _context
            .Games.Include(g => g.GamePlayers)
            .ThenInclude(gp => gp.User)
            .Include(g => g.Hands)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(gameMode))
        {
            query = query.Where(g => g.GameMode == gameMode);
        }

        if (isActive.HasValue)
        {
            // Use EndedAt to determine if game is active
            query = query.Where(g => g.EndedAt == null == isActive.Value);
        }

        if (minPlayers.HasValue)
        {
            query = query.Where(g => g.GamePlayers.Count >= minPlayers.Value);
        }

        if (maxPlayers.HasValue)
        {
            query = query.Where(g => g.GamePlayers.Count <= maxPlayers.Value);
        }

        if (createdAfter.HasValue)
        {
            query = query.Where(g => g.CreatedAt >= createdAfter.Value);
        }

        if (createdBefore.HasValue)
        {
            query = query.Where(g => g.CreatedAt <= createdBefore.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            // Search in GameMode since there's no Description field
            query = query.Where(g => g.GameMode.Contains(search));
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Create a new game.
    /// </summary>
    public async Task<Game> CreateGameAsync(Game game)
    {
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();
        return game;
    }

    /// <summary>
    /// Update an existing game.
    /// </summary>
    public async Task<Game> UpdateGameAsync(Game game)
    {
        var existingGame =
            await _context.Games.FindAsync(game.Id)
            ?? throw new NotFoundException($"Game with ID {game.Id} not found");

        _context.Entry(existingGame).CurrentValues.SetValues(game);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The game you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingGame;
    }

    /// <summary>
    /// Delete a game by its ID.
    /// </summary>
    public async Task<Game> DeleteGameAsync(Guid id)
    {
        var existingGame =
            await _context.Games.FindAsync(id)
            ?? throw new NotFoundException($"Game with ID {id} not found");

        _context.Games.Remove(existingGame);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The game you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingGame;
    }

    /// <summary>
    /// Check if a game exists by its ID.
    /// </summary>
    public async Task<bool> GameExistsAsync(Guid id)
    {
        return await _context.Games.AnyAsync(g => g.Id == id);
    }

    /// <summary>
    /// Update game state.
    /// </summary>
    public async Task<Game> UpdateGameStateAsync(Guid id, string gameState)
    {
        var existingGame =
            await _context.Games.FindAsync(id)
            ?? throw new NotFoundException($"Game with ID {id} not found");

        existingGame.State = gameState;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The game you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingGame;
    }

    /// <summary>
    /// Update game round.
    /// </summary>
    public async Task<Game> UpdateGameRoundAsync(Guid id, int round)
    {
        var existingGame =
            await _context.Games.FindAsync(id)
            ?? throw new NotFoundException($"Game with ID {id} not found");

        existingGame.Round = round;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The game you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingGame;
    }

    /// <summary>
    /// Update game deck ID.
    /// </summary>
    public async Task<Game> UpdateGameDeckIdAsync(Guid id, string deckId)
    {
        var existingGame =
            await _context.Games.FindAsync(id)
            ?? throw new NotFoundException($"Game with ID {id} not found");

        existingGame.DeckId = deckId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The game you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingGame;
    }
}
