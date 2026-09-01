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

        var candidate = new PaymentIntent(
            request.IdempotencyKey,
            request.SenderMobileNumber,
            request.RecipientQuery,
            request.AmountInr);

        var (intent, isNew) = await _store.ReserveAsync(request.IdempotencyKey, candidate, cancellationToken);
        if (!isNew)
            return intent; // Another call already owns this idempotency key — never process it twice.

        try
        {
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
        catch (Exception) when (intent.Status == PaymentStatus.Pending)
        {
            // An idempotency key is reserved the moment ReserveAsync returns IsNew: true — if the
            // request is then cancelled, or the resolver/provider throws, the intent must still
            // reach a terminal state. Otherwise it's stuck Pending forever and a retry with the
            // same key returns that same stuck intent instead of ever progressing.
            //
            // Marking it Failed is a deliberate simplification for this mock/prototype stage: it
            // never retries automatically, which is the safe choice (retrying an interrupted call
            // whose real-world outcome is unknown risks a double payment). A production
            // implementation should instead query the PSP for the transaction's actual status
            // before deciding, rather than assuming failure.
            intent.MarkFailed("Payment processing was interrupted before it could complete.");
            await _store.SaveAsync(intent, CancellationToken.None);
            throw;
        }
    }

    public Task<PaymentIntent?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        _store.FindByIdAsync(id, cancellationToken);
}
