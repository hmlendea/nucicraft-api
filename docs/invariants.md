# Invariants And Rules

| Metadata | Value |
|----------|-------|
| Purpose | Consolidate system rules that future modifications must preserve or intentionally migrate. |
| Scope | Confirmed cross-cutting, transport, domain, persistence, and integration invariants. |
| Primary sources | [Controllers](../NuciCraft.API/Controllers/), [Service](../NuciCraft.API/Service/), [Requests](../NuciCraft.API/Requests/), and [DataAccess/DataObjects](../NuciCraft.API/DataAccess/DataObjects/) |
| Related documents | [Architecture](architecture.md), [design decisions](design-decisions.md), and [repository overview](repository-overview.md) |

## Cross-Cutting Invariants

| Invariant | Established By | Dependants | Violation Consequence |
|-----------|----------------|------------|-----------------------|
| Every controller operation passes `NuciApiAuthorisation.ApiKey(SecuritySettings.ApiKey)` to `ProcessRequest`. | All files under [Controllers](../NuciCraft.API/Controllers/) | Every API client and protected route. | An endpoint may become unauthenticated or incompatible with peer endpoints. |
| Mutation services call repository `SaveChanges` after `Add`, `Update`, or `Remove`. | Concrete services under [Service](../NuciCraft.API/Service/) | Restart persistence and integration tests. | In-memory success may not survive process termination. |
| Services log started and success, or started and failure, then rethrow. | Concrete file-backed and mob services. | Operational diagnosis and error middleware. | Lost operation visibility or altered error propagation. |
| Store preparation includes all seven registered repositories. | [Startup.cs](../NuciCraft.API/Startup.cs) and [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs) | Process readiness. | A novel store may fail only on its first request rather than startup. |
| Runtime data under `Data/` is excluded from source control. | [.gitignore](../.gitignore) | Privacy, secret hygiene, and deployment state. | Personal or operational records may enter version history. |

## Identifier Rules

- Player and Home identifiers are generated as random GUID strings by their services.
- RTP identifiers are generated as random GUID strings and exposed as `Id`.
- Country, World, ZoneType, and Zone identifiers are supplied by clients and assigned to NuciDAL `EntityBase.Id`.
- Identifier mutation is not exposed after creation.
- Player selectors compare with default ordinal case-sensitive `string.Equals` semantics.
- Home identifiers and player references compare ordinally; Home names compare ordinally without case after trimming.
- Zone type filtering is ordinal and case-insensitive, while zone category joins are ordinal and case-sensitive.
- Repository-level duplicate identifier behaviour is supplied by NuciDAL and is not defined locally.

Do not infer uniqueness constraints for username, online UUID, or offline UUID. [PlayerService.cs](../NuciCraft.API/Service/PlayerService.cs) performs no duplicate check during registration.

## Timestamp Rules

`TimestampFormats.Full` in [TimestampFormats.cs](../NuciCraft.API/Service/TimestampFormats.cs) is `yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK`.

Rules are:
- Service-generated persisted timestamps use UTC and this exact format.
- Player registration parses supplied timestamps strictly with `TryParseExact` and rejects mismatches.
- Player patch methods copy supplied timestamp strings without validating the format.
- Player mappings later parse persisted timestamps with invariant-culture `DateTimeOffset.Parse`.
- An invalid timestamp accepted by a patch can consequently cause a later player read or repository materialisation to fail.
- Home service models expose typed timestamps; Home mappings serialise them through the shared format.
- Country, World, ZoneType, and Zone creation/revision timestamps are persistence metadata and are not exposed by their public responses.
- Zone `CreationDate` and `PopulationDate` are domain strings, not persistence timestamps.

## Player Invariants

### Registration

- `Username` is transport-required.
- The service generates `Identifier` and an offline UUID from UTF-8 `OfflinePlayer:{username}` using MD5, RFC 4122 version 3 bits, and RFC 4122 variant bits.
- Null or unknown gender becomes `Gender.Other`.
- Null creation timestamp becomes current UTC; supplied creation and optional moderation/activity timestamps must match `TimestampFormats.Full`.
- New settings use `PlayerSettings` defaults, including Romanian localisation and enabled automatic assistance, private messages, and teleportation requests.
- The supplied password is stored as supplied. No local hash or encryption is applied.

### Retrieval And Patching

- `PlayerService.Get` OR-matches any non-whitespace identifier, username, offline UUID, or online UUID and returns the first repository match.
- Controller GET routes construct exactly one selector, but the service itself does not reject multiple selectors.
- A patch must contain exactly one selector after its route value is assigned.
- Username, offline UUID, online UUID, created timestamp, and identifier are immutable through patch operations.
- A null patch property preserves the persisted value.
- Player-setting booleans use `WasProvided` flags so omitted fields preserve state while explicit `false` applies.
- When persisted settings are absent, applying a non-null settings patch starts from data-object defaults.
- `DisplayName` in `GetPlayerResponse` falls back to Username when persisted display name is null.
- `LastSeenDT` is the maximum of creation, login, logout, sleep, demise, and back timestamps, but it is a service-model property and is not included by `GetPlayerResponse`.

## Home Invariants

- Home addition and ownership transfer require an existing player selected by player identifier.
- The persisted `Player` field is the resolved player's generated identifier, never a username or Minecraft UUID.
- Addition requires a non-null location, non-whitespace location world, and finite X, Y, Z, pitch, and yaw.
- Addition requires at least one non-whitespace value among all eleven localised name fields.
- Within one player, no two Homes may share any trimmed localised name under ordinal case-insensitive comparison, even when the matching strings occupy different language fields.
- Different players may use identical Home names.
- Name patches merge supplied non-null languages and preserve omitted languages.
- Location patches replace the complete nested location.
- Identifier and creation timestamp are immutable; revision timestamp is generated after a successful patch.
- Every Home operation, including reads, executes inside the singleton service's instance lock.
- The lock protects one process only.

## RTP Location Invariants

- Add requests require Biome and World at the transport boundary.
- Proximity uses only X and Z, not Y.
- Locations in different worlds never conflict.
- A candidate is rejected when squared horizontal distance is less than or equal to the configured general minimum squared.
- A candidate is also rejected when it has the identical case-sensitive Biome and World and distance is less than or equal to the configured biome minimum squared.
- Equality with the threshold is too close; accepted distance must be strictly greater.
- Random selection applies optional World and Biome filters with case-sensitive equality.
- No matching location raises `KeyNotFoundException`.
- Selection uses `NuciExtensions.GetRandomElement`; weighting and random algorithm details are external.

## Country Invariants

- Identifier is client-supplied and immutable through the HTTP interface.
- Name and LeaderTitle patches merge non-null language properties into existing localised records.
- Leader replaces the complete string when non-null.
- No service-level reference or content validation exists beyond request presence and patch identifier presence.
- There is no delete operation.

## World Invariants

- Identifier is client-supplied and immutable through the HTTP interface.
- Unknown, null, or whitespace Type values canonicalise to `WorldType.Overworld` on addition or a supplied type patch.
- Name patches merge localisations.
- A supplied SpawnPoint replaces the complete nested object; the service performs no coordinate-world or finite-value validation.
- `HasWebMap` uses nullable patch semantics so explicit `false` remains applicable.
- There is no delete operation.

## ZoneType Invariants

- Identifier is client-supplied and immutable through the HTTP interface.
- A supplied Categories patch replaces the complete collection.
- Name patches merge localisations.
- A null or whitespace category filter returns all zone types.
- A non-whitespace category filter uses ordinal case-insensitive matching.
- ZoneType deletion is not exposed, so no cascade conduct exists.

## Zone Invariants

### References

- Addition requires a non-whitespace World identifier that resolves in the world repository.
- Addition requires a non-whitespace Type identifier that resolves in the zone-type repository.
- A supplied World or Type patch is validated through the identical rule.
- Country, County, Region, Owners, Creators, and Leaders are not validated versus other stores.
- Bounds corner worlds must equal one another ordinally, but the service does not require them to equal the Zone `World` property.
- TeleportationPoint.World is not required to equal either the Zone world or bounds world.

### Creation Defaults

- Bounds are mandatory on addition and both corners must be present.
- Blank CreationDate becomes the current `Europe/Bucharest` calendar date formatted as `yyyy-MM-dd (?)`.
- If Creators is omitted and Owners contains exactly one value, Creators becomes that one-owner collection.
- Creators remains null when Owners is absent or contains any count other than one.

### Bounds And Containment

- Both bounds corner worlds must be non-whitespace and ordinally identical.
- Canonical FirstCorner is minimum X, maximum Y, minimum Z.
- Canonical SecondCorner is maximum X, minimum Y, maximum Z.
- Canonical corner pitch and yaw are zero.
- Addition, bounds patch, single retrieval, type-filtered retrieval, and category retrieval canonicalise bounds.
- A partial bounds patch merges a supplied corner with the persisted opposite corner before validation.
- Coordinate containment is inclusive on every boundary and requires ordinal world equality.
- Records with absent or incomplete bounds do not match coordinate queries.

### Filtering And Enrichment

- Empty or whitespace type filters return every zone.
- Non-empty type filters use ordinal case-insensitive equality.
- Category lookup first selects ZoneTypes whose categories contain the requested string with ordinal case-sensitive equality, then selects Zones whose Type exactly matches those identifiers.
- A non-whitespace explicit MapLink always wins.
- A missing MapLink is derived only when TeleportationPoint exists, Zone.World is non-whitespace, the referenced World exists, and `HasWebMap` is true.
- Derived map URLs use the teleportation point's World, X, and Z and are not persisted.

### Patch Semantics

- Localised Name, Nickname, and LeaderTitle merge per language.
- Supplied collections replace complete Owners, Creators, or Leaders collections.
- Population uses nullable patch semantics, permitting an explicit zero.
- Null properties preserve persisted values.
- Patch does not reapply the single-owner Creators default.

## Mob-Name Invariants

- Supported external names are `cow`, `ender_dragon`, `evoker`, `illusioner`, `pillager`, `pig`, `villager`, `vindicator`, and `wandering_trader`.
- Parsing is case-insensitive; an unknown value becomes `Unsupported` and then raises `NotImplementedException`.
- Count defaults to one and is transport constrained to 1 through 100000.
- WanderingTrader uses Romanian male person names; EnderDragon uses fantasy dragons; Cow and Pig use their respective animal schemas.
- Evoker, Illusioner, Pillager, and Vindicator share the Zaganian male schema.
- Villager randomly selects Romanian male or female person names with two equal `Random.Shared` branches.
- Base URL and bearer API key must be non-whitespace when the service operation starts.
- The remote response must be successful, of type `GenerateNamesResponse`, and contain at least one non-whitespace name.

## Server-Status Invariants

- Hostname must be non-whitespace.
- Java port must be from 1 through 65535.
- Each request creates a new MineStat legacy-protocol query with a five-second timeout.
- `IOException`, `SocketException`, and `ServerUp == false` produce count zero.
- A negative parsed count raises `InvalidOperationException`.
- Non-numeric counts can propagate `FormatException` from MineStat.
- Zero cannot distinguish an unavailable server from an available server with no online players.
- Bedrock port is returned as metadata but is not queried.

## Contract Compatibility Rules

- Public routes are unversioned; route or JSON shape changes affect every current client.
- `JsonPropertyName("id")` and mob `type` or `count` aliases are public serialisation contracts.
- HMAC order values are compatibility-sensitive package metadata.
- Collection `Count` response properties are derived and marked `HmacIgnore`.
- `GetPlayerResponse` currently has duplicate HMAC order 27 values for Gender and Settings. Preserve current behaviour until package expectations and client signatures are deliberately assessed.
- Persisted field names and string timestamp formats have no migration layer. An incompatible data-object revision requires migration planning.
