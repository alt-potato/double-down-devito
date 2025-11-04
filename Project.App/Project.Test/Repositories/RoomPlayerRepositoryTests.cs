using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Api.Utilities.Enums;

namespace Project.Test.Repositories;

public class RoomPlayerRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly RoomPlayerRepository _repository;

    public RoomPlayerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new RoomPlayerRepository(_context);
    }

    private async Task<User> CreateTestUser(
        string email = "test@example.com",
        string name = "Test User"
    )
    {
        var user = new User { Email = email, Name = name };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Room> CreateTestRoom(Guid hostId, string description = "Test Room")
    {
        var room = new Room
        {
            HostId = hostId,
            Description = description,
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = true,
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
        return room;
    }

    private async Task<RoomPlayer> CreateTestRoomPlayer(
        Guid roomId,
        Guid userId,
        Status status = Status.Active
    )
    {
        var roomPlayer = new RoomPlayer
        {
            RoomId = roomId,
            UserId = userId,
            Status = status,
        };
        await _context.RoomPlayers.AddAsync(roomPlayer);
        await _context.SaveChangesAsync();
        return roomPlayer;
    }

    [Fact]
    public async Task GetRoomPlayerByIdAsync_ReturnsRoomPlayer_WhenRoomPlayerExists()
    {
        // Arrange
        var user = await CreateTestUser();
        var room = await CreateTestRoom(user.Id);
        var roomPlayer = await CreateTestRoomPlayer(room.Id, user.Id);

        // Act
        var result = await _repository.GetRoomPlayerByIdAsync(room.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.RoomId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(Status.Active, result.Status);
    }

    [Fact]
    public async Task GetRoomPlayerByIdAsync_ReturnsNull_WhenRoomPlayerDoesNotExist()
    {
        // Act
        var result = await _repository.GetRoomPlayerByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllRoomPlayersAsync_ReturnsAllRoomPlayers()
    {
        // Arrange
        var user1 = await CreateTestUser("user1@test.com", "User 1");
        var user2 = await CreateTestUser("user2@test.com", "User 2");
        var room1 = await CreateTestRoom(user1.Id, "Room 1");
        var room2 = await CreateTestRoom(user2.Id, "Room 2");

        await CreateTestRoomPlayer(room1.Id, user1.Id);
        await CreateTestRoomPlayer(room2.Id, user2.Id);

        // Act
        var result = await _repository.GetAllRoomPlayersAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllRoomPlayersAsync_ReturnsEmptyList_WhenNoRoomPlayersExist()
    {
        // Act
        var result = await _repository.GetAllRoomPlayersAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRoomPlayersByRoomIdAsync_ReturnsRoomPlayers_ForSpecificRoom()
    {
        // Arrange
        var user1 = await CreateTestUser("user1@test.com", "User 1");
        var user2 = await CreateTestUser("user2@test.com", "User 2");
        var room1 = await CreateTestRoom(user1.Id, "Room 1");
        var room2 = await CreateTestRoom(user2.Id, "Room 2");

        await CreateTestRoomPlayer(room1.Id, user1.Id);
        await CreateTestRoomPlayer(room1.Id, user2.Id);
        await CreateTestRoomPlayer(room2.Id, user1.Id);

        // Act
        var result = await _repository.GetRoomPlayersByRoomIdAsync(room1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, rp => Assert.Equal(room1.Id, rp.RoomId));
    }

    [Fact]
    public async Task GetRoomPlayersByRoomIdAsync_ReturnsEmptyList_WhenNoPlayersInRoom()
    {
        // Arrange
        var user = await CreateTestUser();
        var room = await CreateTestRoom(user.Id);

        // Act
        var result = await _repository.GetRoomPlayersByRoomIdAsync(room.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRoomPlayersByUserIdAsync_ReturnsRoomPlayers_ForSpecificUser()
    {
        // Arrange
        var user1 = await CreateTestUser("user1@test.com", "User 1");
        var user2 = await CreateTestUser("user2@test.com", "User 2");
        var room1 = await CreateTestRoom(user1.Id, "Room 1");
        var room2 = await CreateTestRoom(user2.Id, "Room 2");

        await CreateTestRoomPlayer(room1.Id, user1.Id);
        await CreateTestRoomPlayer(room2.Id, user1.Id);
        await CreateTestRoomPlayer(room1.Id, user2.Id);

        // Act
        var result = await _repository.GetRoomPlayersByUserIdAsync(user1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, rp => Assert.Equal(user1.Id, rp.UserId));
    }

    [Fact]
    public async Task GetPlayersInRoomByStatusAsync_ReturnsPlayersByStatus()
    {
        // Arrange
        var user1 = await CreateTestUser("user1@test.com", "User 1");
        var user2 = await CreateTestUser("user2@test.com", "User 2");
        var user3 = await CreateTestUser("user3@test.com", "User 3");
        var room = await CreateTestRoom(user1.Id);

        await CreateTestRoomPlayer(room.Id, user1.Id, Status.Active);
        await CreateTestRoomPlayer(room.Id, user2.Id, Status.Inactive);
        await CreateTestRoomPlayer(room.Id, user3.Id, Status.Away);

        // Act
        var result = await _repository.GetPlayersInRoomByStatusAsync(
            room.Id,
            Status.Active,
            Status.Away
        );

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, rp => rp.UserId == user1.Id && rp.Status == Status.Active);
        Assert.Contains(result, rp => rp.UserId == user3.Id && rp.Status == Status.Away);
        Assert.DoesNotContain(result, rp => rp.UserId == user2.Id);
    }

    [Fact]
    public async Task CreateRoomPlayerAsync_CreatesRoomPlayerSuccessfully()
    {
        // Arrange
        var user = await CreateTestUser();
        var room = await CreateTestRoom(user.Id);
        var roomPlayer = new RoomPlayer
        {
            RoomId = room.Id,
            UserId = user.Id,
            Status = Status.Active,
        };

        // Act
        var result = await _repository.CreateRoomPlayerAsync(roomPlayer);

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
    public async Task UpdateRoomPlayerAsync_UpdatesRoomPlayerSuccessfully()
    {
        // Arrange
        var user = await CreateTestUser();
        var room = await CreateTestRoom(user.Id);
        var roomPlayer = await CreateTestRoomPlayer(room.Id, user.Id);

        var updatedRoomPlayer = new RoomPlayer
        {
            RoomId = room.Id,
            UserId = user.Id,
            Status = Status.Inactive,
        };

        // Act
        var result = await _repository.UpdateRoomPlayerAsync(updatedRoomPlayer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Status.Inactive, result.Status);
    }

    [Fact]
    public async Task DeleteRoomPlayerAsync_DeletesRoomPlayerSuccessfully()
    {
        // Arrange
        var user = await CreateTestUser();
        var room = await CreateTestRoom(user.Id);
        var roomPlayer = await CreateTestRoomPlayer(room.Id, user.Id);

        // Act
        var result = await _repository.DeleteRoomPlayerAsync(roomPlayer.RoomId, roomPlayer.UserId);

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
    public async Task DeleteRoomPlayerAsync_ThrowsNotFoundException_WhenRoomPlayerDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.DeleteRoomPlayerAsync(Guid.NewGuid(), Guid.NewGuid())
        );
    }

    [Fact]
    public async Task IsPlayerInRoomAsync_ReturnsFalse_WhenPlayerIsNotInRoom()
    {
        // Act
        var result = await _repository.IsPlayerInRoomAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPlayerCountInRoomAsync_ReturnsCorrectCount()
    {
        // Arrange
        var user1 = await CreateTestUser("user1@test.com", "User 1");
        var user2 = await CreateTestUser("user2@test.com", "User 2");
        var room = await CreateTestRoom(user1.Id);

        await CreateTestRoomPlayer(room.Id, user1.Id);
        await CreateTestRoomPlayer(room.Id, user2.Id);

        // Act
        var result = await _repository.GetPlayerCountInRoomAsync(room.Id);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetPlayerCountInRoomAsync_ReturnsZero_WhenNoPlayersInRoom()
    {
        // Arrange
        var user = await CreateTestUser();
        var room = await CreateTestRoom(user.Id);

        // Act
        var result = await _repository.GetPlayerCountInRoomAsync(room.Id);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task UpdatePlayerStatusAsync_UpdatesStatusSuccessfully()
    {
        // Arrange
        var user = await CreateTestUser();
        var room = await CreateTestRoom(user.Id);
        var roomPlayer = await CreateTestRoomPlayer(room.Id, user.Id, Status.Active);

        // Act
        var result = await _repository.UpdatePlayerStatusAsync(
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
            _repository.UpdatePlayerStatusAsync(Guid.NewGuid(), Guid.NewGuid(), Status.Active)
        );
    }
}
