namespace WristPay.Domain;

/// <summary>
/// Per-transaction and monthly caps. Defaults mirror NPCI's October 2025 UPI Circle / IoT-device
/// circular (₹5,000 per transaction, ₹15,000 per month per delegation) for the currently-permitted
/// P2M wearable route. These are NOT hard-coded rules of UPI itself — once a PSP bank/NPCI approve a
/// specific authentication and limit model for this app, replace these values from that agreement
/// rather than assuming they still apply.
/// </summary>
public sealed class TransactionLimits
{
    public decimal PerTransactionInr { get; init; } = 5_000m;
    public decimal PerMonthInr { get; init; } = 15_000m;

    public bool ExceedsPerTransaction(decimal amountInr) => amountInr > PerTransactionInr;
}
