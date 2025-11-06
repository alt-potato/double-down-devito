using Project.Api.Models;

namespace Project.Test.Helpers.Builders;

public class GamePlayerBuilder : IBuilder<GamePlayer>
{
    private readonly GamePlayer _gamePlayer;

    public GamePlayerBuilder()
    {
        // start with a valid, empty game player
        _gamePlayer = new GamePlayer
        {
            GameId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            Balance = 1000,
            BalanceDelta = 0,
        };
    }

    public GamePlayerBuilder WithGameId(Guid gameId)
    {
        _gamePlayer.GameId = gameId;
        return this;
    }

    public GamePlayerBuilder WithUserId(Guid userId)
    {
        _gamePlayer.UserId = userId;
        return this;
    }

    public GamePlayerBuilder WithBalance(long balance)
    {
        _gamePlayer.Balance = balance;
        return this;
    }

    public GamePlayerBuilder WithBalanceDelta(long balanceDelta)
    {
        _gamePlayer.BalanceDelta = balanceDelta;
        return this;
    }

    public GamePlayerBuilder WithGame(Game game)
    {
        _gamePlayer.Game = game;
        _gamePlayer.GameId = game.Id;
        return this;
    }

    public GamePlayerBuilder WithUser(User user)
    {
        _gamePlayer.User = user;
        _gamePlayer.UserId = user.Id;
        return this;
    }

    public GamePlayer Build()
    {
        return _gamePlayer;
    }

    /// <summary>
    /// Allows implicit conversion from GamePlayerBuilder to GamePlayer without an explicit call to Build().
    /// </summary>
    public static implicit operator GamePlayer(GamePlayerBuilder builder) => builder.Build();
}
