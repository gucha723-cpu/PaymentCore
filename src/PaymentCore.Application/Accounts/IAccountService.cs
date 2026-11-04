using PaymentCore.Domain.Entities;

namespace PaymentCore.Application.Accounts;


public interface IAccountService
{
    public Task<Guid> CreateAsync(CreateAccountRequest request);
    public Task<decimal?> DepositAsync(Guid id, DepositRequest request);
    public Task<AccountResponse?> GetAsync(Guid id);
}