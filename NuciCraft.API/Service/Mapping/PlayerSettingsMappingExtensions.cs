using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Service.Mapping
{
    internal static class PlayerSettingsMappingExtensions
    {
        internal static PlayerSettings ToServiceModel(this PlayerSettingsDataObject dataObject)
        {
            if (dataObject is null)
            {
                return null;
            }

            PlayerSettings settings = new();

            settings.AutomaticHotbarRefillingIsEnabled = dataObject.AutomaticHotbarRefillingIsEnabled;
            settings.AutomaticSaplingReplantingIsEnabled = dataObject.AutomaticSaplingReplantingIsEnabled;
            settings.AutomaticToolSelectionIsEnabled = dataObject.AutomaticToolSelectionIsEnabled;
            settings.KeepExperienceIsEnabled = dataObject.KeepExperienceIsEnabled;
            settings.KeepInventoryIsEnabled = dataObject.KeepInventoryIsEnabled;
            settings.PrivateMessagesAreEnabled = dataObject.PrivateMessagesAreEnabled;
            settings.PrivateMessagesInterceptionIsEnabled = dataObject.PrivateMessagesInterceptionIsEnabled;
            settings.TeleportationRequestsAreEnabled = dataObject.TeleportationRequestsAreEnabled;

            settings.Localisation = Localisation.FromString(dataObject.Localisation);
            settings.SkinUrl = dataObject.SkinUrl;

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
                PrivateMessagesInterceptionIsEnabled = serviceModel.PrivateMessagesInterceptionIsEnabled,
                Localisation = ToDataObject(serviceModel.Localisation),
                SkinUrl = serviceModel.SkinUrl,
                TeleportationRequestsAreEnabled = serviceModel.TeleportationRequestsAreEnabled
            };
        }

        private static string ToDataObject(Localisation serviceModel)
        {
            if (serviceModel is null)
            {
                return null;
            }

            return serviceModel;
        }
    }
}
