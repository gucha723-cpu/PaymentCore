using System.Data;
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
        
        Assert.NotNull(actualFrom);

        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        Assert.NotNull(actualTo);

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

        Assert.NotNull(actualFrom);

        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        Assert.NotNull(actualTo);

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

        Assert.NotNull(actualFrom);

        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        Assert.NotNull(actualTo);

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

        Assert.NotNull(actualBalance);

        Assert.Equal(100, actualBalance.Balance);
    }

    [Fact]
    public async Task SenderAccountNotFound()
    {
        await InitializeDatabaseAsync();
        await using var db = new AppDbContext(_options);

        var user = CreateUser("Юрий", "Юрьев", $"yuri{Guid.NewGuid()}@test.local");
        var to = CreateAccount(user.Id, "RUB");

        db.Users.Add(user);
        db.Accounts.Add(to);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(Guid.NewGuid(), to.Id, 100);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.TransferAsync(request));

        await using var newDb = new AppDbContext(_options);

        var actualBalance = await newDb.Accounts.FindAsync(to.Id);

        Assert.NotNull(actualBalance);
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == to.Id));
        Assert.Equal(0, actualBalance.Balance);
    }

    [Fact]
    public async Task ReceiverAccountNotFound()
    {
        await InitializeDatabaseAsync();
        await using var db = new AppDbContext(_options);
        
        var user = CreateUser("Степан", "Степанов", $"stepan{Guid.NewGuid()}@test.local");
        var from = CreateAccount(user.Id, "RUB");
        from.Deposit(100);

        db.Users.Add(user);
        db.Accounts.Add(from);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(from.Id, Guid.NewGuid(), 50);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.TransferAsync(request));

        await using var newDb = new AppDbContext(_options);
        
        var actualBalance = await newDb.Accounts.FindAsync(from.Id);

        Assert.NotNull(actualBalance);

        Assert.Equal(100, actualBalance.Balance);
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == from.Id));
    }

    [Fact]
    public async Task TransferAsync_ZeroAmount_ThrowsArgumentExceptionAndDoesNotChangeData()
    {
        await InitializeDatabaseAsync();
        await using var db = new AppDbContext(_options);

        var user1 = CreateUser("Андрей", "Андреев", $"andrey{Guid.NewGuid()}@test.local");
        var user2 = CreateUser("Тимур", "Тимуров", $"timur{Guid.NewGuid()}@test.local");
        var from = CreateAccount(user1.Id, "RUB");
        var to = CreateAccount(user2.Id, "RUB");
        from.Deposit(100);

        db.Users.Add(user1);
        db.Users.Add(user2);
        db.Accounts.Add(from);
        db.Accounts.Add(to);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(from.Id, to.Id, 0);

        await Assert.ThrowsAsync<ArgumentException>(() => service.TransferAsync(request));

        await using var newDb = new AppDbContext(_options);

        var actualFrom = await newDb.Accounts.FindAsync(from.Id);

        Assert.NotNull(actualFrom);

        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        Assert.NotNull(actualTo);

        Assert.Equal(100, actualFrom.Balance);
        Assert.Equal(0, actualTo.Balance);
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == actualFrom.Id));
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == actualTo.Id));
    }

    [Fact]
    public async Task TransferAsync_NegativeAmount_ThrowsArgumentExceptionAndDoesNotChangeData()
    {
        await InitializeDatabaseAsync();
        await using var db = new AppDbContext(_options);

        var user1 = CreateUser("Антон", "Антонов", $"anton{Guid.NewGuid()}@test.local");
        var user2 = CreateUser("Артем", "Артемов", $"artem{Guid.NewGuid()}@test.local");
        var from = CreateAccount(user1.Id, "RUB");
        var to = CreateAccount(user2.Id, "RUB");
        from.Deposit(100);

        db.Users.Add(user1);
        db.Users.Add(user2);
        db.Accounts.Add(from);
        db.Accounts.Add(to);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(from.Id, to.Id, -50);

        await Assert.ThrowsAsync<ArgumentException>(() => service.TransferAsync(request));

        await using var newDb = new AppDbContext(_options);

        var actualFrom = await newDb.Accounts.FindAsync(from.Id);

        Assert.NotNull(actualFrom);
        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        Assert.NotNull(actualTo);

        Assert.Equal(100, actualFrom.Balance);
        Assert.Equal(0, actualTo.Balance);
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == actualFrom.Id));
        Assert.False(await newDb.LedgerEntries.AnyAsync(l => l.AccountId == actualTo.Id));
    }

    [Fact]
    public async Task TransferAsync_ExistingReceiverBalance_UpdatesBalancesCorrectly()
    {
        await InitializeDatabaseAsync();
        await using var db = new AppDbContext(_options);

        var user1 = CreateUser("Роман", "Романов", $"roman{Guid.NewGuid()}@test.local");
        var user2 = CreateUser("Даниил", "Даниилов", $"daniel{Guid.NewGuid()}@test.local");
        var from = CreateAccount(user1.Id, "RUB");
        var to = CreateAccount(user2.Id, "RUB");
        from.Deposit(500);
        to.Deposit(300);

        db.Users.Add(user1);
        db.Users.Add(user2);
        db.Accounts.Add(from);
        db.Accounts.Add(to);
        await db.SaveChangesAsync();

        var service = new TransferService(db);
        var request = new TransferRequest(from.Id, to.Id, 300);

        await service.TransferAsync(request);

        await using var newDb = new AppDbContext(_options);

        var actualFrom = await newDb.Accounts.FindAsync(from.Id);

        Assert.NotNull(actualFrom);
        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        Assert.NotNull(actualTo);

        Assert.Equal(200, actualFrom.Balance);
        Assert.Equal(600, actualTo.Balance);
        
        var entriesFrom = await newDb.LedgerEntries.Where(l => l.AccountId == actualFrom.Id).ToListAsync();
        var entriesTo = await newDb.LedgerEntries.Where(l => l.AccountId == actualTo.Id).ToListAsync();

       Assert.Single(entriesFrom);
       Assert.Single(entriesTo);

       Assert.Equal(-300, entriesFrom[0].Amount);
       Assert.Equal(300, entriesTo[0].Amount);
    }

    [Fact]
    public async Task TransferAsync_ConcurrentTransfers_OnlyOneSucceeds()
    {
        await InitializeDatabaseAsync();
        
        var user1 = CreateUser("Геннадий", "Геннадьев", $"gena{Guid.NewGuid()}@test.local");
        var user2 = CreateUser("Дмитрий", "Дмитрьев", $"dima{Guid.NewGuid()}@test.local");
        var from = CreateAccount(user1.Id, "RUB");
        var to = CreateAccount(user2.Id, "RUB");
        from.Deposit(1000);

        await using var db1 = new AppDbContext(_options);
        await using var db2 = new AppDbContext(_options);

        db1.Users.Add(user1);
        db1.Users.Add(user2);
        db1.Accounts.Add(from);
        db1.Accounts.Add(to);
        await db1.SaveChangesAsync();

        var service1 = new TransferService(db1);
        var request1 = new TransferRequest(from.Id, to.Id, 800);

        var service2 = new TransferService(db2);
        var request2 = new TransferRequest(from.Id, to.Id, 800);

        var from2 = await db2.Accounts.FindAsync(from.Id);

        Assert.NotNull(from2);

        var to2 = await db2.Accounts.FindAsync(to.Id);

        Assert.NotNull(to2);

        await service1.TransferAsync(request1);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service2.TransferAsync(request2));

        await using var newDb = new AppDbContext(_options);

        var actualFrom = await newDb.Accounts.FindAsync(from.Id);

        Assert.NotNull(actualFrom);

        var actualTo = await newDb.Accounts.FindAsync(to.Id);

        Assert.NotNull(actualTo);

        Assert.Equal(200,actualFrom.Balance);
        Assert.Equal(800,actualTo.Balance);

        var entriesFrom = await newDb.LedgerEntries.Where(l => l.AccountId == actualFrom.Id).ToListAsync();
        var entriesTo = await newDb.LedgerEntries.Where(l => l.AccountId == actualTo.Id).ToListAsync();

        Assert.Single(entriesFrom);
        Assert.Single(entriesTo);

        Assert.Equal(-800, entriesFrom[0].Amount);
        Assert.Equal(800, entriesTo[0].Amount);
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