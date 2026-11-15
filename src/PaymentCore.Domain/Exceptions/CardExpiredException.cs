namespace PaymentCore.Domain.Exceptions;

public class CardExpiredException : Exception
{
    public CardExpiredException() : base("Карта просрочена"){}
}