namespace WristPay.Domain;

/// <summary>
/// A single requested wrist payment: "send ₹500 to 9876543210". One <see cref="IdempotencyKey"/>
/// maps to exactly one intent — retries of the same key must return the existing result rather than
/// initiating a second transfer.
/// </summary>
public sealed class PaymentIntent
{
    public Guid Id { get; }
    public string IdempotencyKey { get; }
    public string SenderMobileNumber { get; }
    public string RecipientQuery { get; }
    public decimal AmountInr { get; }
    public PaymentStatus Status { get; private set; }
    public Recipient? Recipient { get; private set; }
    public string? ProviderReference { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public PaymentIntent(string idempotencyKey, string senderMobileNumber, string recipientQuery, decimal amountInr)
    {
        Id = Guid.NewGuid();
        IdempotencyKey = idempotencyKey;
        SenderMobileNumber = senderMobileNumber;
        RecipientQuery = recipientQuery;
        AmountInr = amountInr;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AttachRecipient(Recipient recipient) => Recipient = recipient;

    public void MarkRecipientNotFound()
    {
        Status = PaymentStatus.RecipientNotFound;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkLimitExceeded(string reason)
    {
        Status = PaymentStatus.LimitExceeded;
        FailureReason = reason;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSucceeded(string providerReference)
    {
        Status = PaymentStatus.Succeeded;
        ProviderReference = providerReference;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
