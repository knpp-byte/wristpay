using WristPay.Application;
using WristPay.Domain;

namespace WristPay.Infrastructure;

/// <summary>
/// Stands in for a real PSP-bank/NPCI UPI integration during prototyping. Never wired into a
/// production build — swap in a real <see cref="IPaymentProvider"/> once a sponsor bank/PSP
/// relationship exists. To exercise failure handling, send to mobile number "0000000000", which this
/// mock always declines.
/// </summary>
public sealed class MockPaymentProvider : IPaymentProvider
{
    public async Task<PaymentProviderResult> InitiateAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        // Simulate the round trip to a PSP bank / NPCI without pretending it's instantaneous.
        await Task.Delay(150, cancellationToken);

        if (intent.Recipient?.MobileNumber == "0000000000")
            return new PaymentProviderResult(false, null, "Mock PSP declined the transaction.");

        var reference = $"MOCK-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{intent.Id.ToString()[..8]}";
        return new PaymentProviderResult(true, reference, null);
    }
}
