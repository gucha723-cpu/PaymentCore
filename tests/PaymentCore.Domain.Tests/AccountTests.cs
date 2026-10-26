using PaymentCore.Domain.Entities;

namespace PaymentCore.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Deposit_IncreasesBalance()
    {
        var account = new Account(Guid.NewGuid(), "RUB");

        account.Deposit(1000);

        Assert.Equal(1000, account.Balance);
    }

    [Fact]
    public void Withdraw_DecreasesBalance()
    {
        var account = new Account(Guid.NewGuid(), "RUB");

        account.Deposit(1000);

        account.Withdraw(300);

        Assert.Equal(700, account.Balance);
    }

    [Fact]
    public void Withdraw_MoreThanBalance_Throws()
    {
        var account = new Account(Guid.NewGuid(), "RUB");

        account.Deposit(100);

        Assert.Throws<InvalidOperationException>(() => account.Withdraw(500));
    }
}