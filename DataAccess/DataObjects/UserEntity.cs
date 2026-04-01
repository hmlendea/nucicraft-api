using NuciDAL.DataObjects;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public class UserEntity : EntityBase
    {
        public string Username { get; set; }

        public string OfflineUUID { get; set; }

        public string OnlineUUID { get; set; }

        public string SkinUrl { get; set; }
    }
}
