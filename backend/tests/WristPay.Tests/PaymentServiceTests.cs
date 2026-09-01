using WristPay.Application;
using WristPay.Domain;
using WristPay.Infrastructure;
using Xunit;

namespace WristPay.Tests;

public sealed class PaymentServiceTests
{
    private static PaymentService CreateService(TransactionLimits? limits = null) => new(
        new InMemoryPaymentIntentStore(),
        new MockRecipientResolver(),
        new MockPaymentProvider(),
        limits ?? new TransactionLimits());

    [Fact]
    public async Task SendAsync_resolves_recipient_and_succeeds()
    {
        var service = CreateService();

        var intent = await service.SendAsync(
            new SendPaymentRequest("key-1", "9000000001", "9876543210", 500m),
            CancellationToken.None);

        Assert.Equal(PaymentStatus.Succeeded, intent.Status);
        Assert.Equal("Rahul Sharma", intent.Recipient?.DisplayName);
        Assert.NotNull(intent.ProviderReference);
    }

    [Fact]
    public async Task SendAsync_same_idempotency_key_does_not_reprocess()
    {
        var service = CreateService();
        var request = new SendPaymentRequest("key-2", "9000000001", "9876543210", 500m);

        var first = await service.SendAsync(request, CancellationToken.None);
        var second = await service.SendAsync(request, CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(first.ProviderReference, second.ProviderReference);
    }

    [Fact]
    public async Task SendAsync_unknown_recipient_is_reported_without_calling_provider()
    {
        var service = CreateService();

        var intent = await service.SendAsync(
            new SendPaymentRequest("key-3", "9000000001", "0000000000-not-in-directory", 500m),
            CancellationToken.None);

        Assert.Equal(PaymentStatus.RecipientNotFound, intent.Status);
        Assert.Null(intent.ProviderReference);
    }

    [Fact]
    public async Task SendAsync_amount_over_limit_is_rejected_before_recipient_lookup()
    {
        var service = CreateService(new TransactionLimits { PerTransactionInr = 5_000m });

        var intent = await service.SendAsync(
            new SendPaymentRequest("key-4", "9000000001", "9876543210", 5_001m),
            CancellationToken.None);

        Assert.Equal(PaymentStatus.LimitExceeded, intent.Status);
        Assert.Null(intent.Recipient);
    }

    [Fact]
    public async Task SendAsync_provider_decline_is_reported_as_failed()
    {
        var service = CreateService();

        var intent = await service.SendAsync(
            new SendPaymentRequest("key-5", "9000000001", "0000000000", 100m),
            CancellationToken.None);

        Assert.Equal(PaymentStatus.Failed, intent.Status);
        Assert.NotNull(intent.Recipient);
        Assert.NotNull(intent.FailureReason);
    }

    [Fact]
    public async Task SendAsync_requires_idempotency_key()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.SendAsync(
            new SendPaymentRequest("", "9000000001", "9876543210", 100m),
            CancellationToken.None));
    }
}
