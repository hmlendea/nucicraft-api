namespace NuciCraft.API.Service.Models
{
    public sealed class PlayerSettings
    {
        public bool AutomaticHotbarRefillingIsEnabled { get; set; } = true;

        public bool AutomaticSaplingReplantingIsEnabled { get; set; } = true;

        public bool AutomaticToolSelectionIsEnabled { get; set; } = true;

        public bool KeepExperienceIsEnabled { get; set; } = false;

        public bool KeepInventoryIsEnabled { get; set; } = false;

        public bool PrivateMessagesAreEnabled { get; set; } = true;

        public bool PrivateMessagesInterceptionIsEnabled { get; set; } = false;

        public Localisation Localisation { get; set; } = Localisation.Romanian;

        public string SkinUrl { get; set; }
    }
}