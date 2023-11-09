#nullable disable
namespace signalrclient.Envelope;

public class RequestEnvelope
{
    public string RequestType { get; set; }
    public string Request { get; set; }
    public string JwtBearerToken { get; set; }

    public string RequestId { get; set; }
}