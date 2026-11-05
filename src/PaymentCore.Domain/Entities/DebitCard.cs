using PaymentCore.Domain.Entities.Enums;

namespace PaymentCore.Domain.Entities;

public class DebitCard : Card
{
    private DebitCard(){}

    public DebitCard(Guid accountId, string fullNumber, DateOnly expiryDate) : base(accountId, CardType.Debit, fullNumber, expiryDate)
    {
        
    }
}