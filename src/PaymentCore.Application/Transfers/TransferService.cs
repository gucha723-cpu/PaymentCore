using PaymentCore.Application.Abstractions;
using PaymentCore.Domain.Entities;

namespace PaymentCore.Application.Transfers;

public class TransferService : ITransferService
{
    private readonly IAppDbContext _db;

    public TransferService(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<TransferResponse?> TransferAsync(TransferRequest request)
    {
        if (request.FromAccountId == request.ToAccountId)
    {
        throw new ArgumentException("Нельзя переводить деньги на тот же счет");
    }
    var from = await _db.Accounts.FindAsync(request.FromAccountId);
    if (from == null)
    {
        throw new KeyNotFoundException("Счет отправителя не найден");
    }

    var to = await _db.Accounts.FindAsync(request.ToAccountId);
    if (to == null)
    {
        throw new KeyNotFoundException("Счет получателя не найден");
    }

    from.Withdraw(request.Amount);
    _db.LedgerEntries.Add(new LedgerEntry(from.Id, -request.Amount, "Перевод"));

    to.Deposit(request.Amount);
    _db.LedgerEntries.Add(new LedgerEntry(to.Id, request.Amount, "Перевод"));

    await _db.SaveChangesAsync();

    return new TransferResponse(from.Balance, to.Balance);
    }
}