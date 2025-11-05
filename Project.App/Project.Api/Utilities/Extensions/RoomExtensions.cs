using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Services.Interface;

namespace Project.Api.Utilities.Extensions;

public static class RoomExtensions
{
    public static async Task<string> GetOrCreateDeckId(
        this Game game,
        IDeckApiService deckApiService,
        IGameRepository gameRepository,
        ILogger? logger = null
    )
    {
        if (game.DeckId == null)
        {
            logger?.LogError(
                "Warning: Room {RoomId} has no deck ID. Attempting to create a new one...",
                game.Id
            );

            // create new deck
            game.DeckId = await deckApiService.CreateDeck();
            await gameRepository.UpdateAsync(game);

            logger?.LogInformation(
                "Successfully created new deck {DeckId} for room {RoomId}!",
                game.DeckId,
                game.Id
            );
        }

        return game.DeckId;
    }
}
