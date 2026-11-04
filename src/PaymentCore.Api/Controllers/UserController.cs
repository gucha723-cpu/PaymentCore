using Microsoft.AspNetCore.Mvc;
using PaymentCore.Application.Users;

namespace PaymentCore.Api.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var id = await _service.CreateAsync(request);
        return Ok(new { Id = id });
    }
}