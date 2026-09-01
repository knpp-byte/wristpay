namespace WristPay.Domain;

/// <summary>A UPI recipient resolved from a mobile number or UPI ID. Never invented client-side —
/// always the result of a lookup against the (mocked, for now) UPI/PSP recipient-resolution service.</summary>
public sealed record Recipient(string DisplayName, string MobileNumber, string UpiId);
