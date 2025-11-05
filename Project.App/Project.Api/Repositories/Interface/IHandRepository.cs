using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IHandRepository
{
    /// <summary>
    /// Get a hand by its ID.
    /// </summary>
    Task<Hand?> GetByIdAsync(Guid handId);

    /// <summary>
    /// Get all hands in a game.
    /// </summary>
    Task<IReadOnlyList<Hand>> GetByGameIdAsync(Guid gameId);

    /// <summary>
    /// Get all hands in a specific game for a specific user.
    /// </summary>
    Task<IReadOnlyList<Hand>> GetByGameIdAndUserIdAsync(Guid gameId, Guid userId);

    /// <summary>
    /// Get a hand by game ID, player order, and hand order.
    /// </summary>
    Task<Hand?> GetByGameTurnOrderAsync(Guid gameId, int playerOrder, int handOrder);

    /// <summary>
    /// Create a new hand.
    /// </summary>
    Task<Hand> CreateAsync(Hand hand);

    /// <summary>
    /// Update an existing hand.
    /// </summary>
    Task<Hand> UpdateAsync(Hand hand);

    /// <summary>
    /// Partially update an existing hand.
    /// </summary>
    Task<Hand> PatchAsync(Guid handId, int? Order = null, int? Bet = null);

    /// <summary>
    /// Delete a hand by its ID.
    /// </summary>
    Task<Hand> DeleteAsync(Guid handId);
}
