
using signalrclient.Backend.Models;
using TrumfApp.Framework.Backend.Models;

namespace signalrclient;

public record RegisterPushTokenRequest(string PushToken, bool DeviceInitialized, string InstallId) : IBffRequest;

public record DeregisterDeviceRequest(string InstallId) : IBffRequest;

public record RegisterPaymentSessionDeviceRequest(string PaymentSessionId, string InstallId) : IBffRequest;


public record GetNotificationConsentsRequest : IBffRequest<GetNotificationConsentsResponse>;

public record NotificationConsents(bool TrumfPay, bool Loyalty);

public record UpdateNotificationConsentsRequest(NotificationConsents Consents) : IBffRequest;

public record GetNotificationConsentsResponse(NotificationConsents Consents);

public record DeclinePaymentRequest(string SessionId) : IBffRequest;

public record GenerateQrCodeRequest(string Operation) : IBffRequest<GenerateQrCodeResponse>;

public static class QrCodeOperations
{
    public const string IdOnly = "id";
    public const string MerchantInitiatedPay = "mpay";
    public const string CustomerInitiatedPay = "payment";
}

public record GenerateQrCodeResponse(QrCodeDto QrCode);

public record QrCodeDto(string Payload, DateTimeOffset Expiration);