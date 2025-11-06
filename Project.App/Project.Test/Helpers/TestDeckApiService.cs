using Project.Api.DTOs;
using Project.Api.Services.Interface;
using Project.Api.Utilities.Extensions;

namespace Project.Test.Helpers;

public class TestDeckApiService : IDeckApiService
{
    private readonly Dictionary<
        string,
        (Queue<CardDTO>, Dictionary<string, List<CardDTO>>)
    > _decks = [];

    public Task<bool> AddToHand(string deckId, string handName, string cardCodes)
    {
        // Parse card codes and add to hand
        var cardCodeList = cardCodes.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var cardsToAdd = cardCodeList.Select(code => code.ToCard()).ToList();

        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException($"Deck {deckId} does not exist");
        }

        var (_, hands) = deckData;

        if (!hands.TryGetValue(handName, out var hand))
        {
            hand = [];
            hands[handName] = hand;
        }

        hand.AddRange(cardsToAdd);
        return Task.FromResult(true);
    }

    public Task<string> CreateDeck(int numOfDecks = 6, bool enableJokers = false)
    {
        string newDeckId = Guid.CreateVersion7().ToString();
        _decks.Add(newDeckId, (new Queue<CardDTO>(), []));
        return Task.FromResult(newDeckId);
    }

    public Task<bool> CreateEmptyHand(string deckId, long handId)
    {
        return CreateEmptyHand(deckId, handId.ToString());
    }

    public Task<bool> CreateEmptyHand(string deckId, string handName)
    {
        // if deck doesn't exist, throw
        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException(
                "Test Error: The game tried to create a hand, but the deck does not exist."
            );
        }

        var (_, hands) = deckData;

        // create hand if it doesn't exist
        if (!hands.ContainsKey(handName))
        {
            hands[handName] = [];
        }

        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<CardDTO>> DrawCards(string deckId, long handId, int count = 1)
    {
        return DrawCards(deckId, handId.ToString(), count);
    }

    public Task<IReadOnlyList<CardDTO>> DrawCards(string deckId, string handName, int count = 1)
    {
        // enforce non-negative count
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative");
        }

        // if deck doesn't exist, throw
        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException(
                "Test Error: The game tried to draw a card, but the deck does not exist."
            );
        }

        var (deckQueue, hands) = deckData;

        // create hand if it doesn't exist
        if (!hands.TryGetValue(handName, out var hand))
        {
            hand = [];
            hands[handName] = hand;
        }

        // draw cards from the predetermined queue
        var drawnCards = new List<CardDTO>();
        for (int i = 0; i < count; i++)
        {
            if (deckQueue.Count == 0)
            {
                throw new InvalidOperationException(
                    "Test Error: The game tried to draw a card, but the predefined card sequence for this phase is empty."
                );
            }

            var card = deckQueue.Dequeue();
            drawnCards.Add(card);
            hand.Add(card);
        }

        return Task.FromResult<IReadOnlyList<CardDTO>>(drawnCards);
    }

    public Task<IReadOnlyList<CardDTO>> ListHand(string deckId, string handName)
    {
        // expect deck to exist
        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException($"Deck {deckId} does not exist");
        }

        var (_, hands) = deckData;

        // return empty list if hand doesn't exist
        if (!hands.TryGetValue(handName, out var hand))
        {
            return Task.FromResult<IReadOnlyList<CardDTO>>([]);
        }

        return Task.FromResult<IReadOnlyList<CardDTO>>(hand.AsReadOnly());
    }

    public Task<bool> RemoveFromHand(string deckId, string handName, string cardCodes)
    {
        // expect deck to exist
        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException($"Deck {deckId} does not exist");
        }

        var (_, hands) = deckData;

        // expect hand to exist
        if (!hands.TryGetValue(handName, out var hand))
        {
            throw new InvalidOperationException($"Hand {handName} does not exist in deck {deckId}");
        }

        var codesToRemove = cardCodes.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var removedCount = hand.RemoveAll(card => codesToRemove.Contains(card.Code));
        return Task.FromResult(removedCount > 0);
    }

    public Task<bool> ReturnAllCardsToDeck(string deckId, bool shuffle = true)
    {
        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException($"Deck {deckId} does not exist");
        }

        var (deckQueue, hands) = deckData;

        // return all cards to deck
        foreach (var hand in hands.Values)
        {
            foreach (var card in hand)
            {
                deckQueue.Enqueue(card);
            }
            hand.Clear();
        }

        // ignore shuffle parameter for deterministic testing
        // should not be relied on to shuffle the deck, and really should not be used at all tbh.
        return Task.FromResult(true);
    }

    // --- TEST HELPER METHODS ---

    public void AddCardsToDeck(string deckId, params CardDTO[] cards)
    {
        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException($"Deck {deckId} does not exist");
        }

        var (deckQueue, _) = deckData;
        foreach (var card in cards)
        {
            deckQueue.Enqueue(card);
        }
    }

    public bool IsDeckEmpty(string deckId)
    {
        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException($"Deck {deckId} does not exist");
        }

        var (deckQueue, _) = deckData;
        return deckQueue.Count == 0;
    }

    public void ClearDeck(string deckId)
    {
        if (!_decks.TryGetValue(deckId, out var deckData))
        {
            throw new InvalidOperationException($"Deck {deckId} does not exist");
        }

        var (deckQueue, hands) = deckData;

        // clear deck queue
        deckQueue.Clear();

        // clear all hands in deck
        foreach (var hand in hands.Values)
        {
            hand.Clear();
        }
    }
}
