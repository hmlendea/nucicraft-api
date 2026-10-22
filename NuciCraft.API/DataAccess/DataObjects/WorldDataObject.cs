namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class WorldDataObject : NuciCraftEntityBase
    {
        public LocalisedStringDataObject Name { get; set; }

        public bool HasWebMap { get; set; }

        public CoordinatesDataObject SpawnPoint { get; set; }

        public string Type { get; set; }
    }
}
