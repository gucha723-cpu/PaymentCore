using Microsoft.EntityFrameworkCore;
using PaymentCore.Application.Transfers;
using PaymentCore.Domain.Entities;
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

        var user1 = CreateUser("Ivan", "Ivanov", "ivan@test.local");
        var user2 = CreateUser("Egor", "Egorov", "egor@test.local");

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