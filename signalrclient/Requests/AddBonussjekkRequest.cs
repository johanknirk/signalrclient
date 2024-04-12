using signalrclient.Backend.Models;

namespace signalrclient.Requests;

public record AddBonussjekkRequest : IBffRequest<AddBonussjekkResponse>
{
    public decimal Amount { get; set; }

    public string ForbrukerkjedeNr { get; set; } = null!;
}