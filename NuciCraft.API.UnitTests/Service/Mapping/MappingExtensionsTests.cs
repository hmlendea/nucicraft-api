using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using NuciCraft.API.DataAccess.DataObjects;
using NuciCraft.API.Service.Models;

namespace NuciCraft.API.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class MappingExtensionsTests
    {
        private static string CoordinatesMappingTypeName => "NuciCraft.API.Service.Mapping.CoordinatesMappingExtensions";

        private static string CountryMappingTypeName => "NuciCraft.API.Service.Mapping.CountryMappingExtensions";

        private static string LocalisedStringMappingTypeName => "NuciCraft.API.Service.Mapping.LocalisedStringMappingExtensions";

        private static string PlayerMappingTypeName => "NuciCraft.API.Service.Mapping.PlayerMappingExtensions";

        private static string PlayerSettingsMappingTypeName => "NuciCraft.API.Service.Mapping.PlayerSettingsMappingExtensions";

        private static string RtpLocationMappingTypeName => "NuciCraft.API.Service.Mapping.RtpLocationMappingExtensions";

        private static string ToDataObjectMethodName => "ToDataObject";

        private static string ToDataObjectsMethodName => "ToDataObjects";

        private static string ToDomainModelsMethodName => "ToDomainModels";

        private static string ToServiceModelMethodName => "ToServiceModel";

        private static string ToServiceModelsMethodName => "ToServiceModels";

        private static string ZoneBoundsMappingTypeName => "NuciCraft.API.Service.Mapping.ZoneBoundsMappingExtensions";

        private static string ZoneMappingTypeName => "NuciCraft.API.Service.Mapping.ZoneMappingExtensions";

        [Test]
        public void GivenCoordinatesModels_WhenMappingToDataObjects_ThenAllValuesAreRetained()
        {
            IEnumerable<Coordinates> serviceModels = [BuildCoordinates()];

            IEnumerable<CoordinatesDataObject> dataObjects = MappingMethodInvoker
                .Invoke<IEnumerable<Coordinates>, IEnumerable<CoordinatesDataObject>>(
                    CoordinatesMappingTypeName,
                    ToDataObjectsMethodName,
                    serviceModels);
            CoordinatesDataObject dataObject = dataObjects.Single();

            Assert.That(dataObject.World, Is.EqualTo("world"));
            Assert.That(dataObject.X, Is.EqualTo(6.13f));
            Assert.That(dataObject.Y, Is.EqualTo(64f));
            Assert.That(dataObject.Z, Is.EqualTo(8.73f));
            Assert.That(dataObject.Pitch, Is.EqualTo(3.14f));
            Assert.That(dataObject.Yaw, Is.EqualTo(42f));
        }

        [Test]
        public void GivenCoordinatesDataObjects_WhenMappingToServiceModels_ThenAllValuesAreRetained()
        {
            IEnumerable<CoordinatesDataObject> dataObjects = [BuildCoordinatesDataObject()];

            IEnumerable<Coordinates> serviceModels = MappingMethodInvoker
                .Invoke<IEnumerable<CoordinatesDataObject>, IEnumerable<Coordinates>>(
                    CoordinatesMappingTypeName,
                    ToServiceModelsMethodName,
                    dataObjects);
            Coordinates serviceModel = serviceModels.Single();

            Assert.That(serviceModel.World, Is.EqualTo("world"));
            Assert.That(serviceModel.X, Is.EqualTo(6.13f));
            Assert.That(serviceModel.Y, Is.EqualTo(64f));
            Assert.That(serviceModel.Z, Is.EqualTo(8.73f));
            Assert.That(serviceModel.Pitch, Is.EqualTo(3.14f));
            Assert.That(serviceModel.Yaw, Is.EqualTo(42f));
        }

        [Test]
        public void GivenLocalisedStringModels_WhenMappingToDataObjects_ThenAllValuesAreRetained()
        {
            IEnumerable<LocalisedString> serviceModels = [BuildLocalisedString()];

            IEnumerable<LocalisedStringDataObject> dataObjects = MappingMethodInvoker
                .Invoke<IEnumerable<LocalisedString>, IEnumerable<LocalisedStringDataObject>>(
                    LocalisedStringMappingTypeName,
                    ToDataObjectsMethodName,
                    serviceModels);
            LocalisedStringDataObject dataObject = dataObjects.Single();

            Assert.That(dataObject.Default, Is.EqualTo("Nucilandia"));
            Assert.That(dataObject.Chinese, Is.EqualTo("Nucilandia Chinese"));
            Assert.That(dataObject.Dacian, Is.EqualTo("Nucilandia Dacian"));
            Assert.That(dataObject.English, Is.EqualTo("Nucilandia"));
            Assert.That(dataObject.French, Is.EqualTo("Nucilandia French"));
            Assert.That(dataObject.German, Is.EqualTo("Nucilandien"));
            Assert.That(dataObject.Italian, Is.EqualTo("Nucilandia Italian"));
            Assert.That(dataObject.Japanese, Is.EqualTo("Nucilandia Japanese"));
            Assert.That(dataObject.Latin, Is.EqualTo("Nucilandia Latin"));
            Assert.That(dataObject.Nucian, Is.EqualTo("Nucilandia Nucian"));
            Assert.That(dataObject.Romanian, Is.EqualTo("Nucilandia"));
        }

        [Test]
        public void GivenLocalisedStringDataObjects_WhenMappingToServiceModels_ThenAllValuesAreRetained()
        {
            IEnumerable<LocalisedStringDataObject> dataObjects = [BuildLocalisedStringDataObject()];

            IEnumerable<LocalisedString> serviceModels = MappingMethodInvoker
                .Invoke<IEnumerable<LocalisedStringDataObject>, IEnumerable<LocalisedString>>(
                    LocalisedStringMappingTypeName,
                    ToServiceModelsMethodName,
                    dataObjects);
            LocalisedString serviceModel = serviceModels.Single();

            Assert.That(serviceModel.Default, Is.EqualTo("Nucilandia"));
            Assert.That(serviceModel.Chinese, Is.EqualTo("Nucilandia Chinese"));
            Assert.That(serviceModel.Dacian, Is.EqualTo("Nucilandia Dacian"));
            Assert.That(serviceModel.French, Is.EqualTo("Nucilandia French"));
            Assert.That(serviceModel.Italian, Is.EqualTo("Nucilandia Italian"));
            Assert.That(serviceModel.Japanese, Is.EqualTo("Nucilandia Japanese"));
            Assert.That(serviceModel.Latin, Is.EqualTo("Nucilandia Latin"));
            Assert.That(serviceModel.Nucian, Is.EqualTo("Nucilandia Nucian"));
        }

        [Test]
        public void GivenACountryModelWithLocalisedValues_WhenMappingToADataObject_ThenValuesAreRetained()
        {
            Country serviceModel = new()
            {
                Identifier = "nucilandia",
                Name = BuildLocalisedString(),
                LeaderTitle = BuildLocalisedString(),
                Leader = "IlarionPintilie"
            };

            CountryDataObject dataObject = MappingMethodInvoker.Invoke<Country, CountryDataObject>(
                CountryMappingTypeName,
                ToDataObjectMethodName,
                serviceModel);

            Assert.That(dataObject.Id, Is.EqualTo("nucilandia"));
            Assert.That(dataObject.Name, Is.Not.Null);
            Assert.That(dataObject.LeaderTitle, Is.Not.Null);
            Assert.That(dataObject.Leader, Is.EqualTo("IlarionPintilie"));
        }

        [Test]
        public void GivenACountryModelWithoutLocalisedValues_WhenMappingToADataObject_ThenValuesRemainNull()
        {
            Country serviceModel = new()
            {
                Identifier = "nucilandia",
                Name = null,
                LeaderTitle = null
            };

            CountryDataObject dataObject = MappingMethodInvoker.Invoke<Country, CountryDataObject>(
                CountryMappingTypeName,
                ToDataObjectMethodName,
                serviceModel);

            Assert.That(dataObject.Name, Is.Null);
            Assert.That(dataObject.LeaderTitle, Is.Null);
        }

        [Test]
        public void GivenACountryDataObjectWithoutLocalisedValues_WhenMappingToAServiceModel_ThenValuesRemainNull()
        {
            CountryDataObject dataObject = new()
            {
                Id = "nucilandia",
                Name = null,
                LeaderTitle = null
            };

            Country serviceModel = MappingMethodInvoker.Invoke<CountryDataObject, Country>(
                CountryMappingTypeName,
                ToServiceModelMethodName,
                dataObject);

            Assert.That(serviceModel.Name, Is.Null);
            Assert.That(serviceModel.LeaderTitle, Is.Null);
        }

        [Test]
        public void GivenNullPlayerSettingsData_WhenMappingToAServiceModel_ThenNullIsReturned()
            => Assert.That(
                MappingMethodInvoker.Invoke<PlayerSettingsDataObject, PlayerSettings>(
                    PlayerSettingsMappingTypeName,
                    ToServiceModelMethodName,
                    null),
                Is.Null);

        [Test]
        public void GivenNullPlayerSettings_WhenMappingToADataObject_ThenNullIsReturned()
            => Assert.That(
                MappingMethodInvoker.Invoke<PlayerSettings, PlayerSettingsDataObject>(
                    PlayerSettingsMappingTypeName,
                    ToDataObjectMethodName,
                    null),
                Is.Null);

        [Test]
        public void GivenDefaultPlayerSettingsData_WhenMappingToAServiceModel_ThenTeleportationRequestsAreEnabled()
        {
            PlayerSettingsDataObject dataObject = new();

            PlayerSettings serviceModel = MappingMethodInvoker
                .Invoke<PlayerSettingsDataObject, PlayerSettings>(
                    PlayerSettingsMappingTypeName,
                    ToServiceModelMethodName,
                    dataObject);

            Assert.That(serviceModel.TeleportationRequestsAreEnabled);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenPlayerSettingsDataWithATeleportationPreference_WhenMappingToAServiceModel_ThenTheValueIsRetained(
            bool teleportationRequestsAreEnabled)
        {
            PlayerSettingsDataObject dataObject = new()
            {
                TeleportationRequestsAreEnabled = teleportationRequestsAreEnabled
            };

            PlayerSettings serviceModel = MappingMethodInvoker
                .Invoke<PlayerSettingsDataObject, PlayerSettings>(
                    PlayerSettingsMappingTypeName,
                    ToServiceModelMethodName,
                    dataObject);

            Assert.That(
                serviceModel.TeleportationRequestsAreEnabled,
                Is.EqualTo(teleportationRequestsAreEnabled));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenPlayerSettingsWithATeleportationPreference_WhenMappingToADataObject_ThenTheValueIsRetained(
            bool teleportationRequestsAreEnabled)
        {
            PlayerSettings serviceModel = new()
            {
                TeleportationRequestsAreEnabled = teleportationRequestsAreEnabled
            };

            PlayerSettingsDataObject dataObject = MappingMethodInvoker
                .Invoke<PlayerSettings, PlayerSettingsDataObject>(
                    PlayerSettingsMappingTypeName,
                    ToDataObjectMethodName,
                    serviceModel);

            Assert.That(
                dataObject.TeleportationRequestsAreEnabled,
                Is.EqualTo(teleportationRequestsAreEnabled));
        }

        [Test]
        public void GivenPlayerSettingsWithoutLocalisation_WhenMappingToADataObject_ThenLocalisationRemainsNull()
        {
            PlayerSettings serviceModel = new()
            {
                Localisation = null,
                SkinUrl = "test.nucilandia.ro"
            };

            PlayerSettingsDataObject dataObject = MappingMethodInvoker
                .Invoke<PlayerSettings, PlayerSettingsDataObject>(
                    PlayerSettingsMappingTypeName,
                    ToDataObjectMethodName,
                    serviceModel);

            Assert.That(dataObject.Localisation, Is.Null);
            Assert.That(dataObject.SkinUrl, Is.EqualTo("test.nucilandia.ro"));
        }

        [Test]
        public void GivenPlayerDataObjects_WhenMappingToDomainModels_ThenNullableValuesAreHandled()
        {
            IEnumerable<PlayerDataObject> dataObjects =
            [
                BuildPlayerDataObject(),
                BuildPlayerDataObjectWithoutOptionalValues(),
            ];

            Player[] domainModels = MappingMethodInvoker
                .Invoke<IEnumerable<PlayerDataObject>, IEnumerable<Player>>(
                    PlayerMappingTypeName,
                    ToDomainModelsMethodName,
                    dataObjects)
                .ToArray();

            Assert.That(domainModels, Has.Length.EqualTo(2));
            Assert.That(domainModels[0].DisplayName, Is.EqualTo("Ilarion Pintilie"));
            Assert.That(domainModels[0].Gender, Is.EqualTo(Gender.Male));
            Assert.That(domainModels[0].WikiUrl, Is.EqualTo("https://test.nucilandia.ro"));
            Assert.That(domainModels[0].IsBanned);
            Assert.That(domainModels[0].BannedDT, Is.Not.Null);
            Assert.That(domainModels[0].IsMuted);
            Assert.That(domainModels[0].MutedDT, Is.Not.Null);
            Assert.That(domainModels[0].LastLoginDT, Is.Not.Null);
            Assert.That(domainModels[0].LastLogoutDT, Is.Not.Null);
            Assert.That(domainModels[0].LastLogoutLocation, Is.Not.Null);
            Assert.That(domainModels[0].LastSleptLocation, Is.Not.Null);
            Assert.That(domainModels[0].BedLocation, Is.Not.Null);
            Assert.That(domainModels[0].BackDT, Is.Not.Null);
            Assert.That(domainModels[0].LastDeathLocation, Is.Not.Null);
            Assert.That(domainModels[1].DisplayName, Is.Null);
            Assert.That(domainModels[1].Gender, Is.EqualTo(Gender.Other));
            Assert.That(domainModels[1].BannedDT, Is.Null);
            Assert.That(domainModels[1].MutedDT, Is.Null);
            Assert.That(domainModels[1].LastLoginDT, Is.Null);
            Assert.That(domainModels[1].LastLogoutDT, Is.Null);
            Assert.That(domainModels[1].LastLogoutLocation, Is.Null);
            Assert.That(domainModels[1].LastSleptLocation, Is.Null);
            Assert.That(domainModels[1].BedLocation, Is.Null);
            Assert.That(domainModels[1].BackDT, Is.Null);
            Assert.That(domainModels[1].LastDeathLocation, Is.Null);
            Assert.That(domainModels[1].Settings, Is.Null);
        }

        [Test]
        public void GivenPlayerDomainModels_WhenMappingToDataObjects_ThenNullableValuesAreHandled()
        {
            IEnumerable<Player> domainModels =
            [
                BuildPlayer(),
                BuildPlayerWithoutOptionalValues(),
            ];

            PlayerDataObject[] dataObjects = MappingMethodInvoker
                .Invoke<IEnumerable<Player>, IEnumerable<PlayerDataObject>>(
                    PlayerMappingTypeName,
                    ToDataObjectsMethodName,
                    domainModels)
                .ToArray();

            Assert.That(dataObjects, Has.Length.EqualTo(2));
            Assert.That(dataObjects[0].DisplayName, Is.EqualTo("Ilarion Pintilie"));
            Assert.That(dataObjects[0].Gender, Is.EqualTo("male"));
            Assert.That(dataObjects[0].WikiUrl, Is.EqualTo("https://test.nucilandia.ro"));
            Assert.That(dataObjects[0].IsBanned);
            Assert.That(dataObjects[0].BannedDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(dataObjects[0].IsMuted);
            Assert.That(dataObjects[0].MutedDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(dataObjects[0].LastLoginDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(dataObjects[0].LastLogoutDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(dataObjects[0].LastLogoutLocation, Is.Not.Null);
            Assert.That(dataObjects[0].LastSleptLocation, Is.Not.Null);
            Assert.That(dataObjects[0].BedLocation, Is.Not.Null);
            Assert.That(dataObjects[0].BackDT, Is.EqualTo("2026-08-13T00:00:00.0000000+00:00"));
            Assert.That(dataObjects[0].LastDeathLocation, Is.Not.Null);
            Assert.That(dataObjects[1].DisplayName, Is.Null);
            Assert.That(dataObjects[1].Gender, Is.Null);
            Assert.That(dataObjects[1].BannedDT, Is.Null);
            Assert.That(dataObjects[1].MutedDT, Is.Null);
            Assert.That(dataObjects[1].LastLoginDT, Is.Null);
            Assert.That(dataObjects[1].LastLogoutDT, Is.Null);
            Assert.That(dataObjects[1].LastLogoutLocation, Is.Null);
            Assert.That(dataObjects[1].LastSleptLocation, Is.Null);
            Assert.That(dataObjects[1].BedLocation, Is.Null);
            Assert.That(dataObjects[1].BackDT, Is.Null);
            Assert.That(dataObjects[1].LastDeathLocation, Is.Null);
            Assert.That(dataObjects[1].Settings, Is.Null);
        }

        [Test]
        public void GivenRtpLocationDataObjects_WhenMappingToServiceModels_ThenNullableCoordinatesAreHandled()
        {
            IEnumerable<RtpLocationEntity> dataObjects =
            [
                new() { Id = "solara", Biome = "Forest", Coordinates = BuildCoordinatesDataObject() },
                new() { Id = "cratesia", Biome = "Desert", Coordinates = null },
            ];

            RtpLocation[] serviceModels = MappingMethodInvoker
                .Invoke<IEnumerable<RtpLocationEntity>, IEnumerable<RtpLocation>>(
                    RtpLocationMappingTypeName,
                    ToServiceModelsMethodName,
                    dataObjects)
                .ToArray();

            Assert.That(serviceModels[0].Coordinates, Is.Not.Null);
            Assert.That(serviceModels[1].Coordinates, Is.Null);
        }

        [Test]
        public void GivenRtpLocationModels_WhenMappingToDataObjects_ThenNullableCoordinatesAreHandled()
        {
            IEnumerable<RtpLocation> serviceModels =
            [
                new() { Id = "solara", Biome = "Forest", Coordinates = BuildCoordinates() },
                new() { Id = "cratesia", Biome = "Desert", Coordinates = null },
            ];

            RtpLocationEntity[] dataObjects = MappingMethodInvoker
                .Invoke<IEnumerable<RtpLocation>, IEnumerable<RtpLocationEntity>>(
                    RtpLocationMappingTypeName,
                    ToDataObjectsMethodName,
                    serviceModels)
                .ToArray();

            Assert.That(dataObjects[0].Coordinates, Is.Not.Null);
            Assert.That(dataObjects[1].Coordinates, Is.Null);
        }

        [Test]
        public void GivenZoneBoundsDataObjects_WhenMappingToServiceModels_ThenNullableCornersAreHandled()
        {
            IEnumerable<ZoneBoundsDataObject> dataObjects =
            [
                BuildZoneBoundsDataObject(),
                new() { FirstCorner = null, SecondCorner = null },
            ];

            ZoneBounds[] serviceModels = MappingMethodInvoker
                .Invoke<IEnumerable<ZoneBoundsDataObject>, IEnumerable<ZoneBounds>>(
                    ZoneBoundsMappingTypeName,
                    ToServiceModelsMethodName,
                    dataObjects)
                .ToArray();

            Assert.That(serviceModels[0].FirstCorner, Is.Not.Null);
            Assert.That(serviceModels[1].FirstCorner, Is.Null);
            Assert.That(serviceModels[1].SecondCorner, Is.Null);
        }

        [Test]
        public void GivenZoneBoundsModels_WhenMappingToDataObjects_ThenNullableCornersAreHandled()
        {
            IEnumerable<ZoneBounds> serviceModels =
            [
                BuildZoneBounds(),
                new() { FirstCorner = null, SecondCorner = null },
            ];

            ZoneBoundsDataObject[] dataObjects = MappingMethodInvoker
                .Invoke<IEnumerable<ZoneBounds>, IEnumerable<ZoneBoundsDataObject>>(
                    ZoneBoundsMappingTypeName,
                    ToDataObjectsMethodName,
                    serviceModels)
                .ToArray();

            Assert.That(dataObjects[0].FirstCorner, Is.Not.Null);
            Assert.That(dataObjects[1].FirstCorner, Is.Null);
            Assert.That(dataObjects[1].SecondCorner, Is.Null);
        }

        [Test]
        public void GivenZoneDataObjects_WhenMappingToServiceModels_ThenNullableNestedValuesAreHandled()
        {
            IEnumerable<ZoneDataObject> dataObjects =
            [
                BuildZoneDataObject(),
                new() { Id = "cratesia" },
            ];

            Zone[] serviceModels = MappingMethodInvoker
                .Invoke<IEnumerable<ZoneDataObject>, IEnumerable<Zone>>(
                    ZoneMappingTypeName,
                    ToServiceModelsMethodName,
                    dataObjects)
                .ToArray();

            Assert.That(serviceModels[0].Name, Is.Not.Null);
            Assert.That(serviceModels[0].Bounds, Is.Not.Null);
            Assert.That(serviceModels[1].Name, Is.Null);
            Assert.That(serviceModels[1].Bounds, Is.Null);
        }

        [Test]
        public void GivenZoneModels_WhenMappingToDataObjects_ThenNullableNestedValuesAreHandled()
        {
            IEnumerable<Zone> serviceModels =
            [
                BuildZone(),
                new() { Identifier = "cratesia" },
            ];

            ZoneDataObject[] dataObjects = MappingMethodInvoker
                .Invoke<IEnumerable<Zone>, IEnumerable<ZoneDataObject>>(
                    ZoneMappingTypeName,
                    ToDataObjectsMethodName,
                    serviceModels)
                .ToArray();

            Assert.That(dataObjects[0].Name, Is.Not.Null);
            Assert.That(dataObjects[0].Bounds, Is.Not.Null);
            Assert.That(dataObjects[1].Name, Is.Null);
            Assert.That(dataObjects[1].Bounds, Is.Null);
        }

        private static Coordinates BuildCoordinates() => new()
        {
            World = "world",
            X = 6.13f,
            Y = 64f,
            Z = 8.73f,
            Pitch = 3.14f,
            Yaw = 42f
        };

        private static CoordinatesDataObject BuildCoordinatesDataObject() => new()
        {
            World = "world",
            X = 6.13f,
            Y = 64f,
            Z = 8.73f,
            Pitch = 3.14f,
            Yaw = 42f
        };

        private static LocalisedString BuildLocalisedString() => new()
        {
            Default = "Nucilandia",
            Chinese = "Nucilandia Chinese",
            Dacian = "Nucilandia Dacian",
            English = "Nucilandia",
            French = "Nucilandia French",
            German = "Nucilandien",
            Italian = "Nucilandia Italian",
            Japanese = "Nucilandia Japanese",
            Latin = "Nucilandia Latin",
            Nucian = "Nucilandia Nucian",
            Romanian = "Nucilandia"
        };

        private static LocalisedStringDataObject BuildLocalisedStringDataObject() => new()
        {
            Default = "Nucilandia",
            Chinese = "Nucilandia Chinese",
            Dacian = "Nucilandia Dacian",
            English = "Nucilandia",
            French = "Nucilandia French",
            German = "Nucilandien",
            Italian = "Nucilandia Italian",
            Japanese = "Nucilandia Japanese",
            Latin = "Nucilandia Latin",
            Nucian = "Nucilandia Nucian",
            Romanian = "Nucilandia"
        };

        private static Player BuildPlayer() => new()
        {
            Identifier = "61300000-8730-3000-8000-000000000000",
            Username = "IlarionPintilie",
            DisplayName = "Ilarion Pintilie",
            Gender = Gender.Male,
            OfflineUUID = "61300000-0000-3000-8000-000000000000",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            Password = "NucileRullz!",
            CreatedDT = new DateTimeOffset(2012, 9, 5, 0, 0, 0, TimeSpan.Zero),
            UpdatedDT = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            WikiUrl = "https://test.nucilandia.ro",
            IsBanned = true,
            BannedDT = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            IsMuted = true,
            MutedDT = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            LastLoginDT = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            LastLogoutDT = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            LastLogoutLocation = BuildCoordinates(),
            LastSleptDT = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            LastSleptLocation = BuildCoordinates(),
            BedLocation = BuildCoordinates(),
            LastDeathDT = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            LastDeathLocation = BuildCoordinates(),
            BackDT = new DateTimeOffset(2026, 8, 13, 0, 0, 0, TimeSpan.Zero),
            BackLocation = BuildCoordinates(),
            Settings = new PlayerSettings
            {
                Localisation = Localisation.English,
                SkinUrl = "test.nucilandia.ro"
            }
        };

        private static Player BuildPlayerWithoutOptionalValues() => new()
        {
            Identifier = "87300000-6130-3000-8000-000000000000",
            Username = "DummyUser",
            DisplayName = null,
            CreatedDT = new DateTimeOffset(2012, 9, 5, 0, 0, 0, TimeSpan.Zero),
            UpdatedDT = null,
            BannedDT = null,
            MutedDT = null,
            LastLoginDT = null,
            LastLogoutDT = null,
            LastLogoutLocation = null,
            LastSleptDT = null,
            LastSleptLocation = null,
            BedLocation = null,
            LastDeathDT = null,
            LastDeathLocation = null,
            BackDT = null,
            BackLocation = null,
            Settings = null
        };

        private static PlayerDataObject BuildPlayerDataObject() => new()
        {
            Id = "61300000-8730-3000-8000-000000000000",
            Username = "IlarionPintilie",
            DisplayName = "Ilarion Pintilie",
            Gender = "male",
            OfflineUUID = "61300000-0000-3000-8000-000000000000",
            OnlineUUID = "87300000-0000-0000-0000-000000000000",
            Password = "NucileRullz!",
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            UpdatedDT = "2026-08-13T00:00:00.0000000+00:00",
            WikiUrl = "https://test.nucilandia.ro",
            IsBanned = true,
            BannedDT = "2026-08-13T00:00:00.0000000+00:00",
            IsMuted = true,
            MutedDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLoginDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLogoutDT = "2026-08-13T00:00:00.0000000+00:00",
            LastLogoutLocation = BuildCoordinatesDataObject(),
            LastSleptDT = "2026-08-13T00:00:00.0000000+00:00",
            LastSleptLocation = BuildCoordinatesDataObject(),
            BedLocation = BuildCoordinatesDataObject(),
            LastDeathDT = "2026-08-13T00:00:00.0000000+00:00",
            LastDeathLocation = BuildCoordinatesDataObject(),
            BackDT = "2026-08-13T00:00:00.0000000+00:00",
            BackLocation = BuildCoordinatesDataObject(),
            Settings = new PlayerSettingsDataObject
            {
                Localisation = "english",
                SkinUrl = "test.nucilandia.ro"
            }
        };

        private static PlayerDataObject BuildPlayerDataObjectWithoutOptionalValues() => new()
        {
            Id = "87300000-6130-3000-8000-000000000000",
            Username = "DummyUser",
            DisplayName = null,
            CreatedDT = "2012-09-05T00:00:00.0000000+00:00",
            UpdatedDT = null,
            BannedDT = null,
            MutedDT = null,
            LastLoginDT = null,
            LastLogoutDT = null,
            LastLogoutLocation = null,
            LastSleptDT = null,
            LastSleptLocation = null,
            BedLocation = null,
            LastDeathDT = null,
            LastDeathLocation = null,
            BackDT = null,
            BackLocation = null,
            Settings = null
        };

        private static Zone BuildZone() => new()
        {
            Identifier = "solara",
            Name = BuildLocalisedString(),
            Nickname = BuildLocalisedString(),
            World = "world",
            TeleportationPoint = BuildCoordinates(),
            Bounds = BuildZoneBounds(),
            LeaderTitle = BuildLocalisedString()
        };

        private static ZoneBounds BuildZoneBounds() => new()
        {
            FirstCorner = BuildCoordinates(),
            SecondCorner = BuildCoordinates()
        };

        private static ZoneBoundsDataObject BuildZoneBoundsDataObject() => new()
        {
            FirstCorner = BuildCoordinatesDataObject(),
            SecondCorner = BuildCoordinatesDataObject()
        };

        private static ZoneDataObject BuildZoneDataObject() => new()
        {
            Id = "solara",
            Name = BuildLocalisedStringDataObject(),
            Nickname = BuildLocalisedStringDataObject(),
            World = "world",
            TeleportationPoint = BuildCoordinatesDataObject(),
            Bounds = BuildZoneBoundsDataObject(),
            LeaderTitle = BuildLocalisedStringDataObject()
        };
    }
}
