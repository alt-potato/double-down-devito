using Microsoft.EntityFrameworkCore;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Test.Helpers;
using Project.Test.Helpers.Builders;

namespace Project.Test.Repositories;

public class HandRepositoryTests : RepositoryTestBase<HandRepository, Hand>
{
    [Fact]
    public async Task GetByIdAsync_ReturnsHand_WhenHandExists()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user = await SeedData(
            new UserBuilder().WithEmail("test@test.com").WithName("Test User").Build()
        );
        await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user.Id)
                .WithBalance(1000)
                .Build()
        );
        var hand = await SeedData(
            new HandBuilder()
                .WithGameId(game.Id)
                .WithUserId(user.Id)
                .WithOrder(1)
                .WithBet(100)
                .Build()
        );

        // Act
        var result = await _rut.GetByIdAsync(hand.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hand.Id, result.Id);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(1, result.Order);
        Assert.Equal(100, result.Bet);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenHandDoesNotExist()
    {
        // Act
        var result = await _rut.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByGameIdAsync_ReturnsHands_WhenExist()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        var user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );
        await SeedData<GamePlayer>(
            new GamePlayerBuilder().WithGameId(game.Id).WithUserId(user1.Id).WithBalance(1000),
            new GamePlayerBuilder().WithGameId(game.Id).WithUserId(user2.Id).WithBalance(1500)
        );
        await SeedData<Hand>(
            new HandBuilder().WithGameId(game.Id).WithUserId(user1.Id).WithOrder(1).WithBet(100),
            new HandBuilder().WithGameId(game.Id).WithUserId(user2.Id).WithOrder(2).WithBet(200)
        );

        // Act
        var result = await _rut.GetAllByGameIdAsync(game.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, h => Assert.Equal(game.Id, h.GameId));
    }

    [Fact]
    public async Task GetAllByGameIdAsync_ReturnsEmpty_WhenNoHands()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());

        // Act
        var result = await _rut.GetAllByGameIdAsync(game.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByGameIdAndUserIdAsync_ReturnsHands_WhenExist()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user = await SeedData(
            new UserBuilder().WithEmail("test@test.com").WithName("Test User").Build()
        );
        await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user.Id)
                .WithBalance(1000)
                .Build()
        );
        await SeedData<Hand>(
            new HandBuilder()
                .WithGameId(game.Id)
                .WithUserId(user.Id)
                .WithOrder(1)
                .WithHandNumber(1)
                .WithBet(100),
            new HandBuilder()
                .WithGameId(game.Id)
                .WithUserId(user.Id)
                .WithOrder(1)
                .WithHandNumber(2)
                .WithBet(200)
        );

        // Act
        var result = await _rut.GetAllByGameIdAndUserIdAsync(game.Id, user.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, h => Assert.Equal(game.Id, h.GameId));
        Assert.All(result, h => Assert.Equal(user.Id, h.UserId));
    }

    [Fact]
    public async Task GetAllByGameIdAndUserIdAsync_ReturnsEmpty_WhenNoHands()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user = await SeedData(
            new UserBuilder().WithEmail("test@test.com").WithName("Test User").Build()
        );

        // Act
        var result = await _rut.GetAllByGameIdAndUserIdAsync(game.Id, user.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByGameTurnOrderAsync_ReturnsHand_WhenExists()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user = await SeedData(
            new UserBuilder().WithEmail("test@test.com").WithName("Test User").Build()
        );
        await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user.Id)
                .WithBalance(1000)
                .Build()
        );
        await SeedData(
            new HandBuilder()
                .WithGameId(game.Id)
                .WithUserId(user.Id)
                .WithOrder(1)
                .WithHandNumber(1)
                .WithBet(100)
                .Build()
        );

        // Act
        var result = await _rut.GetByGameTurnOrderAsync(game.Id, 1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(1, result.Order);
        Assert.Equal(1, result.HandNumber);
        Assert.Equal(100, result.Bet);
    }

    [Fact]
    public async Task GetByGameTurnOrderAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());

        // Act
        var result = await _rut.GetByGameTurnOrderAsync(game.Id, 1, 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesHandSuccessfully()
    {
        // Arrange
        var hand = new HandBuilder()
            .WithGameId(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithOrder(1)
            .WithBet(100)
            .Build();

        // Act
        var result = await _rut.CreateAsync(hand);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Order);
        Assert.Equal(100, result.Bet);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(await _context.Hands.AnyAsync(h => h.Id == result.Id));
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequestException_WhenHandIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _rut.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesHandSuccessfully()
    {
        // Arrange
        var hand = await SeedData(
            new HandBuilder()
                .WithGameId(Guid.NewGuid())
                .WithUserId(Guid.NewGuid())
                .WithOrder(1)
                .WithBet(100)
                .Build()
        );

        var updatedHand = new HandBuilder()
            .WithGameId(hand.GameId)
            .WithUserId(hand.UserId)
            .WithOrder(2)
            .WithBet(200)
            .Build();
        updatedHand.Id = hand.Id;

        // Act
        var result = await _rut.UpdateAsync(updatedHand);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Order);
        Assert.Equal(200, result.Bet);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenHandDoesNotExist()
    {
        // Arrange
        var hand = new HandBuilder()
            .WithGameId(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithOrder(1)
            .WithBet(100)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _rut.UpdateAsync(hand));
    }

    [Fact]
    public async Task PatchAsync_UpdatesHandSuccessfully()
    {
        // Arrange
        var hand = await SeedData(
            new HandBuilder()
                .WithGameId(Guid.NewGuid())
                .WithUserId(Guid.NewGuid())
                .WithOrder(1)
                .WithBet(100)
                .Build()
        );

        // Act
        var result = await _rut.PatchAsync(hand.Id, order: 2, bet: 50);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Order);
        Assert.Equal(50, result.Bet); // replaced with 50
    }

    [Fact]
    public async Task PatchAsync_UpdatesOnlyOrder_WhenBetNotProvided()
    {
        // Arrange
        var hand = await SeedData(
            new HandBuilder()
                .WithGameId(Guid.NewGuid())
                .WithUserId(Guid.NewGuid())
                .WithOrder(1)
                .WithBet(100)
                .Build()
        );

        // Act
        var result = await _rut.PatchAsync(hand.Id, order: 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Order);
        Assert.Equal(100, result.Bet); // unchanged
    }

    [Fact]
    public async Task PatchAsync_UpdatesOnlyBet_WhenOrderNotProvided()
    {
        // Arrange
        var hand = await SeedData(
            new HandBuilder()
                .WithGameId(Guid.NewGuid())
                .WithUserId(Guid.NewGuid())
                .WithOrder(1)
                .WithBet(100)
                .Build()
        );

        // Act
        var result = await _rut.PatchAsync(hand.Id, bet: 50);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Order); // unchanged
        Assert.Equal(50, result.Bet); // replaced with 50
    }

    [Fact]
    public async Task PatchAsync_ThrowsNotFoundException_WhenHandDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _rut.PatchAsync(Guid.NewGuid(), order: 2)
        );
    }

    [Fact]
    public async Task DeleteAsync_DeletesHandSuccessfully()
    {
        // Arrange
        var hand = await SeedData(
            new HandBuilder()
                .WithGameId(Guid.NewGuid())
                .WithUserId(Guid.NewGuid())
                .WithOrder(1)
                .WithBet(100)
                .Build()
        );

        // Act
        var result = await _rut.DeleteAsync(hand.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hand.Id, result.Id);
        Assert.False(await _context.Hands.AnyAsync(h => h.Id == hand.Id));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenHandDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _rut.DeleteAsync(Guid.NewGuid()));
    }
}
