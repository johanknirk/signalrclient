namespace signalrclient.Requests
{
    public record DeleteBonussjekkResponse
    {
        public bool Success { get; set; }

        public BonussjekkResponseError? Error { get; set; }
    }
}