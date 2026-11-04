
namespace PaymentCore.Application.Users;
public interface IUserService
{
    Task<Guid> CreateAsync(CreateUserRequest request);
}