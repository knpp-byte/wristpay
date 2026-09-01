namespace WristPay.Domain;

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    RecipientNotFound,
    LimitExceeded
}
