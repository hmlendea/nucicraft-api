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

            return new()
            {
                AutomaticHotbarRefillingIsEnabled = dataObject.AutomaticHotbarRefillingIsEnabled,
                AutomaticSaplingReplantingIsEnabled = dataObject.AutomaticSaplingReplantingIsEnabled,
                AutomaticToolSelectionIsEnabled = dataObject.AutomaticToolSelectionIsEnabled,
                KeepExperinceIsEnabled = dataObject.KeepExperinceIsEnabled,
                KeepInventoryIsEnabled = dataObject.KeepInventoryIsEnabled,
                PrivateMessagesAreEnabled = dataObject.PrivateMessagesAreEnabled,
                PrivateMessagesInterceptionIsEnabled = dataObject.PrivateMessagesInterceptionIsEnabled
            };
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
                KeepExperinceIsEnabled = serviceModel.KeepExperinceIsEnabled,
                KeepInventoryIsEnabled = serviceModel.KeepInventoryIsEnabled,
                PrivateMessagesAreEnabled = serviceModel.PrivateMessagesAreEnabled,
                PrivateMessagesInterceptionIsEnabled = serviceModel.PrivateMessagesInterceptionIsEnabled
            };
        }
    }
}