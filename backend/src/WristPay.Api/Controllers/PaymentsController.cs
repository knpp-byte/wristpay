using Microsoft.AspNetCore.Mvc;
using WristPay.Application;
using WristPay.Domain;

namespace WristPay.Api.Controllers;

public sealed record SendPaymentApiRequest(
    string IdempotencyKey,
    string SenderMobileNumber,
    string RecipientQuery,
    decimal AmountInr);

public sealed record PaymentIntentResponse(
    Guid Id,
    string Status,
    decimal AmountInr,
    string RecipientQuery,
    string? RecipientDisplayName,
    string? ProviderReference,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt)
{
    public static PaymentIntentResponse From(PaymentIntent intent) => new(
        intent.Id,
        intent.Status.ToString(),
        intent.AmountInr,
        intent.RecipientQuery,
        intent.Recipient?.DisplayName,
        intent.ProviderReference,
        intent.FailureReason,
        intent.CreatedAt,
        intent.CompletedAt);
}

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// The mock end-to-end flow this whole prototype exists to prove:
    /// mobile/UPI ID + amount in, resolved recipient + payment status out.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PaymentIntentResponse>> Send(
        [FromBody] SendPaymentApiRequest request, CancellationToken cancellationToken)
    {
        var intent = await _paymentService.SendAsync(
            new SendPaymentRequest(
                request.IdempotencyKey,
                request.SenderMobileNumber,
                request.RecipientQuery,
                request.AmountInr),
            cancellationToken);

        return Ok(PaymentIntentResponse.From(intent));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentIntentResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var intent = await _paymentService.GetAsync(id, cancellationToken);
        return intent is null ? NotFound() : Ok(PaymentIntentResponse.From(intent));
    }
}
