namespace PaymentCore.Application.Accounts;
public record CreateAccountRequest(Guid UserId, string Currency);