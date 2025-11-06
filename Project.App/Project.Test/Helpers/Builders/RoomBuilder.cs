using Project.Api.Models;

namespace Project.Test.Helpers.Builders;

public class RoomBuilder : IBuilder<Room>
{
    private readonly Room _room;

    public RoomBuilder()
    {
        // start with a valid, empty room
        _room = new Room
        {
            Id = Guid.CreateVersion7(),
            HostId = Guid.CreateVersion7(),
            IsPublic = true,
            IsActive = true,
            Description = "Test Room",
            MaxPlayers = 6,
            MinPlayers = 2,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            StartedAt = null,
            EndedAt = null,
            GameId = null,
        };
    }

    public RoomBuilder WithId(Guid id)
    {
        _room.Id = id;
        return this;
    }

    public RoomBuilder WithHostId(Guid hostId)
    {
        _room.HostId = hostId;
        return this;
    }

    public RoomBuilder WithHost(User host)
    {
        _room.Host = host;
        _room.HostId = host.Id;
        return this;
    }

    public RoomBuilder IsPublic(bool isPublic)
    {
        _room.IsPublic = isPublic;
        return this;
    }

    public RoomBuilder IsActive(bool isActive)
    {
        _room.IsActive = isActive;
        return this;
    }

    public RoomBuilder WithDescription(string description)
    {
        _room.Description = description;
        return this;
    }

    public RoomBuilder WithMaxPlayers(int maxPlayers)
    {
        _room.MaxPlayers = maxPlayers;
        return this;
    }

    public RoomBuilder WithMinPlayers(int minPlayers)
    {
        _room.MinPlayers = minPlayers;
        return this;
    }

    public RoomBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _room.CreatedAt = createdAt;
        return this;
    }

    public RoomBuilder UpdatedAt(DateTimeOffset updatedAt)
    {
        _room.UpdatedAt = updatedAt;
        return this;
    }

    public RoomBuilder StartedAt(DateTimeOffset? startedAt)
    {
        _room.StartedAt = startedAt;
        return this;
    }

    public RoomBuilder EndedAt(DateTimeOffset? endedAt)
    {
        _room.EndedAt = endedAt;
        return this;
    }

    public RoomBuilder WithGameId(Guid? gameId)
    {
        _room.GameId = gameId;
        return this;
    }

    public RoomBuilder WithGame(Game game)
    {
        _room.Game = game;
        _room.GameId = game.Id;
        return this;
    }

    public Room Build()
    {
        return _room;
    }

    /// <summary>
    /// Allows implicit conversion from RoomBuilder to Room without an explicit call to Build().
    /// </summary>
    public static implicit operator Room(RoomBuilder builder) => builder.Build();
}
