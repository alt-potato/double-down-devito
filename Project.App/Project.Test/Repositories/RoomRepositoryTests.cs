using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;

namespace Project.Test.Repositories;

public class RoomRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly RoomRepository _repository;

    public RoomRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new RoomRepository(_context);
    }

    [Fact]
    public async Task GetRoomByIdAsync_ReturnsRoom_WhenRoomExists()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var room = new Room
        {
            Description = "Test Description",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = true,
            HostId = user.Id,
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoomByIdAsync(room.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.Id);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(user.Id, result.HostId);
    }

    [Fact]
    public async Task GetRoomByIdAsync_ReturnsNull_WhenRoomDoesNotExist()
    {
        // Act
        var result = await _repository.GetRoomByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllRoomsAsync_ReturnsAllRooms()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var rooms = new[]
        {
            new Room
            {
                Description = "Desc 1",
                MaxPlayers = 4,
                MinPlayers = 2,
                IsPublic = true,
                IsActive = true,
                HostId = user.Id,
            },
            new Room
            {
                Description = "Desc 2",
                MaxPlayers = 6,
                MinPlayers = 2,
                IsPublic = false,
                IsActive = true,
                HostId = user.Id,
            },
            new Room
            {
                Description = "Desc 3",
                MaxPlayers = 8,
                MinPlayers = 2,
                IsPublic = true,
                IsActive = false,
                HostId = user.Id,
            },
        };
        await _context.Rooms.AddRangeAsync(rooms);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllRoomsAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllRoomsAsync_ReturnsEmptyList_WhenNoRoomsExist()
    {
        // Act
        var result = await _repository.GetAllRoomsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveRoomsAsync_ReturnsOnlyActiveRooms()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var rooms = new[]
        {
            new Room
            {
                Description = "Active 1",
                MaxPlayers = 4,
                MinPlayers = 2,
                IsPublic = true,
                IsActive = true,
                HostId = user.Id,
            },
            new Room
            {
                Description = "Active 2",
                MaxPlayers = 6,
                MinPlayers = 2,
                IsPublic = false,
                IsActive = true,
                HostId = user.Id,
            },
            new Room
            {
                Description = "Inactive",
                MaxPlayers = 8,
                MinPlayers = 2,
                IsPublic = true,
                IsActive = false,
                HostId = user.Id,
            },
        };
        await _context.Rooms.AddRangeAsync(rooms);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveRoomsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.True(r.IsActive));
    }

    [Fact]
    public async Task GetActiveRoomsAsync_ReturnsEmptyList_WhenNoActiveRooms()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var room = new Room
        {
            Description = "Inactive",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = false,
            HostId = user.Id,
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveRoomsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPublicRoomsAsync_ReturnsOnlyPublicAndActiveRooms()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var rooms = new[]
        {
            new Room
            {
                Description = "Public Active",
                MaxPlayers = 4,
                MinPlayers = 2,
                IsPublic = true,
                IsActive = true,
                HostId = user.Id,
            },
            new Room
            {
                Description = "Public Inactive",
                MaxPlayers = 6,
                MinPlayers = 2,
                IsPublic = true,
                IsActive = false,
                HostId = user.Id,
            },
            new Room
            {
                Description = "Private Active",
                MaxPlayers = 8,
                MinPlayers = 2,
                IsPublic = false,
                IsActive = true,
                HostId = user.Id,
            },
            new Room
            {
                Description = "Private Inactive",
                MaxPlayers = 10,
                MinPlayers = 2,
                IsPublic = false,
                IsActive = false,
                HostId = user.Id,
            },
        };
        await _context.Rooms.AddRangeAsync(rooms);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPublicRoomsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Public Active", result[0].Description);
        Assert.True(result[0].IsPublic);
        Assert.True(result[0].IsActive);
    }

    [Fact]
    public async Task GetPublicRoomsAsync_ReturnsEmptyList_WhenNoPublicActiveRooms()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var room = new Room
        {
            Description = "Private Active",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = false,
            IsActive = true,
            HostId = user.Id,
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPublicRoomsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRoomByHostIdAsync_ReturnsRoom_WhenRoomExistsForHost()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var room = new Room
        {
            Description = "Host Room",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = true,
            HostId = user.Id,
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoomByHostIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.HostId);
        Assert.Equal("Host Room", result.Description);
    }

    [Fact]
    public async Task GetRoomByHostIdAsync_ReturnsNull_WhenNoRoomExistsForHost()
    {
        // Act
        var result = await _repository.GetRoomByHostIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateRoomAsync_CreatesRoomSuccessfully()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var room = new Room
        {
            Description = "New Description",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = true,
            HostId = user.Id,
        };

        // Act
        var result = await _repository.CreateRoomAsync(room);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Description", result.Description);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(await _context.Rooms.AnyAsync(r => r.Id == result.Id));
    }

    [Fact]
    public async Task UpdateRoomAsync_UpdatesRoomSuccessfully()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var room = new Room
        {
            Description = "Original",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = true,
            HostId = user.Id,
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        var updatedRoom = new Room
        {
            Id = room.Id,
            Description = "Updated",
            MaxPlayers = 6,
            MinPlayers = 3,
            IsPublic = false,
            IsActive = false,
            HostId = user.Id,
        };

        // Act
        var result = await _repository.UpdateRoomAsync(updatedRoom);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Description);
        Assert.Equal(6, result.MaxPlayers);
        Assert.Equal(3, result.MinPlayers);
        Assert.False(result.IsPublic);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateRoomAsync_ThrowsNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Description = "Non-existent",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = true,
            HostId = Guid.NewGuid(),
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _repository.UpdateRoomAsync(room));
    }

    [Fact]
    public async Task DeleteRoomAsync_DeletesRoomSuccessfully()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var room = new Room
        {
            Description = "To Delete",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = true,
            HostId = user.Id,
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteRoomAsync(room.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.Id);
        Assert.False(await _context.Rooms.AnyAsync(r => r.Id == room.Id));
    }

    [Fact]
    public async Task DeleteRoomAsync_ThrowsNotFoundException_WhenRoomDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _repository.DeleteRoomAsync(Guid.NewGuid())
        );
    }

    [Fact]
    public async Task RoomExistsAsync_ReturnsTrue_WhenRoomExists()
    {
        // Arrange
        var user = new User { Email = "host@test.com", Name = "Host User" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var room = new Room
        {
            Description = "Exists",
            MaxPlayers = 4,
            MinPlayers = 2,
            IsPublic = true,
            IsActive = true,
            HostId = user.Id,
        };
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.RoomExistsAsync(room.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RoomExistsAsync_ReturnsFalse_WhenRoomDoesNotExist()
    {
        // Act
        var result = await _repository.RoomExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
