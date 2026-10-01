# Documentation Coverage

| Metadata | Value |
|----------|-------|
| Purpose | Map substantial implementation and test areas to documentation, expose omissions, and preserve the result of the separate completeness audit. |
| Scope | All tracked production C# files, test fixtures and helpers, entry points, capabilities, persistence stores, integrations, configuration, error paths, and automation. |
| Primary sources | Tracked files in [NuciCraft.API](../NuciCraft.API/), [NuciCraft.API.UnitTests](../NuciCraft.API.UnitTests/), [NuciCraft.API.IntegrationTests](../NuciCraft.API.IntegrationTests/), and repository metadata |
| Related documents | [Repository structure](repository-structure.md), [architecture](architecture.md), [testing](testing.md), and [documentation maintenance](documentation-maintenance.md) |

## Audit Method

The audit used these independent checks:
1. Enumerated tracked files by project and source area.
2. Counted and inspected all controller HTTP attributes.
3. Counted and inspected all public service operations.
4. Mapped all seven persisted record types and path settings.
5. Mapped every outbound system and infrastructure package boundary.
6. Mapped every local settings class and committed setting key.
7. Confirmed no background-process implementation exists.
8. Correlated principal failure branches with tests and error documentation.
9. Enumerated all unit and integration test C# files.
10. Searched production basenames across the corpus and explicitly linked omitted transport and mapping files here.
11. Validated local Markdown links separately after composition.

This is a source-reference and semantic coverage audit, not a statement of complete behavioural correctness or measured code coverage.

## Inventory Summary

| Area | Tracked C# Files | Documentation Owner |
|------|------------------|---------------------|
| Composition root | 3 | [Architecture](architecture.md), [host component](components/host-and-composition.md), [startup flow](flows/startup.md) |
| Configuration | 6 | [Configuration](configuration.md) |
| Controllers | 9 | [HTTP boundary](components/http-api-boundary.md), [interfaces](interfaces.md) |
| Data access | 13 | [Data model](data-model.md), [persistence component](components/persistence-and-mapping.md) |
| Logging | 2 | [Cross-cutting concerns](cross-cutting-concerns.md) |
| Requests | 31 | [Data model](data-model.md), [interfaces](interfaces.md) |
| Responses | 16 | [Data model](data-model.md), [interfaces](interfaces.md) |
| Service root and subdirectories | 48 | Component, behaviour, flow, invariant, and dependency documents |
| Production total | 128 | This map links every file below. |
| Unit tests | 42 | [Testing](testing.md) and test map below |
| Integration tests | 14 | [Testing](testing.md) and test map below |

## Principal Component Coverage

| Component | Implementation | Canonical Documentation |
|-----------|----------------|-------------------------|
| Host and composition | Program, Startup, registrations, settings | [Host component](components/host-and-composition.md), [architecture](architecture.md), [startup flow](flows/startup.md) |
| HTTP boundary | Nine controllers and transport contracts | [HTTP component](components/http-api-boundary.md), [interfaces](interfaces.md), [data model](data-model.md) |
| Persistence and mapping | Data objects, NuciDAL adapters, conversions | [Persistence component](components/persistence-and-mapping.md), [state](state-and-persistence.md), [data model](data-model.md) |
| Players and Homes | Two services and related contracts | [Component](components/player-and-homes.md), [behaviour](behaviour/player-and-home-capabilities.md), [flow](flows/player-and-home-requests.md) |
| Geographic metadata | Country, World, ZoneType, Zone | [Component](components/geographic-metadata.md), [behaviour](behaviour/geographic-metadata-capabilities.md), [Zone flow](flows/zone-requests.md) |
| RTP | Location service and spatial algorithm | [Component](components/rtp-locations.md), [behaviour](behaviour/rtp-and-external-capabilities.md), [invariants](invariants.md) |
| External services | Mob names and Java status | [Component](components/external-services.md), [behaviour](behaviour/rtp-and-external-capabilities.md), [flow](flows/external-requests.md) |
| Cross-cutting concerns | Logging, validation, HMAC, time, DI, security | [Cross-cutting concerns](cross-cutting-concerns.md), [security](security.md), [errors](error-handling.md), [concurrency](concurrency-and-scheduling.md) |

## Capability Coverage

| Runtime Capability | Entry Point Count | Behaviour And Flow Coverage |
|--------------------|-------------------|-----------------------------|
| Country add, read, list, patch | 4 | Geographic behaviour and generic file-backed flow. |
| Home add, delete, read variants, list variants, patch | 6 | Player/Home behaviour and method-level flow. |
| Mob-name generation | 1 | External behaviour and outbound flow. |
| Player register, four selector reads, list, four selector patches | 10 | Player/Home behaviour and method-level flow. |
| RTP add and random selection | 2 | RTP behaviour and component algorithm. |
| Server information and live count | 1 | External behaviour and outbound flow. |
| World add, read, list, patch | 4 | Geographic behaviour and generic file-backed flow. |
| Zone add, delete, read, type list, category list, containment, patch | 7 | Geographic behaviour and Zone flow. |
| ZoneType add, read, filtered list, patch | 4 | Geographic behaviour and generic file-backed flow. |
| Total HTTP actions | 39 | All actions appear in [interfaces.md](interfaces.md). |

All 36 public service methods are represented by their owning component or flow. Overloaded list and get methods account for the difference between service-method and controller-action totals.

## Entry-Point Coverage

- Process entry `Program.Main` and `CreateHostBuilder`: [startup flow](flows/startup.md).
- Host registration and pipeline entry: [host component](components/host-and-composition.md).
- All controller actions: [HTTP boundary](components/http-api-boundary.md) and [interfaces](interfaces.md).
- Service interfaces: [interfaces](interfaces.md).
- Maintainer entry points in VS Code tasks, launch configuration, CI, and release script: [build and deployment](build-and-deployment.md).

No CLI command, message handler, hosted worker, webhook receiver, or secondary executable exists.

## Persistence Coverage

| Store | Data Object | Owner | Documentation |
|-------|-------------|-------|---------------|
| Players | `PlayerDataObject` | PlayerService | Data model, Player component, state. |
| Homes | `HomeDataObject` | HomeService | Data model, Home component, state. |
| RTP locations | `RtpLocationEntity` | RtpLocationService | Data model, RTP component, state. |
| Countries | `CountryDataObject` | CountryService | Data model, geographic component, state. |
| Worlds | `WorldDataObject` | WorldService | Data model, geographic component, state. |
| Zones | `ZoneDataObject` | ZoneService | Data model, geographic component, state. |
| Zone types | `ZoneTypeDataObject` | ZoneTypeService | Data model, geographic component, state. |

Store creation, eager loading, save boundaries, consistency limits, ignored local data, migration absence, and backup ownership are covered in [state-and-persistence.md](state-and-persistence.md).

## Integration Coverage

| Integration | Documentation | Test Evidence |
|-------------|---------------|---------------|
| Universal Name Generator | [Integrations](integrations.md), [external component](components/external-services.md), [external flow](flows/external-requests.md) | Mock client in service and HTTP tests; no live remote test. |
| Minecraft Java server | Same integration and flow documents | Real loopback legacy protocol test. |
| JSON filesystem | [Persistence component](components/persistence-and-mapping.md), [state](state-and-persistence.md) | Startup, HTTP, and restart-persistence tests. |
| Nuci API package family | [Dependencies](dependencies.md), [interfaces](interfaces.md), [errors](error-handling.md) | HTTP integration outcomes, not package internals. |
| NuciDAL | [Dependencies](dependencies.md), [ambiguity register](ambiguities-and-open-questions.md) | Ordinary repository usage and restart tests, not concurrency internals. |
| NuciLog | [Cross-cutting concerns](cross-cutting-concerns.md), [security](security.md) | Service log assertions and key tests. |
| Remote release helper | [Build and deployment](build-and-deployment.md), [integrations](integrations.md), [security](security.md) | No local behaviour test. |

## Configuration Coverage

Every key in [appsettings.json](../NuciCraft.API/appsettings.json) is inventoried in [configuration.md](configuration.md):
- Seven data store paths.
- Four server fields.
- Two RTP thresholds.
- Web-map BaseUrl.
- Inbound API key.
- Generator BaseUrl and API key.
- Log path and activation.

Configuration precedence, environment variable names, command-line form, validation timing, restart conduct, placeholder risks, and key interactions are documented.

## Background Process Coverage

No production background process, scheduler, timer, queue, or hosted service exists. This absence and the modification requirements for adding one are documented in [concurrency-and-scheduling.md](concurrency-and-scheduling.md) and [change-guide.md](change-guide.md).

## Error-Path Coverage

| Error Area | Documentation | Test Correlation |
|------------|---------------|------------------|
| API key and model binding | Security and error handling | ApiErrorResponseTests and domain authorisation tests. |
| Missing records | Error handling and capability documents | Unit service tests and HTTP `404` case. |
| Domain conflicts | Invariants and capability documents | Home, RTP, Zone service and HTTP tests. |
| Startup paths and eager load | Host component and startup flow | ProgramTests and StartupTests. |
| Generator failures | External component and errors | MobServiceTests. |
| Java availability and invalid payload | External component and errors | Unit validation and loopback integration. |
| Persistence failure | Error handling and ambiguity register | Mocked exception paths; no interrupted-write test. |

## Complete Production Source Map

### Composition Root

- [Program.cs](../NuciCraft.API/Program.cs)
- [Startup.cs](../NuciCraft.API/Startup.cs)
- [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs)

### Configuration

- [DataStoreSettings.cs](../NuciCraft.API/Configuration/DataStoreSettings.cs)
- [RtpLocationSettings.cs](../NuciCraft.API/Configuration/RtpLocationSettings.cs)
- [SecuritySettings.cs](../NuciCraft.API/Configuration/SecuritySettings.cs)
- [ServerSettings.cs](../NuciCraft.API/Configuration/ServerSettings.cs)
- [UniversalNameGeneratorSettings.cs](../NuciCraft.API/Configuration/UniversalNameGeneratorSettings.cs)
- [WebMapSettings.cs](../NuciCraft.API/Configuration/WebMapSettings.cs)

### Controllers

- [CountriesController.cs](../NuciCraft.API/Controllers/CountriesController.cs)
- [HomesController.cs](../NuciCraft.API/Controllers/HomesController.cs)
- [MobsController.cs](../NuciCraft.API/Controllers/MobsController.cs)
- [PlayersController.cs](../NuciCraft.API/Controllers/PlayersController.cs)
- [RtpLocationsController.cs](../NuciCraft.API/Controllers/RtpLocationsController.cs)
- [ServersController.cs](../NuciCraft.API/Controllers/ServersController.cs)
- [WorldsController.cs](../NuciCraft.API/Controllers/WorldsController.cs)
- [ZonesController.cs](../NuciCraft.API/Controllers/ZonesController.cs)
- [ZoneTypesController.cs](../NuciCraft.API/Controllers/ZoneTypesController.cs)

### Data Objects

- [CoordinatesDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/CoordinatesDataObject.cs)
- [CountryDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/CountryDataObject.cs)
- [HomeDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/HomeDataObject.cs)
- [LocalisedStringDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/LocalisedStringDataObject.cs)
- [NuciCraftEntityBase.cs](../NuciCraft.API/DataAccess/DataObjects/NuciCraftEntityBase.cs)
- [PlayerDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/PlayerDataObject.cs)
- [PlayerSettingsDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/PlayerSettingsDataObject.cs)
- [PlayerSettingsDataObjectExtensions.cs](../NuciCraft.API/DataAccess/DataObjects/PlayerSettingsDataObjectExtensions.cs)
- [RtpLocationEntity.cs](../NuciCraft.API/DataAccess/DataObjects/RtpLocationEntity.cs)
- [WorldDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/WorldDataObject.cs)
- [ZoneBoundsDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/ZoneBoundsDataObject.cs)
- [ZoneDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/ZoneDataObject.cs)
- [ZoneTypeDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/ZoneTypeDataObject.cs)

### Logging

- [MyLogInfoKey.cs](../NuciCraft.API/Logging/MyLogInfoKey.cs)
- [MyOperation.cs](../NuciCraft.API/Logging/MyOperation.cs)

### Requests

Country:
- [AddCountryRequest.cs](../NuciCraft.API/Requests/AddCountryRequest.cs)
- [GetCountriesRequest.cs](../NuciCraft.API/Requests/GetCountriesRequest.cs)
- [GetCountryRequest.cs](../NuciCraft.API/Requests/GetCountryRequest.cs)
- [PatchCountryRequest.cs](../NuciCraft.API/Requests/PatchCountryRequest.cs)

Home:
- [AddHomeRequest.cs](../NuciCraft.API/Requests/AddHomeRequest.cs)
- [GetHomeRequest.cs](../NuciCraft.API/Requests/GetHomeRequest.cs)
- [GetHomesRequest.cs](../NuciCraft.API/Requests/GetHomesRequest.cs)
- [PatchHomeRequest.cs](../NuciCraft.API/Requests/PatchHomeRequest.cs)

Mob and Server:
- [GetMobNameRequest.cs](../NuciCraft.API/Requests/GetMobNameRequest.cs)
- [GetServerRequest.cs](../NuciCraft.API/Requests/GetServerRequest.cs)

Player:
- [GetPlayerRequest.cs](../NuciCraft.API/Requests/GetPlayerRequest.cs)
- [GetPlayersRequest.cs](../NuciCraft.API/Requests/GetPlayersRequest.cs)
- [PatchPlayerRequest.cs](../NuciCraft.API/Requests/PatchPlayerRequest.cs)
- [PatchPlayerSettingsRequest.cs](../NuciCraft.API/Requests/PatchPlayerSettingsRequest.cs)
- [RegisterPlayerRequest.cs](../NuciCraft.API/Requests/RegisterPlayerRequest.cs)

RTP:
- [AddRtpLocationRequest.cs](../NuciCraft.API/Requests/AddRtpLocationRequest.cs)
- [GetRtpLocationRequest.cs](../NuciCraft.API/Requests/GetRtpLocationRequest.cs)

World:
- [AddWorldRequest.cs](../NuciCraft.API/Requests/AddWorldRequest.cs)
- [GetWorldRequest.cs](../NuciCraft.API/Requests/GetWorldRequest.cs)
- [GetWorldsRequest.cs](../NuciCraft.API/Requests/GetWorldsRequest.cs)
- [PatchWorldRequest.cs](../NuciCraft.API/Requests/PatchWorldRequest.cs)

Zone:
- [AddZoneRequest.cs](../NuciCraft.API/Requests/AddZoneRequest.cs)
- [GetZoneRequest.cs](../NuciCraft.API/Requests/GetZoneRequest.cs)
- [GetZonesByCategoryRequest.cs](../NuciCraft.API/Requests/GetZonesByCategoryRequest.cs)
- [GetZonesContainingCoordinatesRequest.cs](../NuciCraft.API/Requests/GetZonesContainingCoordinatesRequest.cs)
- [GetZonesRequest.cs](../NuciCraft.API/Requests/GetZonesRequest.cs)
- [PatchZoneRequest.cs](../NuciCraft.API/Requests/PatchZoneRequest.cs)

ZoneType:
- [AddZoneTypeRequest.cs](../NuciCraft.API/Requests/AddZoneTypeRequest.cs)
- [GetZoneTypeRequest.cs](../NuciCraft.API/Requests/GetZoneTypeRequest.cs)
- [GetZoneTypesRequest.cs](../NuciCraft.API/Requests/GetZoneTypesRequest.cs)
- [PatchZoneTypeRequest.cs](../NuciCraft.API/Requests/PatchZoneTypeRequest.cs)

### Responses

- [GetCountriesResponse.cs](../NuciCraft.API/Responses/GetCountriesResponse.cs)
- [GetCountryResponse.cs](../NuciCraft.API/Responses/GetCountryResponse.cs)
- [GetHomeResponse.cs](../NuciCraft.API/Responses/GetHomeResponse.cs)
- [GetHomesResponse.cs](../NuciCraft.API/Responses/GetHomesResponse.cs)
- [GetMobNameResponse.cs](../NuciCraft.API/Responses/GetMobNameResponse.cs)
- [GetPlayerResponse.cs](../NuciCraft.API/Responses/GetPlayerResponse.cs)
- [GetPlayersResponse.cs](../NuciCraft.API/Responses/GetPlayersResponse.cs)
- [GetRtpLocationResponse.cs](../NuciCraft.API/Responses/GetRtpLocationResponse.cs)
- [GetServerResponse.cs](../NuciCraft.API/Responses/GetServerResponse.cs)
- [GetWorldResponse.cs](../NuciCraft.API/Responses/GetWorldResponse.cs)
- [GetWorldsResponse.cs](../NuciCraft.API/Responses/GetWorldsResponse.cs)
- [GetZoneIdentifiersResponse.cs](../NuciCraft.API/Responses/GetZoneIdentifiersResponse.cs)
- [GetZoneResponse.cs](../NuciCraft.API/Responses/GetZoneResponse.cs)
- [GetZonesResponse.cs](../NuciCraft.API/Responses/GetZonesResponse.cs)
- [GetZoneTypeResponse.cs](../NuciCraft.API/Responses/GetZoneTypeResponse.cs)
- [GetZoneTypesResponse.cs](../NuciCraft.API/Responses/GetZoneTypesResponse.cs)

### Service Contracts And Implementations

- [ICountryService.cs](../NuciCraft.API/Service/ICountryService.cs) and [CountryService.cs](../NuciCraft.API/Service/CountryService.cs)
- [IHomeService.cs](../NuciCraft.API/Service/IHomeService.cs) and [HomeService.cs](../NuciCraft.API/Service/HomeService.cs)
- [IMobService.cs](../NuciCraft.API/Service/IMobService.cs) and [MobService.cs](../NuciCraft.API/Service/MobService.cs)
- [IPlayerService.cs](../NuciCraft.API/Service/IPlayerService.cs) and [PlayerService.cs](../NuciCraft.API/Service/PlayerService.cs)
- [IRtpLocationService.cs](../NuciCraft.API/Service/IRtpLocationService.cs) and [RtpLocationService.cs](../NuciCraft.API/Service/RtpLocationService.cs)
- [IServerStatusService.cs](../NuciCraft.API/Service/IServerStatusService.cs) and [ServerStatusService.cs](../NuciCraft.API/Service/ServerStatusService.cs)
- [IWorldService.cs](../NuciCraft.API/Service/IWorldService.cs) and [WorldService.cs](../NuciCraft.API/Service/WorldService.cs)
- [IZoneService.cs](../NuciCraft.API/Service/IZoneService.cs) and [ZoneService.cs](../NuciCraft.API/Service/ZoneService.cs)
- [IZoneTypeService.cs](../NuciCraft.API/Service/IZoneTypeService.cs) and [ZoneTypeService.cs](../NuciCraft.API/Service/ZoneTypeService.cs)
- [GenerateNamesRequest.cs](../NuciCraft.API/Service/GenerateNamesRequest.cs)
- [GenerateNamesResponse.cs](../NuciCraft.API/Service/GenerateNamesResponse.cs)
- [TimestampFormats.cs](../NuciCraft.API/Service/TimestampFormats.cs)

### Service Models

- [Coordinates.cs](../NuciCraft.API/Service/Models/Coordinates.cs)
- [Country.cs](../NuciCraft.API/Service/Models/Country.cs)
- [Gender.cs](../NuciCraft.API/Service/Models/Gender.cs)
- [Home.cs](../NuciCraft.API/Service/Models/Home.cs)
- [Localisation.cs](../NuciCraft.API/Service/Models/Localisation.cs)
- [LocalisedString.cs](../NuciCraft.API/Service/Models/LocalisedString.cs)
- [MobType.cs](../NuciCraft.API/Service/Models/MobType.cs)
- [Player.cs](../NuciCraft.API/Service/Models/Player.cs)
- [PlayerSettings.cs](../NuciCraft.API/Service/Models/PlayerSettings.cs)
- [RtpLocation.cs](../NuciCraft.API/Service/Models/RtpLocation.cs)
- [World.cs](../NuciCraft.API/Service/Models/World.cs)
- [WorldType.cs](../NuciCraft.API/Service/Models/WorldType.cs)
- [Zone.cs](../NuciCraft.API/Service/Models/Zone.cs)
- [ZoneBounds.cs](../NuciCraft.API/Service/Models/ZoneBounds.cs)
- [ZoneType.cs](../NuciCraft.API/Service/Models/ZoneType.cs)

### Mappings And Helpers

- [CoordinatesMappingExtensions.cs](../NuciCraft.API/Service/Mapping/CoordinatesMappingExtensions.cs)
- [CountryMappingExtensions.cs](../NuciCraft.API/Service/Mapping/CountryMappingExtensions.cs)
- [LocalisedStringMappingExtensions.cs](../NuciCraft.API/Service/Mapping/LocalisedStringMappingExtensions.cs)
- [PlayerMappingExtensions.cs](../NuciCraft.API/Service/Mapping/PlayerMappingExtensions.cs)
- [PlayerSettingsMappingExtensions.cs](../NuciCraft.API/Service/Mapping/PlayerSettingsMappingExtensions.cs)
- [RtpLocationMappingExtensions.cs](../NuciCraft.API/Service/Mapping/RtpLocationMappingExtensions.cs)
- [WorldMappingExtensions.cs](../NuciCraft.API/Service/Mapping/WorldMappingExtensions.cs)
- [ZoneBoundsMappingExtensions.cs](../NuciCraft.API/Service/Mapping/ZoneBoundsMappingExtensions.cs)
- [ZoneMappingExtensions.cs](../NuciCraft.API/Service/Mapping/ZoneMappingExtensions.cs)
- [ZoneTypeMappingExtensions.cs](../NuciCraft.API/Service/Mapping/ZoneTypeMappingExtensions.cs)
- [HomeMappingExtensions.cs](../NuciCraft.API/Service/Mappings/HomeMappingExtensions.cs)
- [LocalisedStringDataObjectExtensions.cs](../NuciCraft.API/Service/Helpers/LocalisedStringDataObjectExtensions.cs)

## Unit-Test Source Map

### Composition And Helpers

- [ProgramTests.cs](../NuciCraft.API.UnitTests/ProgramTests.cs)
- [StartupTests.cs](../NuciCraft.API.UnitTests/StartupTests.cs)
- [ServiceCollectionExtensionsTests.cs](../NuciCraft.API.UnitTests/ServiceCollectionExtensionsTests.cs)
- [TestConfigurationFactory.cs](../NuciCraft.API.UnitTests/TestConfigurationFactory.cs)

### Controller Fixtures

- [ControllerTestContext.cs](../NuciCraft.API.UnitTests/Controllers/ControllerTestContext.cs)
- [CountriesControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/CountriesControllerTests.cs)
- [HomesControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/HomesControllerTests.cs)
- [MobsControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/MobsControllerTests.cs)
- [PlayersControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/PlayersControllerTests.cs)
- [RtpLocationsControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/RtpLocationsControllerTests.cs)
- [ServersControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/ServersControllerTests.cs)
- [WorldsControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/WorldsControllerTests.cs)
- [ZonesControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/ZonesControllerTests.cs)
- [ZoneTypesControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/ZoneTypesControllerTests.cs)

### Service And Model Fixtures

- [CountryServiceTests.cs](../NuciCraft.API.UnitTests/Service/CountryServiceTests.cs)
- [GenderTests.cs](../NuciCraft.API.UnitTests/Service/GenderTests.cs)
- [GenerateNamesResponseTests.cs](../NuciCraft.API.UnitTests/Service/GenerateNamesResponseTests.cs)
- [HomeServiceTests.cs](../NuciCraft.API.UnitTests/Service/HomeServiceTests.cs)
- [LocalisationTests.cs](../NuciCraft.API.UnitTests/Service/LocalisationTests.cs)
- [MobServiceTests.cs](../NuciCraft.API.UnitTests/Service/MobServiceTests.cs)
- [MobTypeTests.cs](../NuciCraft.API.UnitTests/Service/MobTypeTests.cs)
- [PlayerServiceTests.cs](../NuciCraft.API.UnitTests/Service/PlayerServiceTests.cs)
- [PlayerSettingsDataObjectTests.cs](../NuciCraft.API.UnitTests/Service/PlayerSettingsDataObjectTests.cs)
- [PlayerSettingsTests.cs](../NuciCraft.API.UnitTests/Service/PlayerSettingsTests.cs)
- [PlayerTests.cs](../NuciCraft.API.UnitTests/Service/PlayerTests.cs)
- [RtpLocationServiceTests.cs](../NuciCraft.API.UnitTests/Service/RtpLocationServiceTests.cs)
- [ServerStatusServiceTests.cs](../NuciCraft.API.UnitTests/Service/ServerStatusServiceTests.cs)
- [WorldServiceTests.cs](../NuciCraft.API.UnitTests/Service/WorldServiceTests.cs)
- [WorldTypeTests.cs](../NuciCraft.API.UnitTests/Service/WorldTypeTests.cs)
- [ZoneServiceTests.cs](../NuciCraft.API.UnitTests/Service/ZoneServiceTests.cs)
- [ZoneTypeServiceTests.cs](../NuciCraft.API.UnitTests/Service/ZoneTypeServiceTests.cs)

### Mapping, Response, And Logging Fixtures

- [MappingExtensionsTests.cs](../NuciCraft.API.UnitTests/Service/Mapping/MappingExtensionsTests.cs)
- [MappingMethodInvoker.cs](../NuciCraft.API.UnitTests/Service/Mapping/MappingMethodInvoker.cs)
- [GetCountryResponseTests.cs](../NuciCraft.API.UnitTests/Responses/GetCountryResponseTests.cs)
- [GetPlayerResponseTests.cs](../NuciCraft.API.UnitTests/Responses/GetPlayerResponseTests.cs)
- [GetRtpLocationResponseTests.cs](../NuciCraft.API.UnitTests/Responses/GetRtpLocationResponseTests.cs)
- [GetWorldResponseTests.cs](../NuciCraft.API.UnitTests/Responses/GetWorldResponseTests.cs)
- [GetWorldsResponseTests.cs](../NuciCraft.API.UnitTests/Responses/GetWorldsResponseTests.cs)
- [GetZoneResponseTests.cs](../NuciCraft.API.UnitTests/Responses/GetZoneResponseTests.cs)
- [GetZonesResponseTests.cs](../NuciCraft.API.UnitTests/Responses/GetZonesResponseTests.cs)
- [GetZoneTypeResponseTests.cs](../NuciCraft.API.UnitTests/Responses/GetZoneTypeResponseTests.cs)
- [MyLogInfoKeyTests.cs](../NuciCraft.API.UnitTests/Logging/MyLogInfoKeyTests.cs)

## Integration-Test Source Map

- [ApiErrorResponseTests.cs](../NuciCraft.API.IntegrationTests/ApiErrorResponseTests.cs)
- [ApiRequestExtensions.cs](../NuciCraft.API.IntegrationTests/ApiRequestExtensions.cs)
- [ApiTestHost.cs](../NuciCraft.API.IntegrationTests/ApiTestHost.cs)
- [CountriesApiTests.cs](../NuciCraft.API.IntegrationTests/CountriesApiTests.cs)
- [HomesApiTests.cs](../NuciCraft.API.IntegrationTests/HomesApiTests.cs)
- [MobsApiTests.cs](../NuciCraft.API.IntegrationTests/MobsApiTests.cs)
- [PersistenceApiTests.cs](../NuciCraft.API.IntegrationTests/PersistenceApiTests.cs)
- [PlayersApiTests.cs](../NuciCraft.API.IntegrationTests/PlayersApiTests.cs)
- [RtpLocationsApiTests.cs](../NuciCraft.API.IntegrationTests/RtpLocationsApiTests.cs)
- [ServersApiTests.cs](../NuciCraft.API.IntegrationTests/ServersApiTests.cs)
- [ServerStatusServiceTests.cs](../NuciCraft.API.IntegrationTests/ServerStatusServiceTests.cs)
- [WorldsApiTests.cs](../NuciCraft.API.IntegrationTests/WorldsApiTests.cs)
- [ZonesApiTests.cs](../NuciCraft.API.IntegrationTests/ZonesApiTests.cs)
- [ZoneTypesApiTests.cs](../NuciCraft.API.IntegrationTests/ZoneTypesApiTests.cs)

## Repository Metadata Coverage

| File | Documentation |
|------|---------------|
| [NuciCraft.API.slnx](../NuciCraft.API.slnx) | Repository structure and build. |
| [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj) | Dependencies and build. |
| [NuciCraft.API.UnitTests.csproj](../NuciCraft.API.UnitTests/NuciCraft.API.UnitTests.csproj) | Testing and dependencies. |
| [NuciCraft.API.IntegrationTests.csproj](../NuciCraft.API.IntegrationTests/NuciCraft.API.IntegrationTests.csproj) | Testing and dependencies. |
| [.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml) | Build and deployment. |
| [.vscode/tasks.json](../.vscode/tasks.json) | Build and deployment. |
| [.vscode/launch.json](../.vscode/launch.json) | Build and deployment. |
| [release.sh](../release.sh) | Build, integrations, security, and ambiguity register. |
| [.gitignore](../.gitignore) | Repository structure, state, and security. |
| [README.md](../README.md) | Public project documentation. |
| [ARCHITECTURE.md](../ARCHITECTURE.md) | Root architectural synopsis. |
| [SECURITY.md](../SECURITY.md) | Vulnerability-reporting policy and security scope. |
| [.github/FUNDING.yml](../.github/FUNDING.yml) | Repository funding metadata; no runtime consequence. |
| [LICENSE](../LICENSE) | Licence policy; no runtime consequence. |

## Audit Omissions Resolved During Composition

The basename scan initially found transport DTOs, response DTOs, `CoordinatesMappingExtensions`, and `LocalisedStringMappingExtensions` without direct filename references. Their semantics were already represented by the HTTP and data-model documents, but this source map now links every one explicitly.

The test audit initially found many fixtures represented only by category. This document now links every tracked test C# file while [testing.md](testing.md) remains the semantic test-architecture owner.

## Known Coverage Limitations

This corpus intentionally does not assert 100 percent behavioural coverage. Residual limitations are:
- External NuciAPI, NuciDAL, NuciLog, NuciSecurity, NuciText, MineStat, and NuciExtensions internals are not contained in this repository.
- The mutable remote release script is unavailable from local source at audit time.
- Deployment infrastructure, TLS termination, secret providers, backups, and process supervision are not defined here.
- Ignored local operational data is not inspected or reproduced because it can contain personal and credential data and is not durable source evidence.
- No live Universal Name Generator contract test validates the deployed remote service.
- No multi-process file concurrency or interrupted-write test establishes persistence guarantees.
- No generated OpenAPI description exists for independent route comparison.
- Test existence does not demonstrate every branch or package interaction; no current measured coverage report is tracked.
- Mermaid rendering is not enforced by repository CI.

Future audits must revise this list when evidence changes rather than converting absence of detected gaps into a completeness guarantee.
