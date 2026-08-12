using NuciSecurity.HMAC;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public sealed class PlayerSettingsDataObject
    {
        [HmacOrder(1)]
        public bool? AutomaticHotbarRefillingIsEnabled { get; set; }

        [HmacOrder(2)]
        public bool? AutomaticSaplingReplantingIsEnabled { get; set; }

        [HmacOrder(3)]
        public bool? AutomaticToolSelectionIsEnabled { get; set; }

        [HmacOrder(4)]
        public bool? KeepExperienceIsEnabled { get; set; }

        [HmacOrder(5)]
        public bool? KeepInventoryIsEnabled { get; set; }

        [HmacOrder(6)]
        public string Localisation { get; set; }

        [HmacOrder(7)]
        public bool? PrivateMessagesAreEnabled { get; set; }

        [HmacOrder(8)]
        public bool? PrivateMessagesInterceptionIsEnabled { get; set; }

        [HmacOrder(9)]
        public string SkinUrl { get; set; }
    }
}