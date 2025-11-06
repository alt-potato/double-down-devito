using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Project.Api.Data;
using Project.Api.DTOs;
using Project.Api.Models;
using Project.Api.Models.Games;
using Project.Api.Services;
using Project.Api.Services.Interface;
using Project.Api.Utilities;
using Project.Api.Utilities.Constants;
using Project.Test.Helpers;
using Project.Test.Helpers.Builders;

namespace Project.Test.Services;

public class RoomServiceTests : ServiceTestBase<RoomService>
{
    private readonly Mock<IGameService<IGameState, GameConfig>> _mockBlackjackService;
    private readonly List<IGameService<IGameState, GameConfig>> _mockGameServices;
    private readonly RoomService _sut; // service under test

    public RoomServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"testdb_{Guid.CreateVersion7()}")
            .ConfigureWarnings(w =>
                w.Ignore(
                    Microsoft
                        .EntityFrameworkCore
                        .Diagnostics
                        .InMemoryEventId
                        .TransactionIgnoredWarning
                )
            )
            .Options;

        _mockBlackjackService = new Mock<IGameService<IGameState, GameConfig>>();
        _mockBlackjackService.Setup(s => s.GameMode).Returns(GameModes.Blackjack);
        _mockBlackjackService
            .Setup(s => s.PlayerJoinAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _mockBlackjackService
            .Setup(s => s.PlayerLeaveAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _mockBlackjackService
            .Setup(s =>
                s.PerformActionAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<JsonElement>()
                )
            )
            .Returns(Task.CompletedTask);
        _mockBlackjackService
            .Setup(s => s.StartGameAsync(It.IsAny<Guid>(), It.IsAny<GameConfig>()))
            .Returns(Task.CompletedTask);
        _mockBlackjackService
            .Setup(s => s.GetConfigAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new BlackjackConfig());
        _mockBlackjackService
            .Setup(s => s.GetGameStateAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new BlackjackState { CurrentStage = new BlackjackInitStage() });

        _mockGameServices = [_mockBlackjackService.Object];

        _sut = new RoomService(
            _mockUoW.Object,
            _mockGameServices,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region GetRoomByIdAsync Tests

    [Fact]
    public async Task GetRoomByIdAsync_ReturnsRoomDTO_WhenRoomExists()
    {
        var roomId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).Build();
        var resultDto = new RoomDTO { Id = roomId };

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockMapper.Setup(m => m.Map<RoomDTO>(room)).Returns(resultDto);

        var result = await _sut.GetRoomByIdAsync(roomId);

        result.Should().NotBeNull();
        result!.Should().Be(resultDto);
        result!.Id.Should().Be(roomId);
        _mockRoomRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
        _mockMapper.Verify(m => m.Map<RoomDTO>(room), Times.Once);
    }

    [Fact]
    public async Task GetRoomByIdAsync_ReturnsNull_WhenRoomDoesNotExist()
    {
        var roomId = Guid.NewGuid();
        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync((Room?)null);

        var result = await _sut.GetRoomByIdAsync(roomId);

        result.Should().BeNull();
        _mockRoomRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
    }

    #endregion

    #region GetAllRoomsAsync Tests

    [Fact]
    public async Task GetAllRoomsAsync_ReturnsAllRooms_WhenSuccessful()
    {
        var rooms = new List<Room> { new RoomBuilder(), new RoomBuilder(), new RoomBuilder() };
        var roomDtos = rooms.Select(r => new RoomDTO { Id = r.Id }).ToList();

        _mockRoomRepository.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(rooms);
        _mockMapper.Setup(m => m.Map<IReadOnlyList<RoomDTO>>(rooms)).Returns(roomDtos);

        var result = await _sut.GetAllRoomsAsync();

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(roomDtos);
        _mockRoomRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockMapper.Verify(m => m.Map<IReadOnlyList<RoomDTO>>(rooms), Times.Once);
    }

    [Fact]
    public async Task GetAllRoomsAsync_ReturnsEmptyList_WhenNoRoomsExist()
    {
        var emptyRooms = new List<Room>();
        _mockRoomRepository.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(emptyRooms);
        _mockMapper.Setup(m => m.Map<IReadOnlyList<RoomDTO>>(emptyRooms)).Returns([]);

        var result = await _sut.GetAllRoomsAsync();

        result.Should().BeEmpty();
        _mockRoomRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockMapper.Verify(m => m.Map<IReadOnlyList<RoomDTO>>(emptyRooms), Times.Once);
    }

    #endregion

    #region GetActiveRoomsAsync Tests

    [Fact]
    public async Task GetActiveRoomsAsync_ReturnsActiveRooms_WhenSuccessful()
    {
        var activeRooms = new List<Room>
        {
            new RoomBuilder().IsActive(true),
            new RoomBuilder().IsActive(true),
        };
        var roomDtos = activeRooms.Select(r => new RoomDTO { Id = r.Id, IsActive = true }).ToList();

        _mockRoomRepository.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(activeRooms);
        _mockMapper.Setup(m => m.Map<IReadOnlyList<RoomDTO>>(activeRooms)).Returns(roomDtos);

        var result = await _sut.GetActiveRoomsAsync();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.IsActive.Should().BeTrue());
        _mockRoomRepository.Verify(r => r.GetAllActiveAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<IReadOnlyList<RoomDTO>>(activeRooms), Times.Once);
    }

    [Fact]
    public async Task GetActiveRoomsAsync_ReturnsEmptyList_WhenNoActiveRoomsExist()
    {
        var emptyRooms = new List<Room>();
        _mockRoomRepository.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(emptyRooms);
        _mockMapper.Setup(m => m.Map<IReadOnlyList<RoomDTO>>(emptyRooms)).Returns([]);

        var result = await _sut.GetActiveRoomsAsync();

        result.Should().BeEmpty();
        _mockRoomRepository.Verify(r => r.GetAllActiveAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<IReadOnlyList<RoomDTO>>(emptyRooms), Times.Once);
    }

    #endregion

    #region GetPublicRoomsAsync Tests

    [Fact]
    public async Task GetPublicRoomsAsync_ReturnsPublicRooms_WhenSuccessful()
    {
        var publicRooms = new List<Room>
        {
            new RoomBuilder().IsActive(true).IsPublic(true),
            new RoomBuilder().IsActive(true).IsPublic(true),
        };
        var roomDtos = publicRooms.Select(r => new RoomDTO { Id = r.Id, IsPublic = true }).ToList();

        _mockRoomRepository.Setup(r => r.GetAllPublicAsync()).ReturnsAsync(publicRooms);
        _mockMapper.Setup(m => m.Map<IReadOnlyList<RoomDTO>>(publicRooms)).Returns(roomDtos);

        var result = await _sut.GetPublicRoomsAsync();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.IsPublic.Should().BeTrue());
        _mockRoomRepository.Verify(r => r.GetAllPublicAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<IReadOnlyList<RoomDTO>>(publicRooms), Times.Once);
    }

    [Fact]
    public async Task GetPublicRoomsAsync_ReturnsEmptyList_WhenNoPublicRoomsExist()
    {
        var emptyRooms = new List<Room>();
        _mockRoomRepository.Setup(r => r.GetAllPublicAsync()).ReturnsAsync(emptyRooms);
        _mockMapper.Setup(m => m.Map<IReadOnlyList<RoomDTO>>(emptyRooms)).Returns([]);

        var result = await _sut.GetPublicRoomsAsync();

        result.Should().BeEmpty();
        _mockRoomRepository.Verify(r => r.GetAllPublicAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<IReadOnlyList<RoomDTO>>(emptyRooms), Times.Once);
    }

    #endregion

    #region GetRoomByHostIdAsync Tests

    [Fact]
    public async Task GetRoomByHostIdAsync_ReturnsRoomDTO_WhenRoomExists()
    {
        var hostId = Guid.NewGuid();
        var room = new RoomBuilder().WithHostId(hostId).Build();
        var roomDto = new RoomDTO { Id = room.Id, HostId = hostId };

        _mockRoomRepository.Setup(r => r.GetByHostIdAsync(hostId)).ReturnsAsync(room);
        _mockMapper.Setup(m => m.Map<RoomDTO>(room)).Returns(roomDto);

        var result = await _sut.GetRoomByHostIdAsync(hostId);

        result.Should().NotBeNull();
        result!.Should().Be(roomDto);
        result!.HostId.Should().Be(hostId);
        _mockRoomRepository.Verify(r => r.GetByHostIdAsync(hostId), Times.Once);
        _mockMapper.Verify(m => m.Map<RoomDTO>(room), Times.Once);
    }

    [Fact]
    public async Task GetRoomByHostIdAsync_ReturnsNull_WhenRoomDoesNotExist()
    {
        var hostId = Guid.NewGuid();
        _mockRoomRepository.Setup(r => r.GetByHostIdAsync(hostId)).ReturnsAsync((Room?)null);

        var result = await _sut.GetRoomByHostIdAsync(hostId);

        result.Should().BeNull();
        _mockRoomRepository.Verify(r => r.GetByHostIdAsync(hostId), Times.Once);
    }

    #endregion

    #region CreateRoomAsync Tests

    [Fact]
    public async Task CreateRoomAsync_CreatesRoomAndHostPlayer_WhenDtoIsValid()
    {
        var createDto = new CreateRoomDTO
        {
            HostId = Guid.NewGuid(),
            IsPublic = true,
            GameMode = GameModes.Blackjack,
            GameConfig = "{}",
            Description = "A valid test room",
            MaxPlayers = 4,
            MinPlayers = 2,
        };
        var roomFromMapper = new Room { HostId = createDto.HostId };
        var resultDto = new RoomDTO
        {
            HostId = createDto.HostId,
            Description = createDto.Description,
        };

        _mockMapper.Setup(m => m.Map<Room>(createDto)).Returns(roomFromMapper);
        _mockMapper
            .Setup(m => m.Map<RoomDTO>(It.IsAny<Room>()))
            .Returns(
                (Room r) =>
                    new RoomDTO
                    {
                        Id = r.Id,
                        HostId = r.HostId,
                        Description = r.Description,
                    }
            );

        Room? capturedRoom = null;
        _mockRoomRepository
            .Setup(r => r.CreateAsync(It.IsAny<Room>()))
            .Callback<Room>(room => capturedRoom = room)
            .ReturnsAsync((Room room) => room);

        var result = await _sut.CreateRoomAsync(createDto);

        result.Should().NotBeNull();
        capturedRoom.Should().NotBeNull();
        result.Id.Should().Be(capturedRoom!.Id);
        result.HostId.Should().Be(createDto.HostId);

        _mockRoomRepository.Verify(r => r.CreateAsync(It.IsAny<Room>()), Times.Once);
        _mockMapper.Verify(m => m.Map<Room>(createDto), Times.Once);
        _mockMapper.Verify(m => m.Map<RoomDTO>(It.IsAny<Room>()), Times.Once);

        capturedRoom!.HostId.Should().Be(createDto.HostId);
        capturedRoom.IsActive.Should().BeTrue();
        capturedRoom.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateRoomAsync_RollsBackTransaction_OnFailure()
    {
        var createDto = new CreateRoomDTO
        {
            HostId = Guid.NewGuid(),
            IsPublic = true,
            GameMode = GameModes.Blackjack,
            GameConfig = "{}",
            Description = "A valid test room",
            MaxPlayers = 4,
            MinPlayers = 2,
        };
        var roomFromMapper = new Room { HostId = createDto.HostId };

        _mockMapper.Setup(m => m.Map<Room>(createDto)).Returns(roomFromMapper);
        _mockRoomRepository
            .Setup(r => r.CreateAsync(It.IsAny<Room>()))
            .ThrowsAsync(new Exception("Database error"));

        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateRoomAsync(createDto));

        _mockRoomRepository.Verify(r => r.CreateAsync(It.IsAny<Room>()), Times.Once);
    }

    #endregion

    #region UpdateRoomAsync Tests

    [Fact]
    public async Task UpdateRoomAsync_UpdatesRoom_WhenRoomExists()
    {
        var roomId = Guid.NewGuid();
        var existingRoom = new RoomBuilder().WithId(roomId).Build();
        var updateDto = new UpdateRoomDTO
        {
            Id = roomId,
            HostId = existingRoom.HostId,
            IsPublic = false,
            GameMode = GameModes.Blackjack,
            GameConfig = "{}",
            Description = "Updated Description",
            MaxPlayers = 8,
            MinPlayers = 3,
        };
        var resultDto = new RoomDTO { Id = roomId, Description = "Updated Description" };

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(existingRoom);
        _mockRoomRepository.Setup(r => r.UpdateAsync(It.IsAny<Room>())).ReturnsAsync(existingRoom);
        _mockMapper.Setup(m => m.Map<RoomDTO>(It.IsAny<Room>())).Returns(resultDto);

        var result = await _sut.UpdateRoomAsync(updateDto);

        result.Should().NotBeNull();
        result!.Id.Should().Be(roomId);
        _mockMapper.Verify(m => m.Map(updateDto, existingRoom), Times.Once);
        _mockMapper.Verify(m => m.Map<RoomDTO>(It.IsAny<Room>()), Times.Once);
        _mockRoomRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
        _mockRoomRepository.Verify(r => r.UpdateAsync(existingRoom), Times.Once);
    }

    [Fact]
    public async Task UpdateRoomAsync_ReturnsNull_WhenRoomDoesNotExist()
    {
        var roomId = Guid.NewGuid();
        var updateDto = new UpdateRoomDTO
        {
            Id = roomId,
            HostId = Guid.NewGuid(),
            IsPublic = true,
            GameMode = GameModes.Blackjack,
            GameConfig = "{}",
            MaxPlayers = 6,
            MinPlayers = 2,
        };

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync((Room?)null);

        var result = await _sut.UpdateRoomAsync(updateDto);

        result.Should().BeNull();
        _mockRoomRepository.Verify(r => r.UpdateAsync(It.IsAny<Room>()), Times.Never);
    }

    #endregion

    #region DeleteRoomAsync Tests

    [Fact]
    public async Task DeleteRoomAsync_ReturnsTrue_WhenRoomIsDeleted()
    {
        var roomId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).Build();
        _mockRoomRepository.Setup(r => r.DeleteAsync(roomId)).ReturnsAsync(room);

        var result = await _sut.DeleteRoomAsync(roomId);

        result.Should().BeTrue();
        _mockRoomRepository.Verify(r => r.DeleteAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task DeleteRoomAsync_ThrowsNotFoundException_WhenRoomDoesNotExist()
    {
        var roomId = Guid.NewGuid();
        _mockRoomRepository
            .Setup(r => r.DeleteAsync(roomId))
            .ThrowsAsync(new NotFoundException("Room not found."));

        var result = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.DeleteRoomAsync(roomId)
        );

        _mockRoomRepository.Verify(r => r.DeleteAsync(roomId), Times.Once);
    }

    #endregion

    #region RoomExistsAsync Tests

    [Fact]
    public async Task RoomExistsAsync_ReturnsTrue_WhenRoomExists()
    {
        var roomId = Guid.NewGuid();
        _mockRoomRepository.Setup(r => r.ExistsAsync(roomId)).ReturnsAsync(true);

        var result = await _sut.RoomExistsAsync(roomId);

        result.Should().BeTrue();
        _mockRoomRepository.Verify(r => r.ExistsAsync(roomId), Times.Once);
    }

    [Fact]
    public async Task RoomExistsAsync_ReturnsFalse_WhenRoomDoesNotExist()
    {
        var roomId = Guid.NewGuid();
        _mockRoomRepository.Setup(r => r.ExistsAsync(roomId)).ReturnsAsync(false);

        var result = await _sut.RoomExistsAsync(roomId);

        result.Should().BeFalse();
        _mockRoomRepository.Verify(r => r.ExistsAsync(roomId), Times.Once);
    }

    #endregion

    #region Game Association Tests

    [Fact]
    public async Task SetRoomGameAsync_ShouldCreateGame_WhenRoomHasNoGame()
    {
        var roomId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).WithGameId(null).Build();
        var gameMode = GameModes.Blackjack;
        var gameConfig = "{}";

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRoomRepository.Setup(r => r.UpdateAsync(It.IsAny<Room>())).ReturnsAsync(room);
        _mockGameRepository
            .Setup(r => r.CreateAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game game) => game);
        _mockMapper.Setup(m => m.Map<RoomDTO>(room)).Returns(new RoomDTO { Id = roomId });

        var result = await _sut.SetRoomGameAsync(roomId, gameMode, gameConfig);

        result.Should().NotBeNull();
        result.Id.Should().Be(roomId);
        _mockGameRepository.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Once);
        _mockRoomRepository.Verify(r => r.UpdateAsync(It.IsAny<Room>()), Times.Once);
    }

    [Fact]
    public async Task SetRoomGameAsync_ShouldReplaceGame_WhenRoomHasExistingGame()
    {
        var roomId = Guid.NewGuid();
        var existingGameId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).WithGameId(existingGameId).Build();
        var gameMode = GameModes.Blackjack;

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRoomRepository.Setup(r => r.UpdateAsync(It.IsAny<Room>())).ReturnsAsync(room);
        _mockGameRepository
            .Setup(r => r.DeleteAsync(existingGameId))
            .ReturnsAsync(new GameBuilder().Build());
        _mockGameRepository
            .Setup(r => r.CreateAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game game) => game);
        _mockMapper.Setup(m => m.Map<RoomDTO>(room)).Returns(new RoomDTO { Id = roomId });

        var result = await _sut.SetRoomGameAsync(roomId, gameMode);

        result.Should().NotBeNull();
        _mockGameRepository.Verify(r => r.DeleteAsync(existingGameId), Times.Once);
        _mockGameRepository.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Once);
    }

    [Fact]
    public async Task RemoveGameFromRoomAsync_ShouldRemoveGame_WhenRoomHasGame()
    {
        var roomId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).WithGameId(gameId).Build();

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRoomRepository.Setup(r => r.UpdateAsync(It.IsAny<Room>())).ReturnsAsync(room);
        _mockGameRepository
            .Setup(r => r.DeleteAsync(gameId))
            .ReturnsAsync(new GameBuilder().Build());
        _mockMapper.Setup(m => m.Map<RoomDTO>(room)).Returns(new RoomDTO { Id = roomId });

        var result = await _sut.RemoveGameFromRoomAsync(roomId);

        result.Should().NotBeNull();
        _mockGameRepository.Verify(r => r.DeleteAsync(gameId), Times.Once);
        _mockRoomRepository.Verify(r => r.UpdateAsync(It.IsAny<Room>()), Times.Once);
    }

    [Fact]
    public async Task RemoveGameFromRoomAsync_ShouldThrow_WhenRoomHasNoGame()
    {
        var roomId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).WithGameId(null).Build();

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        await Assert.ThrowsAsync<BadRequestException>(() => _sut.RemoveGameFromRoomAsync(roomId));

        _mockGameRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Rooms Without Games Tests

    [Fact]
    public async Task JoinRoomAsync_ShouldWork_WhenRoomHasNoGame()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).WithGameId(null).IsActive(true).Build();

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRoomPlayerRepository
            .Setup(r => r.RoomHasPlayerAsync(roomId, userId))
            .ReturnsAsync(false);
        _mockRoomPlayerRepository
            .Setup(r => r.AddAsync(roomId, userId))
            .ReturnsAsync(new RoomPlayerBuilder().WithRoomId(roomId).WithUserId(userId).Build());
        _mockMapper.Setup(m => m.Map<RoomDTO>(room)).Returns(new RoomDTO { Id = roomId });

        var result = await _sut.JoinRoomAsync(roomId, userId);

        result.Should().NotBeNull();
        _mockRoomPlayerRepository.Verify(r => r.AddAsync(roomId, userId), Times.Once);
        _mockBlackjackService.Verify(s => s.PlayerJoinAsync(roomId, userId), Times.Never);
    }

    [Fact]
    public async Task LeaveRoomAsync_ShouldWork_WhenRoomHasNoGame()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).WithGameId(null).Build();

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockRoomPlayerRepository
            .Setup(r => r.DeleteAsync(roomId, userId))
            .ReturnsAsync(new RoomPlayerBuilder().WithRoomId(roomId).WithUserId(userId).Build());
        _mockMapper.Setup(m => m.Map<RoomDTO>(room)).Returns(new RoomDTO { Id = roomId });

        var result = await _sut.LeaveRoomAsync(roomId, userId);

        result.Should().NotBeNull();
        _mockRoomPlayerRepository.Verify(r => r.DeleteAsync(roomId, userId), Times.Once);
        _mockBlackjackService.Verify(s => s.PlayerLeaveAsync(roomId, userId), Times.Never);
    }

    [Fact]
    public async Task GetGameStateAsync_ShouldReturnEmpty_WhenRoomHasNoGame()
    {
        var roomId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).WithGameId(null).Build();

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        var result = await _sut.GetGameStateAsync(roomId);

        result.Should().BeEmpty();
        _mockGameRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateGameStateAsync_ShouldThrow_WhenRoomHasNoGame()
    {
        var roomId = Guid.NewGuid();
        var room = new RoomBuilder().WithId(roomId).WithGameId(null).Build();

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateGameStateAsync(roomId, "{}")
        );

        _mockGameRepository.Verify(
            r => r.UpdateGamestateAsync(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never
        );
    }

    #endregion
}
