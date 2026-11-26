using Microsoft.EntityFrameworkCore;
using PaymentCore.Application.Transfers;
using PaymentCore.Domain.Entities;
using PaymentCore.Domain.Exceptions;
using PaymentCore.Infrastructure.Persistence;

namespace PaymentCore.IntegrationTests;

public class TransferServiceTests
{
    private readonly DbContextOptions<AppDbContext> _options;

    public TransferServiceTests()
    {
        var connectionString = Environment.GetEnvironmentVariable("PAYMENTCORE_TEST_CONNECTION");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        _options = optionsBuilder.Options;
    }

    private async Task InitializeDatabaseAsync()
    {
        await using var db = new AppDbContext(_options);

        await db.Database.MigrateAsync();
    }



    [Fact]
    public async Task TransferAsync_SuccessfulTransfer_UpdatesBalancesAndLedgerEntries()
    {
        await InitializeDatabaseAsync();

        await using var db = new AppDbContext(_options);

        var user1 = CreateUser("Ivan", "Ivanov", $"ivan{Guid.NewGuid()}@test.local");
        var user2 = CreateUser("Egor", "Egorov", $"egor{Guid.NewGuid()}@test.local");

        var from = CreateAccount(user1.Id, "RUB");
        var to = CreateAccount(user2.Id, "RUB");

        from.Deposit(1000);

        db.Users.Add(user1);
        db.Users.Add(user2);
        db.Accounts.Add(from);
        db.Accounts.Add(to);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(from.Id, to.Id, 300);

        var response = await service.TransferAsync(request);

        await using var newDb = new AppDbContext(_options);

        var actualFrom =  await newDb.Accounts.FindAsync(from.Id);        
        
        if(actualFrom == null)
        {
            throw new Exception("Счет отправителя не найден");
        }

        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        if(actualTo == null)
        {
            throw new Exception("Счет получателя не найден");
        }

        Assert.Equal(700, actualFrom.Balance);
        Assert.Equal(300, actualTo.Balance);

        var fromEntries = await newDb.LedgerEntries.Where(fe => fe.AccountId == actualFrom.Id).ToListAsync();
        var toEntries = await newDb.LedgerEntries.Where(te => te.AccountId == actualTo.Id).ToListAsync();

        var fromEntry = Assert.Single(fromEntries);
        var toEntry = Assert.Single(toEntries);

        Assert.Equal(-300, fromEntry.Amount);
        Assert.Equal(300, toEntry.Amount);
    }

    [Fact]
    public async Task TransferAsync_InsufficientFunds_DoesNotChangeBalancesOrCreateLedgerEntries()
    {

        await InitializeDatabaseAsync();
        await using var db = new AppDbContext(_options);

        var user1 = CreateUser("Алексей", "Алексеев", $"alexsey{Guid.NewGuid()}@test.local");
        var user2 = CreateUser("Сергей", "Сергеев", $"sergey{Guid.NewGuid()}@test.local");

        var from = CreateAccount(user1.Id, "RUB");
        var to = CreateAccount(user2.Id, "RUB");

        from.Deposit(100);

        db.Users.Add(user1);
        db.Users.Add(user2);
        db.Accounts.Add(from);
        db.Accounts.Add(to);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(from.Id, to.Id, 1000);

        await Assert.ThrowsAsync<InsufficientFundsException>(() => service.TransferAsync(request));

        await using var newDb = new AppDbContext(_options);

        var actualFrom = await newDb.Accounts.FindAsync(from.Id);

        if(actualFrom == null)
        {
            throw new Exception("Счет отправителя не найден");
        }

        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        if(actualTo == null)
        {
            throw new Exception("Счет получателя не найден");
        }

        Assert.Equal(100, actualFrom.Balance);
        Assert.Equal(0, actualTo.Balance);

        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == actualFrom.Id));
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == actualTo.Id));
    }

    [Fact]
    public async Task TransferAsync_DifferentCurrencies_DoesNotChangeBalancesOrCreateLedgerEntries()
    {
        await InitializeDatabaseAsync();
        await using var db = new AppDbContext(_options);

        var user1 = CreateUser("Максим", "Максимов", $"maxim{Guid.NewGuid()}@test.local");
        var user2 = CreateUser("Владимир", "Владимиров", $"vladimir{Guid.NewGuid()}@test.local");

        var from = CreateAccount(user1.Id, "RUB");
        var to = CreateAccount(user2.Id,"USD");

        from.Deposit(1000);

        db.Users.Add(user1);
        db.Users.Add(user2);
        db.Accounts.Add(from);
        db.Accounts.Add(to);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(from.Id, to.Id, 1000);

        await Assert.ThrowsAsync<DifferentCurrenciesException>(() => service.TransferAsync(request));

        await using var newDb = new AppDbContext(_options);

         var actualFrom = await newDb.Accounts.FindAsync(from.Id);

        if(actualFrom == null)
        {
            throw new Exception("Счет отправителя не найден");
        }

        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        if(actualTo == null)
        {
            throw new Exception("Счет получателя не найден");
        }

        Assert.Equal(1000, actualFrom.Balance);
        Assert.Equal(0, actualTo.Balance);

        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == actualFrom.Id));
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == actualTo.Id));
    }

    [Fact]
    public async Task TransferAsync_SameAccount_ThrowsArgumentException()
    {
        await InitializeDatabaseAsync();
        await using var db = new AppDbContext(_options);

        var user = CreateUser("Семен", "Семенов", $"semen{Guid.NewGuid()}@test.local");
        var account = CreateAccount(user.Id, "RUB");

        account.Deposit(100);

        db.Users.Add(user);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(account.Id, account.Id, 50);

        await Assert.ThrowsAsync<ArgumentException>(() => service.TransferAsync(request));

        await using var newDb = new AppDbContext(_options);
        
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == account.Id));

        var actualBalance = await newDb.Accounts.FindAsync(account.Id);

        if(actualBalance == null)
        {
            throw new Exception("Счет не найден");
        }

        Assert.Equal(100, actualBalance.Balance);
    }

    private User CreateUser(string firstName, string lastName, string email)
    {
        var user = new User(firstName, lastName, email);

        return user;
    }

    private Account CreateAccount(Guid userId, string currency)
    {
        var account = new Account(userId, currency);

        return account;
    }


}