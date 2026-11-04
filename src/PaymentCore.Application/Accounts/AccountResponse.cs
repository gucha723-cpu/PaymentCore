namespace PaymentCore.Application.Accounts;
public record AccountResponse(Guid Id, string Currency, decimal Balance);