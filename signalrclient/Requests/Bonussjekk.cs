#nullable disable
namespace signalrclient.Requests
{
    public class Bonussjekk
    {
        public string BonussjekkId { get; set; }

        public string Beskrivelse { get; set; }

        public decimal Belop { get; set; }

        public string Status { get; set; }

        public DateTimeOffset GyldigTil { get; set; }

    }
}
