using System;

namespace NuciCraft.API.Service.Models
{
    public class Player
    {
        public string Username { get; set; }

        public string OfflineUUID { get; set; }

        public string OnlineUUID { get; set; }

        public string Password { get; set; }

        public DateTimeOffset CreatedDT { get; set; }

        public DateTimeOffset? UpdatedDT { get; set; }

        public string IpAddress { get; set; }

        public string SkinUrl { get; set; }
    }
}
