namespace signalrclient.Requests;

public record GetActiveBonussjekkResponse
{
    public required Bonussjekk[] BonusSjekker { get; set; }
}