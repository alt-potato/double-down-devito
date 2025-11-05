using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;

namespace Project.Api.Repositories;

public class HandRepository(AppDbContext context, ILogger<HandRepository> logger)
    : Repository<Hand>(context, logger),
        IHandRepository
{
    /// <summary>
    /// Get a hand by its ID.
    /// </summary>
    public async Task<Hand?> GetByIdAsync(Guid handId) => await GetAsync(handId);

    /// <summary>
    /// Get all hands in a game.
    /// </summary>
    public async Task<IReadOnlyList<Hand>> GetByGameIdAsync(Guid gameId) =>
        await GetAllAsync(h => h.GameId == gameId);

    /// <summary>
    /// Get all hands in a specific game for a specific user.
    /// </summary>
    public async Task<IReadOnlyList<Hand>> GetByGameIdAndUserIdAsync(Guid gameId, Guid userId) =>
        await GetAllAsync(h => h.GameId == gameId && h.UserId == userId);

    /// <summary>
    /// Get a hand by game ID, player order, and hand order.
    /// </summary>
    public async Task<Hand?> GetByGameTurnOrderAsync(Guid gameId, int playerOrder, int handOrder) =>
        await GetAsync(h =>
            h.GameId == gameId && h.Order == playerOrder && h.HandNumber == handOrder
        );

    /// <summary>
    /// Create a new hand.
    /// </summary>
    public new async Task<Hand> CreateAsync(Hand hand) => await CreateAsync(hand);

    /// <summary>
    /// Update an existing hand.
    /// </summary>
    public new async Task<Hand> UpdateAsync(Hand hand) => await UpdateAsync(hand);

    /// <summary>
    /// Partially update an existing hand.
    /// </summary>
    public async Task<Hand> PatchAsync(Guid handId, int? Order = null, int? Bet = null) =>
        await UpdateAsync(
            handId,
            h =>
            {
                h.Order = Order ?? h.Order;
                h.Bet = Bet ?? h.Bet;
            }
        );

    /// <summary>
    /// Delete a hand by its ID.
    /// </summary>
    public new async Task<Hand> DeleteAsync(Guid handId) => await DeleteAsync(handId);
}
