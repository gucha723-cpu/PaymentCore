namespace PaymentCore.Application.Cards;

public record IssuedCardResponse(Guid CardId, string CardNumber, string Cvv, DateOnly ExpiryDate);