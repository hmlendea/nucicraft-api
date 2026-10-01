# Interfaces And Contracts

| Metadata | Value |
|----------|-------|
| Purpose | Catalogue significant inbound HTTP, internal service, filesystem, configuration, and outbound network contracts. |
| Scope | Public routes and process boundaries visible from repository source. Package internals are identified as external. |
| Primary sources | [Controllers](../NuciCraft.API/Controllers/), [Service interfaces](../NuciCraft.API/Service/), [appsettings.json](../NuciCraft.API/appsettings.json), and [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs) |
| Related documents | [HTTP component](components/http-api-boundary.md), [data model](data-model.md), [integrations](integrations.md), and [security](security.md) |

## Inbound HTTP Contract

### Common Semantics

- Producer: NuciCraft API controllers.
- Consumers: clients possessing the configured service-wide API key.
- Authentication hand-off: bearer API key through `NuciApiAuthorisation.ApiKey` and `ProcessRequest`.
- Request format: route and query values plus JSON bodies where applicable.
- Success format: package-defined Nuci API response, optionally containing local response content.
- Validation: ASP.NET Core data annotations plus service validation.
- Side effects: commands synchronously save one repository; external queries synchronously contact a remote system.
- Versioning: no route version segment or compatibility negotiation.

### Endpoint Contract Matrix

| Method And Route | Input Contract | Output Content | Side Effect Or Query |
|------------------|----------------|----------------|----------------------|
| `POST /Countries` | AddCountryRequest | None | Add and save Country. |
| `GET /Countries/{id}` | Route id | GetCountryResponse | Direct Country query. |
| `GET /Countries` | None | GetCountriesResponse | Enumerate Countries. |
| `PATCH /Countries/{id}` | PatchCountryRequest | None | Merge and save Country. |
| `POST /Homes` | AddHomeRequest | GetHomeResponse | Validate owner and names; add and save Home. |
| `DELETE /Homes/{id}` | Route id | None | Remove and save Home. |
| `GET /Homes/{id}` | Route id | GetHomeResponse | Direct Home query. |
| `GET /Homes` | Optional player and name | Single or collection content | Branch among lookup, player filter, and all. |
| `GET /Homes/by-player/{id}` | Player id | GetHomesResponse | Filter Homes. |
| `PATCH /Homes/{id}` | PatchHomeRequest | GetHomeResponse | Merge, validate, and save Home. |
| `GET /Mobs/{type}/random-name` | Optional count | GetMobNameResponse | Outbound generator HTTP request. |
| `POST /Players` | RegisterPlayerRequest | None | Generate identity and save Player. |
| `GET /Players/{id}` | Identifier | GetPlayerResponse | Find Player. |
| `GET /Players/by-username/{value}` | Username | GetPlayerResponse | Find Player. |
| `GET /Players/by-offline-uuid/{value}` | Offline UUID | GetPlayerResponse | Find Player. |
| `GET /Players/by-online-uuid/{value}` | Online UUID | GetPlayerResponse | Find Player. |
| `GET /Players` | None | GetPlayersResponse | Enumerate Players. |
| Four Player PATCH selector routes | PatchPlayerRequest | None | Select, merge, and save Player. |
| `POST /RtpLocations` | AddRtpLocationRequest | None | Validate proximity; add and save. |
| `GET /RtpLocations/random` | Optional World and Biome | GetRtpLocationResponse | Filter and select random record. |
| `GET /Server` | None | GetServerResponse | Live Java TCP query plus settings. |
| `POST /Worlds` | AddWorldRequest | None | Add and save World. |
| `GET /Worlds/{id}` | Route id | GetWorldResponse | Direct World query. |
| `GET /Worlds` | None | GetWorldsResponse | Enumerate Worlds. |
| `PATCH /Worlds/{id}` | PatchWorldRequest | None | Merge and save World. |
| `POST /Zones` | AddZoneRequest | None | Validate references and bounds; add and save. |
| `DELETE /Zones/{id}` | Route id | None | Remove and save Zone. |
| `GET /Zones/{id}` | Route id | GetZoneResponse | Query, canonicalise, and enrich Zone. |
| `GET /Zones` | Optional Type | GetZonesResponse | Filter, canonicalise, and enrich. |
| `GET /Zones/by-category/{category}` | Category | GetZonesResponse | Join ZoneTypes to Zones. |
| `GET /Zones/by-coordinates` | World, X, Y, Z | GetZoneIdentifiersResponse | Inclusive cuboid containment. |
| `PATCH /Zones/{id}` | PatchZoneRequest | None | Validate, merge, canonicalise, and save. |
| `POST /ZoneTypes` | AddZoneTypeRequest | None | Add and save ZoneType. |
| `GET /ZoneTypes/{id}` | Route id | GetZoneTypeResponse | Direct ZoneType query. |
| `GET /ZoneTypes` | Optional Category | GetZoneTypesResponse | Optional category filter. |
| `PATCH /ZoneTypes/{id}` | PatchZoneTypeRequest | None | Merge and save ZoneType. |

The Player PATCH matrix row represents four concrete routes documented in [PlayersController.cs](../NuciCraft.API/Controllers/PlayersController.cs). The detailed route and branch catalogue is in [http-api-boundary.md](components/http-api-boundary.md).

### Error Contract

Confirmed observable status semantics are:
- `401` for absent or invalid API keys.
- `400` for missing required properties and malformed JSON.
- `404` for an absent direct record in tested World paths.
- `501` for unsupported MobType in integration tests.

Domain argument failures, repository exceptions, and external failures pass through package-supplied exception middleware. The local repository does not define the complete error body or every mapping.

### Compatibility

The subsequent elements are compatibility-sensitive:
- Route casing and pluralisation, especially singular `/Server`.
- JSON field names and response envelopes.
- HMAC order annotations.
- Polymorphic `GET /Homes` content based on query keys.
- Player response exposure and data types.
- Enum-like model serialisation supplied by System.Text.Json and public properties.

## Internal Service Contracts

| Interface | Consumers | Methods | Side Effects |
|-----------|-----------|---------|--------------|
| [ICountryService](../NuciCraft.API/Service/ICountryService.cs) | CountriesController | Add, Get, GetAll, Update | Country repository writes. |
| [IHomeService](../NuciCraft.API/Service/IHomeService.cs) | HomesController | Add, Delete, two Get forms, GetAll, GetAllByPlayer, Update | Home repository writes under lock. |
| [IMobService](../NuciCraft.API/Service/IMobService.cs) | MobsController | GetRandomMobName | Outbound HTTP call. |
| [IPlayerService](../NuciCraft.API/Service/IPlayerService.cs) | PlayersController, HomeService | Register, Get, GetAll, Update | Player repository writes. |
| [IRtpLocationService](../NuciCraft.API/Service/IRtpLocationService.cs) | RtpLocationsController | AddRtpLocation, GetRtpLocation | RTP writes or random query. |
| [IServerStatusService](../NuciCraft.API/Service/IServerStatusService.cs) | ServersController | GetOnlinePlayersCount | Outbound TCP query. |
| [IWorldService](../NuciCraft.API/Service/IWorldService.cs) | WorldsController | Add, GetWorld, GetAllWorlds, Update | World repository writes. |
| [IZoneService](../NuciCraft.API/Service/IZoneService.cs) | ZonesController | Add, Delete, GetZone, GetAllZones, category query, coordinate query, Update | Zone writes and cross-store reads. |
| [IZoneTypeService](../NuciCraft.API/Service/IZoneTypeService.cs) | ZoneTypesController | Add, GetZoneType, GetAllZoneTypes, Update | ZoneType repository writes. |

All methods are synchronous and accept no cancellation token. Exceptions propagate to callers after service logging where implemented.

## Repository Contract

Producer: external NuciDAL package. Consumers: application services and startup.

Locally used operations are:
- `Get(id)`
- `GetAll()`
- `Add(record)`
- `Update(record)`
- `Remove(id)`
- `SaveChanges()`

One singleton `IFileRepository<T>` maps to one configured JSON file. Exact duplicate-id, write atomicity, file-locking, enumeration snapshot, external-edit detection, and disposal semantics are not defined by local source.

## Filesystem Contract

The process requires seven path strings. Before request handling it creates absent parent directories and absent files containing `[]`, then repositories must enumerate every file successfully.

Deployment must provide:
- Read and write access.
- Durable storage when persistence across restarts is required.
- Protected access for personal and credential data.
- Exclusive or otherwise coordinated writer topology.
- Backup and restoration external to the application.

The application provides no migration or schema file.

## Configuration Contract

Configuration sections are named for settings classes in lower camel case. Nested environment variables use the standard double-underscore separator, such as `securitySettings__apiKey`. Values bind once into singleton objects during registration.

The complete inventory is in [configuration.md](configuration.md).

## Universal Name Generator Contract

| Aspect | Contract |
|--------|----------|
| Producer | `MobService` through `INuciApiClient`. |
| Consumer | Configured Universal Name Generator API. |
| Method and endpoint | GET `Names`, interpreted relative to configured BaseUrl. |
| Input | Query/request model with lower-case `schema` and `count`. |
| Authentication | Outbound bearer token from generator ApiKey. |
| Output | `GenerateNamesResponse` derived from `NuciApiSuccessResponse`, with top-level `names`. |
| Validation | Local success flag, runtime type, non-vacant collection, and non-whitespace elements. |
| Failure | Invalid operation or propagated client exception; no retry. |

## Minecraft Java Status Contract

| Aspect | Contract |
|--------|----------|
| Producer | `ServerStatusService` through MineStat. |
| Consumer | Minecraft Java server or compatible proxy at configured Hostname and Java port. |
| Protocol | Java legacy server-list status over TCP. |
| Timeout | Five seconds. |
| Input | Status handshake; integration test confirms initial bytes `0xfe 0x01`. |
| Output | Non-negative online player count. |
| Fallback | Zero for I/O or socket construction failure and unavailable status. |
| Failure | Invalid settings, negative count, and some parse failures propagate. |

## Logging Contract

Services call NuciLog `ILogger` with an operation, `OperationStatus`, optional structured `LogInfo`, and exception on failure. [MyOperation.cs](../NuciCraft.API/Logging/MyOperation.cs) defines 31 operations. [MyLogInfoKey.cs](../NuciCraft.API/Logging/MyLogInfoKey.cs) defines context names.

Logging is operational observation, not an audit transaction. Some context can contain personal or location data.

## Absent Interfaces

The repository exposes no CLI, message queue, event stream, webhook, public library package, plug-in interface, health endpoint, OpenAPI document, or background scheduling contract.
