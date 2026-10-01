# Persistence And Mapping Component

| Metadata | Value |
|----------|-------|
| Purpose | Explain persisted representations, repository adapters, explicit save points, mappings, and state compatibility constraints. |
| Scope | NuciDAL integration, seven stores, data objects, mapping extensions, timestamps, and patch merge helpers. |
| Primary sources | [DataAccess/DataObjects](../../NuciCraft.API/DataAccess/DataObjects/), [Service/Mapping](../../NuciCraft.API/Service/Mapping/), [Home mappings](../../NuciCraft.API/Service/Mappings/HomeMappingExtensions.cs), and [ServiceCollectionExtensions.cs](../../NuciCraft.API/ServiceCollectionExtensions.cs) |
| Related documents | [Data model](../data-model.md), [state and persistence](../state-and-persistence.md), [architecture](../architecture.md), and [file-backed request flow](../flows/file-backed-request.md) |

## Purpose

The persistence and mapping component gives application services durable JSON-backed records without exposing persistence-specific string timestamps or NuciDAL entities directly through service models and responses.

## Scope

This component owns:
- Application-defined persisted record shapes.
- Nested persisted coordinates, bounds, localised strings, and settings.
- Conversion between data objects and service models.
- Timestamp formatting and parsing at mapping boundaries.
- Localised patch merge and player-settings patch merge.
- Concrete `JsonRepository<T>` selection and store path association.

It does not own:
- NuciDAL repository implementation internals.
- Filesystem provisioning outside startup.
- Domain validation before a write.
- HTTP serialisation of response envelopes.
- Backup, migration, encryption, retention, or multi-process coordination.

## Position In The Architecture

Application services invoke `IFileRepository<TDataObject>`. The composition root binds that interface to an external NuciDAL JSON adapter. Mapping extensions convert repository records into application representations before controllers construct response content.

## Dependencies

| Dependency | Use |
|------------|-----|
| NuciDAL `EntityBase` | Provides persisted `Id`. |
| NuciDAL `IFileRepository<T>` | Service-facing storage operations. |
| NuciDAL `JsonRepository<T>` | File-backed adapter selected by DI. |
| System.Text.Json attributes | Explicit localised field names and nested HMAC metadata. |
| `TimestampFormats` | Generated string format for application timestamps. |
| Service models | Typed application representations. |

## Dependants

- Every file-backed service reads or mutates one repository.
- `HomeService` and `ZoneService` use merge helpers before persistence.
- Response constructors depend on mapped service models.
- Startup depends on all repository registrations for eager enumeration.
- Mapping and persistence integration tests depend on stable conversion semantics.

## Internal Structure

### Root Records

| Data Object | Store Setting | Primary Service |
|-------------|---------------|-----------------|
| `PlayerDataObject` | `PlayersStorePath` | `PlayerService` |
| `HomeDataObject` | `HomesStorePath` | `HomeService` |
| `RtpLocationEntity` | `RtpLocationsStorePath` | `RtpLocationService` |
| `CountryDataObject` | `CountriesStorePath` | `CountryService` |
| `WorldDataObject` | `WorldsStorePath` | `WorldService` |
| `ZoneDataObject` | `ZonesStorePath` | `ZoneService` |
| `ZoneTypeDataObject` | `ZoneTypesStorePath` | `ZoneTypeService` |

All root records derive from `NuciCraftEntityBase`, which derives from NuciDAL `EntityBase` and adds `CreatedDT` and `UpdatedDT` strings. `RtpLocationEntity` is the only root record named `Entity` rather than `DataObject`; no behavioural distinction is evident.

### Nested Records

- `CoordinatesDataObject` stores World, X, Y, Z, Pitch, and Yaw.
- `ZoneBoundsDataObject` stores FirstCorner and SecondCorner.
- `LocalisedStringDataObject` stores eleven optional language strings with explicit lower-case JSON property names.
- `PlayerSettingsDataObject` stores ten settings with defaults.

### Mappings

Ten classes under [Service/Mapping](../../NuciCraft.API/Service/Mapping/) map coordinates, countries, localised strings, players, player settings, RTP locations, worlds, zone bounds, zones, and zone types. [HomeMappingExtensions.cs](../../NuciCraft.API/Service/Mappings/HomeMappingExtensions.cs) maps Homes from the separate plural directory.

Mappings are internal static extension methods. Collection helpers use LINQ and are frequently deferred until controllers or logging count operations enumerate them.

### Merge Helpers

[LocalisedStringDataObjectExtensions.cs](../../NuciCraft.API/Service/Helpers/LocalisedStringDataObjectExtensions.cs) creates or reuses an existing localised record and replaces only non-null incoming language fields.

[PlayerSettingsDataObjectExtensions.cs](../../NuciCraft.API/DataAccess/DataObjects/PlayerSettingsDataObjectExtensions.cs) creates default settings when the persisted object is null and applies only fields whose patch DTO indicates presence, plus non-null localisation and skin URL strings.

## Behaviour

### Repository Use

Service read patterns are either:
- `Get(id)` for direct identifier access.
- `GetAll()` followed by LINQ for alternate selectors, filters, uniqueness, proximity, joins, or random selection.

Mutation patterns are:
1. Construct or retrieve a data object.
2. Apply validation and changes in the service.
3. Call `Add`, `Update`, or `Remove`.
4. Call `SaveChanges` synchronously.

No service calls a transaction API. Zone reference validation can read another store before the zone write; Home player validation calls another service before the Home write.

### Timestamp Conversion

- Service-generated timestamps use `DateTimeOffset.UtcNow` and `TimestampFormats.Full`.
- Player and Home mappings expose typed `DateTimeOffset` values.
- Other domain service models omit persistence metadata.
- Player mapping uses invariant-culture `DateTimeOffset.Parse` for stored values.
- Home mapping uses exact shared formatting on writes and parsing on reads.

Timestamp parse failure is a read failure; no mapper provides a fallback or repair.

### Enum-Like Conversion

String persistence fields convert through `Gender.FromString`, `Localisation.FromString`, and `WorldType.FromString`. Unknown values become fallback model instances rather than parse exceptions. On reverse conversion, `ExternalName` or implicit string conversion is stored.

### Read Mutation

`ZoneService` assigns canonicalised bounds back to retrieved `ZoneDataObject` instances before mapping. Whether this mutates repository-held in-memory state without `SaveChanges` depends on NuciDAL object identity semantics and is not established locally. No intentional save occurs during a read.

## Inputs And Outputs

Inputs are service-constructed records, repository-returned records, and configured file paths. Outputs are:
- JSON persistence delegated to NuciDAL.
- Service models returned to application services and responses.
- Revised nested records after merge operations.

The complete field catalogue appears in [data-model.md](../data-model.md).

## State

- Each repository is a singleton for the process lifetime.
- JSON files persist independently across restarts.
- Local `Data/` paths are ignored by Git.
- The repository defines no cache distinct from whatever state NuciDAL retains internally.
- Startup enumeration demonstrates repositories can materialise each store but does not locally prove whether later reads re-read files or use retained objects.

## Error Handling

Potential failures include:
- Direct `Get` of an absent identifier.
- Duplicate identifier conduct delegated to NuciDAL.
- Deserialisation failure from malformed or incompatible JSON.
- Timestamp parse failure in mappings.
- Filesystem permission, capacity, or path failure during save.
- Partial application state when one cross-store validation succeeds and the subsequent write fails.

Services log and rethrow repository failures. There is no retry, atomic file group, compensation, or corrupted-store quarantine.

## Configuration

Seven path values in `DataStoreSettings` select stores. The defaults under `Data/` are relative to process working directory. Startup creates absent paths, but operators own permissions and durable storage.

No serialiser settings, write mode, indentation, locking option, encoding, schema version, or backup path is configured locally.

## Extension And Modification Guidance

### Adding A Field

Review:
1. Public request and response requirements.
2. Service model representation.
3. Data-object representation and old-record defaults.
4. Forward and reverse mappings.
5. Patch omission and clearing semantics.
6. HMAC order compatibility.
7. Existing runtime file compatibility and migration.
8. Mapping, service, response, integration, and restart-persistence tests.

### Adding A Store

Add the data object, settings path, repository registration, startup creation, eager load, service registration, tests, configuration documentation, state documentation, and operations guidance.

### Replacing NuciDAL

Preserve the `IFileRepository<T>` usage contract or revise every service. Explicitly define duplicate IDs, enumeration snapshots, concurrency, atomic writes, flush behaviour, and exception types because these are not locally codified today.

## Relevant Processes

- [Process startup](../flows/startup.md)
- [File-backed request flow](../flows/file-backed-request.md)
- [Player and Home flow](../flows/player-and-home-requests.md)
- [Zone flow](../flows/zone-requests.md)
