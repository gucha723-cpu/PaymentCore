namespace PaymentCore.Application.Cards;

public interface ICardService
{
    public Task<IssuedCardResponse?> IssueDebitAsync(Guid accountId);
    
        
    
}