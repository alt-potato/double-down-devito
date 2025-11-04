using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Test.Helpers;

namespace Project.Test.Repositories;

public class HandRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly HandRepository _repository;

    public HandRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new HandRepository(_context);
    }

    [Fact]
    public async Task GetHandByIdAsync_ReturnsHand_WhenHandExists()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        var user = RepositoryTestHelper.CreateTestUser();
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

        var hand = RepositoryTestHelper.CreateTestHand(gameId: game.Id, userId: user.Id);
        await _context.Hands.AddAsync(hand);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHandByIdAsync(hand.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hand.Id, result.Id);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(1, result.Order);
        Assert.Equal(100, result.Bet);
    }

    [Fact]
    public async Task GetHandByIdAsync_ReturnsNull_WhenHandDoesNotExist()
    {
        // Act
        var result = await _repository.GetHandByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetHandByIdAsync_ThrowsArgumentException_WhenInvalidHandId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetHandByIdAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetHandsByGameIdAsync_ReturnsHands_WhenExist()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        var user1 = RepositoryTestHelper.CreateTestUser(email: "user1@test.com", name: "User 1");
        var user2 = RepositoryTestHelper.CreateTestUser(email: "user2@test.com", name: "User 2");
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

        var hands = new[]
        {
            RepositoryTestHelper.CreateTestHand(
                gameId: game.Id,
                userId: user1.Id,
                order: 1,
                bet: 100
            ),
            RepositoryTestHelper.CreateTestHand(
                gameId: game.Id,
                userId: user2.Id,
                order: 2,
                bet: 200
            ),
        };
        await _context.Hands.AddRangeAsync(hands);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHandsByGameIdAsync(game.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, h => Assert.Equal(game.Id, h.GameId));
    }

    [Fact]
    public async Task GetHandsByGameIdAsync_ReturnsEmpty_WhenNoHands()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHandsByGameIdAsync(game.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHandsByGameIdAsync_ThrowsArgumentException_WhenInvalidGameId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetHandsByGameIdAsync(Guid.Empty)
        );
    }

    [Fact]
    public async Task GetHandsByGameIdAndUserIdAsync_ReturnsHands_WhenExist()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        var user = RepositoryTestHelper.CreateTestUser();
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

        var hands = new[]
        {
            RepositoryTestHelper.CreateTestHand(
                gameId: game.Id,
                userId: user.Id,
                order: 1,
                handNumber: 1,
                bet: 100
            ),
            RepositoryTestHelper.CreateTestHand(
                gameId: game.Id,
                userId: user.Id,
                order: 1,
                handNumber: 2,
                bet: 200
            ),
        };
        await _context.Hands.AddRangeAsync(hands);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHandsByGameIdAndUserIdAsync(game.Id, user.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, h => Assert.Equal(game.Id, h.GameId));
        Assert.All(result, h => Assert.Equal(user.Id, h.UserId));
    }

    [Fact]
    public async Task GetHandsByGameIdAndUserIdAsync_ReturnsEmpty_WhenNoHands()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        var user = RepositoryTestHelper.CreateTestUser();
        await _context.Games.AddAsync(game);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHandsByGameIdAndUserIdAsync(game.Id, user.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHandsByGameIdAndUserIdAsync_ThrowsBadRequestException_WhenInvalidIds()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _repository.GetHandsByGameIdAndUserIdAsync(Guid.Empty, Guid.NewGuid())
        );
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _repository.GetHandsByGameIdAndUserIdAsync(Guid.NewGuid(), Guid.Empty)
        );
    }

    [Fact]
    public async Task GetHandByGameTurnOrderAsync_ReturnsHand_WhenExists()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        var user = RepositoryTestHelper.CreateTestUser();
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

        var hand = RepositoryTestHelper.CreateTestHand(
            gameId: game.Id,
            userId: user.Id,
            order: 1,
            handNumber: 1,
            bet: 100
        );
        await _context.Hands.AddAsync(hand);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHandByGameTurnOrderAsync(game.Id, 1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(1, result.Order);
        Assert.Equal(1, result.HandNumber);
        Assert.Equal(100, result.Bet);
    }

    [Fact]
    public async Task GetHandByGameTurnOrderAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Arrange
        var game = RepositoryTestHelper.CreateTestGame();
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHandByGameTurnOrderAsync(game.Id, 1, 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetHandByGameTurnOrderAsync_ThrowsArgumentException_WhenInvalidGameId()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetHandByGameTurnOrderAsync(Guid.Empty, 1, 1)
        );
    }

    [Fact]
    public async Task CreateHandAsync_CreatesHandSuccessfully()
    {
        // Arrange
        var hand = RepositoryTestHelper.CreateTestHand();

        // Act
        var result = await _repository.CreateHandAsync(hand);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Order);
        Assert.Equal(100, result.Bet);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(await _context.Hands.AnyAsync(h => h.Id == result.Id));
    }

    [Fact]
    public async Task CreateHandAsync_ThrowsArgumentNullException_WhenHandIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateHandAsync(null!));
    }

    [Fact]
    public async Task UpdateHandAsync_UpdatesHandSuccessfully()
    {
        // Arrange
        var hand = RepositoryTestHelper.CreateTestHand();
        await _context.Hands.AddAsync(hand);
        await _context.SaveChangesAsync();

        var updatedHand = RepositoryTestHelper.CreateTestHand(
            gameId: hand.GameId,
            userId: hand.UserId,
            order: 2,
            bet: 200
        );
        updatedHand.Id = hand.Id;

        // Act
        var result = await _repository.UpdateHandAsync(hand.Id, updatedHand);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Order);
        Assert.Equal(200, result.Bet);
    }

    [Fact]
    public async Task UpdateHandAsync_ThrowsNotFoundException_WhenHandDoesNotExist()
    {
        // Arrange
        var hand = new Hand
        {
            Id = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Order = 1,
            Bet = 100,
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.UpdateHandAsync(hand.Id, hand)
        );
    }

    [Fact]
    public async Task PatchHandAsync_UpdatesHandSuccessfully()
    {
        // Arrange
        var hand = RepositoryTestHelper.CreateTestHand();
        await _context.Hands.AddAsync(hand);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PatchHandAsync(hand.Id, Order: 2, Bet: 50);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Order);
        Assert.Equal(150, result.Bet); // 100 + 50
    }

    [Fact]
    public async Task PatchHandAsync_UpdatesOnlyOrder_WhenBetNotProvided()
    {
        // Arrange
        var hand = RepositoryTestHelper.CreateTestHand();
        await _context.Hands.AddAsync(hand);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PatchHandAsync(hand.Id, Order: 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Order);
        Assert.Equal(100, result.Bet); // unchanged
    }

    [Fact]
    public async Task PatchHandAsync_UpdatesOnlyBet_WhenOrderNotProvided()
    {
        // Arrange
        var hand = new Hand
        {
            GameId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Order = 1,
            Bet = 100,
        };
        await _context.Hands.AddAsync(hand);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.PatchHandAsync(hand.Id, Bet: 50);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Order); // unchanged
        Assert.Equal(150, result.Bet); // 100 + 50
    }

    [Fact]
    public async Task PatchHandAsync_ThrowsNotFoundException_WhenHandDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.PatchHandAsync(Guid.NewGuid(), Order: 2)
        );
    }

    [Fact]
    public async Task DeleteHandAsync_DeletesHandSuccessfully()
    {
        // Arrange
        var hand = new Hand
        {
            GameId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Order = 1,
            Bet = 100,
        };
        await _context.Hands.AddAsync(hand);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteHandAsync(hand.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hand.Id, result.Id);
        Assert.False(await _context.Hands.AnyAsync(h => h.Id == hand.Id));
    }

    [Fact]
    public async Task DeleteHandAsync_ThrowsNotFoundException_WhenHandDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.DeleteHandAsync(Guid.NewGuid())
        );
    }
}
