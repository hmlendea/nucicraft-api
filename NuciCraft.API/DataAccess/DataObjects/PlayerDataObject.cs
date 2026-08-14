namespace NuciCraft.API.DataAccess.DataObjects
{
    public class PlayerDataObject : NuciCraftEntityBase
    {
        public string Username { get; set; }

        public string OfflineUUID { get; set; }

        public string OnlineUUID { get; set; }

        public string Password { get; set; }

        public string IpAddress { get; set; }

        public string DiscordId { get; set; }

        public string EmailAddress { get; set; }

        public string WikiUrl { get; set; }

        public bool IsBanned { get; set; }

        public string BannedDT { get; set; }

        public bool IsMuted { get; set; }

        public string MutedDT { get; set; }

        public string LastLoginDT { get; set; }

        public string LastLogoutDT { get; set; }

        public CoordinatesDataObject LastLogoutLocation { get; set; }

        public string LastSleptDT { get; set; }

        public CoordinatesDataObject BedLocation { get; set; }

        public string LastDeathDT { get; set; }

        public CoordinatesDataObject LastDeathLocation { get; set; }

        public string BackDT { get; set; }

        public CoordinatesDataObject BackLocation { get; set; }

        public PlayerSettingsDataObject Settings { get; set; }
    }
}
