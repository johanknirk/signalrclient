using signalrclient.Backend.Models;

namespace signalrclient.Requests;

public record DeleteBonussjekkRequest : IBffRequest<DeleteBonussjekkResponse>
{
    public string BonusSjekkId { get; set; } = null!;
}