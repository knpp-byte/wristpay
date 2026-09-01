using WristPay.Domain;

namespace WristPay.Application;

public interface IPaymentIntentStore
{
    Task<PaymentIntent?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task<PaymentIntent?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveAsync(PaymentIntent intent, CancellationToken cancellationToken);
}
