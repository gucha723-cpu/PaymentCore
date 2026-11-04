using Microsoft.AspNetCore.Mvc;
using PaymentCore.Application.Transfers;

namespace PaymentCore.Api.Controllers;

[ApiController]
[Route("transfers")]
public class TransferController : ControllerBase
{
    private readonly ITransferService _transfer;

    public TransferController(ITransferService transfer)
    {
        _transfer = transfer;
    }

    [HttpPost]
    public async Task<IActionResult> TransferAsync(TransferRequest request)
    {
        var transfer = await _transfer.TransferAsync(request);
        if (transfer == null)
        {
            return NotFound();
        }

        return Ok(transfer);
    }
}