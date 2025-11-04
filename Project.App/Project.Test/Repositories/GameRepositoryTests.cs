using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Test.Helpers;

namespace Project.Test.Repositories;

public class GameRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly GameRepository _repository;
    private readonly string _databaseName;

    public GameRepositoryTests()
    {
        _databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .AddInterceptors(new RowVersionInterceptor())
            .Options;

        _context = new AppDbContext(options);
        _repository = new GameRepository(_context);
    }

    [Fact]
    public async Task GetGameByIdAsync_ReturnsGame_WhenGameExists()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGameByIdAsync(game.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.Id);
        Assert.Equal("Blackjack", result.GameMode);
    }

    [Fact]
    public async Task GetGameByIdAsync_ReturnsNull_WhenGameDoesNotExist()
    {
        // Act
        var result = await _repository.GetGameByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetGamesAsync_ReturnsAllGames_WhenNoFilters()
    {
        // Arrange
        var games = new[]
        {
            RepositoryTestHelper.CreateTestGame(gameMode: "Blackjack"),
            RepositoryTestHelper.CreateTestGame(gameMode: "Poker"),
        };
        await _context.Games.AddRangeAsync(games);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGamesAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetGamesAsync_FiltersByGameMode()
    {
        // Arrange
        var games = new[]
        {
            RepositoryTestHelper.CreateTestGame(gameMode: "Blackjack"),
            RepositoryTestHelper.CreateTestGame(gameMode: "Poker"),
        };
        await _context.Games.AddRangeAsync(games);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGamesAsync(gameMode: "Blackjack");

        // Assert
        Assert.Single(result);
        Assert.Equal("Blackjack", result[0].GameMode);
    }

    [Fact]
    public async Task GetGamesAsync_FiltersByActiveStatus()
    {
        // Arrange
        var activeGame = RepositoryTestHelper.CreateTestGame(gameMode: "Blackjack");
        var endedGame = RepositoryTestHelper.CreateTestGame(
            gameMode: "Poker",
            endedAt: DateTimeOffset.UtcNow
        );
        await _context.Games.AddRangeAsync(activeGame, endedGame);
        await _context.SaveChangesAsync();

        // Act
        var activeResult = await _repository.GetGamesAsync(isActive: true);
        var endedResult = await _repository.GetGamesAsync(isActive: false);

        // Assert
        Assert.Single(activeResult);
        Assert.Equal("Blackjack", activeResult[0].GameMode);
        Assert.Single(endedResult);
        Assert.Equal("Poker", endedResult[0].GameMode);
    }

    [Fact]
    public async Task GetGamesAsync_FiltersByPlayerCount()
    {
        // Arrange
        var game1 = RepositoryTestHelper.CreateTestGame(gameMode: "Blackjack");
        var game2 = RepositoryTestHelper.CreateTestGame(gameMode: "Poker");

        var user1 = RepositoryTestHelper.CreateTestUser(email: "user1@test.com", name: "user1");
        var user2 = RepositoryTestHelper.CreateTestUser(email: "user2@test.com", name: "user2");

        await _context.Games.AddRangeAsync(game1, game2);
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        // Add players to game1
        await _context.GamePlayers.AddAsync(
            RepositoryTestHelper.CreateTestGamePlayer(gameId: game1.Id, userId: user1.Id)
        );
        await _context.GamePlayers.AddAsync(
            RepositoryTestHelper.CreateTestGamePlayer(gameId: game1.Id, userId: user2.Id)
        );

        // Add one player to game2
        await _context.GamePlayers.AddAsync(
            RepositoryTestHelper.CreateTestGamePlayer(gameId: game2.Id, userId: user1.Id)
        );

        await _context.SaveChangesAsync();

        // Act
        var minPlayersResult = await _repository.GetGamesAsync(minPlayers: 2);
        var maxPlayersResult = await _repository.GetGamesAsync(maxPlayers: 1);

        // Assert
        Assert.Single(minPlayersResult);
        Assert.Equal("Blackjack", minPlayersResult[0].GameMode);
        Assert.Single(maxPlayersResult);
        Assert.Equal("Poker", maxPlayersResult[0].GameMode);
    }

    [Fact]
    public async Task GetGamesAsync_SearchesByGameMode()
    {
        // Arrange
        var games = new[]
        {
            RepositoryTestHelper.CreateTestGame(gameMode: "Blackjack Tournament"),
            RepositoryTestHelper.CreateTestGame(gameMode: "Poker Night"),
            RepositoryTestHelper.CreateTestGame(gameMode: "Blackjack Pro"),
        };
        await _context.Games.AddRangeAsync(games);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGamesAsync(search: "Blackjack");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, g => Assert.Contains("Blackjack", g.GameMode));
    }

    [Fact]
    public async Task CreateGameAsync_CreatesGameSuccessfully()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();

        // Act
        var result = await _repository.CreateGameAsync(game);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Blackjack", result.GameMode);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(await _context.Games.AnyAsync(g => g.Id == result.Id));
    }

    [Fact]
    public async Task UpdateGameAsync_UpdatesGameSuccessfully()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        var updatedGame = RepositoryTestHelper.CreateTestGame(gameMode: "Poker");
        updatedGame.Id = game.Id;

        // Act
        var result = await _repository.UpdateGameAsync(updatedGame);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Poker", result.GameMode);
    }

    [Fact]
    public async Task UpdateGameAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateGameAsync(game));
    }

    [Fact]
    public async Task DeleteGameAsync_DeletesGameSuccessfully()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteGameAsync(game.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Blackjack", result.GameMode);
        Assert.False(await _context.Games.AnyAsync(g => g.Id == game.Id));
    }

    [Fact]
    public async Task DeleteGameAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.DeleteGameAsync(Guid.NewGuid())
        );
    }

    [Fact]
    public async Task GameExistsAsync_ReturnsTrue_WhenGameExists()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GameExistsAsync(game.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GameExistsAsync_ReturnsFalse_WhenGameDoesNotExist()
    {
        // Act
        var result = await _repository.GameExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateGameStateAsync_UpdatesStateSuccessfully()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateGameStateAsync(game.Id, "{\"state\": \"playing\"}");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("{\"state\": \"playing\"}", result.State);
    }

    [Fact]
    public async Task UpdateGameRoundAsync_UpdatesRoundSuccessfully()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame(round: 1);
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateGameRoundAsync(game.Id, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Round);
    }

    [Fact]
    public async Task UpdateGameDeckIdAsync_UpdatesDeckIdSuccessfully()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame(deckId: "old");
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateGameDeckIdAsync(game.Id, "new");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new", result.DeckId);
    }

    [Fact]
    public async Task UpdateGameStateAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.UpdateGameStateAsync(Guid.NewGuid(), "{\"state\": \"playing\"}")
        );
    }

    [Fact]
    public async Task UpdateGameRoundAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.UpdateGameRoundAsync(Guid.NewGuid(), 2)
        );
    }

    [Fact]
    public async Task UpdateGameDeckIdAsync_ThrowsNotFoundException_WhenGameDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.UpdateGameDeckIdAsync(Guid.NewGuid(), "new")
        );
    }

    // TODO: concurrency tests
}
