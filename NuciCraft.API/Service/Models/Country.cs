namespace NuciCraft.API.Service.Models
{
    public class Country
    {
        public string Identifier { get; set; }

        public LocalisedString Name { get; set; }

        public LocalisedString LeaderTitle { get; set; }

        public string Leader { get; set; }
    }
}
