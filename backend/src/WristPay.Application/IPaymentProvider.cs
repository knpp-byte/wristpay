using WristPay.Domain;

namespace WristPay.Application;

public sealed record PaymentProviderResult(bool Success, string? ProviderReference, string? FailureReason);

/// <summary>
/// Abstraction over "whoever actually moves the money" — today a <c>MockPaymentProvider</c>, later a
/// real PSP-bank/NPCI-certified UPI integration. The rest of the app must never assume which one is
/// wired up, and must never see or store a UPI PIN regardless of implementation.
/// </summary>
public interface IPaymentProvider
{
    Task<PaymentProviderResult> InitiateAsync(PaymentIntent intent, CancellationToken cancellationToken);
}
