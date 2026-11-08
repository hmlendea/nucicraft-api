using System;

namespace NuciCraft.API.Service.Anonymisation
{
    public sealed class AnonymisationDetectionCacheEntry
    {
        public bool IsAnonymisationService { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }
    }
}