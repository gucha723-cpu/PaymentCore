using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace PaymentCore.Domain.Services;

public static class CardNumberGenerator
{
    public static bool IsValid(string number)
    {
        if(string.IsNullOrEmpty(number) || number.Length != 16 || !number.All(char.IsDigit))
        {
            return false;
        }
        int sum = 0;
        int counter = 0;

        for(int i = number.Length -1; i >= 0; i--)
        {
            int doubledNumber;
            if(counter != 0 && counter % 2 != 0)
            {
                doubledNumber = (number[i] - '0') * 2;
               if(doubledNumber > 9)
                {
                  doubledNumber -= 9;
                
                }
                  sum += doubledNumber;
            }
            else
            {
                sum += number[i] - '0';
            }
            counter++;
        }

        return sum % 10 == 0;
    }

    public static string Generate()
    {
        
        StringBuilder randomCardGenerator = new StringBuilder();

        randomCardGenerator.Append("400000");
        
        for(int i = 0; i < 9; i++)
        {
            randomCardGenerator.Append(RandomNumberGenerator.GetInt32(0,10));
        }

        for(int i = 0; i <= 9; i++)
        {
            randomCardGenerator.Append(i);
            if (IsValid(randomCardGenerator.ToString()))
            {
                return randomCardGenerator.ToString();
            }

            randomCardGenerator.Length--;
        }

        throw new InvalidOperationException("Не удалось сгенерировать номер карты");
    }

    public static string GenerateCvv()
    {
        return RandomNumberGenerator.GetInt32(0,1000).ToString("D3");
    }
}