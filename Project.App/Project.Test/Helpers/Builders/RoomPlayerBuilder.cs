using Project.Api.Models;
using Project.Api.Utilities.Enums;

namespace Project.Test.Helpers.Builders;

public class RoomPlayerBuilder : IBuilder<RoomPlayer>
{
    private readonly RoomPlayer _roomPlayer;

    public RoomPlayerBuilder()
    {
        // start with a valid, empty room player
        _roomPlayer = new RoomPlayer
        {
            RoomId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            Status = Status.Active,
        };
    }

    public RoomPlayerBuilder WithRoomId(Guid roomId)
    {
        _roomPlayer.RoomId = roomId;
        return this;
    }

    public RoomPlayerBuilder WithUserId(Guid userId)
    {
        _roomPlayer.UserId = userId;
        return this;
    }

    public RoomPlayerBuilder WithStatus(Status status)
    {
        _roomPlayer.Status = status;
        return this;
    }

    public RoomPlayerBuilder WithRoom(Room room)
    {
        _roomPlayer.Room = room;
        _roomPlayer.RoomId = room.Id;
        return this;
    }

    public RoomPlayerBuilder WithUser(User user)
    {
        _roomPlayer.User = user;
        _roomPlayer.UserId = user.Id;
        return this;
    }

    public RoomPlayer Build()
    {
        return _roomPlayer;
    }

    /// <summary>
    /// Allows implicit conversion from RoomPlayerBuilder to RoomPlayer without an explicit call to Build().
    /// </summary>
    public static implicit operator RoomPlayer(RoomPlayerBuilder builder) => builder.Build();
}
