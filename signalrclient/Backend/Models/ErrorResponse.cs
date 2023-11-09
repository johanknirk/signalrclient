namespace TrumfApp.Framework.Backend.Models;

public class ErrorResponse
{
    public string ErrorCode { get; set; }

    /// <summary>
    /// True if the ErrorCode is one of the transport errors defined in `ErrorCodes`
    /// False if it is any other (domain-specific) error code.
    /// </summary>
    public bool IsTransportError =>
        ErrorCode == ErrorCodes.UnknownRequestType
        || ErrorCode == ErrorCodes.InvalidRequestMessageFormat
        || ErrorCode == ErrorCodes.Offline
        || ErrorCode == ErrorCodes.Exception
        || ErrorCode == ErrorCodes.Unknown;
}

public static class ErrorCodes
{
    public const string UnknownRequestType = nameof(UnknownRequestType);
    public const string InvalidRequestMessageFormat = nameof(InvalidRequestMessageFormat);

    // app-side errors
    public const string Offline = nameof(Offline);
    public const string Exception = nameof(Exception);
    public const string Unknown = nameof(Unknown);
    public const string NoCurrentPaymentSession = nameof(NoCurrentPaymentSession);
    public const string NoPurchasesMadeYet = nameof(NoPurchasesMadeYet);
    public const string Unauthorized = nameof(Unauthorized);
}