namespace PaymentCore.Application.Transfers;

public interface ITransferService
{
    public Task<TransferResponse?> TransferAsync(TransferRequest request);
}
