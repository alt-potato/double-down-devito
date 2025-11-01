using Project.Api.DTOs;

namespace Project.Api.Utilities.Extensions;

public static class CardExtensions
{
    private static readonly Dictionary<char, string> SuitMap = new()
    {
        { 'S', "SPADES" },
        { 'D', "DIAMONDS" },
        { 'C', "CLUBS" },
        { 'H', "HEARTS" },
        { '1', "BLACK" },
        { '2', "RED" },
    };
    private static readonly Dictionary<char, string> ValueMap = new()
    {
        { 'A', "ACE" },
        { '2', "2" },
        { '3', "3" },
        { '4', "4" },
        { '5', "5" },
        { '6', "6" },
        { '7', "7" },
        { '8', "8" },
        { '9', "9" },
        { '0', "10" },
        { 'J', "JACK" },
        { 'Q', "QUEEN" },
        { 'K', "KING" },
        { 'X', "JOKER" },
    };

    /// <summary>
    /// Creates a CardDTO from the given two-character card code.
    ///
    /// For reference: https://github.com/crobertsbmw/deckofcards/blob/master/deck/models.py
    /// </summary>
    public static CardDTO ToCard(this string code, string? image = null)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 2)
            throw new ArgumentException($"Invalid card code: '{code}'");

        code = code.ToUpperInvariant();

        if (!ValueMap.TryGetValue(code[0], out var value))
            throw new ArgumentException($"Invalid card value: '{code[0]}'");

        if (!SuitMap.TryGetValue(code[1], out var suit))
            throw new ArgumentException($"Invalid card suit: '{code[1]}'");

        return new CardDTO
        {
            Value = value,
            Suit = suit,
            Code = code,
            Image = image ?? $"https://deckofcardsapi.com/static/img/{code}.png",
        };
    }
}
