using System;
using System.Globalization;

namespace NuciCraft.API.Service
{
    internal static class TimestampFormats
    {
        internal static string Full => "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

        internal static string GetCurrentUtcTimestamp()
            => DateTimeOffset.UtcNow.ToString(Full, CultureInfo.InvariantCulture);
    }
}
