using WristPay.Application;
using WristPay.Domain;

namespace WristPay.Infrastructure;

/// <summary>
/// Process-memory implementation of <see cref="IPaymentIntentStore"/> — enough to prove the
/// end-to-end flow and idempotency behavior locally. Replace with an EF Core + PostgreSQL-backed
/// store (unique constraint on idempotency key, atomic INSERT ... ON CONFLICT) before anything
/// touches real transactions; state here is lost on restart and never shared across instances.
///
/// Uses a plain lock rather than lock-free ConcurrentDictionary tricks: this class exists to be
/// obviously correct and short-lived, not to be a high-throughput production store.
/// </summary>
public sealed class InMemoryPaymentIntentStore : IPaymentIntentStore
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, PaymentIntent> _byId = new();
    private readonly Dictionary<string, Guid> _idByIdempotencyKey = new();

    public Task<(PaymentIntent Intent, bool IsNew)> ReserveAsync(
        string idempotencyKey, PaymentIntent candidate, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_idByIdempotencyKey.TryGetValue(idempotencyKey, out var existingId))
                return Task.FromResult((_byId[existingId], false));

            _byId[candidate.Id] = candidate;
            _idByIdempotencyKey[idempotencyKey] = candidate.Id;
            return Task.FromResult((candidate, true));
        }
    }

    public Task<PaymentIntent?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (_gate)
            return Task.FromResult(_byId.GetValueOrDefault(id));
    }

    public Task SaveAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        lock (_gate)
            _byId[intent.Id] = intent;
        return Task.CompletedTask;
    }
}
