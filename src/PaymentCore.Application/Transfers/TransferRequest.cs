namespace PaymentCore.Application.Transfers;
public record TransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount);