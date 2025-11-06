using Microsoft.EntityFrameworkCore;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Api.Utilities.Enums;
using Project.Test.Helpers;
using Project.Test.Helpers.Builders;

namespace Project.Test.Repositories;

public class RoomPlayerRepositoryTests : RepositoryTestBase<RoomPlayerRepository, RoomPlayer>
{
    [Fact]
    public async Task GetByRoomIdAndUserIdAsync_ReturnsRoomPlayer_WhenRoomPlayerExists()
    {
        // Arrange
        User user = await SeedData(
            new UserBuilder().WithEmail("test@example.com").WithName("Test User").Build()
        );
        Room room = await SeedData(
            new RoomBuilder().WithHostId(user.Id).WithDescription("Test Room").Build()
        );
        await SeedData(
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user.Id)
                .WithStatus(Status.Active)
                .Build()
        );

        // Act
        var result = await _rut.GetByRoomIdAndUserIdAsync(room.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.RoomId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(Status.Active, result.Status);
    }

    [Fact]
    public async Task GetByRoomIdAndUserIdAsync_ReturnsNull_WhenRoomPlayerDoesNotExist()
    {
        // Act
        var result = await _rut.GetByRoomIdAndUserIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRoomPlayers()
    {
        // Arrange
        User user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        User user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );
        Room room1 = await SeedData(
            new RoomBuilder().WithHostId(user1.Id).WithDescription("Room 1").Build()
        );
        Room room2 = await SeedData(
            new RoomBuilder().WithHostId(user2.Id).WithDescription("Room 2").Build()
        );

        await SeedData(
            new RoomPlayerBuilder()
                .WithRoomId(room1.Id)
                .WithUserId(user1.Id)
                .WithStatus(Status.Active)
                .Build(),
            new RoomPlayerBuilder()
                .WithRoomId(room2.Id)
                .WithUserId(user2.Id)
                .WithStatus(Status.Active)
                .Build()
        );

        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoRoomPlayersExist()
    {
        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByRoomIdAsync_ReturnsRoomPlayers_ForSpecificRoom()
    {
        // Arrange
        User user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        User user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );
        Room room1 = await SeedData(
            new RoomBuilder().WithHostId(user1.Id).WithDescription("Room 1").Build()
        );
        Room room2 = await SeedData(
            new RoomBuilder().WithHostId(user2.Id).WithDescription("Room 2").Build()
        );

        await SeedData(
            new RoomPlayerBuilder()
                .WithRoomId(room1.Id)
                .WithUserId(user1.Id)
                .WithStatus(Status.Active)
                .Build(),
            new RoomPlayerBuilder()
                .WithRoomId(room1.Id)
                .WithUserId(user2.Id)
                .WithStatus(Status.Active)
                .Build(),
            new RoomPlayerBuilder()
                .WithRoomId(room2.Id)
                .WithUserId(user1.Id)
                .WithStatus(Status.Active)
                .Build()
        );

        // Act
        var result = await _rut.GetAllByRoomIdAsync(room1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, rp => Assert.Equal(room1.Id, rp.RoomId));
    }

    [Fact]
    public async Task GetAllByRoomIdAsync_ReturnsEmptyList_WhenNoPlayersInRoom()
    {
        // Arrange
        User user = await SeedData(
            new UserBuilder().WithEmail("test@example.com").WithName("Test User").Build()
        );
        Room room = await SeedData(
            new RoomBuilder().WithHostId(user.Id).WithDescription("Test Room").Build()
        );

        // Act
        var result = await _rut.GetAllByRoomIdAsync(room.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ReturnsRoomPlayers_ForSpecificUser()
    {
        // Arrange
        var user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        var user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );
        var room1 = await SeedData(
            new RoomBuilder().WithHostId(user1.Id).WithDescription("Room 1").Build()
        );
        var room2 = await SeedData(
            new RoomBuilder().WithHostId(user2.Id).WithDescription("Room 2").Build()
        );

        await SeedData<RoomPlayer>(
            new RoomPlayerBuilder()
                .WithRoomId(room1.Id)
                .WithUserId(user1.Id)
                .WithStatus(Status.Active),
            new RoomPlayerBuilder()
                .WithRoomId(room2.Id)
                .WithUserId(user1.Id)
                .WithStatus(Status.Active),
            new RoomPlayerBuilder()
                .WithRoomId(room1.Id)
                .WithUserId(user2.Id)
                .WithStatus(Status.Active)
        );

        // Act
        var result = await _rut.GetAllByUserIdAsync(user1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, rp => Assert.Equal(user1.Id, rp.UserId));
    }

    [Fact]
    public async Task GetAllInRoomByStatusAsync_ReturnsPlayersByStatus()
    {
        // Arrange
        var user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        var user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );
        var user3 = await SeedData(
            new UserBuilder().WithEmail("user3@test.com").WithName("User 3").Build()
        );
        var room = await SeedData(
            new RoomBuilder().WithHostId(user1.Id).WithDescription("Test Room").Build()
        );

        await SeedData(
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user1.Id)
                .WithStatus(Status.Active)
                .Build(),
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user2.Id)
                .WithStatus(Status.Inactive)
                .Build(),
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user3.Id)
                .WithStatus(Status.Away)
                .Build()
        );

        // Act
        var result = await _rut.GetAllInRoomByStatusAsync(room.Id, Status.Active, Status.Away);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, rp => rp.UserId == user1.Id && rp.Status == Status.Active);
        Assert.Contains(result, rp => rp.UserId == user3.Id && rp.Status == Status.Away);
        Assert.DoesNotContain(result, rp => rp.UserId == user2.Id);
    }

    [Fact]
    public async Task CreateAsync_CreatesRoomPlayerSuccessfully()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("test@example.com").WithName("Test User").Build()
        );
        var room = await SeedData(
            new RoomBuilder().WithHostId(user.Id).WithDescription("Test Room").Build()
        );
        var roomPlayer = new RoomPlayerBuilder()
            .WithRoomId(room.Id)
            .WithUserId(user.Id)
            .WithStatus(Status.Active)
            .Build();

        // Act
        var result = await _rut.CreateAsync(roomPlayer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.RoomId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(Status.Active, result.Status);
        Assert.True(
            await _context.RoomPlayers.AnyAsync(rp => rp.RoomId == room.Id && rp.UserId == user.Id)
        );
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRoomPlayerSuccessfully()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("test@example.com").WithName("Test User").Build()
        );
        var room = await SeedData(
            new RoomBuilder().WithHostId(user.Id).WithDescription("Test Room").Build()
        );
        var roomPlayer = await SeedData(
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user.Id)
                .WithStatus(Status.Active)
                .Build()
        );

        var updatedRoomPlayer = new RoomPlayerBuilder()
            .WithRoomId(room.Id)
            .WithUserId(user.Id)
            .WithStatus(Status.Inactive)
            .Build();

        // Act
        var result = await _rut.UpdateAsync(updatedRoomPlayer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Status.Inactive, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_DeletesRoomPlayerSuccessfully()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("test@example.com").WithName("Test User").Build()
        );
        var room = await SeedData(
            new RoomBuilder().WithHostId(user.Id).WithDescription("Test Room").Build()
        );
        var roomPlayer = await SeedData(
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user.Id)
                .WithStatus(Status.Active)
                .Build()
        );

        // Act
        var result = await _rut.DeleteAsync(roomPlayer.RoomId, roomPlayer.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roomPlayer.RoomId, result.RoomId);
        Assert.Equal(roomPlayer.UserId, result.UserId);
        Assert.False(
            await _context.RoomPlayers.AnyAsync(rp =>
                rp.RoomId == roomPlayer.RoomId && rp.UserId == roomPlayer.UserId
            )
        );
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenRoomPlayerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _rut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid())
        );
    }

    [Fact]
    public async Task RoomHasPlayerAsync_ReturnsFalse_WhenPlayerIsNotInRoom()
    {
        // Act
        var result = await _rut.RoomHasPlayerAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPlayerCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var user1 = await SeedData(
            new UserBuilder().WithEmail("user1@test.com").WithName("User 1").Build()
        );
        var user2 = await SeedData(
            new UserBuilder().WithEmail("user2@test.com").WithName("User 2").Build()
        );
        var room = await SeedData(
            new RoomBuilder().WithHostId(user1.Id).WithDescription("Test Room").Build()
        );

        await SeedData(
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user1.Id)
                .WithStatus(Status.Active)
                .Build(),
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user2.Id)
                .WithStatus(Status.Active)
                .Build()
        );

        // Act
        var result = await _rut.GetPlayerCountAsync(room.Id);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetPlayerCountAsync_ReturnsZero_WhenNoPlayersInRoom()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("test@example.com").WithName("Test User").Build()
        );
        var room = await SeedData(
            new RoomBuilder().WithHostId(user.Id).WithDescription("Test Room").Build()
        );

        // Act
        var result = await _rut.GetPlayerCountAsync(room.Id);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task UpdatePlayerStatusAsync_UpdatesStatusSuccessfully()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("test@example.com").WithName("Test User").Build()
        );
        var room = await SeedData(
            new RoomBuilder().WithHostId(user.Id).WithDescription("Test Room").Build()
        );
        var roomPlayer = await SeedData(
            new RoomPlayerBuilder()
                .WithRoomId(room.Id)
                .WithUserId(user.Id)
                .WithStatus(Status.Active)
                .Build()
        );

        // Act
        var result = await _rut.UpdatePlayerStatusAsync(
            roomPlayer.RoomId,
            roomPlayer.UserId,
            Status.Inactive
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Status.Inactive, result.Status);
    }

    [Fact]
    public async Task UpdatePlayerStatusAsync_ThrowsNotFoundException_WhenRoomPlayerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _rut.UpdatePlayerStatusAsync(Guid.NewGuid(), Guid.NewGuid(), Status.Active)
        );
    }
}
