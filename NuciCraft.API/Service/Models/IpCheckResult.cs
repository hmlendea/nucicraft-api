namespace NuciCraft.API.Service.Models
{
    public sealed class IpCheckResult
    {
        public bool IsAllowed { get; set; }

        public string? DeniedReason { get; set; }

        public string? CountryCode { get; set; }
    }
}