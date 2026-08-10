namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class PlayerSettingsDataObject
    {
        public bool AutomaticHotbarRefillingIsEnabled { get; set; }

        public bool AutomaticSaplingReplantingIsEnabled { get; set; }

        public bool AutomaticToolSelectionIsEnabled { get; set; }

        public bool KeepExperinceIsEnabled { get; set; }

        public bool KeepInventoryIsEnabled { get; set; }

        public bool PrivateMessagesAreEnabled { get; set; }

        public bool PrivateMessagesInterceptionIsEnabled { get; set; }
    }
}