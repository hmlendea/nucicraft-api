namespace NuciCraft.API.DataAccess.DataObjects
{
    public class CountryDataObject : NuciCraftEntityBase
    {
        public LocalisedStringDataObject Name { get; set; }

        public LocalisedStringDataObject LeaderTitle { get; set; }

        public string Leader { get; set; }
    }
}
