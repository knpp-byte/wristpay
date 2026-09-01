using WristPay.Application;
using WristPay.Domain;

namespace WristPay.Infrastructure;

/// <summary>
/// A fixed directory standing in for the real UPI recipient-resolution lookup. Replace with a call
/// into the PSP bank's UPI Number / UPI ID resolution API once that access exists.
/// </summary>
public sealed class MockRecipientResolver : IRecipientResolver
{
    private static readonly Dictionary<string, Recipient> Directory = new()
    {
        ["9876543210"] = new Recipient("Rahul Sharma", "9876543210", "rahul@okhdfcbank"),
        ["rahul@okhdfcbank"] = new Recipient("Rahul Sharma", "9876543210", "rahul@okhdfcbank"),
        ["9123456780"] = new Recipient("Kishore P.", "9123456780", "kishore@okhdfcbank"),
        ["kishore@okhdfcbank"] = new Recipient("Kishore P.", "9123456780", "kishore@okhdfcbank"),

        // Resolves successfully but MockPaymentProvider always declines this mobile number —
        // lets tests exercise the "recipient found, provider failed" path deliberately.
        ["0000000000"] = new Recipient("Always-Declines Test Recipient", "0000000000", "decline@mockpsp"),
    };

    public Task<Recipient?> ResolveAsync(string query, CancellationToken cancellationToken) =>
        Task.FromResult(Directory.GetValueOrDefault(query.Trim()));
}
