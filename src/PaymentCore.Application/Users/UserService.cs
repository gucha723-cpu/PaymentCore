using PaymentCore.Application.Abstractions;
using PaymentCore.Domain.Entities;

namespace PaymentCore.Application.Users;

public class UserService : IUserService
{
    private readonly IAppDbContext _db;

    public UserService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> CreateAsync(CreateUserRequest request)
    {
    User user = new User(request.FirstName, request.LastName, request.Email);
    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    return user.Id;
    }
}