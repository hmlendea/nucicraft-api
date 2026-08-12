using System;

using NUnit.Framework;

using NuciCraft.API.Responses;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Responses
{
    [TestFixture]
    public class GetPlayerResponseTests
    {
        // ── Field mapping ──────────────────────────────────────────────────────

        [Test]
        public void GivenAPlayer_WhenBuildingTheResponse_ThenAllFieldsAreMapped()
        {
            Player player = BuildPlayer();

            GetPlayerResponse response = new(player);

            Assert.That(response.Identifier, Is.EqualTo(player.Identifier));
            Assert.That(response.Username, Is.EqualTo(player.Username));
            Assert.That(response.OfflineUUID, Is.EqualTo(player.OfflineUUID));
            Assert.That(response.OnlineUUID, Is.EqualTo(player.OnlineUUID));
            Assert.That(response.Password, Is.EqualTo(player.Password));
            Assert.That(response.CreatedDT, Is.EqualTo(player.CreatedDT));
            Assert.That(response.UpdatedDT, Is.EqualTo(player.UpdatedDT));
            Assert.That(response.IpAddress, Is.EqualTo(player.IpAddress));
            Assert.That(response.DiscordId, Is.EqualTo(player.DiscordId));
            Assert.That(response.EmailAddress, Is.EqualTo(player.EmailAddress));
            Assert.That(response.LastSleptDT, Is.EqualTo(player.LastSleptDT));
            Assert.That(response.LastDeathDT, Is.EqualTo(player.LastDeathDT));
            Assert.That(response.BackLocation, Is.Not.Null);
            Assert.That(response.BackLocation.World, Is.EqualTo(player.BackLocation.World));
            Assert.That(response.BackLocation.X, Is.EqualTo(player.BackLocation.X));
            Assert.That(response.BackLocation.Y, Is.EqualTo(player.BackLocation.Y));
            Assert.That(response.BackLocation.Z, Is.EqualTo(player.BackLocation.Z));
            Assert.That(response.BackLocation.Pitch, Is.EqualTo(player.BackLocation.Pitch));
            Assert.That(response.BackLocation.Yaw, Is.EqualTo(player.BackLocation.Yaw));
            Assert.That(response.LogoutLocation, Is.Not.Null);
            Assert.That(response.LogoutLocation.World, Is.EqualTo(player.LogoutLocation.World));
            Assert.That(response.LogoutLocation.X, Is.EqualTo(player.LogoutLocation.X));
            Assert.That(response.LogoutLocation.Y, Is.EqualTo(player.LogoutLocation.Y));
            Assert.That(response.LogoutLocation.Z, Is.EqualTo(player.LogoutLocation.Z));
            Assert.That(response.LogoutLocation.Pitch, Is.EqualTo(player.LogoutLocation.Pitch));
            Assert.That(response.LogoutLocation.Yaw, Is.EqualTo(player.LogoutLocation.Yaw));
            Assert.That(response.Settings, Is.Not.Null);
            Assert.That(response.Settings.Localisation, Is.EqualTo(Localisation.Romanian));
            Assert.That(response.SkinUrl, Is.EqualTo(player.SkinUrl));
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
        public void GivenAPlayerWithNoLogoutLocation_WhenBuildingTheResponse_ThenLogoutLocationIsNull()
        {
            Player player = BuildPlayer();
            player.LogoutLocation = null;

            GetPlayerResponse response = new(player);

            Assert.That(response.LogoutLocation, Is.Null);
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
            OfflineUUID = "61300000-0000-3000-8000-000000000000",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            Password = "NucilandiaPass1",
            CreatedDT = DateTimeOffset.Parse("2012-09-05T00:00:00.0000000+00:00"),
            UpdatedDT = DateTimeOffset.Parse("2026-01-01T00:00:00.0000000+00:00"),
            IpAddress = "192.168.1.1",
            DiscordId = "613873000",
            EmailAddress = "ilarion.pintilie@nucilandia.ro",
            LastSleptDT = DateTimeOffset.Parse("2012-09-05T00:00:00.0000000+00:00"),
            LastDeathDT = null,
            LastDeathLocation = null,
            BackLocation = new() { World = "world", X = 13.0f, Y = 64.0f, Z = -21.5f, Pitch = 30.0f, Yaw = 150.0f },
            LogoutLocation = new() { World = "world_the_end", X = 8.5f, Y = 90.0f, Z = -3.25f, Pitch = 5.0f, Yaw = 240.0f },
            Settings = new() { Localisation = Localisation.Romanian },
            SkinUrl = "test.nucilandia.ro"
        };
    }
}
