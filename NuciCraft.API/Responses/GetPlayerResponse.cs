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
        public string DisplayName { get; set; } = GetDisplayName(player);

        [HmacOrder(27)]
        public Gender Gender { get; set; } = player.Gender;

        [HmacOrder(4)]
        public string OfflineUUID { get; set; } = player.OfflineUUID;

        [HmacOrder(5)]
        public string OnlineUUID { get; set; } = player.OnlineUUID;

        [HmacOrder(6)]
        public string Password { get; set; } = player.Password;

        [HmacOrder(7)]
        public DateTimeOffset CreatedDT { get; set; } = player.CreatedDT;

        [HmacOrder(8)]
        public DateTimeOffset? UpdatedDT { get; set; } = player.UpdatedDT;

        [HmacOrder(9)]
        public string LastIpAddress { get; set; } = player.LastIpAddress;

        [HmacOrder(10)]
        public string DiscordId { get; set; } = player.DiscordId;

        [HmacOrder(11)]
        public string EmailAddress { get; set; } = player.EmailAddress;

        [HmacOrder(12)]
        public string WikiUrl { get; set; } = player.WikiUrl;

        [HmacOrder(13)]
        public bool IsBanned { get; set; } = player.IsBanned;

        [HmacOrder(14)]
        public DateTimeOffset? BannedDT { get; set; } = player.BannedDT;

        [HmacOrder(15)]
        public bool IsMuted { get; set; } = player.IsMuted;

        [HmacOrder(16)]
        public DateTimeOffset? MutedDT { get; set; } = player.MutedDT;

        [HmacOrder(17)]
        public DateTimeOffset? LastLoginDT { get; set; } = player.LastLoginDT;

        [HmacOrder(18)]
        public DateTimeOffset? LastLogoutDT { get; set; } = player.LastLogoutDT;

        [HmacOrder(19)]
        public Coordinates LastLogoutLocation { get; set; } = player.LastLogoutLocation;

        [HmacOrder(20)]
        public DateTimeOffset? LastSleptDT { get; set; } = player.LastSleptDT;

        [HmacOrder(21)]
        public Coordinates LastSleptLocation { get; set; } = player.LastSleptLocation;

        [HmacOrder(22)]
        public Coordinates BedLocation { get; set; } = player.BedLocation;

        [HmacOrder(23)]
        public DateTimeOffset? LastDeathDT { get; set; } = player.LastDeathDT;

        [HmacOrder(24)]
        public Coordinates LastDeathLocation { get; set; } = player.LastDeathLocation;

        [HmacOrder(25)]
        public DateTimeOffset? BackDT { get; set; } = player.BackDT;

        [HmacOrder(26)]
        public Coordinates BackLocation { get; set; } = player.BackLocation;

        [HmacOrder(27)]
        public PlayerSettings Settings { get; set; } = player.Settings;

        private static string GetDisplayName(Player player)
        {
            if (player.DisplayName is null)
            {
                return player.Username;
            }

            return player.DisplayName;
        }
    }
}
