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
        public string LastIpAddress { get; set; } = player.LastIpAddress;

        [HmacOrder(9)]
        public string DiscordId { get; set; } = player.DiscordId;

        [HmacOrder(10)]
        public string EmailAddress { get; set; } = player.EmailAddress;

        [HmacOrder(11)]
        public string WikiUrl { get; set; } = player.WikiUrl;

        [HmacOrder(12)]
        public bool IsBanned { get; set; } = player.IsBanned;

        [HmacOrder(13)]
        public DateTimeOffset? BannedDT { get; set; } = player.BannedDT;

        [HmacOrder(14)]
        public bool IsMuted { get; set; } = player.IsMuted;

        [HmacOrder(15)]
        public DateTimeOffset? MutedDT { get; set; } = player.MutedDT;

        [HmacOrder(16)]
        public DateTimeOffset? LastLoginDT { get; set; } = player.LastLoginDT;

        [HmacOrder(17)]
        public DateTimeOffset? LastLogoutDT { get; set; } = player.LastLogoutDT;

        [HmacOrder(18)]
        public Coordinates LastLogoutLocation { get; set; } = player.LastLogoutLocation;

        [HmacOrder(19)]
        public DateTimeOffset? LastSleptDT { get; set; } = player.LastSleptDT;

        [HmacOrder(20)]
        public Coordinates BedLocation { get; set; } = player.BedLocation;

        [HmacOrder(21)]
        public DateTimeOffset? LastDeathDT { get; set; } = player.LastDeathDT;

        [HmacOrder(22)]
        public Coordinates LastDeathLocation { get; set; } = player.LastDeathLocation;

        [HmacOrder(23)]
        public DateTimeOffset? BackDT { get; set; } = player.BackDT;

        [HmacOrder(24)]
        public Coordinates BackLocation { get; set; } = player.BackLocation;

        [HmacOrder(25)]
        public PlayerSettings Settings { get; set; } = player.Settings;
    }
}
