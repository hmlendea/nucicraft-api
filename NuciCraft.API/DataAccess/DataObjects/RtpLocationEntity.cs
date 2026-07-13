namespace NuciCraft.API.DataAccess.DataObjects
{
    public class RtpLocationEntity : NuciCraftEntityBase
    {
        public string Biome { get; set; }

        public CoordinatesDataObject Coordinates { get; set; }
    }
}
