namespace PaymentCore.Domain.Exceptions;

public class DifferentCurrenciesException : Exception
{
    public DifferentCurrenciesException() : base("Разные валюты"){}
}