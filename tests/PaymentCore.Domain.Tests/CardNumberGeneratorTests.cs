using PaymentCore.Domain.Services;
namespace PaymentCore.Domain.Tests;

public class CardNumberGeneratorTests
{
    [Fact]
    public void IsValid_ValidNumber_ReturnsTrue()
    {
        string number = "4539578763621486";

        var result = CardNumberGenerator.IsValid(number);

        Assert.True(result);        
    }

    [Fact]
    public void IsValid_InvalidCardNumber_ReturnsFalse()
    {
        string number = "4539578763621487";

        var result = CardNumberGenerator.IsValid(number);
        
        Assert.False(result);
    }

    [Fact]
    public void IsValid_NumberHasIncorrectLength_ReturnsFalse()
    {
        string number = "123";

        var result = CardNumberGenerator.IsValid(number);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_NumberContainsLetters_ReturnsFalse()
    {
        string number = "45395787636214a6";

        var result = CardNumberGenerator.IsValid(number);

        Assert.False(result);
    }
}