using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Utilities.Enums;

namespace Project.Test.Helpers;

// Helper class for repository unit tests, providing common utilities and test data creation methods.
public static class RepositoryTestHelper
{
    // Creates an in-memory database context for testing.
    public static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Creates a test user with optional custom properties.
    public static User CreateTestUser(
        Guid? id = null,
        string? name = null,
        string? email = null,
        double balance = 1000,
        string? avatarUrl = null
    )
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? "Test User",
            Email = email ?? $"test{Guid.NewGuid()}@example.com",
            Balance = balance,
            AvatarUrl = avatarUrl,
        };
    }

    // Creates a test room with optional custom properties.
    public static Room CreateTestRoom(
        Guid? id = null,
        Guid? hostId = null,
        bool isPublic = true,
        bool isActive = true,
        string? description = null,
        int maxPlayers = 6,
        int minPlayers = 2
    )
    {
        return new Room
        {
            Id = id ?? Guid.NewGuid(),
            HostId = hostId ?? Guid.NewGuid(),
            IsPublic = isPublic,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            Description = description ?? "Test Room",
            MaxPlayers = maxPlayers,
            MinPlayers = minPlayers,
        };
    }

    // Creates a test room player with optional custom properties.
    public static RoomPlayer CreateTestRoomPlayer(
        Guid? roomId = null,
        Guid? userId = null,
        Status status = Status.Active
    )
    {
        return new RoomPlayer
        {
            RoomId = roomId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Status = status,
        };
    }

    // Creates a test game with optional custom properties.
    public static Game CreateTestGame(
        Guid? id = null,
        string? gameMode = null,
        string? gameState = null,
        string? state = null,
        int round = 0,
        string? deckId = null,
        DateTimeOffset? endedAt = null,
        DateTimeOffset? createdAt = null
    )
    {
        return new Game
        {
            Id = id ?? Guid.NewGuid(),
            GameMode = gameMode ?? "Blackjack",
            GameState = gameState ?? "{}",
            State = state ?? "{}",
            Round = round,
            DeckId = deckId ?? "",
            EndedAt = endedAt,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
        };
    }

    // Creates a test game player with optional custom properties.
    public static GamePlayer CreateTestGamePlayer(
        Guid? gameId = null,
        Guid? userId = null,
        long balance = 1000,
        long balanceDelta = 0
    )
    {
        return new GamePlayer
        {
            GameId = gameId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Balance = balance,
            BalanceDelta = balanceDelta,
        };
    }

    // Creates a test hand with optional custom properties.
    public static Hand CreateTestHand(
        Guid? gameId = null,
        Guid? userId = null,
        int order = 1,
        int handNumber = 1,
        int bet = 100
    )
    {
        return new Hand
        {
            GameId = gameId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Order = order,
            HandNumber = handNumber,
            Bet = bet,
        };
    }
}
