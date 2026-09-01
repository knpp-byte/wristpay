using WristPay.Domain;

namespace WristPay.Application;

/// <summary>
/// Orchestrates a wrist payment end to end: idempotency check, limit check, recipient resolution,
/// then handoff to the payment provider. This is the one place that sequence is allowed to live —
/// controllers and providers should stay thin around it.
/// </summary>
public sealed class PaymentService
{
    private readonly IPaymentIntentStore _store;
    private readonly IRecipientResolver _recipientResolver;
    private readonly IPaymentProvider _paymentProvider;
    private readonly TransactionLimits _limits;

    public PaymentService(
        IPaymentIntentStore store,
        IRecipientResolver recipientResolver,
        IPaymentProvider paymentProvider,
        TransactionLimits limits)
    {
        _store = store;
        _recipientResolver = recipientResolver;
        _paymentProvider = paymentProvider;
        _limits = limits;
    }

    public async Task<PaymentIntent> SendAsync(SendPaymentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new ArgumentException("An idempotency key is required.", nameof(request));

        if (request.AmountInr <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(request));

        var existing = await _store.FindByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing is not null)
            return existing;

        var intent = new PaymentIntent(
            request.IdempotencyKey,
            request.SenderMobileNumber,
            request.RecipientQuery,
            request.AmountInr);

        if (_limits.ExceedsPerTransaction(request.AmountInr))
        {
            intent.MarkLimitExceeded(
                $"Amount exceeds the current per-transaction limit of ₹{_limits.PerTransactionInr}.");
            await _store.SaveAsync(intent, cancellationToken);
            return intent;
        }

        var recipient = await _recipientResolver.ResolveAsync(request.RecipientQuery, cancellationToken);
        if (recipient is null)
        {
            intent.MarkRecipientNotFound();
            await _store.SaveAsync(intent, cancellationToken);
            return intent;
        }

        intent.AttachRecipient(recipient);
        await _store.SaveAsync(intent, cancellationToken);

        var result = await _paymentProvider.InitiateAsync(intent, cancellationToken);

        if (result.Success)
            intent.MarkSucceeded(result.ProviderReference ?? "unknown");
        else
            intent.MarkFailed(result.FailureReason ?? "Payment provider declined the transaction.");

        await _store.SaveAsync(intent, cancellationToken);
        return intent;
    }

    public Task<PaymentIntent?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        _store.FindByIdAsync(id, cancellationToken);
}
