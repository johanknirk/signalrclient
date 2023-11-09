#nullable disable
using TrumfApp;

namespace TrumfApp.Framework.Backend.Models.Envelope;

public class ResponseEnvelope
{
    public string ResponseType { get; set; }
    public string Response { get; set; }

    public bool IsError => ResponseType == nameof(ErrorResponse);
}