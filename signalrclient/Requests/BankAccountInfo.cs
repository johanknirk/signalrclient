namespace signalrclient.Requests;

public record BankAccountInfo (string Id, string DisplayName, string AccountNumber, PaymentMethods[]? SupportedPaymentMethods);