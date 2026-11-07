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

    [Fact]
    public void Generate_ReturnsValidNumber()
    {
        for(int i = 0; i < 1000; i++)
        {
        string number = CardNumberGenerator.Generate();

        var result = CardNumberGenerator.IsValid(number);

        Assert.True(result);
        }
    }

    [Fact]
    public void Generate_ReturnsSixteenDigitsStartingWithBin()
    {
        string number = CardNumberGenerator.Generate();

        Assert.Equal(16, number.Length);
        Assert.StartsWith("400000", number);
    }

    [Fact]
    public void GenerateCvv_ReturnsThreeDigitNumber()
    {
        for(int i = 0; i < 1000; i++)
        {
            string cvv = CardNumberGenerator.GenerateCvv();
            

            Assert.Equal(3, cvv.Length);
            Assert.True(cvv.All(char.IsDigit));
        }
    }
}