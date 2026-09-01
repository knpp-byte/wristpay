using Microsoft.AspNetCore.Mvc;
using WristPay.Application;
using WristPay.Domain;

namespace WristPay.Api.Controllers;

[ApiController]
[Route("api/recipients")]
public sealed class RecipientsController : ControllerBase
{
    private readonly IRecipientResolver _recipientResolver;

    public RecipientsController(IRecipientResolver recipientResolver)
    {
        _recipientResolver = recipientResolver;
    }

    /// <summary>
    /// The "type a mobile number, see a verified name before you confirm" step — no beneficiary
    /// needs to be pre-added. Used by the Watch UI before it shows the amount/confirm screen.
    /// </summary>
    [HttpGet("lookup")]
    public async Task<ActionResult<Recipient>> Lookup([FromQuery] string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("query is required.");

        var recipient = await _recipientResolver.ResolveAsync(query, cancellationToken);
        return recipient is null ? NotFound() : Ok(recipient);
    }
}
