namespace PaymentCore.Domain.Entities;
public class User : BaseEntity
{
    public string FirstName {get;private set;} = string.Empty;
    public string MiddleName {get;private set;} = string.Empty;
    public string LastName {get;private set;} = string.Empty;

    public string Email {get;private set;} = string.Empty;
}