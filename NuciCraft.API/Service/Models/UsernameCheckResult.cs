namespace NuciCraft.API.Service.Models
{
    public sealed class UsernameCheckResult
    {
        public bool IsAllowed { get; set; }

        public string? DeniedReason { get; set; }
    }
}