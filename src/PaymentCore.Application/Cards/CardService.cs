using PaymentCore.Application.Abstractions;
using PaymentCore.Domain.Entities;
using PaymentCore.Domain.Services;

namespace PaymentCore.Application.Cards;

public class CardService : ICardService
{
    private readonly IAppDbContext _db;

    public CardService(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<IssuedCardResponse?> IssueDebitAsync(Guid accountId)
    {
        var account = await _db.Accounts.FindAsync(accountId);
        if(account == null)
        {
            return null;
        }

        var cardNumber = CardNumberGenerator.Generate();
        var cardCvv = CardNumberGenerator.GenerateCvv();
        DateOnly expiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(4);
        var debitCard = new DebitCard(accountId, cardNumber, expiryDate);

        _db.Cards.Add(debitCard);
        await _db.SaveChangesAsync();

        return new IssuedCardResponse(debitCard.Id, cardNumber,cardCvv, expiryDate);
    }
}