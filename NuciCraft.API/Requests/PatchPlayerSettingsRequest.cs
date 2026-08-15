using NuciSecurity.HMAC;

namespace NuciCraft.API.Requests
{
    public sealed class PatchPlayerSettingsRequest
    {
        private bool automaticHotbarRefillingIsEnabled = true;
        private bool automaticSaplingReplantingIsEnabled = true;
        private bool automaticToolSelectionIsEnabled = true;
        private bool keepExperienceIsEnabled;
        private bool keepInventoryIsEnabled;
        private bool privateMessagesAreEnabled = true;
        private bool privateMessagesInterceptionIsEnabled;
        private bool teleportationRequestsAreEnabled = true;

        [HmacOrder(1)]
        public bool AutomaticHotbarRefillingIsEnabled
        {
            get => automaticHotbarRefillingIsEnabled;
            set
            {
                automaticHotbarRefillingIsEnabled = value;
                AutomaticHotbarRefillingWasProvided = true;
            }
        }

        [HmacOrder(2)]
        public bool AutomaticSaplingReplantingIsEnabled
        {
            get => automaticSaplingReplantingIsEnabled;
            set
            {
                automaticSaplingReplantingIsEnabled = value;
                AutomaticSaplingReplantingWasProvided = true;
            }
        }

        [HmacOrder(3)]
        public bool AutomaticToolSelectionIsEnabled
        {
            get => automaticToolSelectionIsEnabled;
            set
            {
                automaticToolSelectionIsEnabled = value;
                AutomaticToolSelectionWasProvided = true;
            }
        }

        [HmacOrder(4)]
        public bool KeepExperienceIsEnabled
        {
            get => keepExperienceIsEnabled;
            set
            {
                keepExperienceIsEnabled = value;
                KeepExperienceWasProvided = true;
            }
        }

        [HmacOrder(5)]
        public bool KeepInventoryIsEnabled
        {
            get => keepInventoryIsEnabled;
            set
            {
                keepInventoryIsEnabled = value;
                KeepInventoryWasProvided = true;
            }
        }

        [HmacOrder(6)]
        public string Localisation { get; set; }

        [HmacOrder(7)]
        public bool PrivateMessagesAreEnabled
        {
            get => privateMessagesAreEnabled;
            set
            {
                privateMessagesAreEnabled = value;
                PrivateMessagesWereProvided = true;
            }
        }

        [HmacOrder(8)]
        public bool PrivateMessagesInterceptionIsEnabled
        {
            get => privateMessagesInterceptionIsEnabled;
            set
            {
                privateMessagesInterceptionIsEnabled = value;
                PrivateMessagesInterceptionWasProvided = true;
            }
        }

        [HmacOrder(9)]
        public string SkinUrl { get; set; }

        [HmacOrder(10)]
        public bool TeleportationRequestsAreEnabled
        {
            get => teleportationRequestsAreEnabled;
            set
            {
                teleportationRequestsAreEnabled = value;
                TeleportationRequestsWereProvided = true;
            }
        }

        internal bool AutomaticHotbarRefillingWasProvided { get; private set; }

        internal bool AutomaticSaplingReplantingWasProvided { get; private set; }

        internal bool AutomaticToolSelectionWasProvided { get; private set; }

        internal bool KeepExperienceWasProvided { get; private set; }

        internal bool KeepInventoryWasProvided { get; private set; }

        internal bool PrivateMessagesWereProvided { get; private set; }

        internal bool PrivateMessagesInterceptionWasProvided { get; private set; }

        internal bool TeleportationRequestsWereProvided { get; private set; }
    }
}
