using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;

namespace Project.Test.Repositories;

public class GamePlayerRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly GamePlayerRepository _repository;

    public GamePlayerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new GamePlayerRepository(_context);
    }

    [Fact]
    public async Task GetGamePlayerByGameIdAndUserIdAsync_ReturnsGamePlayer_WhenExists()
    {
        // Arrange
        var game = new Game { GameMode = "Blackjack", GameState = "{}" };
        var user = new User { Email = "test@test.com", Name = "Test User" };
        await _context.Games.AddAsync(game);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var gamePlayer = new GamePlayer
        {
            GameId = game.Id,
            UserId = user.Id,
            Balance = 1000,
        };
        await _context.GamePlayers.AddAsync(gamePlayer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGamePlayerByGameIdAndUserIdAsync(game.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(1000, result.Balance);
    }

    [Fact]
    public async Task GetGamePlayerByGameIdAndUserIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Act
        var result = await _repository.GetGamePlayerByGameIdAndUserIdAsync(
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetGamePlayerByGameIdAndUserIdAsync_ThrowsArgumentException_WhenInvalidIds()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetGamePlayerByGameIdAndUserIdAsync(Guid.Empty, Guid.NewGuid())
        );
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetGamePlayerByGameIdAndUserIdAsync(Guid.NewGuid(), Guid.Empty)
        );
    }

    [Fact]
    public async Task GetGamePlayersByGameIdAsync_ReturnsGamePlayers_WhenExist()
    {
        // Arrange
        var game = new Game { GameMode = "Blackjack", GameState = "{}" };
        var user1 = new User { Email = "user1@test.com", Name = "User 1" };
        var user2 = new User { Email = "user2@test.com", Name = "User 2" };
        await _context.Games.AddAsync(game);
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var gamePlayers = new[]
        {
            new GamePlayer
            {
                GameId = game.Id,
                UserId = user1.Id,
                Balance = 1000,
            },
            new GamePlayer
            {
                GameId = game.Id,
                UserId = user2.Id,
                Balance = 1500,
            },
        };
        await _context.GamePlayers.AddRangeAsync(gamePlayers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGamePlayersByGameIdAsync(game.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, gp => Assert.Equal(game.Id, gp.GameId));
    }

    [Fact]
    public async Task GetGamePlayersByGameIdAsync_ReturnsEmpty_WhenNoPlayers()
    {
        // Arrange
        var game = new Game { GameMode = "Blackjack", GameState = "{}" };
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGamePlayersByGameIdAsync(game.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetGamePlayersByGameIdAsync_ThrowsArgumentException_WhenInvalidGameId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetGamePlayersByGameIdAsync(Guid.Empty)
        );
    }

    [Fact]
    public async Task GetGamePlayersByUserIdAsync_ReturnsGamePlayers_WhenExist()
    {
        // Arrange
        var game1 = new Game { GameMode = "Blackjack", GameState = "{}" };
        var game2 = new Game { GameMode = "Poker", GameState = "{}" };
        var user = new User { Email = "test@test.com", Name = "Test User" };
        await _context.Games.AddRangeAsync(game1, game2);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var gamePlayers = new[]
        {
            new GamePlayer
            {
                GameId = game1.Id,
                UserId = user.Id,
                Balance = 1000,
            },
            new GamePlayer
            {
                GameId = game2.Id,
                UserId = user.Id,
                Balance = 1500,
            },
        };
        await _context.GamePlayers.AddRangeAsync(gamePlayers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGamePlayersByUserIdAsync(user.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, gp => Assert.Equal(user.Id, gp.UserId));
    }

    [Fact]
    public async Task GetGamePlayersByUserIdAsync_ReturnsEmpty_WhenNoGames()
    {
        // Arrange
        var user = new User { Email = "test@test.com", Name = "Test User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGamePlayersByUserIdAsync(user.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetGamePlayersByUserIdAsync_ThrowsArgumentException_WhenInvalidUserId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetGamePlayersByUserIdAsync(Guid.Empty)
        );
    }

    [Fact]
    public async Task CreateGamePlayerAsync_CreatesGamePlayerSuccessfully()
    {
        // Arrange
        var game = new Game { GameMode = "Blackjack", GameState = "{}" };
        var user = new User { Email = "test@test.com", Name = "Test User" };
        await _context.Games.AddAsync(game);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var gamePlayer = new GamePlayer
        {
            GameId = game.Id,
            UserId = user.Id,
            Balance = 1000,
        };

        // Act
        var result = await _repository.CreateGamePlayerAsync(gamePlayer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(1000, result.Balance);
        Assert.True(
            await _context.GamePlayers.AnyAsync(gp => gp.GameId == game.Id && gp.UserId == user.Id)
        );
    }

    [Fact]
    public async Task CreateGamePlayerAsync_ThrowsArgumentNullException_WhenGamePlayerIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.CreateGamePlayerAsync(null!)
        );
    }

    [Fact]
    public async Task UpdateGamePlayerAsync_UpdatesGamePlayerSuccessfully()
    {
        // Arrange
        var game = new Game { GameMode = "Blackjack", GameState = "{}" };
        var user = new User { Email = "test@test.com", Name = "Test User" };
        await _context.Games.AddAsync(game);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var gamePlayer = new GamePlayer
        {
            GameId = game.Id,
            UserId = user.Id,
            Balance = 1000,
            BalanceDelta = 0,
        };
        await _context.GamePlayers.AddAsync(gamePlayer);
        await _context.SaveChangesAsync();

        var updatedGamePlayer = new GamePlayer
        {
            GameId = game.Id,
            UserId = user.Id,
            Balance = 1500,
            BalanceDelta = 500,
        };

        // Act
        var result = await _repository.UpdateGamePlayerAsync(game.Id, user.Id, updatedGamePlayer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1500, result.Balance);
        Assert.Equal(500, result.BalanceDelta);
    }

    [Fact]
    public async Task UpdateGamePlayerAsync_ThrowsNotFoundException_WhenGamePlayerDoesNotExist()
    {
        // Arrange
        var gamePlayer = new GamePlayer
        {
            GameId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Balance = 1000,
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.UpdateGamePlayerAsync(gamePlayer.GameId, gamePlayer.UserId, gamePlayer)
        );
    }

    [Fact]
    public async Task UpdateGamePlayerBalanceAsync_UpdatesBalanceSuccessfully()
    {
        // Arrange
        var game = new Game { GameMode = "Blackjack", GameState = "{}" };
        var user = new User { Email = "test@test.com", Name = "Test User" };
        await _context.Games.AddAsync(game);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var gamePlayer = new GamePlayer
        {
            GameId = game.Id,
            UserId = user.Id,
            Balance = 1000,
            BalanceDelta = 0,
        };
        await _context.GamePlayers.AddAsync(gamePlayer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateGamePlayerBalanceAsync(game.Id, user.Id, 500);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1500, result.Balance);
        Assert.Equal(500, result.BalanceDelta);
    }

    [Fact]
    public async Task UpdateGamePlayerBalanceAsync_ThrowsNotFoundException_WhenGamePlayerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.UpdateGamePlayerBalanceAsync(Guid.NewGuid(), Guid.NewGuid(), 500)
        );
    }

    [Fact]
    public async Task DeleteGamePlayerAsync_DeletesGamePlayerSuccessfully()
    {
        // Arrange
        var game = new Game { GameMode = "Blackjack", GameState = "{}" };
        var user = new User { Email = "test@test.com", Name = "Test User" };
        await _context.Games.AddAsync(game);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var gamePlayer = new GamePlayer
        {
            GameId = game.Id,
            UserId = user.Id,
            Balance = 1000,
        };
        await _context.GamePlayers.AddAsync(gamePlayer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteGamePlayerAsync(game.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.False(
            await _context.GamePlayers.AnyAsync(gp => gp.GameId == game.Id && gp.UserId == user.Id)
        );
    }

    [Fact]
    public async Task DeleteGamePlayerAsync_ThrowsNotFoundException_WhenGamePlayerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.DeleteGamePlayerAsync(Guid.NewGuid(), Guid.NewGuid())
        );
    }
}
