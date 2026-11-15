namespace PaymentCore.Domain.Exceptions;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException() : base("Недостаточно средств")
    {
        
    }
}