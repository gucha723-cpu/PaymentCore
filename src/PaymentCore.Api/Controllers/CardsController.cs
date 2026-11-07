using Microsoft.AspNetCore.Mvc;
using PaymentCore.Application.Cards;

namespace PaymentCore.Api.Controllers;

[ApiController]
[Route("accounts/{accountId}/cards")]
public class CardsController : ControllerBase
{
    private readonly ICardService _card;

    public CardsController(ICardService card)
    {
        _card = card;
    }

    [HttpPost]
    public async Task<IActionResult> IssueDebitAsync(Guid accountId)
    {
        var card = await _card.IssueDebitAsync(accountId);

        if(card == null)
        {
            return NotFound();
        }

        return Ok(card);
    }
}