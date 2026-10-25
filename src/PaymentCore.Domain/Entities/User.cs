namespace PaymentCore.Domain.Entities;
public class User : BaseEntity
{
    public string FirstName {get;private set;} = string.Empty;
    public string MiddleName {get;private set;} = string.Empty;
    public string LastName {get;private set;} = string.Empty;
    public string Email {get;private set;} = string.Empty;

    private User(){}

    public User(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        throw new ArgumentException("Заполните имя", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
        throw new ArgumentException("Заполните фамилию", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        throw new ArgumentException("Введите корректный email", nameof(email));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
}