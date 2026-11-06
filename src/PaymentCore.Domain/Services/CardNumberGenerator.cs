using System.Linq;
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
}