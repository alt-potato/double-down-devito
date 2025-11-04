using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Utilities;

namespace Project.Api.Repositories;

/*
    Name: HandRepository.cs
    Description: Repository for Hand entity
    Children: IHandRepository.cs
*/
public class HandRepository(AppDbContext context) : IHandRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Get a hand by its ID.
    /// </summary>
    public async Task<Hand?> GetHandByIdAsync(Guid handId)
    {
        // Validate handId
        if (handId == Guid.Empty)
        {
            throw new ArgumentException("Invalid handId");
        }
        // Retrieve the hand from the database
        Hand? hand = await _context.Hands.FirstOrDefaultAsync(h => h.Id == handId);
        // return hand (can be null)
        return hand;
    }

    /// <summary>
    /// Get all hands in a game.
    /// </summary>
    public async Task<List<Hand>> GetHandsByGameIdAsync(Guid gameId)
    {
        // Validate gameId
        if (gameId == Guid.Empty)
        {
            throw new ArgumentException("Invalid gameId");
        }
        // Retrieve the hands from the database
        List<Hand> hands = await _context
            .Hands.Include(h => h.GamePlayer)
            .Where(h => h.GameId == gameId)
            .ToListAsync();
        // return hands (can be empty)
        return hands;
    }

    /// <summary>
    /// Get all hands in a specific game for a specific user.
    /// </summary>
    public async Task<List<Hand>> GetHandsByGameIdAndUserIdAsync(Guid gameId, Guid userId)
    {
        // Validate gameId and userId
        if (userId == Guid.Empty || gameId == Guid.Empty)
        {
            throw new BadRequestException(
                userId == Guid.Empty ? "Invalid userId" : "Invalid gameId"
            );
        }
        // Retrieve the hands from the database
        List<Hand> hands = await _context
            .Hands.Include(h => h.GamePlayer)
            .Where(h => h.GameId == gameId && h.UserId == userId)
            .ToListAsync();
        // return hands (can be empty)
        return hands;
    }

    /// <summary>
    /// Get a hand by game ID, player order, and hand order.
    /// </summary>
    public async Task<Hand?> GetHandByGameTurnOrderAsync(
        Guid gameId,
        int playerOrder,
        int handOrder
    )
    {
        // Validate gameId
        if (gameId == Guid.Empty)
        {
            throw new ArgumentException("Invalid gameId");
        }

        // Retrieve the hand from the database
        Hand? hand = await _context
            .Hands.Include(h => h.GamePlayer)
            .FirstOrDefaultAsync(h =>
                h.GameId == gameId && h.Order == playerOrder && h.HandNumber == handOrder
            );
        // return hand (can be null)
        return hand;
    }

    /// <summary>
    /// Create a new hand.
    /// </summary>
    public async Task<Hand> CreateHandAsync(Hand hand)
    {
        //check if hand does not exist
        ArgumentNullException.ThrowIfNull(hand);

        // Asynchronously add the hand to the context and save changes
        await _context.Hands.AddAsync(hand);
        await _context.SaveChangesAsync();
        // return hand
        return hand;
    }

    /// <summary>
    /// Update an existing hand.
    /// </summary>
    public async Task<Hand> UpdateHandAsync(Guid handId, Hand hand)
    {
        //check if hand does not exist add hand if it does
        var existingHand =
            await _context.Hands.FindAsync(handId)
            ?? throw new NotFoundException($"Hand with ID {handId} not found");

        // Update properties
        existingHand.Order = hand.Order;
        existingHand.Bet = hand.Bet;

        // Update the hand in the context and save changes
        _context.Hands.Update(existingHand);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The hand you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        // return newly updated hand
        return existingHand;
    }

    /// <summary>
    /// Partially update an existing hand.
    /// </summary>
    public async Task<Hand> PatchHandAsync(Guid handId, int? Order = null, int? Bet = null)
    {
        // Check if hand exists and retrieve it
        var existingHand =
            await _context.Hands.FindAsync(handId)
            ?? throw new NotFoundException($"Hand with ID {handId} not found");

        // Update properties if provided
        existingHand.Order = Order ?? existingHand.Order;
        existingHand.Bet += Bet ?? 0;

        // Update the hand in the context and save changes
        _context.Hands.Update(existingHand);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The hand you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        // Return the updated hand
        return existingHand;
    }

    /// <summary>
    /// Delete a hand by its ID.
    /// </summary>
    public async Task<Hand> DeleteHandAsync(Guid handId)
    {
        // Check if hand exists and retrieve it
        var existingHand =
            await _context.Hands.FindAsync(handId)
            ?? throw new NotFoundException($"Hand with ID {handId} not found");

        // Remove the hand from the context and save changes
        _context.Hands.Remove(existingHand);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The hand you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        // Return the deleted hand
        return existingHand;
    }
}
