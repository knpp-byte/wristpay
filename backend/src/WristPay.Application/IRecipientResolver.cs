using WristPay.Domain;

namespace WristPay.Application;

/// <summary>
/// Resolves a mobile number or UPI ID typed on the Watch into a verified recipient name — the
/// "no pre-added beneficiary required" lookup step. Backed by a mock directory today; a real
/// implementation must come from the PSP bank/NPCI-approved UPI lookup, never guessed client-side.
/// </summary>
public interface IRecipientResolver
{
    Task<Recipient?> ResolveAsync(string query, CancellationToken cancellationToken);
}
