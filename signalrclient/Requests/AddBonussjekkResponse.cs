namespace signalrclient.Requests
{
    public record AddBonussjekkResponse
    {
        public bool Success { get; set; }

        public BonussjekkResponseError? Error { get; set; }
    }
}