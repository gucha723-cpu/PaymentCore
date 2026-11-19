namespace PaymentCore.Domain.Exceptions;

public class CardExpiredException : DomainException
{
    public CardExpiredException() : base("Карта просрочена"){}
}