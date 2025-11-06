using Project.Api.Models;

namespace Project.Test.Helpers.Builders;

public class GameBuilder : IBuilder<Game>
{
    private readonly Game _game;

    public GameBuilder()
    {
        // start with a valid, empty game
        _game = new Game
        {
            Id = Guid.NewGuid(),
            GameMode = "Blackjack",
            GameState = "{}",
            State = "{}",
            Round = 1,
            DeckId = "new",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    public GameBuilder WithId(Guid id)
    {
        _game.Id = id;
        return this;
    }

    public GameBuilder WithGameMode(string gameMode)
    {
        _game.GameMode = gameMode;
        return this;
    }

    public GameBuilder WithRound(int round)
    {
        _game.Round = round;
        return this;
    }

    public GameBuilder WithGameState(string gameState)
    {
        _game.GameState = gameState;
        return this;
    }

    public GameBuilder WithDeckId(string deckId)
    {
        _game.DeckId = deckId;
        return this;
    }

    public GameBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _game.CreatedAt = createdAt;
        return this;
    }

    public GameBuilder UpdatedAt(DateTimeOffset updatedAt)
    {
        _game.UpdatedAt = updatedAt;
        return this;
    }

    public GameBuilder StartedAt(DateTimeOffset? startedAt = null)
    {
        _game.StartedAt = startedAt ?? DateTimeOffset.UtcNow;
        return this;
    }

    public GameBuilder EndedAt(DateTimeOffset? endedAt = null)
    {
        _game.EndedAt = endedAt ?? DateTimeOffset.UtcNow;
        return this;
    }

    public GameBuilder WithPlayers(params GamePlayer[] player)
    {
        foreach (var p in player)
        {
            _game.GamePlayers.Add(p);
        }
        return this;
    }

    public Game Build()
    {
        return _game;
    }

    /// <summary>
    /// Allows implicit conversion from GameBuilder to Game without an explicit call to Build().
    /// </summary>
    public static implicit operator Game(GameBuilder builder) => builder.Build();
}
