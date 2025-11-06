using Microsoft.EntityFrameworkCore;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Test.Helpers;
using Project.Test.Helpers.Builders;

namespace Project.Test.Repositories;

public class RoomRepositoryTests : RepositoryTestBase<RoomRepository, Room>
{
    [Fact]
    public async Task GetByIdAsync_ReturnsRoom_WhenRoomExists()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        var room = await SeedData(
            new RoomBuilder()
                .WithDescription("Test Description")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(true)
                .WithHostId(user.Id)
                .Build()
        );

        // Act
        var result = await _rut.GetByIdAsync(room.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.Id);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(user.Id, result.HostId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenRoomDoesNotExist()
    {
        // Act
        var result = await _rut.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRooms()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        await SeedData<Room>(
            new RoomBuilder()
                .WithDescription("Desc 1")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(true)
                .WithHostId(user.Id),
            new RoomBuilder()
                .WithDescription("Desc 2")
                .WithMaxPlayers(6)
                .WithMinPlayers(2)
                .IsPublic(false)
                .IsActive(true)
                .WithHostId(user.Id),
            new RoomBuilder()
                .WithDescription("Desc 3")
                .WithMaxPlayers(8)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(false)
                .WithHostId(user.Id)
        );

        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoRoomsExist()
    {
        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveRooms()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        await SeedData<Room>(
            new RoomBuilder()
                .WithDescription("Active 1")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(true)
                .WithHostId(user.Id),
            new RoomBuilder()
                .WithDescription("Active 2")
                .WithMaxPlayers(6)
                .WithMinPlayers(2)
                .IsPublic(false)
                .IsActive(true)
                .WithHostId(user.Id),
            new RoomBuilder()
                .WithDescription("Inactive")
                .WithMaxPlayers(8)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(false)
                .WithHostId(user.Id)
        );

        // Act
        var result = await _rut.GetAllActiveAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.True(r.IsActive));
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsEmptyList_WhenNoActiveRooms()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        await SeedData(
            new RoomBuilder()
                .WithDescription("Inactive")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(false)
                .WithHostId(user.Id)
                .Build()
        );

        // Act
        var result = await _rut.GetAllActiveAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllPublicAsync_ReturnsOnlyPublicAndActiveRooms()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        await SeedData<Room>(
            new RoomBuilder()
                .WithDescription("Public Active")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(true)
                .WithHostId(user.Id),
            new RoomBuilder()
                .WithDescription("Public Inactive")
                .WithMaxPlayers(6)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(false)
                .WithHostId(user.Id),
            new RoomBuilder()
                .WithDescription("Private Active")
                .WithMaxPlayers(8)
                .WithMinPlayers(2)
                .IsPublic(false)
                .IsActive(true)
                .WithHostId(user.Id),
            new RoomBuilder()
                .WithDescription("Private Inactive")
                .WithMaxPlayers(10)
                .WithMinPlayers(2)
                .IsPublic(false)
                .IsActive(false)
                .WithHostId(user.Id)
        );

        // Act
        var result = await _rut.GetAllPublicAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Public Active", result[0].Description);
        Assert.True(result[0].IsPublic);
        Assert.True(result[0].IsActive);
    }

    [Fact]
    public async Task GetAllPublicAsync_ReturnsEmptyList_WhenNoPublicActiveRooms()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        await SeedData(
            new RoomBuilder()
                .WithDescription("Private Active")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(false)
                .IsActive(true)
                .WithHostId(user.Id)
                .Build()
        );

        // Act
        var result = await _rut.GetAllPublicAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByHostIdAsync_ReturnsRoom_WhenRoomExistsForHost()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        await SeedData(
            new RoomBuilder()
                .WithDescription("Host Room")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(true)
                .WithHostId(user.Id)
                .Build()
        );

        // Act
        var result = await _rut.GetByHostIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.HostId);
        Assert.Equal("Host Room", result.Description);
    }

    [Fact]
    public async Task GetByHostIdAsync_ReturnsNull_WhenNoRoomExistsForHost()
    {
        // Act
        var result = await _rut.GetByHostIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesRoomSuccessfully()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        var room = new RoomBuilder()
            .WithDescription("New Description")
            .WithMaxPlayers(4)
            .WithMinPlayers(2)
            .IsPublic(true)
            .IsActive(true)
            .WithHostId(user.Id)
            .Build();

        // Act
        var result = await _rut.CreateAsync(room);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Description", result.Description);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(await _context.Rooms.AnyAsync(r => r.Id == result.Id));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRoomSuccessfully()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        var room = await SeedData(
            new RoomBuilder()
                .WithDescription("Original")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(true)
                .WithHostId(user.Id)
                .Build()
        );

        var updatedRoom = new RoomBuilder()
            .WithDescription("Updated")
            .WithMaxPlayers(6)
            .WithMinPlayers(3)
            .IsPublic(false)
            .IsActive(false)
            .WithHostId(user.Id)
            .Build();
        updatedRoom.Id = room.Id;

        // Act
        var result = await _rut.UpdateAsync(updatedRoom);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Description);
        Assert.Equal(6, result.MaxPlayers);
        Assert.Equal(3, result.MinPlayers);
        Assert.False(result.IsPublic);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        var room = new RoomBuilder()
            .WithDescription("Non-existent")
            .WithMaxPlayers(4)
            .WithMinPlayers(2)
            .IsPublic(true)
            .IsActive(true)
            .WithHostId(Guid.NewGuid())
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _rut.UpdateAsync(room));
    }

    [Fact]
    public async Task DeleteAsync_DeletesRoomSuccessfully()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        var room = await SeedData(
            new RoomBuilder()
                .WithDescription("To Delete")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(true)
                .WithHostId(user.Id)
                .Build()
        );

        // Act
        var result = await _rut.DeleteAsync(room.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.Id);
        Assert.False(await _context.Rooms.AnyAsync(r => r.Id == room.Id));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenRoomDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _rut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenRoomExists()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithEmail("host@test.com").WithName("Host User").Build()
        );
        var room = await SeedData(
            new RoomBuilder()
                .WithDescription("Exists")
                .WithMaxPlayers(4)
                .WithMinPlayers(2)
                .IsPublic(true)
                .IsActive(true)
                .WithHostId(user.Id)
                .Build()
        );

        // Act
        var result = await _rut.ExistsAsync(room.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenRoomDoesNotExist()
    {
        // Act
        var result = await _rut.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
