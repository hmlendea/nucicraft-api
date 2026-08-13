namespace NuciCraft.API.DataAccess.DataObjects
{
    internal static class PlayerSettingsDataObjectExtensions
    {
        internal static PlayerSettingsDataObject MergeWith(
            this PlayerSettingsDataObject existingSettings,
            PlayerSettingsDataObject incomingSettings)
        {
            if (existingSettings is null)
            {
                existingSettings = new();
            }

            if (incomingSettings.AutomaticHotbarRefillingIsEnabled is not null)
            {
                existingSettings.AutomaticHotbarRefillingIsEnabled =
                    incomingSettings.AutomaticHotbarRefillingIsEnabled.Value;
            }

            if (incomingSettings.AutomaticSaplingReplantingIsEnabled is not null)
            {
                existingSettings.AutomaticSaplingReplantingIsEnabled =
                    incomingSettings.AutomaticSaplingReplantingIsEnabled.Value;
            }

            if (incomingSettings.AutomaticToolSelectionIsEnabled is not null)
            {
                existingSettings.AutomaticToolSelectionIsEnabled =
                    incomingSettings.AutomaticToolSelectionIsEnabled.Value;
            }

            if (incomingSettings.KeepExperienceIsEnabled is not null)
            {
                existingSettings.KeepExperienceIsEnabled = incomingSettings.KeepExperienceIsEnabled.Value;
            }

            if (incomingSettings.KeepInventoryIsEnabled is not null)
            {
                existingSettings.KeepInventoryIsEnabled = incomingSettings.KeepInventoryIsEnabled.Value;
            }

            if (incomingSettings.PrivateMessagesAreEnabled is not null)
            {
                existingSettings.PrivateMessagesAreEnabled = incomingSettings.PrivateMessagesAreEnabled.Value;
            }

            if (incomingSettings.PrivateMessagesInterceptionIsEnabled is not null)
            {
                existingSettings.PrivateMessagesInterceptionIsEnabled =
                    incomingSettings.PrivateMessagesInterceptionIsEnabled.Value;
            }

            if (incomingSettings.Localisation is not null)
            {
                existingSettings.Localisation = incomingSettings.Localisation;
            }

            if (incomingSettings.SkinUrl is not null)
            {
                existingSettings.SkinUrl = incomingSettings.SkinUrl;
            }

            return existingSettings;
        }
    }
}
