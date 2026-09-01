using WristPay.Domain;

namespace WristPay.Application;

public interface IPaymentIntentStore
{
    /// <summary>
    /// Atomically claims <paramref name="idempotencyKey"/>. If no intent is registered under that
    /// key yet, <paramref name="candidate"/> becomes the one stored and is returned with
    /// <c>IsNew: true</c>. If another caller already claimed the key first — even concurrently —
    /// that caller's intent is returned instead, with <c>IsNew: false</c>, and
    /// <paramref name="candidate"/> must be discarded rather than processed further. This is what
    /// makes retries of the same key safe against duplicate payment initiation.
    /// </summary>
    Task<(PaymentIntent Intent, bool IsNew)> ReserveAsync(
        string idempotencyKey, PaymentIntent candidate, CancellationToken cancellationToken);

    Task<PaymentIntent?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveAsync(PaymentIntent intent, CancellationToken cancellationToken);
}
