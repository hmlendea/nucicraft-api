# Geographic Metadata Capabilities

| Metadata | Value |
|----------|-------|
| Purpose | Describe Country, World, ZoneType, and Zone runtime capabilities from HTTP initiation through persistence or response. |
| Scope | Metadata additions, retrieval, patching, filtering, zone deletion, geometry, reference checks, and derived map links. |
| Primary sources | [CountriesController.cs](../../NuciCraft.API/Controllers/CountriesController.cs), [WorldsController.cs](../../NuciCraft.API/Controllers/WorldsController.cs), [ZoneTypesController.cs](../../NuciCraft.API/Controllers/ZoneTypesController.cs), [ZonesController.cs](../../NuciCraft.API/Controllers/ZonesController.cs), and their services |
| Related documents | [Geographic components](../components/geographic-metadata.md), [Zone flow](../flows/zone-requests.md), [file-backed flow](../flows/file-backed-request.md), and [invariants](../invariants.md) |

## Shared Metadata Conduct

Country, World, and ZoneType use a common operation form:
- POST accepts a client-controlled identifier, constructs a persistence record, generates CreatedDT, adds, and saves.
- GET by identifier delegates to repository direct lookup and maps the record.
- GET collection maps repository enumeration and computes response count.
- PATCH assigns the route identifier into the body, retrieves the record, selectively revises it, generates UpdatedDT, updates, and saves.
- Services log Started, Success, or Failure and rethrow exceptions.
- No delete route exists for these three record types.

The HTTP commands return a base success response without the resulting record.

## Country Management

### Addition

`POST /Countries` requires Identifier at the transport boundary. Name, LeaderTitle, and Leader can be absent. The service writes values without content validation.

### Retrieval

`GET /Countries/{id}` returns Identifier, mapped localised Name, mapped localised LeaderTitle, and Leader. `GET /Countries` returns mapped records plus a derived count.

### Patch

`PATCH /Countries/{id}` requires the route identifier. Supplied Name and LeaderTitle merge only non-null languages. Supplied Leader replaces the complete string. Omitted properties persist unchanged.

### Exceptional Paths

Missing records and persistence failures are logged and propagated. The service does not validate a country code format or leadership reference.

## World Management

### Addition

`POST /Worlds` requires Identifier. The service stores Name, HasWebMap, SpawnPoint, and a canonical Type. `WorldType.FromString` accepts case-insensitive `overworld`, `nether`, or `end`; every other value, including null, becomes `overworld`.

SpawnPoint is not validated for World presence, finite coordinates, or consistency with the enclosing World identifier.

### Retrieval

`GET /Worlds/{id}` maps Type to a `WorldType` model and nested SpawnPoint to service coordinates. `GET /Worlds` performs the identical mapping for all records.

### Patch

Name merges per localisation. Nullable HasWebMap applies explicit false. SpawnPoint replaces the complete nested object. A non-null Type canonicalises through the same fallback conversion.

### Postconditions

Every successfully added or patched persisted Type is one of the three canonical external strings, even when the client supplied an unsupported value.

## ZoneType Management

### Addition And Patch

`POST /ZoneTypes` requires Identifier and stores the supplied Categories collection and localised Name. Patch replaces Categories when supplied and merges Name.

### Filtering

`GET /ZoneTypes?category={value}` returns every record when value is null or whitespace. Otherwise it returns records whose Categories collection contains value under ordinal case-insensitive comparison. A null Categories collection never matches a non-empty filter.

The direct category action parameter is not represented in the empty request passed to `ProcessRequest`.

## Zone Addition

### Initiator And Input

An authorised client sends `POST /Zones` with required Identifier, Type, World, and Bounds. The request can also contain localised labels, territorial strings, person collections, teleportation coordinates, population, and links.

### Validation Sequence

1. Require non-whitespace Zone.World.
2. Resolve the World record; absent references become `ArgumentException`.
3. Require Bounds.
4. Require both corners.
5. Require both corner worlds.
6. Require ordinal equality between corner worlds.
7. Require non-whitespace Type.
8. Resolve the ZoneType record; absent references become `ArgumentException`.

These validations execute before the service's started log and `try` block.

### Transformation And Persistence

1. Canonicalise Bounds orientation and zero both corner orientations.
2. Use supplied CreationDate or current Romania date with ` (?)` suffix.
3. Use supplied Creators; otherwise copy Owners only when exactly one Owner exists.
4. Copy all other values into `ZoneDataObject`.
5. Generate CreatedDT.
6. Add and save the Zone.

### Success And Failure

Success produces a command response; canonical values are observable through a later GET. Preliminary validation failures do not receive the method's failure log. Repository-stage failures are logged and rethrown. There is no compensation for successful reference reads followed by failed Zone persistence.

## Zone Retrieval

### By Identifier

`GET /Zones/{id}` retrieves one record, canonicalises Bounds in the retrieved object, maps to Zone, and enriches MapLink when eligible.

### All Or By Type

`GET /Zones?type={value}` filters case-insensitively when Type is non-whitespace and otherwise includes every record. Each result is canonicalised, mapped, and enriched. The direct query parameter is outside the empty `GetZonesRequest` object.

### By Category

`GET /Zones/by-category/{category}`:
1. Requires a non-whitespace category.
2. Enumerates ZoneTypes and selects identifiers with exact case-sensitive category membership.
3. Enumerates Zones and selects exact case-sensitive Type matches to those identifiers.
4. Canonicalises, maps, and enriches results.

This differs intentionally or accidentally from ZoneType's case-insensitive category filter. Source does not record the rationale.

### By Coordinates

`GET /Zones/by-coordinates` requires World and nullable X, Y, Z values through data annotations. The controller constructs `CoordinatesDataObject` after validation. The service:
1. Requires non-whitespace World.
2. Enumerates all Zones.
3. Canonicalises each candidate Bounds.
4. Ignores missing or incomplete Bounds.
5. Requires exact World equality.
6. Applies inclusive X, Y, and Z bounds.
7. Returns identifiers only.

No ordering guarantee is added beyond repository enumeration order.

## Read-Time Map-Link Enrichment

For each retrieved Zone:
1. Preserve an explicit non-whitespace MapLink.
2. Return unchanged when TeleportationPoint or Zone.World is absent.
3. Retrieve the World identified by Zone.World.
4. Return unchanged when that World has no web map.
5. Construct `BaseUrl?worldname={escaped teleport world}&x={invariant X}&z={invariant Z}`.

The derived URL exists only in the returned model. No HTTP call verifies it and no save persists it.

## Zone Patch

### Validation And Merge Sequence

1. Controller assigns route Identifier.
2. Service validates Identifier.
3. Validate a supplied World reference.
4. Validate a supplied Type reference.
5. Retrieve persisted Zone.
6. If Bounds is supplied, merge each supplied corner with the persisted opposite corner.
7. Validate and canonicalise the merged pair.
8. Apply all other supplied values.
9. Assign canonical Bounds when supplied.
10. Generate UpdatedDT.
11. Update and save.

### Branch Semantics

- Localised Name, Nickname, and LeaderTitle merge.
- Collections replace completely.
- Nested TeleportationPoint replaces completely.
- Population applies when nullable input has a value, including zero.
- Null scalar references preserve persisted values.
- Creators are not recalculated from Owners.

### Failure And Postconditions

Validation occurs before repository mutation. Successful patch persists one revised Zone. No response record is returned. Failures inside the `try` block are logged and rethrown; there is no retry.

## Zone Deletion

`DELETE /Zones/{id}` removes and saves one Zone. The service does not prevalidate whitespace and does not verify dependants. Repeated deletion behaviour is repository-defined.

## Retries, Cleanup, And Consistency

- No metadata or Zone operation retries.
- Each write affects one file repository only.
- Cross-store reference reads and Zone writes are not transactional.
- GET enrichment can fail after successful Zone retrieval if its World reference is absent.
- No operation modifies World or ZoneType as a consequence of a Zone mutation.
- No cleanup is necessary beyond ordinary object disposal and exception unwinding.

## Capability Invariants

Canonical rules, including bounds orientation, case sensitivity, reference scope, and patch semantics, are consolidated in [invariants.md](../invariants.md).
