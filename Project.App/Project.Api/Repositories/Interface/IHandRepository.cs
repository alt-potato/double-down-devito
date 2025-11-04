using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IHandRepository
{
    /// <summary>
    /// Get a hand by its ID.
    /// </summary>
    Task<Hand?> GetHandByIdAsync(Guid handId);

    /// <summary>
    /// Get all hands in a game.
    /// </summary>
    Task<List<Hand>> GetHandsByGameIdAsync(Guid gameId);

    /// <summary>
    /// Get all hands in a specific game for a specific user.
    /// </summary>
    Task<List<Hand>> GetHandsByGameIdAndUserIdAsync(Guid gameId, Guid userId);

    /// <summary>
    /// Get a hand by game ID, player order, and hand order.
    /// </summary>
    Task<Hand?> GetHandByGameTurnOrderAsync(Guid gameId, int playerOrder, int handOrder);

    /// <summary>
    /// Create a new hand.
    /// </summary>
    Task<Hand> CreateHandAsync(Hand hand);

    /// <summary>
    /// Update an existing hand.
    /// </summary>
    Task<Hand> UpdateHandAsync(Guid handId, Hand hand);

    /// <summary>
    /// Partially update an existing hand.
    /// </summary>
    Task<Hand> PatchHandAsync(Guid handId, int? Order = null, int? Bet = null);

    /// <summary>
    /// Delete a hand by its ID.
    /// </summary>
    Task<Hand> DeleteHandAsync(Guid handId);
}
