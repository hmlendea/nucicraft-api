namespace NuciCraft.API.DataAccess.DataObjects
{
    public class RtpLocationEntity : NuciCraftEntityBase
    {
        public string Biome { get; set; }

        public string World { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }
    }
}
