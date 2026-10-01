# Geographic Metadata Components

| Metadata | Value |
|----------|-------|
| Purpose | Document Country, World, ZoneType, and Zone metadata, reference validation, localisation, geometry, filtering, and web-map enrichment. |
| Scope | Four controllers, four services, their contracts, models, persisted records, mappings, and cross-store rules. |
| Primary sources | [CountryService.cs](../../NuciCraft.API/Service/CountryService.cs), [WorldService.cs](../../NuciCraft.API/Service/WorldService.cs), [ZoneTypeService.cs](../../NuciCraft.API/Service/ZoneTypeService.cs), and [ZoneService.cs](../../NuciCraft.API/Service/ZoneService.cs) |
| Related documents | [Geographic behaviour](../behaviour/geographic-metadata-capabilities.md), [Zone request flow](../flows/zone-requests.md), [data model](../data-model.md), and [invariants](../invariants.md) |

## Purpose

These components maintain NuciCraft descriptive geography and territorial geometry:
- Countries provide localised identity and leadership metadata.
- Worlds provide localised identity, web-map availability, spawn points, and a canonical type.
- ZoneTypes provide localised classifications and category membership.
- Zones combine references, localised labels, ownership and leadership metadata, teleportation, population, links, and bounded volumes.

## Scope

The components own CRUD-like metadata operations, patch semantics, filtering, zone reference validation, bounds canonicalisation, containment, and derived web-map links.

They do not own:
- Referential validation for country, county, region, owner, creator, or leader strings.
- Zone overlap detection.
- World coordinate validity or Minecraft world existence.
- ZoneType or World deletion.
- Cascading updates or deletions.
- Web-map network communication.

## Position In The Architecture

Each component has one controller, service interface, service implementation, root persistence record, service model, and mapping. `ZoneService` is the aggregate orchestrator: it uses the Zone repository for state, World repository for validation and map eligibility, ZoneType repository for validation and category joins, and WebMap settings for URL derivation.

## Dependencies

| Component | Dependencies |
|-----------|--------------|
| Country | Country repository, localised merge helper, mappings, logger. |
| World | World repository, `WorldType`, localised merge helper, mappings, logger. |
| ZoneType | ZoneType repository, localised merge helper, mappings, logger. |
| Zone | Zone, World, and ZoneType repositories; `WebMapSettings`; localised merge; mappings; logger; Romania time-zone database. |

## Dependants

- Corresponding controllers and response constructors.
- Zone creation and patching depend on World and ZoneType identifiers.
- Zone category queries depend on ZoneType categories.
- Zone read enrichment depends on World `HasWebMap`.
- Clients depend on canonical zone bounds and stable classification semantics.

## Internal Structure

### Country

- Contract and implementation: [ICountryService.cs](../../NuciCraft.API/Service/ICountryService.cs) and [CountryService.cs](../../NuciCraft.API/Service/CountryService.cs).
- Model and persistence: [Country.cs](../../NuciCraft.API/Service/Models/Country.cs) and [CountryDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/CountryDataObject.cs).
- Mapping: [CountryMappingExtensions.cs](../../NuciCraft.API/Service/Mapping/CountryMappingExtensions.cs).
- Transport: [CountriesController.cs](../../NuciCraft.API/Controllers/CountriesController.cs) and Country request/response classes under their respective directories.

### World

- Contract and implementation: [IWorldService.cs](../../NuciCraft.API/Service/IWorldService.cs) and [WorldService.cs](../../NuciCraft.API/Service/WorldService.cs).
- Model and persistence: [World.cs](../../NuciCraft.API/Service/Models/World.cs), [WorldType.cs](../../NuciCraft.API/Service/Models/WorldType.cs), and [WorldDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/WorldDataObject.cs).
- Mapping: [WorldMappingExtensions.cs](../../NuciCraft.API/Service/Mapping/WorldMappingExtensions.cs).

### ZoneType

- Contract and implementation: [IZoneTypeService.cs](../../NuciCraft.API/Service/IZoneTypeService.cs) and [ZoneTypeService.cs](../../NuciCraft.API/Service/ZoneTypeService.cs).
- Model and persistence: [ZoneType.cs](../../NuciCraft.API/Service/Models/ZoneType.cs) and [ZoneTypeDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/ZoneTypeDataObject.cs).
- Mapping: [ZoneTypeMappingExtensions.cs](../../NuciCraft.API/Service/Mapping/ZoneTypeMappingExtensions.cs).

### Zone

- Contract and implementation: [IZoneService.cs](../../NuciCraft.API/Service/IZoneService.cs) and [ZoneService.cs](../../NuciCraft.API/Service/ZoneService.cs).
- Models: [Zone.cs](../../NuciCraft.API/Service/Models/Zone.cs), [ZoneBounds.cs](../../NuciCraft.API/Service/Models/ZoneBounds.cs), and [Coordinates.cs](../../NuciCraft.API/Service/Models/Coordinates.cs).
- Persistence: [ZoneDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/ZoneDataObject.cs), [ZoneBoundsDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/ZoneBoundsDataObject.cs), and [CoordinatesDataObject.cs](../../NuciCraft.API/DataAccess/DataObjects/CoordinatesDataObject.cs).
- Mapping: [ZoneMappingExtensions.cs](../../NuciCraft.API/Service/Mapping/ZoneMappingExtensions.cs) and [ZoneBoundsMappingExtensions.cs](../../NuciCraft.API/Service/Mapping/ZoneBoundsMappingExtensions.cs).

## Behaviour

### Country Lifecycle

Addition maps the client identifier and metadata directly into a Country record, generates CreatedDT, adds, and saves. Retrieval uses repository direct lookup. GetAll maps every record. Patch requires a non-whitespace route-assigned identifier, merges Name and LeaderTitle per language, replaces Leader when supplied, generates UpdatedDT, updates, and saves.

There is no delete, identifier mutation, country-code validation, or reference validation.

### World Lifecycle

Addition generates CreatedDT and canonicalises Type through `WorldType.FromString(...).ExternalName`. Unknown, null, and whitespace values become `overworld`. Name, HasWebMap, and SpawnPoint are stored without additional service validation.

Patch merges Name, applies nullable HasWebMap, replaces SpawnPoint when supplied, and canonicalises a supplied Type. Retrieval maps string Type back to a `WorldType`, again defaulting unknown values to Overworld.

### ZoneType Lifecycle And Filtering

Addition stores client identifier, category collection, localised Name, and CreatedDT. Patch replaces Categories as a complete collection and merges Name. Retrieval by category returns all records when the filter is null or whitespace; otherwise category matching is ordinal and case-insensitive.

### Zone Addition

Validation executes before started-operation logging:
1. Require and resolve Zone.World in the World repository.
2. Require Bounds, both corners, both corner worlds, and exact corner-world equality.
3. Require and resolve Type in the ZoneType repository.

The service canonicalises bounds. It then constructs a Zone record with all client fields, derives CreationDate when vacant, derives Creators from exactly one Owner when Creators is absent, generates CreatedDT, adds, and saves.

No check requires Bounds or TeleportationPoint worlds to equal Zone.World.

### Zone Retrieval And Filters

Direct retrieval canonicalises the record's bounds, maps it, and applies read-time enrichment.

Type-filtered retrieval:
- Returns all Zones for null or whitespace Type.
- Uses ordinal case-insensitive Type matching otherwise.
- Canonicalises and enriches every result.

Category retrieval:
- Rejects a null or whitespace category.
- Finds ZoneTypes whose Categories contain the value with ordinal case-sensitive equality.
- Finds Zones whose Type exactly equals one of those identifiers.
- Canonicalises and enriches every result.

Coordinate retrieval validates only a non-whitespace World, then returns identifiers for inclusive three-dimensional containment. Missing or incomplete persisted bounds do not match.

### Zone Update

Update validates the route-assigned identifier and any supplied World or Type before retrieving the Zone. A supplied partial Bounds object merges with persisted bounds, validates the resulting pair, and canonicalises it.

Patch conduct is:
- Name, Nickname, and LeaderTitle merge by non-null language.
- Scalar strings replace when non-null.
- Owners, Creators, and Leaders replace complete collections when non-null.
- TeleportationPoint replaces the complete nested object.
- nullable Population permits explicit zero.
- Bounds replace the complete canonical pair after partial merge.
- UpdatedDT is generated before save.

The creation-time Creators derivation is not repeated during patch.

### Zone Deletion

Delete passes the identifier directly to repository Remove, then saves. It does not first validate whitespace and does not alter World or ZoneType state.

### Bounds Canonicalisation

For two supplied corners:
- FirstCorner receives minimum X, maximum Y, minimum Z.
- SecondCorner receives maximum X, minimum Y, maximum Z.
- Both retain FirstCorner.World after validation establishes identical worlds.
- Pitch and Yaw become zero.

Containment is inclusive and follows the inverted canonical Y orientation: query Y must be less than or equal to FirstCorner.Y and greater than or equal to SecondCorner.Y.

### Web-Map Enrichment

Enrichment returns the original Zone unchanged when it has an explicit MapLink, lacks TeleportationPoint, or lacks Zone.World. Otherwise it directly retrieves Zone.World. If `HasWebMap` is false, no link is added. If true, the service concatenates configured BaseUrl with escaped teleportation-point world and invariant X and Z query values.

The URL is not persisted and omits Y, pitch, and yaw.

## Inputs And Outputs

Country, World, and ZoneType outputs use typed service models. Zone output contains nineteen ordered fields spanning identity, localisation, classification, territorial references, people, coordinates, bounds, population, and links.

Creation and patch commands return the base Nuci API success response rather than the revised record. Clients must issue a GET to observe generated timestamps, canonical Type or Bounds, and derived MapLink.

## State

- Four independent JSON stores own records.
- Zone references are string identifiers, not repository-enforced foreign keys.
- Services and repositories are singletons.
- No application locks protect these operations.
- Derived map links are request-duration state only.
- Country, World, and ZoneType records cannot be deleted through current interfaces.

## Error Handling

- Missing direct records propagate repository lookup exceptions.
- Invalid Zone World or Type converts a missing referenced record into `ArgumentException` with the repository exception as inner exception.
- Invalid bounds, category, coordinates World, or patch selector raise `ArgumentException`.
- Missing zone world during read enrichment propagates the World repository failure.
- Romania time-zone resolution can fail on a host lacking the expected time-zone identifier.
- Persistence errors are logged and rethrown.

Zone addition performs reference and bounds validation before entering its `try` logging block, so failures in those preliminary validations do not receive the service's failure log record. Other operation details vary by method placement.

## Configuration

- Four store paths in `DataStoreSettings`.
- `WebMapSettings.BaseUrl` for derived links.
- API key at each controller.
- NuciLog settings.

World types, zone categories, reference requirements, canonical geometry, and Romania time zone are compiled behaviour rather than configuration.

## Extension And Modification Guidance

### Country, World, Or ZoneType Fields

Revise request, patch, service model, data object, mappings, response, HMAC metadata, service tests, response tests, and HTTP tests. Determine whether patches merge, replace, or preserve each field.

### Zone Fields Or Rules

Trace addition, patch application, mapping, response ordering, enrichment, every filter, and persisted compatibility. Geometry changes require containment and canonicalisation regression tests.

### New Reference Constraints

Select whether validation belongs on creation, patch, and read. Account for existing persisted records that may violate a novel rule; startup does not migrate or repair them.

## Relevant Processes

- [Zone request flow](../flows/zone-requests.md)
- [Generic file-backed flow](../flows/file-backed-request.md)
- [Geographic capability behaviour](../behaviour/geographic-metadata-capabilities.md)
