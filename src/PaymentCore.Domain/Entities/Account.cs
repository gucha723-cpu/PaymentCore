namespace PaymentCore.Domain.Entities;

public class Account : BaseEntity
{
    
    public User User{get;private set;} = null!;
    public Guid UserId{get;private set;}
    public string Currency{get; private set;} = string.Empty;
    public decimal Balance{get;private set;}

    private Account(){}
    public Account(Guid userId, string currency)
    {
        if (userId == Guid.Empty)
        throw new ArgumentException("Пользователь обязателен", nameof(userId));

        if(string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        throw new ArgumentException("Валюта: код из 3 букв, например RUB", nameof(currency));

        UserId = userId;
        Currency = currency.ToUpperInvariant();
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        throw new ArgumentException("Сумма должна быть положительной", nameof(amount));

        Balance += amount;
        Touch();
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
        throw new ArgumentException("Сумма должна быть положительной", nameof(amount));
        
        if (Balance < amount)
        throw new InvalidOperationException("Недостаточно средств");

        Balance -= amount;
        Touch();
    }
}