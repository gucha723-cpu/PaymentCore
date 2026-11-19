namespace PaymentCore.Domain.Exceptions;

public class DifferentCurrenciesException : DomainException
{
    public DifferentCurrenciesException() : base("Разные валюты"){}
}