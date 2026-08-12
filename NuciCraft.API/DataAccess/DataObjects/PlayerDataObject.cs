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

        public string LastSleptDT { get; set; }

        public string LastDeathDT { get; set; }

        public CoordinatesDataObject LastDeathLocation { get; set; }

        public CoordinatesDataObject BackLocation { get; set; }

        public CoordinatesDataObject LogoutLocation { get; set; }

        public PlayerSettingsDataObject Settings { get; set; }

        public string SkinUrl { get; set; }
    }
}
