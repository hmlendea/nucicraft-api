using NuciCraft.API.Requests;

namespace NuciCraft.API.DataAccess.DataObjects
{
    internal static class PlayerSettingsDataObjectExtensions
    {
        internal static PlayerSettingsDataObject MergeWith(
            this PlayerSettingsDataObject existingSettings,
            PatchPlayerSettingsRequest incomingSettings)
        {
            if (existingSettings is null)
            {
                existingSettings = new();
            }

            if (incomingSettings.AutomaticHotbarRefillingWasProvided)
            {
                existingSettings.AutomaticHotbarRefillingIsEnabled =
                    incomingSettings.AutomaticHotbarRefillingIsEnabled;
            }

            if (incomingSettings.AutomaticSaplingReplantingWasProvided)
            {
                existingSettings.AutomaticSaplingReplantingIsEnabled =
                    incomingSettings.AutomaticSaplingReplantingIsEnabled;
            }

            if (incomingSettings.AutomaticToolSelectionWasProvided)
            {
                existingSettings.AutomaticToolSelectionIsEnabled =
                    incomingSettings.AutomaticToolSelectionIsEnabled;
            }

            if (incomingSettings.KeepExperienceWasProvided)
            {
                existingSettings.KeepExperienceIsEnabled = incomingSettings.KeepExperienceIsEnabled;
            }

            if (incomingSettings.KeepInventoryWasProvided)
            {
                existingSettings.KeepInventoryIsEnabled = incomingSettings.KeepInventoryIsEnabled;
            }

            if (incomingSettings.PrivateMessagesWereProvided)
            {
                existingSettings.PrivateMessagesAreEnabled = incomingSettings.PrivateMessagesAreEnabled;
            }

            if (incomingSettings.PrivateMessagesInterceptionWasProvided)
            {
                existingSettings.PrivateMessagesInterceptionIsEnabled =
                    incomingSettings.PrivateMessagesInterceptionIsEnabled;
            }

            if (incomingSettings.Localisation is not null)
            {
                existingSettings.Localisation = incomingSettings.Localisation;
            }

            if (incomingSettings.SkinUrl is not null)
            {
                existingSettings.SkinUrl = incomingSettings.SkinUrl;
            }

            if (incomingSettings.TeleportationRequestsWereProvided)
            {
                existingSettings.TeleportationRequestsAreEnabled =
                    incomingSettings.TeleportationRequestsAreEnabled;
            }

            return existingSettings;
        }
    }
}
