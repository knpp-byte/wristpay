namespace WristPay.Application;

public sealed record SendPaymentRequest(
    string IdempotencyKey,
    string SenderMobileNumber,
    string RecipientQuery,
    decimal AmountInr);
