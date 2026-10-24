namespace PaymentCore.Domain.Entities;
public class LedgerEntry : BaseEntity
{
    public Guid AccountId {get; private set;}
    public Account Account{get; private set;} = null!;
    public decimal Amount{get; private set;}
    public string Description{get; private set;} = string.Empty;

    private LedgerEntry(){}
    public LedgerEntry(Guid accountId, decimal amount, string? description = null)
    {
        if (accountId == Guid.Empty)
        throw new ArgumentException("Счет обязателен", nameof(accountId));

        if (amount == 0)
        throw new ArgumentException("Сумма проводки не может быть нулевой", nameof(amount));

        AccountId = accountId;
        Amount = amount;
        Description = description ?? string.Empty;
    }
}