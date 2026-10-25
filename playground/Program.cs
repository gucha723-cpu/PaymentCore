using PaymentCore.Domain.Entities;

var user = new User("Алекс", "Иванов", "alex@example.com");
var account = new Account(user.Id, "RUB");

var entries = new List<LedgerEntry>();

account.Deposit(1000);
entries.Add(new LedgerEntry(account.Id, 1000, "Пополнение"));

var userTwo = new User("Борис", "Ященко", "Borya@gmail.com");
var accountTwo = new Account(userTwo.Id, "RUB");

account.Withdraw(300);
entries.Add(new LedgerEntry(account.Id, -300, "Перевод Борису"));
accountTwo.Deposit(300);
entries.Add(new LedgerEntry(accountTwo.Id, 300, "Перевод от Алекс"));


foreach(var entry in entries)
{
    Console.WriteLine($"{entry.Description}:  {entry.Amount}");
}

Console.WriteLine($"Баланс: {account.Balance}");
Console.WriteLine($"Баланс: {accountTwo.Balance}");

Console.WriteLine($"Сумма проводок: {entries.Sum(e => e.Amount)}");
Console.WriteLine($"Сумма балансов: {account.Balance + accountTwo.Balance}");