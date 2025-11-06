using Microsoft.EntityFrameworkCore;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Test.Helpers;
using Project.Test.Helpers.Builders;

namespace Project.Test.Repositories;

public class GamePlayerRepositoryTests : RepositoryTestBase<GamePlayerRepository, GamePlayer>
{
    [Fact]
    public async Task GetByGameIdAndUserIdAsync_ReturnsGamePlayer_WhenExists()
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

        // Act
        var result = await _rut.GetByGameIdAndUserIdAsync(game.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(1000, result.Balance);
    }

    [Fact]
    public async Task GetByGameIdAndUserIdAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Act
        var result = await _rut.GetByGameIdAndUserIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByGameIdAsync_ReturnsGamePlayers_WhenExist()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        var user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );

        await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user1.Id)
                .WithBalance(1000)
                .Build(),
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user2.Id)
                .WithBalance(1500)
                .Build()
        );

        // Act
        var result = await _rut.GetAllByGameIdAsync(game.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, gp => Assert.Equal(game.Id, gp.GameId));
    }

    [Fact]
    public async Task GetAllByGameIdAsync_ReturnsEmpty_WhenNoPlayers()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());

        // Act
        var result = await _rut.GetAllByGameIdAsync(game.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ReturnsGamePlayers_WhenExist()
    {
        // Arrange
        var game1 = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var game2 = await SeedData(new GameBuilder().WithGameMode("Poker").Build());
        var user = await SeedData(
            new UserBuilder().WithEmail("test@test.com").WithName("Test User").Build()
        );

        await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game1.Id)
                .WithUserId(user.Id)
                .WithBalance(1000)
                .Build(),
            new GamePlayerBuilder()
                .WithGameId(game2.Id)
                .WithUserId(user.Id)
                .WithBalance(1500)
                .Build()
        );

        // Act
        var result = await _rut.GetAllByUserIdAsync(user.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, gp => Assert.Equal(user.Id, gp.UserId));
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ReturnsEmpty_WhenNoGames()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("test@test.com").WithName("Test User").Build()
        );

        // Act
        var result = await _rut.GetAllByUserIdAsync(user.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesGamePlayerSuccessfully()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user = await SeedData(
            new UserBuilder().WithEmail("test@test.com").WithName("Test User").Build()
        );
        var gamePlayer = new GamePlayerBuilder()
            .WithGameId(game.Id)
            .WithUserId(user.Id)
            .WithBalance(1000)
            .Build();

        // Act
        var result = await _rut.CreateAsync(gamePlayer);

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
    public async Task CreateAsync_ThrowsBadRequestException_WhenGamePlayerIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _rut.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesGamePlayerSuccessfully()
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
                .WithBalanceDelta(0)
                .Build()
        );

        var updatedGamePlayer = new GamePlayerBuilder()
            .WithGameId(game.Id)
            .WithUserId(user.Id)
            .WithBalance(1500)
            .WithBalanceDelta(500)
            .Build();

        // Act
        var result = await _rut.UpdateAsync(updatedGamePlayer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1500, result.Balance);
        Assert.Equal(500, result.BalanceDelta);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenGamePlayerDoesNotExist()
    {
        // Arrange
        var gamePlayer = new GamePlayerBuilder()
            .WithGameId(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithBalance(1000)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _rut.UpdateAsync(gamePlayer));
    }

    [Fact]
    public async Task UpdateBalanceAsync_UpdatesBalanceSuccessfully()
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
                .WithBalanceDelta(0)
                .Build()
        );

        // Act
        var result = await _rut.UpdateBalanceAsync(game.Id, user.Id, 500);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1500, result.Balance);
        Assert.Equal(500, result.BalanceDelta);
    }

    [Fact]
    public async Task UpdateBalanceAsync_ThrowsNotFoundException_WhenGamePlayerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _rut.UpdateBalanceAsync(Guid.NewGuid(), Guid.NewGuid(), 500)
        );
    }

    [Fact]
    public async Task DeleteAsync_DeletesGamePlayerSuccessfully()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user = await SeedData(
            new UserBuilder().WithEmail("test@test.com").WithName("Test User").Build()
        );
        var gamePlayer = await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user.Id)
                .WithBalance(1000)
                .Build()
        );

        // Act
        var result = await _rut.DeleteAsync(game.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal(user.Id, result.UserId);
        Assert.False(
            await _context.GamePlayers.AnyAsync(gp => gp.GameId == game.Id && gp.UserId == user.Id)
        );
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenGamePlayerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _rut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid())
        );
    }

    [Fact]
    public async Task GetAllInRoomByStatusAsync_ReturnsCorrectPlayers_ForSingleStatus()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        var user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );
        var user3 = await SeedData(
            new UserBuilder().WithEmail("user3@test.com").WithName("User 3").Build()
        );

        await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user1.Id)
                .WithStatus(GamePlayer.PlayerStatus.Active)
                .Build(),
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user2.Id)
                .WithStatus(GamePlayer.PlayerStatus.Away)
                .Build(),
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user3.Id)
                .WithStatus(GamePlayer.PlayerStatus.Active)
                .Build()
        );

        // Act
        var result = await _rut.GetAllInRoomByStatusAsync(game.Id, GamePlayer.PlayerStatus.Active);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, gp => Assert.Equal(GamePlayer.PlayerStatus.Active, gp.Status));
    }

    [Fact]
    public async Task GetAllInRoomByStatusAsync_ReturnsCorrectPlayers_ForMultipleStatuses()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        var user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );
        var user3 = await SeedData(
            new UserBuilder().WithEmail("user3@test.com").WithName("User 3").Build()
        );
        var user4 = await SeedData(
            new UserBuilder().WithEmail("user4@test.com").WithName("User 4").Build()
        );

        await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user1.Id)
                .WithStatus(GamePlayer.PlayerStatus.Active)
                .Build(),
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user2.Id)
                .WithStatus(GamePlayer.PlayerStatus.Away)
                .Build(),
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user3.Id)
                .WithStatus(GamePlayer.PlayerStatus.Inactive)
                .Build(),
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user4.Id)
                .WithStatus(GamePlayer.PlayerStatus.Left)
                .Build()
        );

        // Act
        var result = await _rut.GetAllInRoomByStatusAsync(
            game.Id,
            GamePlayer.PlayerStatus.Away,
            GamePlayer.PlayerStatus.Inactive
        );

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, gp => gp.Status == GamePlayer.PlayerStatus.Away);
        Assert.Contains(result, gp => gp.Status == GamePlayer.PlayerStatus.Inactive);
    }

    [Fact]
    public async Task GetAllInRoomByStatusAsync_ReturnsEmptyList_WhenNoPlayersMatchStatus()
    {
        // Arrange
        var game = await SeedData(new GameBuilder().WithGameMode("Blackjack").Build());
        var user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );

        await SeedData(
            new GamePlayerBuilder()
                .WithGameId(game.Id)
                .WithUserId(user1.Id)
                .WithStatus(GamePlayer.PlayerStatus.Active)
                .Build()
        );

        // Act
        var result = await _rut.GetAllInRoomByStatusAsync(game.Id, GamePlayer.PlayerStatus.Away);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdatePlayerStatusAsync_UpdatesStatusSuccessfully()
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
                .WithStatus(GamePlayer.PlayerStatus.Active)
                .Build()
        );

        // Act
        var result = await _rut.UpdatePlayerStatusAsync(
            game.Id,
            user.Id,
            GamePlayer.PlayerStatus.Inactive
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(GamePlayer.PlayerStatus.Inactive, result.Status);

        var dbEntity = await _context.GamePlayers.FindAsync(game.Id, user.Id);
        Assert.NotNull(dbEntity);
        Assert.Equal(GamePlayer.PlayerStatus.Inactive, dbEntity.Status);
    }

    [Fact]
    public async Task UpdatePlayerStatusAsync_ThrowsNotFoundException_WhenGamePlayerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _rut.UpdatePlayerStatusAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                GamePlayer.PlayerStatus.Away
            )
        );
    }

    [Fact]
    public async Task GetByGameIdAndUserIdAsync_IncludesGameAndUser()
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

        // Act
        var result = await _rut.GetByGameIdAndUserIdAsync(game.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Game);
        Assert.NotNull(result.User);
        Assert.Equal("Blackjack", result.Game!.GameMode);
        Assert.Equal("Test User", result.User!.Name);
    }

    [Fact]
    public async Task GetAllByGameIdAsync_IncludesGameAndUser()
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

        // Act
        var result = await _rut.GetAllByGameIdAsync(game.Id);

        // Assert
        Assert.Single(result);
        var gamePlayer = result[0];
        Assert.NotNull(gamePlayer.Game);
        Assert.NotNull(gamePlayer.User);
        Assert.Equal("Blackjack", gamePlayer.Game!.GameMode);
        Assert.Equal("Test User", gamePlayer.User!.Name);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_IncludesGameAndUser()
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

        // Act
        var result = await _rut.GetAllByUserIdAsync(user.Id);

        // Assert
        Assert.Single(result);
        var gamePlayer = result[0];
        Assert.NotNull(gamePlayer.Game);
        Assert.NotNull(gamePlayer.User);
        Assert.Equal("Blackjack", gamePlayer.Game!.GameMode);
        Assert.Equal("Test User", gamePlayer.User!.Name);
    }
}
