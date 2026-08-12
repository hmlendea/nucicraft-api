using System;

using NuciAPI.Responses;

using NuciSecurity.HMAC;

using NuciCraft.API.Service.Models;

namespace NuciCraft.API.Responses
{
    public class GetPlayerResponse(Player player) : NuciApiSuccessResponse
    {
        [HmacOrder(1)]
        public string Identifier { get; set; } = player.Identifier;

        [HmacOrder(2)]
        public string Username { get; set; } = player.Username;

        [HmacOrder(3)]
        public string OfflineUUID { get; set; } = player.OfflineUUID;

        [HmacOrder(4)]
        public string OnlineUUID { get; set; } = player.OnlineUUID;

        [HmacOrder(5)]
        public string Password { get; set; } = player.Password;

        [HmacOrder(6)]
        public DateTimeOffset CreatedDT { get; set; } = player.CreatedDT;

        [HmacOrder(7)]
        public DateTimeOffset? UpdatedDT { get; set; } = player.UpdatedDT;

        [HmacOrder(8)]
        public string IpAddress { get; set; } = player.IpAddress;

        [HmacOrder(9)]
        public string DiscordId { get; set; } = player.DiscordId;

        [HmacOrder(10)]
        public string EmailAddress { get; set; } = player.EmailAddress;

        [HmacOrder(11)]
        public DateTimeOffset? LastSleptDT { get; set; } = player.LastSleptDT;

        [HmacOrder(12)]
        public DateTimeOffset? LastDeathDT { get; set; } = player.LastDeathDT;

        [HmacOrder(13)]
        public Coordinates LastDeathLocation { get; set; } = player.LastDeathLocation;

        [HmacOrder(14)]
        public Coordinates BackLocation { get; set; } = player.BackLocation;

        [HmacOrder(15)]
        public Coordinates LogoutLocation { get; set; } = player.LogoutLocation;

        [HmacOrder(17)]
        public PlayerSettings Settings { get; set; } = player.Settings;
    }
}
