using PaymentCore.Domain.Entities.Enums;
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
        throw new ArgumentException("Счет обязателен", nameof(accountId));

        AccountId = accountId;
        CardType = type;
        Last4Digits = fullNumber[^4 ..];
        ExpiryDate = expiryDate;
        Status = CardStatus.Active;
    }

    public void Block()
    {
        Status = CardStatus.Blocked;
        Touch();
    }
}
