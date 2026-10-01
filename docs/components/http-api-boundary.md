# HTTP API Boundary Component

| Metadata | Value |
|----------|-------|
| Purpose | Document controller ownership, route and transport contracts, authorisation hand-off, response construction, and compatibility risks. |
| Scope | All controllers, request DTOs, response content types, and their package-supplied processing boundary. |
| Primary sources | [Controllers](../../NuciCraft.API/Controllers/), [Requests](../../NuciCraft.API/Requests/), [Responses](../../NuciCraft.API/Responses/), and [controller tests](../../NuciCraft.API.UnitTests/Controllers/) |
| Related documents | [Interfaces](../interfaces.md), [error handling](../error-handling.md), [security](../security.md), and [file-backed request flow](../flows/file-backed-request.md) |

## Purpose

The HTTP boundary presents NuciCraft capabilities as authenticated, unversioned REST-style routes. It converts transport values into application requests, selects one service operation, and constructs standard Nuci API responses.

## Scope

This component owns:
- Attribute routes and HTTP verbs.
- Route, query, and JSON body binding.
- Transport-required and range validation.
- Selected JSON property aliases.
- HMAC ordering annotations.
- API-key authorisation descriptors.
- Success response content construction.

It does not own:
- API-key validation internals.
- Domain validation.
- Persistence or external network operations.
- Exception-to-status translation internals.
- HMAC computation.

## Position In The Architecture

This is the inbound adapter. It depends on service interfaces and transport types and is invoked by ASP.NET Core after middleware routing. It is the only public entry point implemented by the repository.

## Dependencies

- ASP.NET Core MVC for routing, model binding, `ApiController` conduct, and action results.
- `NuciApiController.ProcessRequest` for request processing, authorisation, and common responses.
- `NuciApiAuthorisation.ApiKey` for service-wide bearer credential configuration.
- `NuciApiContentResponse<T>` and `NuciApiResponseContent` for success envelopes.
- NuciSecurity HMAC attributes for canonical field metadata.
- Nine application service interfaces.

## Dependants

- External API clients depend on route, JSON, status, and HMAC compatibility.
- Integration tests exercise this boundary through `WebApplicationFactory`.
- Controller unit tests depend on request construction and service delegation.
- Middleware observes every operation entering this boundary.

## Internal Structure

### Controllers

All nine controllers are sealed primary-constructor classes derived from `NuciApiController`. Each constructs one private authorisation descriptor from `SecuritySettings.ApiKey`.

### Requests

Request classes derive from `NuciApiRequest` except nested `PatchPlayerSettingsRequest`. They use:
- `Required` for transport-mandatory reference values.
- `Range` for mob-name count.
- nullable value types where a patch must distinguish omission from zero or false.
- custom setter flags for non-nullable player-setting booleans.
- `JsonPropertyName` for selected wire names.
- `HmacOrder` for signing order metadata.

### Responses

Single-record response types derive from `NuciApiResponseContent` or project into service models. Collection response types expose an enumerable and a computed `Count` marked `HmacIgnore`.

The response envelope itself is supplied by NuciAPI and is not reimplemented locally.

## Endpoint Catalogue

### Countries

| Method | Route | Input | Service Transition | Success Content |
|--------|-------|-------|--------------------|-----------------|
| POST | `/Countries` | `AddCountryRequest` body | `ICountryService.Add` | Base success response. |
| GET | `/Countries/{countryIdentifier}` | Route to `GetCountryRequest.Identifier` | `ICountryService.Get` | `GetCountryResponse`. |
| GET | `/Countries` | Empty `GetCountriesRequest` | `ICountryService.GetAll` | `GetCountriesResponse`. |
| PATCH | `/Countries/{countryIdentifier}` | Route overwrites body `Identifier` | `ICountryService.Update` | Base success response. |

### Homes

| Method | Route | Input | Service Transition | Success Content |
|--------|-------|-------|--------------------|-----------------|
| POST | `/Homes` | `AddHomeRequest` body | `IHomeService.Add` | `GetHomeResponse` for the created Home. |
| DELETE | `/Homes/{homeIdentifier}` | Route to `GetHomeRequest.Identifier` | `IHomeService.Delete` | Base success response. |
| GET | `/Homes/{homeIdentifier}` | Route to `GetHomeRequest.Identifier` | `IHomeService.Get(identifier)` | `GetHomeResponse`. |
| GET | `/Homes` | `GetHomesRequest` query | Conditional branch described below | Single or collection content. |
| GET | `/Homes/by-player/{playerIdentifier}` | Route to `GetHomesRequest.Player` | `IHomeService.GetAllByPlayer` | `GetHomesResponse`. |
| PATCH | `/Homes/{homeIdentifier}` | Route assigned to ignored body `Identifier` | `IHomeService.Update` | `GetHomeResponse` for the revised Home. |

`GET /Homes` is polymorphic:
1. If the `name` query key exists, even with a null-bound value, call `Get(player, name)` and return one Home.
2. Otherwise, if the `player` query key exists, call the by-player action and return a collection.
3. Otherwise, return every Home.

Name lookup consequently requires both a valid player and name even though neither property has a `Required` attribute on `GetHomesRequest`.

### Mobs

| Method | Route | Input | Service Transition | Success Content |
|--------|-------|-------|--------------------|-----------------|
| GET | `/Mobs/{mobType}/random-name?count={count}` | Required route type; optional count from 1 through 100000, default one | `IMobService.GetRandomMobName` | `GetMobNameResponse`. |

### Players

| Method | Route | Selector | Service Transition | Success Content |
|--------|-------|----------|--------------------|-----------------|
| POST | `/Players` | `RegisterPlayerRequest` body | `IPlayerService.Register` | Base success response. |
| GET | `/Players/{identifier}` | Identifier | `IPlayerService.Get` | `GetPlayerResponse`. |
| GET | `/Players/by-username/{username}` | Username | `IPlayerService.Get` | `GetPlayerResponse`. |
| GET | `/Players/by-offline-uuid/{offlineUUID}` | Offline UUID | `IPlayerService.Get` | `GetPlayerResponse`. |
| GET | `/Players/by-online-uuid/{onlineUUID}` | Online UUID | `IPlayerService.Get` | `GetPlayerResponse`. |
| GET | `/Players` | None | `IPlayerService.GetAll` | `GetPlayersResponse`. |
| PATCH | `/Players/{playerIdentifier}` | Identifier | `IPlayerService.Update` | Base success response. |
| PATCH | `/Players/by-username/{username}` | Username | `IPlayerService.Update` | Base success response. |
| PATCH | `/Players/by-offline-uuid/{offlineUUID}` | Offline UUID | `IPlayerService.Update` | Base success response. |
| PATCH | `/Players/by-online-uuid/{onlineUUID}` | Online UUID | `IPlayerService.Update` | Base success response. |

Patch routes assign their route selector into the body object. If a client body supplies another selector, service validation observes more than one selector and rejects the request.

### RTP Locations

| Method | Route | Input | Service Transition | Success Content |
|--------|-------|-------|--------------------|-----------------|
| POST | `/RtpLocations` | `AddRtpLocationRequest` body | `IRtpLocationService.AddRtpLocation` | Base success response. |
| GET | `/RtpLocations/random` | Optional World and Biome query values | `IRtpLocationService.GetRtpLocation` | `GetRtpLocationResponse`. |

### Server

| Method | Route | Input | Operation | Success Content |
|--------|-------|-------|-----------|-----------------|
| GET | `/Server` | Empty `GetServerRequest` | Read `ServerSettings`; call `IServerStatusService.GetOnlinePlayersCount` | `GetServerResponse`. |

The singular route is explicit and does not derive from the plural controller name.

### Worlds

| Method | Route | Input | Service Transition | Success Content |
|--------|-------|-------|--------------------|-----------------|
| POST | `/Worlds` | `AddWorldRequest` body | `IWorldService.Add` | Base success response. |
| GET | `/Worlds/{worldIdentifier}` | Route identifier | `IWorldService.GetWorld` | `GetWorldResponse`. |
| GET | `/Worlds` | Empty `GetWorldsRequest` | `IWorldService.GetAllWorlds` | `GetWorldsResponse`. |
| PATCH | `/Worlds/{worldIdentifier}` | Route overwrites body `Identifier` | `IWorldService.Update` | Base success response. |

### Zones

| Method | Route | Input | Service Transition | Success Content |
|--------|-------|-------|--------------------|-----------------|
| POST | `/Zones` | `AddZoneRequest` body | `IZoneService.Add` | Base success response. |
| DELETE | `/Zones/{zoneIdentifier}` | Route identifier | `IZoneService.Delete` | Base success response. |
| GET | `/Zones/{zoneIdentifier}` | Route identifier | `IZoneService.GetZone` | `GetZoneResponse`. |
| GET | `/Zones?type={type}` | Direct optional action parameter | `IZoneService.GetAllZones(type)` | `GetZonesResponse`. |
| GET | `/Zones/by-category/{category}` | Required route category | `IZoneService.GetZonesByCategory` | `GetZonesResponse`. |
| GET | `/Zones/by-coordinates?world={w}&x={x}&y={y}&z={z}` | Required World and nullable required numeric values | `IZoneService.GetZoneIdentifiersContainingCoordinates` | `GetZoneIdentifiersResponse`. |
| PATCH | `/Zones/{zoneIdentifier}` | Route overwrites body `Identifier` | `IZoneService.Update` | Base success response. |

`GET /Zones` passes an empty `GetZonesRequest` to `ProcessRequest`; its direct `type` action parameter is not represented in that request object. Coordinate values are nullable for model validation and dereferenced with `.Value` only inside the service delegate.

### Zone Types

| Method | Route | Input | Service Transition | Success Content |
|--------|-------|-------|--------------------|-----------------|
| POST | `/ZoneTypes` | `AddZoneTypeRequest` body | `IZoneTypeService.Add` | Base success response. |
| GET | `/ZoneTypes/{zoneTypeIdentifier}` | Route identifier | `IZoneTypeService.GetZoneType` | `GetZoneTypeResponse`. |
| GET | `/ZoneTypes?category={category}` | Direct optional action parameter | `IZoneTypeService.GetAllZoneTypes(category)` | `GetZoneTypesResponse`. |
| PATCH | `/ZoneTypes/{zoneTypeIdentifier}` | Route overwrites body `Identifier` | `IZoneTypeService.Update` | Base success response. |

As with Zone type filtering, the category value is not represented in the empty `GetZoneTypesRequest` passed to `ProcessRequest`.

## Behaviour

Every action follows the same outer sequence:
1. ASP.NET Core binds values and executes `ApiController` model validation.
2. The controller constructs or adjusts a request object.
3. The controller passes the request, operation delegate, and API-key descriptor to `ProcessRequest`.
4. Package-owned processing validates authorisation and invokes the delegate.
5. The delegate invokes one service method and optionally projects response content.
6. Package-owned processing produces the HTTP result or translates an exception.

## Inputs And Outputs

Important transport conventions are:
- JSON naming follows the configured ASP.NET Core serializer except explicit `id`, `type`, `count`, and outbound name-generator aliases.
- Empty command success responses originate in NuciAPI.
- Retrieval content appears within `NuciApiContentResponse<T>`.
- Player retrieval includes password and personal data.
- Collection count is computed by enumerating the collection and excluded from HMAC ordering.

## State

Controllers hold only constructor-injected dependencies and a private authorisation descriptor. They are framework-created and do not own persistent state. Request and response instances have request duration.

## Error Handling

Confirmed integration-level results are:
- Missing bearer credentials: `401 Unauthorized`.
- Invalid API key: `401 Unauthorized`.
- Missing data-annotation-required body properties: `400 Bad Request`.
- Malformed JSON: `400 Bad Request` with `application/problem+json`.
- Missing World record: `404 Not Found`.
- Unsupported Mob type: tested as `501 Not Implemented` through package translation.

Other exception mappings are inferred from package use and service exception types, not locally defined. Consult [error-handling.md](../error-handling.md).

## Configuration

- `SecuritySettings.ApiKey` controls the authorisation descriptor for every route.
- `ServerSettings` supplies four `/Server` response fields and Java query coordinates.
- No route prefix, API version, CORS policy, serializer option, or maximum body size is configured locally.

## Extension And Modification Guidance

When adding an endpoint:
1. Define or reuse a request DTO with transport validation and deliberate HMAC ordering.
2. Add a response content type when the operation returns data.
3. Add a service-interface method and implementation when the capability contains application logic.
4. Pass the identical API-key descriptor through `ProcessRequest` unless an intentional security redesign is in scope.
5. Add controller delegation tests and an authorised HTTP integration test.
6. Add authorisation and invalid-input tests when the contract introduces a novel path.
7. Update the interface catalogue, behavioural document, flow, and data model.

Do not trust a body selector when the route identifies the resource. Follow the established route-assignment pattern and validate conflicting selectors where relevant.

## Relevant Processes

- [File-backed request flow](../flows/file-backed-request.md)
- [Player and Home flow](../flows/player-and-home-requests.md)
- [Zone flow](../flows/zone-requests.md)
- [External request flow](../flows/external-requests.md)
