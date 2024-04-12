namespace signalrclient.Requests;

public record BonussjekkResponseError
{
    public int MessageCode { get; set; }

    public string? Reason { get; set; }

    public string Message { get; set; }
}