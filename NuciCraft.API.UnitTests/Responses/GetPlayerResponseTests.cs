using System;

using NUnit.Framework;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public sealed class GetPlayerResponseTests
    {
        [Test]
        public void GivenAPlayer_WhenBuildingTheResponse_ThenAllFieldsAreMapped()
        {
            Player player = BuildPlayer();

            GetPlayerResponse response = new(player);

            Assert.That(response.Identifier, Is.EqualTo(player.Identifier));
            Assert.That(response.Username, Is.EqualTo(player.Username));
            Assert.That(response.DisplayName, Is.EqualTo(player.DisplayName));
            Assert.That(response.OfflineUUID, Is.EqualTo(player.OfflineUUID));
            Assert.That(response.OnlineUUID, Is.EqualTo(player.OnlineUUID));
            Assert.That(response.Password, Is.EqualTo(player.Password));
            Assert.That(response.CreatedDT, Is.EqualTo(player.CreatedDT));
            Assert.That(response.UpdatedDT, Is.EqualTo(player.UpdatedDT));
            Assert.That(response.LastIpAddress, Is.EqualTo(player.LastIpAddress));
            Assert.That(response.DiscordId, Is.EqualTo(player.DiscordId));
            Assert.That(response.EmailAddress, Is.EqualTo(player.EmailAddress));
            Assert.That(response.WikiUrl, Is.EqualTo(player.WikiUrl));
            Assert.That(response.IsBanned, Is.EqualTo(player.IsBanned));
            Assert.That(response.BannedDT, Is.EqualTo(player.BannedDT));
            Assert.That(response.IsMuted, Is.EqualTo(player.IsMuted));
            Assert.That(response.MutedDT, Is.EqualTo(player.MutedDT));
            Assert.That(response.LastLoginDT, Is.EqualTo(player.LastLoginDT));
            Assert.That(response.LastLogoutDT, Is.EqualTo(player.LastLogoutDT));
            Assert.That(response.LastLogoutLocation, Is.Not.Null);
            Assert.That(response.LastLogoutLocation.World, Is.EqualTo(player.LastLogoutLocation.World));
            Assert.That(response.LastLogoutLocation.X, Is.EqualTo(player.LastLogoutLocation.X));
            Assert.That(response.LastLogoutLocation.Y, Is.EqualTo(player.LastLogoutLocation.Y));
            Assert.That(response.LastLogoutLocation.Z, Is.EqualTo(player.LastLogoutLocation.Z));
            Assert.That(response.LastLogoutLocation.Pitch, Is.EqualTo(player.LastLogoutLocation.Pitch));
            Assert.That(response.LastLogoutLocation.Yaw, Is.EqualTo(player.LastLogoutLocation.Yaw));
            Assert.That(response.LastSleptDT, Is.EqualTo(player.LastSleptDT));
            Assert.That(response.BedLocation, Is.Not.Null);
            Assert.That(response.BedLocation.World, Is.EqualTo(player.BedLocation.World));
            Assert.That(response.BedLocation.X, Is.EqualTo(player.BedLocation.X));
            Assert.That(response.BedLocation.Y, Is.EqualTo(player.BedLocation.Y));
            Assert.That(response.BedLocation.Z, Is.EqualTo(player.BedLocation.Z));
            Assert.That(response.BedLocation.Pitch, Is.EqualTo(player.BedLocation.Pitch));
            Assert.That(response.BedLocation.Yaw, Is.EqualTo(player.BedLocation.Yaw));
            Assert.That(response.LastDeathDT, Is.EqualTo(player.LastDeathDT));
            Assert.That(response.BackDT, Is.EqualTo(player.BackDT));
            Assert.That(response.BackLocation, Is.Not.Null);
            Assert.That(response.BackLocation.World, Is.EqualTo(player.BackLocation.World));
            Assert.That(response.BackLocation.X, Is.EqualTo(player.BackLocation.X));
            Assert.That(response.BackLocation.Y, Is.EqualTo(player.BackLocation.Y));
            Assert.That(response.BackLocation.Z, Is.EqualTo(player.BackLocation.Z));
            Assert.That(response.BackLocation.Pitch, Is.EqualTo(player.BackLocation.Pitch));
            Assert.That(response.BackLocation.Yaw, Is.EqualTo(player.BackLocation.Yaw));
            Assert.That(response.Settings, Is.Not.Null);
            Assert.That(response.Settings.Localisation, Is.EqualTo(Localisation.Romanian));
            Assert.That(response.Settings.SkinUrl, Is.EqualTo(player.Settings.SkinUrl));
        }

        [Test]
        public void GivenAPlayerWithNoDisplayName_WhenBuildingTheResponse_ThenUsernameIsUsed()
        {
            Player player = BuildPlayer();
            player.DisplayName = null;

            GetPlayerResponse response = new(player);

            Assert.That(response.DisplayName, Is.EqualTo(player.Username));
        }

        [Test]
        public void GivenAPlayerWithALastDeathLocation_WhenBuildingTheResponse_ThenTheLastDeathLocationIsMapped()
        {
            Player player = BuildPlayer();
            player.LastDeathDT = DateTimeOffset.Parse("2025-03-15T12:00:00.0000000+00:00");
            player.LastDeathLocation = new()
            {
                World = "world_nether",
                X = 613.5f,
                Y = 64.0f,
                Z = -873.25f
            };

            GetPlayerResponse response = new(player);

            Assert.That(response.LastDeathDT, Is.EqualTo(player.LastDeathDT));
            Assert.That(response.LastDeathLocation, Is.Not.Null);
            Assert.That(response.LastDeathLocation.World, Is.EqualTo(player.LastDeathLocation.World));
            Assert.That(response.LastDeathLocation.X, Is.EqualTo(player.LastDeathLocation.X));
            Assert.That(response.LastDeathLocation.Y, Is.EqualTo(player.LastDeathLocation.Y));
            Assert.That(response.LastDeathLocation.Z, Is.EqualTo(player.LastDeathLocation.Z));
        }

        [Test]
        public void GivenAPlayerWithNoLastDeathLocation_WhenBuildingTheResponse_ThenLastDeathLocationIsNull()
        {
            Player player = BuildPlayer();
            player.LastDeathLocation = null;
            player.LastDeathDT = null;

            GetPlayerResponse response = new(player);

            Assert.That(response.LastDeathLocation, Is.Null);
            Assert.That(response.LastDeathDT, Is.Null);
        }

        [Test]
        public void GivenAPlayerWithNoBackLocation_WhenBuildingTheResponse_ThenBackLocationIsNull()
        {
            Player player = BuildPlayer();
            player.BackLocation = null;

            GetPlayerResponse response = new(player);

            Assert.That(response.BackLocation, Is.Null);
        }

        [Test]
        public void GivenAPlayerWithNoLastLogoutLocation_WhenBuildingTheResponse_ThenLastLogoutLocationIsNull()
        {
            Player player = BuildPlayer();
            player.LastLogoutLocation = null;

            GetPlayerResponse response = new(player);

            Assert.That(response.LastLogoutLocation, Is.Null);
        }

        [Test]
        public void GivenAPlayerWithNoBedLocation_WhenBuildingTheResponse_ThenBedLocationIsNull()
        {
            Player player = BuildPlayer();
            player.BedLocation = null;

            GetPlayerResponse response = new(player);

            Assert.That(response.BedLocation, Is.Null);
        }

        [Test]
        public void GivenAPlayerWithNoOptionalTimestamps_WhenBuildingTheResponse_ThenOptionalTimestampsAreNull()
        {
            Player player = BuildPlayer();
            player.BannedDT = null;
            player.MutedDT = null;
            player.LastLoginDT = null;
            player.LastLogoutDT = null;
            player.BackDT = null;

            GetPlayerResponse response = new(player);

            Assert.That(response.BannedDT, Is.Null);
            Assert.That(response.MutedDT, Is.Null);
            Assert.That(response.LastLoginDT, Is.Null);
            Assert.That(response.LastLogoutDT, Is.Null);
            Assert.That(response.BackDT, Is.Null);
        }

        [Test]
        public void GivenAPlayerWithNoUpdatedDT_WhenBuildingTheResponse_ThenUpdatedDTIsNull()
        {
            Player player = BuildPlayer();
            player.UpdatedDT = null;

            GetPlayerResponse response = new(player);

            Assert.That(response.UpdatedDT, Is.Null);
        }

        private static Player BuildPlayer() => new()
        {
            Identifier = "61300000-8730-3000-8000-000000000000",
            Username = "IlarionPintilie",
            DisplayName = "Ilarion Pintilie",
            OfflineUUID = "61300000-0000-3000-8000-000000000000",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            Password = "NucilandiaPass1",
            CreatedDT = DateTimeOffset.Parse("2012-09-05T00:00:00.0000000+00:00"),
            UpdatedDT = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000+00:00"),
            LastIpAddress = "192.168.1.1",
            DiscordId = "613873000",
            EmailAddress = "ilarion.pintilie@nucilandia.ro",
            WikiUrl = "https://test.nucilandia.ro",
            IsBanned = true,
            BannedDT = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000+00:00"),
            IsMuted = true,
            MutedDT = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000+00:00"),
            LastLoginDT = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000+00:00"),
            LastLogoutDT = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000+00:00"),
            LastLogoutLocation = new() { World = "world_the_end", X = 8.5f, Y = 90.0f, Z = -3.25f, Pitch = 5.0f, Yaw = 240.0f },
            LastSleptDT = DateTimeOffset.Parse("2012-09-05T00:00:00.0000000+00:00"),
            BedLocation = new() { World = "world", X = 6.13f, Y = 64.0f, Z = 8.73f, Pitch = 3.14f, Yaw = 42.0f },
            LastDeathDT = null,
            LastDeathLocation = null,
            BackDT = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000+00:00"),
            BackLocation = new() { World = "world", X = 13.0f, Y = 64.0f, Z = -21.5f, Pitch = 30.0f, Yaw = 150.0f },
            Settings = new() { Localisation = Localisation.Romanian, SkinUrl = "test.nucilandia.ro" }
        };
    }
}
