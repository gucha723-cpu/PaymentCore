using PaymentCore.Domain.Entities.Enums;
using PaymentCore.Domain.Exceptions;
namespace PaymentCore.Domain.Entities;

public abstract class Card : BaseEntity
{
    public Account Account{get; protected set;} = null!;
    public Guid AccountId{get;protected set;}
    public CardType CardType{get;protected set;}
    public string Last4Digits{get;protected set;} = string.Empty;
    public DateOnly ExpiryDate{get;protected set;}
    public CardStatus Status{get; protected set;}

    protected Card(){}
    protected Card(Guid accountId, CardType type, string fullNumber, DateOnly expiryDate)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Счет обязателен", nameof(accountId));
        }
        if(string.IsNullOrWhiteSpace(fullNumber) || fullNumber.Length != 16 || !fullNumber.All(char.IsDigit))
        {
            throw new ArgumentException("Некорректный номер карты", nameof(fullNumber));
        }
        AccountId = accountId;
        CardType = type;
        Last4Digits = fullNumber[^4 ..];
        ExpiryDate = expiryDate;
        Status = CardStatus.Active;
    }

    public void Block()
    {
        if(Status == CardStatus.Blocked)
        {
            return;
        }

        Status = CardStatus.Blocked;
        Touch();
    }

    public void Unblock()
    {
        EnsureNotExpired();
        
        if(Status == CardStatus.Active)
        {
            return;
        }

        Status = CardStatus.Active;
        Touch();
    }

    private void EnsureNotExpired()
    {
        if(ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            if(Status != CardStatus.Expired)
            {
                Status = CardStatus.Expired;
                Touch();
            }
            throw new CardExpiredException(); 
        }
    }
}
