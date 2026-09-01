using System.Collections.Concurrent;
using WristPay.Application;
using WristPay.Domain;

namespace WristPay.Infrastructure;

/// <summary>
/// Process-memory implementation of <see cref="IPaymentIntentStore"/> — enough to prove the
/// end-to-end flow and idempotency behavior locally. Replace with an EF Core + PostgreSQL-backed
/// store before anything touches real transactions; state here is lost on restart and never shared
/// across instances.
/// </summary>
public sealed class InMemoryPaymentIntentStore : IPaymentIntentStore
{
    private readonly ConcurrentDictionary<Guid, PaymentIntent> _byId = new();
    private readonly ConcurrentDictionary<string, Guid> _idByIdempotencyKey = new();

    public Task<PaymentIntent?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        if (_idByIdempotencyKey.TryGetValue(idempotencyKey, out var id) && _byId.TryGetValue(id, out var intent))
            return Task.FromResult<PaymentIntent?>(intent);

        return Task.FromResult<PaymentIntent?>(null);
    }

    public Task<PaymentIntent?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public Task SaveAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        _byId[intent.Id] = intent;
        _idByIdempotencyKey[intent.IdempotencyKey] = intent.Id;
        return Task.CompletedTask;
    }
}
