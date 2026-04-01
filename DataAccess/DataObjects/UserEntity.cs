namespace NuciCraft.API.DataAccess.DataObjects
{
    public class UserEntity : NuciCraftEntityBase
    {
        public string OfflineUUID { get; set; }

        public string OnlineUUID { get; set; }

        public string Password { get; set; }

        public string IpAddress { get; set; }

        public string SkinUrl { get; set; }
    }
}
