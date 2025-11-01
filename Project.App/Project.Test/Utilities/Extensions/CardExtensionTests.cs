using FluentAssertions;
using Project.Api.DTOs;
using Project.Api.Utilities.Extensions;
using Xunit;

namespace Project.Test.Utilities.Extensions;

public class CardExtensionTests
{
    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("ABC")]
    public void ToCard_ShouldThrowArgumentException_WhenCodeIsInvalidLength(string invalidCode)
    {
        // Act
        Action act = () => invalidCode.ToCard();

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage($"Invalid card code: '{invalidCode}'*");
    }

    [Theory]
    [InlineData("ZA")] // Invalid value 'Z'
    [InlineData("1H")] // Invalid value '1' (should be '0' for 10)
    public void ToCard_ShouldThrowArgumentException_WhenValueIsInvalid(string invalidCode)
    {
        // Act
        Action act = () => invalidCode.ToCard();

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage($"Invalid card value: '{invalidCode[0]}'*"); // Expect message about invalid value
    }

    [Theory]
    [InlineData("A9")] // Invalid suit '9'
    [InlineData("K_")] // Invalid suit '_'
    public void ToCard_ShouldThrowArgumentException_WhenSuitIsInvalid(string invalidCode)
    {
        // Act
        Action act = () => invalidCode.ToCard();

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage($"Invalid card suit: '{invalidCode[1]}'*"); // Expect message about invalid suit
    }

    [Theory]
    [InlineData("AS", "ACE", "SPADES", "https://deckofcardsapi.com/static/img/AS.png")]
    [InlineData("2D", "2", "DIAMONDS", "https://deckofcardsapi.com/static/img/2D.png")]
    [InlineData("0C", "10", "CLUBS", "https://deckofcardsapi.com/static/img/0C.png")] // Ten of Clubs
    [InlineData("JH", "JACK", "HEARTS", "https://deckofcardsapi.com/static/img/JH.png")]
    [InlineData("QS", "QUEEN", "SPADES", "https://deckofcardsapi.com/static/img/QS.png")]
    [InlineData("KD", "KING", "DIAMONDS", "https://deckofcardsapi.com/static/img/KD.png")]
    [InlineData("X1", "JOKER", "BLACK", "https://deckofcardsapi.com/static/img/X1.png")] // Black Joker (using '1' for BLACK suit)
    [InlineData("X2", "JOKER", "RED", "https://deckofcardsapi.com/static/img/X2.png")] // Red Joker (using '2' for RED suit)
    public void ToCard_ShouldReturnCorrectCardDTO_ForValidCodes(
        string code,
        string expectedValue,
        string expectedSuit,
        string expectedImage
    )
    {
        // Act
        var card = code.ToCard();

        // Assert
        card.Should().NotBeNull();
        card.Code.Should().Be(code.ToUpperInvariant());
        card.Value.Should().Be(expectedValue);
        card.Suit.Should().Be(expectedSuit);
        card.Image.Should().Be(expectedImage);
    }

    [Fact]
    public void ToCard_ShouldHandleLowercaseInput()
    {
        // Arrange
        var code = "as";
        var expectedImage = "https://deckofcardsapi.com/static/img/AS.png";

        // Act
        var card = code.ToCard();

        // Assert
        card.Should().NotBeNull();
        card.Code.Should().Be("AS"); // Should be converted to uppercase
        card.Value.Should().Be("ACE");
        card.Suit.Should().Be("SPADES");
        card.Image.Should().Be(expectedImage);
    }

    [Fact]
    public void ToCard_ShouldUseProvidedImage_WhenSpecified()
    {
        // Arrange
        var code = "KH";
        var customImage = "https://example.com/custom_king_hearts.png";

        // Act
        var card = code.ToCard(customImage);

        // Assert
        card.Should().NotBeNull();
        card.Code.Should().Be("KH");
        card.Value.Should().Be("KING");
        card.Suit.Should().Be("HEARTS");
        card.Image.Should().Be(customImage); // Should use the provided image
    }
}
