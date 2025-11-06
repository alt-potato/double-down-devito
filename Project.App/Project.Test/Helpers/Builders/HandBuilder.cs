using Project.Api.Models;

namespace Project.Test.Helpers.Builders;

public class HandBuilder : IBuilder<Hand>
{
    private readonly Hand _hand;

    public HandBuilder()
    {
        // start with a valid, empty hand
        _hand = new Hand
        {
            Id = Guid.CreateVersion7(),
            GameId = Guid.CreateVersion7(),
            UserId = Guid.CreateVersion7(),
            Order = 1,
            HandNumber = 1,
            Bet = 100,
        };
    }

    public HandBuilder WithId(Guid id)
    {
        _hand.Id = id;
        return this;
    }

    public HandBuilder WithGameId(Guid gameId)
    {
        _hand.GameId = gameId;
        return this;
    }

    public HandBuilder WithUserId(Guid userId)
    {
        _hand.UserId = userId;
        return this;
    }

    public HandBuilder WithOrder(int order)
    {
        _hand.Order = order;
        return this;
    }

    public HandBuilder WithHandNumber(int handNumber)
    {
        _hand.HandNumber = handNumber;
        return this;
    }

    public HandBuilder WithBet(long bet)
    {
        _hand.Bet = bet;
        return this;
    }

    public HandBuilder WithGamePlayer(GamePlayer gamePlayer)
    {
        _hand.GamePlayer = gamePlayer;
        _hand.GameId = gamePlayer.GameId;
        _hand.UserId = gamePlayer.UserId;
        return this;
    }

    public Hand Build()
    {
        return _hand;
    }

    /// <summary>
    /// Allows implicit conversion from HandBuilder to Hand without an explicit call to Build().
    /// </summary>
    public static implicit operator Hand(HandBuilder builder) => builder.Build();
}
