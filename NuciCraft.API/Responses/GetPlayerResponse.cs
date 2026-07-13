using System;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public class GetPlayerResponse : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public string Identifier { get; set; }

        [HmacOrder(2)]
        public string Username { get; set; }

        [HmacOrder(3)]
        public string OfflineUUID { get; set; }

        [HmacOrder(4)]
        public string OnlineUUID { get; set; }

        [HmacOrder(5)]
        public string Password { get; set; }

        [HmacOrder(6)]
        public DateTimeOffset CreatedDT { get; set; }

        [HmacOrder(7)]
        public DateTimeOffset? UpdatedDT { get; set; }

        [HmacOrder(8)]
        public string IpAddress { get; set; }

        [HmacOrder(9)]
        public string DiscordId { get; set; }

        [HmacOrder(10)]
        public string EmailAddress { get; set; }

        [HmacOrder(11)]
        public DateTimeOffset? LastSleptDT { get; set; }

        [HmacOrder(12)]
        public DateTimeOffset? LastDeathDT { get; set; }

        [HmacOrder(13)]
        public Coordinates LastDeathLocation { get; set; }

        [HmacOrder(14)]
        public string SkinUrl { get; set; }

        public GetPlayerResponse(Player player)
        {
            Identifier = player.Identifier;
            Username = player.Username;
            OfflineUUID = player.OfflineUUID;
            OnlineUUID = player.OnlineUUID;
            Password = player.Password;
            CreatedDT = player.CreatedDT;
            UpdatedDT = player.UpdatedDT;
            IpAddress = player.IpAddress;
            DiscordId = player.DiscordId;
            EmailAddress = player.EmailAddress;
            LastSleptDT = player.LastSleptDT;
            LastDeathDT = player.LastDeathDT;
            LastDeathLocation = player.LastDeathLocation;
            SkinUrl = player.SkinUrl;
        }
    }
}
