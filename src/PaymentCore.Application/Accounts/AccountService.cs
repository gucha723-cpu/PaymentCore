using PaymentCore.Application.Abstractions;
using PaymentCore.Domain.Entities;

namespace PaymentCore.Application.Accounts;
public class AccountService : IAccountService
{
    private readonly IAppDbContext _db;
    public AccountService(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<Guid> CreateAsync(CreateAccountRequest request)
    {
        Account account = new Account(request.UserId, request.Currency);
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return(account.Id);
    }

    public async Task<decimal?> DepositAsync(Guid id, DepositRequest request)
    {
        var account = await _db.Accounts.FindAsync(id);
   if (account == null)
    {
        return null;
    } 
    account.Deposit(request.Amount);
    _db.LedgerEntries.Add(new LedgerEntry(account.Id, request.Amount, "Пополнение"));
    await _db.SaveChangesAsync();
    return (account.Balance);
    } 

    public async Task<AccountResponse?> GetAsync(Guid id)
    {
         var account = await _db.Accounts.FindAsync(id);
    if (account == null)
    {
        return null;
    }
    return (new AccountResponse(account.Id, account.Currency, account.Balance));
    }
}

