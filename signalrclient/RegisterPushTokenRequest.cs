
using signalrclient.Backend.Models;
using TrumfApp.Framework.Backend.Models;

namespace signalrclient;

public record RegisterPushTokenRequest(string PushToken, bool DeviceInitialized, string InstallId) : IBffRequest;

public record DeregisterDeviceRequest(string InstallId) : IBffRequest;

public record RegisterPaymentSessionDeviceRequest(string PaymentSessionId, string InstallId) : IBffRequest;