namespace signalrclient.Requests;

public record GetBankAccountSummary(string Id, string DisplayName, string AccountNumber, PaymentMethods[]? AeraPaymentMethodsArray);