using System.Text.Json;
using Project.Api.Utilities.Constants;

namespace Project.Api.DTOs;

/// <summary>
/// The base class for all blackjack-specific action DTOs.
/// </summary>
public abstract record BlackjackActionDTO : GameActionDTO;

public record BetAction(long Amount) : BlackjackActionDTO;

public record HitAction : BlackjackActionDTO;

public record StandAction : BlackjackActionDTO;

public record DoubleAction : BlackjackActionDTO;

public record SplitAction(long Amount) : BlackjackActionDTO;

public record SurrenderAction : BlackjackActionDTO;

public record HurryUpAction : BlackjackActionDTO;

public static class JsonElementExtensions
{
    private static readonly Dictionary<string, Type> BlackjackActionTypeMap = new()
    {
        { "bet", typeof(BetAction) },
        { "hit", typeof(HitAction) },
        { "stand", typeof(StandAction) },
        { "double", typeof(DoubleAction) },
        { "split", typeof(SplitAction) },
        { "surrender", typeof(SurrenderAction) },
        { "hurry_up", typeof(HurryUpAction) },
    };

    /// <summary>
    /// Extension method to deserialize a <see cref="BlackjackActionDTO"/> from a <see cref="JsonElement"/>.
    /// Any errors thrown during deserialization will be passed to the caller.
    /// </summary>
    public static BlackjackActionDTO ToBlackjackAction(this JsonElement element, string action)
    {
        if (!BlackjackActionTypeMap.TryGetValue(action, out var actionType))
        {
            throw new NotSupportedException(
                $"Action '{action}' is not a valid action for Blackjack."
            );
        }

        var deserializedAction = element.Deserialize(actionType, ApiJsonSerializerOptions.DefaultOptions);

        return (BlackjackActionDTO)(
            deserializedAction
            ?? throw new InvalidOperationException($"Could not deserialize action {action}.")
        );
    }
}
