# Player And Home Components

| Metadata | Value |
|----------|-------|
| Purpose | Document Player lifecycle and player-owned Home management, including selectors, settings, identifiers, localisation, locking, and persistence. |
| Scope | `PlayerService`, `HomeService`, their HTTP contracts, service models, data objects, mappings, and direct tests. |
| Primary sources | [PlayerService.cs](../../NuciCraft.API/Service/PlayerService.cs), [HomeService.cs](../../NuciCraft.API/Service/HomeService.cs), [PlayersController.cs](../../NuciCraft.API/Controllers/PlayersController.cs), and [HomesController.cs](../../NuciCraft.API/Controllers/HomesController.cs) |
| Related documents | [Player and Home behaviour](../behaviour/player-and-home-capabilities.md), [request flow](../flows/player-and-home-requests.md), [data model](../data-model.md), and [security](../security.md) |

## Purpose

The Player component retains identity, profile, moderation, activity, location, and preference data for NuciCraft players. The Home component retains named saved locations associated with those generated Player identifiers. Home uses Player as a validation dependency but maintains a separate store and lifecycle.

## Scope

The components own:
- Player registration and deterministic offline UUID derivation.
- Player retrieval by four selectors and patching by exactly one selector.
- Default and selectively mutable player settings.
- Home creation, retrieval, filtering, patching, and deletion.
- Per-player Home-name uniqueness across all localisations.
- Home coordinate validation and process-local serialisation of Home operations.

They do not own:
- Minecraft account authentication or session verification.
- Password hashing, credential verification, or credential rotation.
- Per-player HTTP authorisation.
- Referential cascade when a Player is changed outside available APIs.
- Multi-process Home locking.
- Limits on Home count per player.

## Position In The Architecture

`PlayersController` and `HomesController` form the inbound adapters. `IPlayerService` and `IHomeService` define application contracts. `PlayerService` and `HomeService` own decisions. Two NuciDAL repositories own persistence. `HomeService` calls `IPlayerService` to ensure stored ownership always uses a real generated Player identifier.

## Dependencies

### Player

- `IFileRepository<PlayerDataObject>` for all Player records.
- NuciLog `ILogger` for operation traces.
- MD5 and GUID primitives for Minecraft-compatible offline UUID construction.
- Player, coordinate, and settings mappings.

### Home

- `IFileRepository<HomeDataObject>` for all Home records.
- `IPlayerService` for owner validation and canonical identifier resolution.
- NuciLog `ILogger`.
- Home, coordinate, and localised-string mappings and merge helpers.

## Dependants

- Player and Home controllers.
- Home service owner validation depends on Player retrieval semantics.
- API clients consuming Player identity, settings, personal data, or Home locations.
- Integration tests that use a registered Player before creating a Home.
- Persistence restart tests that rely on stable Home representation and uniqueness checks.

## Internal Structure

### Player Types

| Concern | Types |
|---------|-------|
| Service contract | [IPlayerService.cs](../../NuciCraft.API/Service/IPlayerService.cs) |
| Service implementation | [PlayerService.cs](../../NuciCraft.API/Service/PlayerService.cs) |
| Transport input | [RegisterPlayerRequest.cs](../../NuciCraft.API/Requests/RegisterPlayerRequest.cs), [GetPlayerRequest.cs](../../NuciCraft.API/Requests/GetPlayerRequest.cs), [PatchPlayerRequest.cs](../../NuciCraft.API/Requests/PatchPlayerRequest.cs), and [PatchPlayerSettingsRequest.cs](../../NuciCraft.API/Requests/PatchPlayerSettingsRequest.cs) |
| Transport output | [GetPlayerResponse.cs](../../NuciCraft.API/Responses/GetPlayerResponse.cs) and [GetPlayersResponse.cs](../../NuciCraft.API/Responses/GetPlayersResponse.cs) |
| Service models | [Player.cs](../../NuciCraft.API/Service/Models/Player.cs), [PlayerSettings.cs](../../NuciCraft.API/Service/Models/PlayerSettings.cs), [Gender.cs](../../NuciCraft.API/Service/Models/Gender.cs), and [Localisation.cs](../../NuciCraft.API/Service/Models/Localisation.cs) |
| Persistence | [PlayerDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/PlayerDataObject.cs), [PlayerSettingsDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/PlayerSettingsDataObject.cs), and [PlayerSettingsDataObjectExtensions.cs](../../NuciCraft.API/DataAccess/DataObjects/PlayerSettingsDataObjectExtensions.cs) |
| Mapping | [PlayerMappingExtensions.cs](../../NuciCraft.API/Service/Mapping/PlayerMappingExtensions.cs) and [PlayerSettingsMappingExtensions.cs](../../NuciCraft.API/Service/Mapping/PlayerSettingsMappingExtensions.cs) |

### Home Types

| Concern | Types |
|---------|-------|
| Service contract and implementation | [IHomeService.cs](../../NuciCraft.API/Service/IHomeService.cs) and [HomeService.cs](../../NuciCraft.API/Service/HomeService.cs) |
| Transport input | [AddHomeRequest.cs](../../NuciCraft.API/Requests/AddHomeRequest.cs), [PatchHomeRequest.cs](../../NuciCraft.API/Requests/PatchHomeRequest.cs), [GetHomeRequest.cs](../../NuciCraft.API/Requests/GetHomeRequest.cs), and [GetHomesRequest.cs](../../NuciCraft.API/Requests/GetHomesRequest.cs) |
| Transport output | [GetHomeResponse.cs](../../NuciCraft.API/Responses/GetHomeResponse.cs) and [GetHomesResponse.cs](../../NuciCraft.API/Responses/GetHomesResponse.cs) |
| Models and persistence | [Home.cs](../../NuciCraft.API/Service/Models/Home.cs), [HomeDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/HomeDataObject.cs), [Coordinates.cs](../../NuciCraft.API/Service/Models/Coordinates.cs), and [LocalisedString.cs](../../NuciCraft.API/Service/Models/LocalisedString.cs) |
| Mapping and merge | [HomeMappingExtensions.cs](../../NuciCraft.API/Service/Mappings/HomeMappingExtensions.cs) and [LocalisedStringDataObjectExtensions.cs](../../NuciCraft.API/Service/Helpers/LocalisedStringDataObjectExtensions.cs) |

## Behaviour

### Player Registration

The HTTP boundary requires Username. `PlayerService.Register` then:
1. Records Username, OnlineUUID, supplied CreatedDT, and LastIpAddress as log context.
2. Generates a random GUID identifier.
3. Derives an offline UUID from `OfflinePlayer:{username}` with MD5, version 3, and RFC 4122 variant bits.
4. Parses Gender case-insensitively, falling back to Other.
5. Uses current UTC when CreatedDT is absent, or validates its exact format.
6. Parses optional ban, mute, login, logout, and back timestamps with the identical strict format.
7. Converts supplied nested coordinates where present.
8. Creates default PlayerSettings.
9. Adds the mapped data object and calls `SaveChanges`.

Registration does not search for an existing Username, offline UUID, or online UUID. Duplicate alternate selectors can therefore produce ambiguous first-match retrieval.

### Offline UUID Algorithm

`GetOfflineUuid` hashes the UTF-8 string `OfflinePlayer:{username}` through MD5. It clears and sets the relevant byte bits to UUID version 3 and RFC 4122 variant, then constructs a big-endian `Guid`. This algorithm is compatibility-sensitive with Minecraft offline identity.

### Player Retrieval

`FindPlayerDataObject` builds one predicate that OR-matches every non-whitespace selector. Matching uses case-sensitive ordinal string equality. `FirstOrDefault` chooses the first record in repository enumeration order. If no record matches, the service raises `KeyNotFoundException`.

The four GET routes submit one selector each. Direct service callers can submit multiple selectors, and any matching selector succeeds.

`GetAll` maps the repository enumerable and logs its count. Returned enumeration semantics depend on NuciDAL and LINQ materialisation by later consumers.

### Player Update

Update requires exactly one non-whitespace selector among Identifier, Username, OfflineUUID, and OnlineUUID. The selector locates the record but is not itself mutable. Non-null fields replace persisted values, including personal, moderation, activity, location, and profile values.

Timestamp strings in patches are copied without strict validation. Settings merge uses explicit presence flags for booleans and non-null checks for strings. The service generates UpdatedDT, calls `Update`, and saves.

### Player Projection

Mappings parse persisted timestamps and nested values. `GetPlayerResponse` projects all principal fields, including Password, LastIpAddress, DiscordId, EmailAddress, and location history. DisplayName falls back to Username when absent. The `Player.LastSeenDT` derived property is not included in the response.

`GetPlayerResponse` assigns `HmacOrder(27)` to both Gender and Settings. The local repository does not establish the package behaviour for duplicate order values.

### Home Add

Every Home operation enters `Execute`, acquires `persistenceLock`, and logs within that lock. Addition then:
1. Requires a request and non-whitespace Player identifier.
2. Requires a location with non-whitespace World and finite X, Y, Z, Pitch, and Yaw.
3. Resolves the Player by generated identifier.
4. Generates a random GUID and current UTC creation timestamp.
5. Requires a non-null Name object.
6. Extracts, trims, and filters all eleven localised names.
7. Requires at least one non-whitespace name.
8. Rejects overlap with any name from another Home belonging to the identical Player, using ordinal case-insensitive comparison.
9. Adds and saves the record.

The service returns the persisted representation mapped back into a Home model.

### Home Retrieval

- `Get(identifier)` delegates to repository direct lookup.
- `Get(playerIdentifier, name)` scans all Homes for ordinal owner equality and a trimmed ordinal case-insensitive match versus any localised name.
- `GetAll` returns a materialised array.
- `GetAllByPlayer` validates a non-whitespace selector and returns a materialised array.

Only add and ownership patch validate the Player against `IPlayerService`. Read filters treat Player as a stored data selector.

### Home Update And Delete

Update retrieves the record, maps it through the Home service model into a detached data object, and applies:
- Language-by-language Name merge.
- Optional Player re-resolution and canonical identifier replacement.
- Complete Location replacement after finite-value validation.

It validates resulting name uniqueness, generates UpdatedDT, updates, saves, and returns the mapped result. Identifier and CreatedDT remain unchanged.

Delete calls repository `Remove(identifier)` and `SaveChanges` under the identical lock.

## Inputs And Outputs

Player registration accepts broad personal and gameplay data. Player patches add Discord, e-mail, demise, back-location, and settings fields. Responses expose the complete projected Player record.

Home input uses the service-layer `LocalisedString` and `Coordinates` structures directly. Home responses expose generated identifier, typed creation and revision timestamps, localised Name, canonical Player identifier, and Location.

Collection responses add computed counts.

## State

- Players and Homes occupy separate JSON stores.
- Both repositories and both services are process singletons.
- Player operations have no local lock.
- Every Home read and mutation shares one instance lock.
- Home foreign keys are plain strings and have no storage-level constraint.
- Neither component caches response objects independently of repository behaviour.

## Error Handling

Representative failures are:
- Missing Player match: `KeyNotFoundException`.
- Player patch with zero or multiple selectors: `ArgumentException`.
- Invalid registration timestamp: `ArgumentException` naming the timestamp parameter.
- Persisted invalid timestamp: mapping parse exception on retrieval.
- Missing Home or repository removal failure: repository exception.
- Missing or non-finite Home location: argument exception.
- Vacant or duplicate Home name: argument exception.
- Missing owner during Home add or transfer: propagated Player lookup failure.

Both services log and rethrow. Home logging and failure propagation occur while holding the Home lock.

## Configuration

- `DataStoreSettings.PlayersStorePath`
- `DataStoreSettings.HomesStorePath`
- `SecuritySettings.ApiKey` at the controller boundary
- NuciLog settings for service and request records

No password policy, identifier policy, Home limit, or locality rule is configurable.

## Extension And Modification Guidance

### Player Fields

Review registration, patching, Player model, data object, both mapping directions, response projection, HMAC ordering, personal-data exposure, logging, unit tests, integration tests, and persisted-record compatibility.

### Player Settings

Add defaults consistently to the patch backing field, presence flag, service model, data object, merge extension, both mappings, and tests. A simple non-nullable auto-property would lose omission semantics.

### Home Rules

Preserve lock coverage around read-check-write sequences. If adding a uniqueness dimension, validate the complete candidate after all patches but before repository update. Ownership remains based on generated Player identifiers unless a deliberate migration revises every stored Home.

## Relevant Processes

- [Player and Home request flow](../flows/player-and-home-requests.md)
- [Generic file-backed flow](../flows/file-backed-request.md)
- [Player and Home capability behaviour](../behaviour/player-and-home-capabilities.md)
