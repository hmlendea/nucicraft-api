namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class HomeDataObject : NuciCraftEntityBase
    {
        public LocalisedStringDataObject Name { get; set; }

        public string Player { get; set; }

        public CoordinatesDataObject Location { get; set; }
    }
}