using NuciCraft.API.DataAccess.DataObjects;

namespace NuciCraft.API.Service.Helpers
{
    internal static class LocalisedStringDataObjectExtensions
    {
        internal static LocalisedStringDataObject MergeWith(
            this LocalisedStringDataObject existingLocalisedString,
            LocalisedStringDataObject incomingLocalisedString)
        {
            if (existingLocalisedString is null)
            {
                return incomingLocalisedString;
            }

            existingLocalisedString.Default = MergeLocalisedValue(
                existingLocalisedString.Default,
                incomingLocalisedString.Default);
            existingLocalisedString.Chinese = MergeLocalisedValue(
                existingLocalisedString.Chinese,
                incomingLocalisedString.Chinese);
            existingLocalisedString.Dacian = MergeLocalisedValue(
                existingLocalisedString.Dacian,
                incomingLocalisedString.Dacian);
            existingLocalisedString.English = MergeLocalisedValue(
                existingLocalisedString.English,
                incomingLocalisedString.English);
            existingLocalisedString.French = MergeLocalisedValue(
                existingLocalisedString.French,
                incomingLocalisedString.French);
            existingLocalisedString.German = MergeLocalisedValue(
                existingLocalisedString.German,
                incomingLocalisedString.German);
            existingLocalisedString.Italian = MergeLocalisedValue(
                existingLocalisedString.Italian,
                incomingLocalisedString.Italian);
            existingLocalisedString.Japanese = MergeLocalisedValue(
                existingLocalisedString.Japanese,
                incomingLocalisedString.Japanese);
            existingLocalisedString.Latin = MergeLocalisedValue(
                existingLocalisedString.Latin,
                incomingLocalisedString.Latin);
            existingLocalisedString.Nucian = MergeLocalisedValue(
                existingLocalisedString.Nucian,
                incomingLocalisedString.Nucian);
            existingLocalisedString.Romanian = MergeLocalisedValue(
                existingLocalisedString.Romanian,
                incomingLocalisedString.Romanian);

            return existingLocalisedString;
        }

        private static string MergeLocalisedValue(
            string existingValue,
            string incomingValue)
        {
            if (incomingValue is not null)
            {
                return incomingValue;
            }

            return existingValue;
        }
    }
}
