namespace PaymentCore.Domain.Exceptions;

public class InsufficientFundsException : DomainException
{
    public InsufficientFundsException() : base("Недостаточно средств")
    {
        
    }
}