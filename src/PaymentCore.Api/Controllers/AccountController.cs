using Microsoft.AspNetCore.Mvc;
using PaymentCore.Application.Accounts;

namespace PaymentCore.Api.Controllers;

[ApiController]
[Route("accounts")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _service;

    public AccountController(IAccountService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateAccountRequest request)
    {
        var id = await _service.CreateAsync(request);
        return Ok(new { Id = id });
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> DepositAsync(Guid id,DepositRequest request)
    {
        var account = await _service.DepositAsync(id, request);
        if (account == null)
        {
            return NotFound();
        }

        return Ok(new { Balance = account });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var account = await _service.GetAsync(id);
        if (account == null)
        {
            return NotFound();
        }

        return Ok(account);
    }
}