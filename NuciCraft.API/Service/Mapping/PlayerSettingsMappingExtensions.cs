using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    static class PlayerSettingsMappingExtensions
    {
        internal static PlayerSettings ToServiceModel(this PlayerSettingsDataObject dataObject)
        {
            if (dataObject is null)
            {
                return null;
            }

            PlayerSettings settings = new();

            if (dataObject.AutomaticHotbarRefillingIsEnabled is not null)
            {
                settings.AutomaticHotbarRefillingIsEnabled = dataObject.AutomaticHotbarRefillingIsEnabled.Value;
            }

            if (dataObject.AutomaticSaplingReplantingIsEnabled is not null)
            {
                settings.AutomaticSaplingReplantingIsEnabled = dataObject.AutomaticSaplingReplantingIsEnabled.Value;
            }

            if (dataObject.AutomaticToolSelectionIsEnabled is not null)
            {
                settings.AutomaticToolSelectionIsEnabled = dataObject.AutomaticToolSelectionIsEnabled.Value;
            }

            if (dataObject.KeepExperienceIsEnabled is not null)
            {
                settings.KeepExperienceIsEnabled = dataObject.KeepExperienceIsEnabled.Value;
            }

            if (dataObject.KeepInventoryIsEnabled is not null)
            {
                settings.KeepInventoryIsEnabled = dataObject.KeepInventoryIsEnabled.Value;
            }

            if (dataObject.PrivateMessagesAreEnabled is not null)
            {
                settings.PrivateMessagesAreEnabled = dataObject.PrivateMessagesAreEnabled.Value;
            }

            if (dataObject.PrivateMessagesInterceptionIsEnabled is not null)
            {
                settings.PrivateMessagesInterceptionIsEnabled = dataObject.PrivateMessagesInterceptionIsEnabled.Value;
            }

            return settings;
        }

        internal static PlayerSettingsDataObject ToDataObject(this PlayerSettings serviceModel)
        {
            if (serviceModel is null)
            {
                return null;
            }

            return new()
            {
                AutomaticHotbarRefillingIsEnabled = serviceModel.AutomaticHotbarRefillingIsEnabled,
                AutomaticSaplingReplantingIsEnabled = serviceModel.AutomaticSaplingReplantingIsEnabled,
                AutomaticToolSelectionIsEnabled = serviceModel.AutomaticToolSelectionIsEnabled,
                KeepExperienceIsEnabled = serviceModel.KeepExperienceIsEnabled,
                KeepInventoryIsEnabled = serviceModel.KeepInventoryIsEnabled,
                PrivateMessagesAreEnabled = serviceModel.PrivateMessagesAreEnabled,
                PrivateMessagesInterceptionIsEnabled = serviceModel.PrivateMessagesInterceptionIsEnabled
            };
        }
    }
}