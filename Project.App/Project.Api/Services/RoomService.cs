using System.Text.Json;
using AutoMapper;
using Project.Api.DTOs;
using Project.Api.Models;
using Project.Api.Models.Games;
using Project.Api.Repositories.Interface;
using Project.Api.Services.Interface;
using Project.Api.Utilities;
using Project.Api.Utilities.Constants;

namespace Project.Api.Services;

public class RoomService(
    IUnitOfWork unitOfWork,
    IEnumerable<IGameService<IGameState, GameConfig>> gameServices,
    IMapper mapper,
    ILogger<RoomService> logger
) : IRoomService
{
    private readonly IUnitOfWork _uow = unitOfWork;
    private readonly Dictionary<string, IGameService<IGameState, GameConfig>> _gameServices =
        gameServices.ToDictionary(s => s.GameMode.ToLowerInvariant(), s => s); // initialize dictionary
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<RoomService> _logger = logger;

    /// <summary>
    /// Helper method to get the correct game service.
    /// </summary>
    private IGameService<IGameState, GameConfig> GetGameService(string gameMode)
    {
        if (!_gameServices.TryGetValue(gameMode.ToLowerInvariant(), out var service))
        {
            throw new BadRequestException($"Unsupported game mode: {gameMode}");
        }
        return service;
    }

    public async Task<RoomDTO?> GetRoomByIdAsync(Guid id)
    {
        var room = await _uow.Rooms.GetByIdAsync(id);
        return room is null ? null : _mapper.Map<RoomDTO>(room);
    }

    public async Task<IReadOnlyList<RoomDTO>> GetAllRoomsAsync()
    {
        var rooms = await _uow.Rooms.GetAllAsync();
        return _mapper.Map<IReadOnlyList<RoomDTO>>(rooms);
    }

    public async Task<IReadOnlyList<RoomDTO>> GetActiveRoomsAsync()
    {
        var rooms = await _uow.Rooms.GetAllActiveAsync();
        return _mapper.Map<IReadOnlyList<RoomDTO>>(rooms);
    }

    public async Task<IReadOnlyList<RoomDTO>> GetPublicRoomsAsync()
    {
        var rooms = await _uow.Rooms.GetAllPublicAsync();
        return _mapper.Map<IReadOnlyList<RoomDTO>>(rooms);
    }

    public async Task<RoomDTO?> GetRoomByHostIdAsync(Guid hostId)
    {
        var room = await _uow.Rooms.GetByHostIdAsync(hostId);
        return room is null ? null : _mapper.Map<RoomDTO>(room);
    }

    public async Task<RoomDTO> CreateRoomAsync(CreateRoomDTO dto)
    {
        _logger.LogInformation("Creating new room...");

        Room room = _mapper.Map<Room>(dto);

        room.Id = Guid.CreateVersion7();
        room.CreatedAt = DateTimeOffset.UtcNow;
        room.IsActive = true;

        Room createdRoom = await _uow.Rooms.CreateAsync(room); // create room

        // if a game mode is specified, create a game and associate it with the room
        if (!string.IsNullOrWhiteSpace(dto.GameMode))
        {
            var gameService = GetGameService(dto.GameMode);

            // create game entity
            Game game = new()
            {
                GameMode = dto.GameMode,
                GameState = gameService.GetInitialStateAsync(),
                GameConfig = dto.GameConfig,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            await _uow.Games.CreateAsync(game);

            // associate the game with the room
            createdRoom.GameId = game.Id;
            await _uow.Rooms.UpdateAsync(createdRoom);

            // delegate host player creation to the game service
            await gameService.PlayerJoinAsync(createdRoom.Id, dto.HostId);
        }

        _logger.LogInformation("Successfully created room with ID: {RoomId}", createdRoom.Id);
        return _mapper.Map<RoomDTO>(createdRoom);
    }

    public async Task<RoomDTO?> UpdateRoomAsync(UpdateRoomDTO dto)
    {
        var existingRoom = await _uow.Rooms.GetByIdAsync(dto.Id);
        if (existingRoom is null)
            return null;

        _mapper.Map(dto, existingRoom);

        var updatedRoom = await _uow.Rooms.UpdateAsync(existingRoom);
        return updatedRoom is null ? null : _mapper.Map<RoomDTO>(updatedRoom);
    }

    public async Task<bool> DeleteRoomAsync(Guid id)
    {
        var deletedRoom = await _uow.Rooms.DeleteAsync(id);
        return deletedRoom != null;
    }

    public async Task<bool> RoomExistsAsync(Guid id)
    {
        return await _uow.Rooms.ExistsAsync(id);
    }

    public async Task<string> GetGameStateAsync(Guid roomId)
    {
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room?.GameId == null)
        {
            return string.Empty;
        }

        var game = await _uow.Games.GetByIdAsync(room.GameId.Value);
        return game?.GameState ?? string.Empty;
    }

    public async Task<bool> UpdateGameStateAsync(Guid roomId, string gameState)
    {
        if (string.IsNullOrWhiteSpace(gameState))
            throw new BadRequestException("Game state cannot be empty.");

        _logger.LogInformation("Updating game state for room {RoomId}", roomId);

        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room?.GameId == null)
        {
            throw new BadRequestException("Room does not have an associated game.");
        }

        var updatedGame = await _uow.Games.UpdateGamestateAsync(room.GameId.Value, gameState);
        return updatedGame != null;
    }

    public async Task<string> GetGameConfigAsync(Guid roomId)
    {
        _logger.LogInformation("Getting game config for room {RoomId}", roomId);

        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room?.GameId == null)
        {
            return string.Empty;
        }

        var game = await _uow.Games.GetByIdAsync(room.GameId.Value);
        return game?.GameConfig ?? string.Empty;
    }

    public async Task<bool> UpdateGameConfigAsync(Guid roomId, string gameConfig)
    {
        if (string.IsNullOrWhiteSpace(gameConfig))
            throw new BadRequestException("Game config cannot be empty.");

        _logger.LogInformation("Updating game config for room {RoomId}", roomId);

        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room?.GameId == null)
        {
            throw new BadRequestException("Room does not have an associated game.");
        }

        var game = await _uow.Games.GetByIdAsync(room.GameId.Value);
        if (game == null)
            return false;

        game.GameConfig = gameConfig;
        var updatedGame = await _uow.Games.UpdateAsync(game);
        return updatedGame != null;
    }

    // --- game association methods ---

    /// <summary>
    /// Sets or updates the game for a room. If the room already has a game, it will be replaced.
    /// </summary>
    public async Task<RoomDTO> SetRoomGameAsync(
        Guid roomId,
        string gameMode,
        string? gameConfig = null
    )
    {
        _logger.LogDebug("Setting game for room {RoomId} to mode {GameMode}", roomId, gameMode);

        var room =
            await _uow.Rooms.GetByIdAsync(roomId)
            ?? throw new NotFoundException($"Room with ID {roomId} not found.");

        var gameService = GetGameService(gameMode);

        // If room already has a game, delete it first
        if (room.GameId != null)
        {
            await _uow.Games.DeleteAsync(room.GameId.Value);
        }

        // Create new game entity
        Game game = new()
        {
            GameMode = gameMode,
            GameState = gameService.GetInitialStateAsync(),
            GameConfig = gameConfig ?? string.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await _uow.Games.CreateAsync(game);

        // Associate the game with the room
        room.GameId = game.Id;
        var updatedRoom = await _uow.Rooms.UpdateAsync(room);

        // await transaction.CommitAsync();

        _logger.LogInformation("Successfully set game for room {RoomId}", roomId);
        return _mapper.Map<RoomDTO>(updatedRoom);
    }

    /// <summary>
    /// Removes the game association from a room.
    /// </summary>
    public async Task<RoomDTO> RemoveGameFromRoomAsync(Guid roomId)
    {
        _logger.LogDebug("Removing game association from room {RoomId}", roomId);

        var room =
            await _uow.Rooms.GetByIdAsync(roomId)
            ?? throw new NotFoundException($"Room with ID {roomId} not found.");

        if (room.GameId == null)
        {
            throw new BadRequestException("Room does not have an associated game.");
        }

        // Delete the game
        await _uow.Games.DeleteAsync(room.GameId.Value);

        // Remove game association from room
        room.GameId = null;
        var updatedRoom = await _uow.Rooms.UpdateAsync(room);

        _logger.LogInformation("Successfully removed game association from room {RoomId}", roomId);
        return _mapper.Map<RoomDTO>(updatedRoom);
    }

    // --- game functionality ---

    public async Task<RoomDTO> StartGameAsync(Guid roomId, string? gameConfigJson = null)
    {
        _logger.LogDebug("Starting game for room {RoomId}...", roomId);

        // get the room
        Room room =
            await _uow.Rooms.GetByIdAsync(roomId)
            ?? throw new NotFoundException($"Room with ID {roomId} not found.");

        // validate room is not already started
        if (room.StartedAt != null)
        {
            _logger.LogWarning("Attempted to start already-started game in room {RoomId}", roomId);
            throw new BadRequestException("Game has already been started.");
        }

        // get player count
        int playerCount = await _uow.RoomPlayers.GetPlayerCountAsync(roomId);

        // validate minimum players
        if (playerCount < room.MinPlayers)
        {
            _logger.LogWarning(
                "Cannot start game in room {RoomId}. Need {MinPlayers} players, have {PlayerCount}",
                roomId,
                room.MinPlayers,
                playerCount
            );
            throw new BadRequestException(
                $"Cannot start game. Minimum {room.MinPlayers} players required, but only {playerCount} present."
            );
        }

        // get game associated with the room
        if (room.GameId == null)
        {
            throw new BadRequestException("Room does not have an associated game.");
        }

        var game =
            await _uow.Games.GetByIdAsync(room.GameId.Value)
            ?? throw new NotFoundException($"Game with ID {room.GameId} not found.");

        // get game service

        var gameService = GetGameService(game.GameMode);

        // determine config based on game mode using a switch statement
        GameConfig config;
        try
        {
            switch (game.GameMode.ToLowerInvariant())
            {
                case GameModes.Blackjack:
                    if (!string.IsNullOrWhiteSpace(gameConfigJson))
                    {
                        // Deserialize to specific BlackjackConfig type
                        config =
                            JsonSerializer.Deserialize<BlackjackConfig>(gameConfigJson)
                            ?? throw new BadRequestException(
                                "Invalid Blackjack game configuration JSON."
                            );
                        _logger.LogInformation(
                            "Using custom Blackjack config for room {RoomId}",
                            roomId
                        );
                    }
                    else if (!string.IsNullOrWhiteSpace(game.GameConfig))
                    {
                        // use existing config for Blackjack
                        config =
                            JsonSerializer.Deserialize<BlackjackConfig>(game.GameConfig)
                            ?? throw new BadRequestException(
                                "Invalid existing Blackjack game configuration."
                            );
                        _logger.LogInformation(
                            "Using existing Blackjack config for room {RoomId}",
                            roomId
                        );
                    }
                    else
                    {
                        // use default Blackjack config
                        config = new BlackjackConfig();
                        _logger.LogInformation(
                            "Using default Blackjack config for room {RoomId}",
                            roomId
                        );
                    }
                    break;
                // this is where i would add more game modes
                // ...if i had them
                default:
                    // should be caught by gameServices, but just in case
                    throw new BadRequestException(
                        $"Unsupported game mode for configuration: {game.GameMode}"
                    );
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize game configuration for room {RoomId}",
                roomId
            );
            throw new BadRequestException("Invalid game configuration format.");
        }

        // start the game!
        await gameService.StartGameAsync(roomId, config); // delegate setup to the generic game service

        _logger.LogInformation("Successfully started game for room {RoomId}!", roomId);
        return _mapper.Map<RoomDTO>(
            await _uow.Rooms.GetByIdAsync(roomId)
                ?? throw new NotFoundException($"Room with ID {roomId} not found.")
        );
    }

    public async Task PerformPlayerActionAsync(
        Guid roomId,
        Guid playerId,
        string action,
        JsonElement data
    )
    {
        _logger.LogDebug(
            "Player {PlayerId} performing action '{Action}' in room {RoomId}",
            playerId,
            action,
            roomId
        );

        // validate room exists
        Room room =
            await _uow.Rooms.GetByIdAsync(roomId)
            ?? throw new NotFoundException($"Room with ID {roomId} not found.");

        // validate game has started
        if (room.StartedAt == null)
        {
            _logger.LogWarning(
                "Player {PlayerId} attempted action in room {RoomId} before game started",
                playerId,
                roomId
            );
            throw new BadRequestException("Game has not been started yet.");
        }

        // validate player is in the room
        bool isPlayerInRoom = await _uow.RoomPlayers.RoomHasPlayerAsync(roomId, playerId);
        if (!isPlayerInRoom)
        {
            _logger.LogWarning(
                "Player {PlayerId} not in room {RoomId} attempted action",
                playerId,
                roomId
            );
            throw new BadRequestException($"Player {playerId} is not in room {roomId}.");
        }

        // get the game associated with the room
        if (room.GameId == null)
        {
            throw new BadRequestException("Room does not have an associated game.");
        }

        Game game =
            await _uow.Games.GetByIdAsync(room.GameId.Value)
            ?? throw new NotFoundException($"Game with ID {room.GameId} not found.");

        // delegate to the appropriate game service based on game mode
        var gameService = GetGameService(game.GameMode);
        await gameService.PerformActionAsync(roomId, playerId, action, data);
        _logger.LogInformation(
            "Successfully performed action '{Action}' for player {PlayerId} in room {RoomId}",
            action,
            playerId,
            roomId
        );
    }

    // --- player management ---

    public async Task<RoomDTO> JoinRoomAsync(Guid roomId, Guid userId)
    {
        _logger.LogDebug("User {UserId} attempting to join room {RoomId}...", userId, roomId);

        // validate room exists
        Room room =
            await _uow.Rooms.GetByIdAsync(roomId)
            ?? throw new NotFoundException($"Room with ID {roomId} not found.");

        // validate room is active
        if (!room.IsActive)
        {
            _logger.LogWarning(
                "User {UserId} attempted to join inactive room {RoomId}.",
                userId,
                roomId
            );
            throw new BadRequestException("Room is not active.");
        }

        // add user to room
        if (!await _uow.RoomPlayers.RoomHasPlayerAsync(roomId, userId))
        {
            await _uow.RoomPlayers.AddAsync(roomId, userId);
        }
        else
        {
            _logger.LogWarning(
                "User {UserId} attempted to join room {RoomId} they are already in.",
                userId,
                roomId
            );
            throw new BadRequestException("User is already in the room.");
        }

        // if room has a game, also add player to game (delegated to the appropriate game service)
        if (room.GameId != null)
        {
            var game =
                await _uow.Games.GetByIdAsync(room.GameId.Value)
                ?? throw new NotFoundException($"Game with ID {room.GameId} not found.");

            var gameService = GetGameService(game.GameMode);
            await gameService.PlayerJoinAsync(roomId, userId);
        }

        // return the room (we already have it)
        return _mapper.Map<RoomDTO>(room);
    }

    public async Task<RoomDTO> LeaveRoomAsync(Guid roomId, Guid userId)
    {
        _logger.LogDebug("User {UserId} attempting to leave room {RoomId}...", userId, roomId);

        // validate room exists
        var room =
            await _uow.Rooms.GetByIdAsync(roomId)
            ?? throw new NotFoundException($"Room with ID {roomId} not found.");

        // remove player from roomplayers
        await _uow.RoomPlayers.DeleteAsync(roomId, userId);

        // if room has a game, also remove player from game (delegated to the appropriate game service)
        if (room.GameId != null)
        {
            var game =
                await _uow.Games.GetByIdAsync(room.GameId.Value)
                ?? throw new NotFoundException($"Game with ID {room.GameId} not found.");

            var gameService = GetGameService(game.GameMode);
            await gameService.PlayerLeaveAsync(roomId, userId);
        }

        // return the room (we already have it)
        return _mapper.Map<RoomDTO>(room);
    }
}
