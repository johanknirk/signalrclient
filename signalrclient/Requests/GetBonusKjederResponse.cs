namespace signalrclient.Requests;

public record KjedeNavn
{
    public KjedeNavn(string navn, string forbrukerKjedeNr)
    {
        Navn = navn;
        ForbrukerKjedeNr = forbrukerKjedeNr;
    }

    public string ForbrukerKjedeNr { get; set; }

    public string Navn { get; set; }
}

public record GetBonusKjederResponse
{
    public required KjedeNavn[] Kjeder { get; set; }
}