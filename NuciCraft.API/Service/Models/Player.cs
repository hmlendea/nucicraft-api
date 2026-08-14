using System;

namespace NuciCraft.API.Service.Models
{
    public class Player
    {
        public string Identifier { get; set; }

        public string Username { get; set; }

        public string OfflineUUID { get; set; }

        public string OnlineUUID { get; set; }

        public string Password { get; set; }

        public DateTimeOffset CreatedDT { get; set; }

        public DateTimeOffset? UpdatedDT { get; set; }

        public string IpAddress { get; set; }

        public string DiscordId { get; set; }

        public string EmailAddress { get; set; }

        public string WikiUrl { get; set; }

        public bool IsBanned { get; set; }

        public DateTimeOffset? BannedDT { get; set; }

        public bool IsMuted { get; set; }

        public DateTimeOffset? MutedDT { get; set; }

        public DateTimeOffset? LastLoginDT { get; set; }

        public DateTimeOffset? LastLogoutDT { get; set; }

        public Coordinates LastLogoutLocation { get; set; }

        public DateTimeOffset? LastSleptDT { get; set; }

        public Coordinates BedLocation { get; set; }

        public DateTimeOffset? LastDeathDT { get; set; }

        public Coordinates LastDeathLocation { get; set; }

        public DateTimeOffset? BackDT { get; set; }

        public Coordinates BackLocation { get; set; }

        public PlayerSettings Settings { get; set; }
    }
}
