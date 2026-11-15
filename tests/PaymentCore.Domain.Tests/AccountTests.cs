using PaymentCore.Domain.Entities;
using PaymentCore.Domain.Exceptions;

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

        Assert.Throws<InsufficientFundsException>(() => account.Withdraw(500));
    }

    [Fact]
    public void Deposit_IncreasesVersion()
    {
        var account = new Account(Guid.NewGuid(), "RUB");

        account.Deposit(1000);

        Assert.Equal(1, account.Version);    
    }

    [Fact]
    public void Withdraw_IncreasesVersion()
    {
        var account = new Account(Guid.NewGuid(), "RUB");

        account.Deposit(1000);

        account.Withdraw(500);

        Assert.Equal(2, account.Version);
    }

    [Fact]
    public void Withdraw_FailedWithdrawal_DoesNotIncreaseVersion()
    {
        var account = new Account(Guid.NewGuid(), "RUB");

        account.Deposit(100);

        Assert.Throws<InsufficientFundsException>(() => account.Withdraw(1000));

        Assert.Equal(1, account.Version);

        Assert.Equal(100, account.Balance);
    }
}