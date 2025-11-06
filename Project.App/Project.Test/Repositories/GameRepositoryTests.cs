using Microsoft.EntityFrameworkCore;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Test.Helpers;
using Project.Test.Helpers.Builders;

namespace Project.Test.Repositories;

public class GameRepositoryTests : RepositoryTestBase<GameRepository, Game>
{
    [Fact]
    public async Task GetByIdAsync_ReturnsGame_WhenGameExists()
    {
        // Arrange
        Game game = await SeedData<Game>(new GameBuilder().WithGameMode("Blackjack"));

        // Act
        var result = await _rut.GetByIdAsync(game.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.Id);
        Assert.Equal("Blackjack", result.GameMode);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenGameDoesNotExist()
    {
        // Act
        var result = await _rut.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllGames_WhenNoFilters()
    {
        // Arrange
        await SeedData<Game>(
            new GameBuilder().WithGameMode("Blackjack"),
            new GameBuilder().WithGameMode("Poker")
        );

        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByGameMode()
    {
        // Arrange
        await SeedData<Game>(
            new GameBuilder().WithGameMode("Blackjack"),
            new GameBuilder().WithGameMode("Poker")
        );

        // Act
        var result = await _rut.GetAllAsync(gameMode: "Blackjack");

        // Assert
        Assert.Single(result);
        Assert.Equal("Blackjack", result[0].GameMode);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByCreatedBefore()
    {
        // Arrange
        var cutoffDate = DateTimeOffset.UtcNow;
        var game1 = new GameBuilder().WithGameMode("Blackjack");
        var game2 = new GameBuilder().WithGameMode("Poker");

        await SeedData<Game>(game1, game2);

        // Act
        var result = await _rut.GetAllAsync(createdBefore: cutoffDate.AddMinutes(1));

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByCreatedAfter()
    {
        // Arrange
        var cutoffDate = DateTimeOffset.UtcNow.AddMinutes(-1);
        var game1 = new GameBuilder().WithGameMode("Blackjack");
        var game2 = new GameBuilder().WithGameMode("Poker");

        await SeedData<Game>(game1, game2);

        // Act
        var result = await _rut.GetAllAsync(createdAfter: cutoffDate);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStartedBefore()
    {
        // Arrange
        DateTimeOffset cutoffDate = DateTimeOffset.UtcNow;
        Game game1 = new GameBuilder()
            .WithGameMode("Blackjack")
            .StartedAt(cutoffDate.AddMinutes(-30));
        Game game2 = new GameBuilder().WithGameMode("Poker").StartedAt(cutoffDate.AddMinutes(30));

        await SeedData<Game>(game1, game2);

        // Act
        var result = await _rut.GetAllAsync(startedBefore: cutoffDate);

        // Assert
        Assert.Single(result);
        Assert.Equal("Blackjack", result[0].GameMode);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStartedAfter()
    {
        // Arrange
        DateTimeOffset cutoffDate = DateTimeOffset.UtcNow;
        Game game1 = new GameBuilder()
            .WithGameMode("Blackjack")
            .StartedAt(cutoffDate.AddMinutes(-30));
        Game game2 = new GameBuilder().WithGameMode("Poker").StartedAt(cutoffDate.AddMinutes(30));

        await SeedData<Game>(game1, game2);

        // Act
        var result = await _rut.GetAllAsync(startedAfter: cutoffDate);

        // Assert
        Assert.Single(result);
        Assert.Equal("Poker", result[0].GameMode);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByHasEnded_True()
    {
        // Arrange
        await SeedData<Game>(
            new GameBuilder().WithGameMode("Blackjack").EndedAt(DateTimeOffset.UtcNow),
            new GameBuilder().WithGameMode("Poker")
        );

        // Act
        var result = await _rut.GetAllAsync(hasEnded: true);

        // Assert
        Assert.Single(result);
        Assert.Equal("Blackjack", result[0].GameMode);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByHasEnded_False()
    {
        // Arrange
        await SeedData<Game>(
            new GameBuilder().WithGameMode("Blackjack").EndedAt(DateTimeOffset.UtcNow),
            new GameBuilder().WithGameMode("Poker")
        );

        // Act
        var result = await _rut.GetAllAsync(hasEnded: false);

        // Assert
        Assert.Single(result);
        Assert.Equal("Poker", result[0].GameMode);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByHasEnded_Null()
    {
        // Arrange
        await SeedData<Game>(
            new GameBuilder().WithGameMode("Blackjack").EndedAt(DateTimeOffset.UtcNow),
            new GameBuilder().WithGameMode("Poker")
        );

        // Act
        var result = await _rut.GetAllAsync(hasEnded: null);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithSkipAndTake_PaginatesResults()
    {
        // Arrange
        await SeedData(
            new GameBuilder().WithGameMode("Blackjack").Build(),
            new GameBuilder().WithGameMode("Poker").Build(),
            new GameBuilder().WithGameMode("Blackjack").Build()
        );

        // Act
        var result = await _rut.GetAllAsync(skip: 1, take: 1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Poker", result[0].GameMode);
    }

    [Fact]
    public async Task GetAllAsync_IncludesGamePlayersAndUsers()
    {
        // Arrange
        var game = new GameBuilder().WithGameMode("Blackjack").Build();
        var user = new UserBuilder().WithName("Test User").Build();
        var gamePlayer = new GamePlayerBuilder().WithUser(user).WithGame(game).Build();

        await SeedData(user);
        await SeedData(game);
        await SeedData(gamePlayer);

        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        Assert.Single(result);
        var retrievedGame = result[0];
        Assert.Single(retrievedGame.GamePlayers);
        Assert.NotNull(retrievedGame.GamePlayers.First().User);
        Assert.Equal("Test User", retrievedGame.GamePlayers.First().User!.Name);
    }

    [Fact]
    public async Task CreateAsync_CreatesGameSuccessfully()
    {
        // Arrange
        Game game = new GameBuilder().WithGameMode("Blackjack");

        // Act
        var result = await _rut.CreateAsync(game);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Blackjack", result.GameMode);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(await _context.Games.AnyAsync(g => g.Id == result.Id));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesGameSuccessfully()
    {
        // Arrange
        Game game = await SeedData<Game>(new GameBuilder().WithGameMode("Blackjack"));

        // Modify the game
        game.GameMode = "Poker";
        game.Round = 5;

        // Act
        var result = await _rut.UpdateAsync(game);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Poker", result.GameMode);
        Assert.Equal(5, result.Round);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Arrange
        Game game = new GameBuilder().WithGameMode("Blackjack");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _rut.UpdateAsync(game));
    }

    [Fact]
    public async Task DeleteAsync_DeletesGameSuccessfully()
    {
        // Arrange
        Game game = await SeedData<Game>(new GameBuilder().WithGameMode("Blackjack"));

        // Act
        var result = await _rut.DeleteAsync(game.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Blackjack", result.GameMode);
        Assert.False(await _context.Games.AnyAsync(g => g.Id == game.Id));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _rut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenGameExists()
    {
        // Arrange
        Game game = await SeedData<Game>(new GameBuilder().WithGameMode("Blackjack"));

        // Act
        var result = await _rut.ExistsAsync(game.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenGameDoesNotExist()
    {
        // Act
        var result = await _rut.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateGamestateAsync_UpdatesGameStateSuccessfully()
    {
        // Arrange
        Game game = await SeedData<Game>(new GameBuilder().WithGameMode("Blackjack"));
        var newGameState = "{\"state\": \"playing\", \"currentPlayer\": \"player1\"}";

        // Act
        var result = await _rut.UpdateGamestateAsync(game.Id, newGameState);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newGameState, result.GameState);
    }

    [Fact]
    public async Task UpdateGamestateAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _rut.UpdateGamestateAsync(Guid.NewGuid(), "{\"state\": \"playing\"}")
        );
    }

    [Fact]
    public async Task UpdateRoundAsync_UpdatesRoundSuccessfully()
    {
        // Arrange
        Game game = await SeedData<Game>(new GameBuilder().WithGameMode("Blackjack").WithRound(1));

        // Act
        var result = await _rut.UpdateRoundAsync(game.Id, 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Round);
    }

    [Fact]
    public async Task UpdateRoundAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _rut.UpdateRoundAsync(Guid.NewGuid(), 5));
    }

    [Fact]
    public async Task UpdateDeckIdAsync_UpdatesDeckIdSuccessfully()
    {
        // Arrange
        Game game = await SeedData<Game>(new GameBuilder().WithGameMode("Blackjack"));

        // Act
        var result = await _rut.UpdateDeckIdAsync(game.Id, "new-deck-id");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-deck-id", result.DeckId);
    }

    [Fact]
    public async Task UpdateDeckIdAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _rut.UpdateDeckIdAsync(Guid.NewGuid(), "new-deck-id")
        );
    }

    [Fact]
    public async Task CreateAsync_WithNullGame_ThrowsBadRequestException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _rut.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNullGame_ThrowsBadRequestException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _rut.UpdateAsync(null!));
    }

    [Fact]
    public async Task GetAllAsync_WithComplexFilterCombination_ReturnsCorrectResults()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        var game1 = new GameBuilder()
            .WithGameMode("Blackjack")
            .WithRound(1)
            .StartedAt(now.AddMinutes(-30))
            .EndedAt(now.AddMinutes(-10))
            .Build();

        var game2 = new GameBuilder()
            .WithGameMode("Blackjack")
            .WithRound(2)
            .StartedAt(now.AddMinutes(-10))
            // Not ended
            .Build();

        var game3 = new GameBuilder()
            .WithGameMode("Poker")
            .WithRound(1)
            .StartedAt(now.AddMinutes(-40))
            .EndedAt(now.AddMinutes(-5))
            .Build();

        await SeedData<Game>(game1, game2, game3);

        // Act - Filter for Blackjack games that started more than 25 minutes ago and have ended
        var result = await _rut.GetAllAsync(
            gameMode: "Blackjack",
            startedBefore: now.AddMinutes(-25),
            hasEnded: true
        );

        // Assert
        Assert.Single(result);
        Assert.Equal("Blackjack", result[0].GameMode);
        Assert.NotNull(result[0].EndedAt);
    }

    [Fact]
    public async Task GetAllAsync_OrdersByCreatedAtAscending()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        await SeedData<Game>(
            new GameBuilder().WithGameMode("Blackjack").CreatedAt(now.AddMinutes(-10)),
            new GameBuilder().WithGameMode("Poker").CreatedAt(now.AddMinutes(-5))
        );

        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Blackjack", result[0].GameMode); // Should be first due to earlier CreatedAt
        Assert.Equal("Poker", result[1].GameMode);
    }

    [Fact]
    public async Task Concurrency_UpdateFails_WhenRowVersionMismatch()
    {
        // Arrange
        Game game = await SeedData<Game>(new GameBuilder().WithGameMode("Blackjack"));

        // Simulate concurrent modification by changing the row version
        var concurrentGame = new GameBuilder().WithId(game.Id).WithGameMode("Poker").Build();
        concurrentGame.RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]; // Different rowversion

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _rut.UpdateAsync(concurrentGame));
    }
}
