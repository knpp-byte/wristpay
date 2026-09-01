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

    public Task<(PaymentIntent Intent, bool IsNew)> ReserveAsync(
        string idempotencyKey, PaymentIntent candidate, CancellationToken cancellationToken)
    {
        // Store the candidate under its own id first, so that whichever id "wins" the
        // GetOrAdd below is already guaranteed to be present in _byId for every other thread
        // that reads it — closing the reserve/lookup race a plain check-then-save has.
        _byId[candidate.Id] = candidate;

        var winningId = _idByIdempotencyKey.GetOrAdd(idempotencyKey, candidate.Id);
        if (winningId == candidate.Id)
            return Task.FromResult((candidate, true));

        return Task.FromResult((_byId[winningId], false));
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
