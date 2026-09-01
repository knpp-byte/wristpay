using Microsoft.Extensions.DependencyInjection;
using WristPay.Application;
using WristPay.Domain;

namespace WristPay.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Wires up today's mock implementations. Swapping to real PostgreSQL storage and a real PSP
    /// provider later means changing the registrations here — the rest of the app depends only on
    /// the interfaces.
    /// </summary>
    public static IServiceCollection AddWristPayInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentIntentStore, InMemoryPaymentIntentStore>();
        services.AddSingleton<IRecipientResolver, MockRecipientResolver>();
        services.AddSingleton<IPaymentProvider, MockPaymentProvider>();
        services.AddSingleton(new TransactionLimits());
        services.AddScoped<PaymentService>();
        return services;
    }
}
