using PaymentCore.Domain.Entities;

namespace PaymentCore.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Deposit_IncreaseasBalance()
    {
        var account = new Account(Guid.NewGuid(), "RUB");

        account.Deposit(1000);

        Assert.Equal(1000, account.Balance);
    }
}