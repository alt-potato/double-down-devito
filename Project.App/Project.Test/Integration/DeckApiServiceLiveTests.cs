using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Api.Services;
using Project.Test.Helpers;

namespace Project.Test.Integration;

public class DeckApiServiceLiveTests : IAsyncLifetime
{
    private readonly DeckApiService _sut;
    private string _deckId = string.Empty;
    private const string DeckApiUrl = "https://deckofcardsapi.com/";

    public DeckApiServiceLiveTests()
    {
        var httpClient = new HttpClient();
        _sut = new DeckApiService(httpClient, Mock.Of<ILogger<DeckApiService>>());
    }

    public async Task InitializeAsync()
    {
        try
        {
            // create single deck for all tests
            _deckId = await _sut.CreateDeck(1);
        }
        catch (HttpRequestException)
        {
            // test should be skipped, but don't throw an exception
            _deckId = string.Empty;
        }
    }

    // external API does not need cleanup
    public Task DisposeAsync() => Task.CompletedTask;

    [LiveApiFact(DeckApiUrl)]
    public async Task CreateDeck_ShouldReturnValidDeckId()
    {
        // Arrange & Act
        var newDeckId = await _sut.CreateDeck(1);

        // Assert
        newDeckId.Should().NotBeNullOrEmpty();

        // Further verification: try to draw from the new deck
        var cards = await _sut.DrawCards(newDeckId, "test_hand", 1);
        cards.Should().HaveCount(1);
    }

    [LiveApiFact(DeckApiUrl)]
    public async Task DrawCards_ShouldReturnCorrectNumberOfCards()
    {
        // Arrange
        const int count = 5;

        // Act
        var cards = await _sut.DrawCards(_deckId, "draw_test_hand", count);

        // Assert
        cards.Should().HaveCount(count);
        cards.Should().OnlyHaveUniqueItems(c => c.Code);
    }

    [LiveApiFact(DeckApiUrl)]
    public async Task DrawCards_WithZeroCount_ShouldReturnEmptyList()
    {
        // Arrange
        const int count = 0;

        // Act
        var cards = await _sut.DrawCards(_deckId, "draw_zero_hand", count);

        // Assert
        cards.Should().BeEmpty();
    }

    [LiveApiFact(DeckApiUrl)]
    public async Task CreateEmptyHand_ShouldSucceed()
    {
        // Arrange
        var handName = $"hand_{Guid.NewGuid()}";

        // Act
        var result = await _sut.CreateEmptyHand(_deckId, handName);
        var handContents = await _sut.ListHand(_deckId, handName);

        // Assert
        result.Should().BeTrue();
        handContents.Should().BeEmpty();
    }

    [LiveApiFact(DeckApiUrl)]
    public async Task ListHand_ForNonExistentHand_ShouldReturnEmptyList()
    {
        // Arrange
        var handName = $"non_existent_hand_{Guid.NewGuid()}";

        // Act
        var handContents = await _sut.ListHand(_deckId, handName);

        // Assert
        handContents.Should().BeEmpty();
    }

    [LiveApiFact(DeckApiUrl)]
    public async Task FullHandManagementWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var handName = $"workflow_hand_{Guid.NewGuid()}";
        var initialCards = await _sut.DrawCards(_deckId, "temp", 3);
        var cardCodesToAdd = string.Join(",", initialCards.Select(c => c.Code));
        var cardToRemove = initialCards.First();

        // Act: Add 3 cards to a new hand
        var addResult = await _sut.AddToHand(_deckId, handName, cardCodesToAdd);
        var handAfterAdd = await _sut.ListHand(_deckId, handName);

        // Assert: Hand should have 3 cards
        addResult.Should().BeTrue();
        handAfterAdd.Should().HaveCount(3);
        handAfterAdd.Select(c => c.Code).Should().BeEquivalentTo(initialCards.Select(c => c.Code));

        // Act: Remove 1 card from the hand
        var removeResult = await _sut.RemoveFromHand(_deckId, handName, cardToRemove.Code);
        var handAfterRemove = await _sut.ListHand(_deckId, handName);

        // Assert: Hand should have 2 cards
        removeResult.Should().BeTrue();
        handAfterRemove.Should().HaveCount(2);
        handAfterRemove.Should().NotContain(c => c.Code == cardToRemove.Code);
    }

    [LiveApiFact(DeckApiUrl)]
    public async Task ReturnAllCardsToDeck_ShouldSucceedAndEmptyPiles()
    {
        // Arrange: Create a separate deck for this test to not interfere with others
        var testDeckId = await _sut.CreateDeck(1);
        var handName = "hand_to_return";
        await _sut.DrawCards(testDeckId, handName, 5);
        var handBeforeReturn = await _sut.ListHand(testDeckId, handName);
        handBeforeReturn.Should().HaveCount(5);

        // Act
        var result = await _sut.ReturnAllCardsToDeck(testDeckId);
        var handAfterReturn = await _sut.ListHand(testDeckId, handName);

        // Assert
        result.Should().BeTrue();
        // The API documentation states that returning cards does not remove piles,
        // but it does empty them.
        handAfterReturn.Should().BeEmpty();
    }
}
