#nullable disable
namespace signalrclient.Envelope;

public class ResponseEnvelope
{
    public string ResponseType { get; set; }
    public string Response { get; set; }

    public bool IsError => ResponseType == nameof(ErrorResponse);
}