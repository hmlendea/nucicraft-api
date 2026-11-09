# Design Decisions

| Metadata | Value |
|----------|-------|
| Purpose | Record architectural choices and non-obvious constraints established by repository evidence. |
| Scope | Current implementation decisions, their evidence, consequences, and modification constraints. Historical motivation is identified as indeterminate where source does not establish it. |
| Primary sources | [Startup.cs](../NuciCraft.API/Startup.cs), [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs), [Service](../NuciCraft.API/Service/), and [DataAccess](../NuciCraft.API/DataAccess/) |
| Related documents | [Architecture](architecture.md), [invariants](invariants.md), and [repository overview](repository-overview.md) |

## Layered Modular Monolith

**Decision:** Host all capabilities in one ASP.NET Core project and process while separating controllers, service interfaces, service implementations, models, mappings, and data objects.

**Evidence:** [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj) is the sole runtime project; [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs) registers every domain.

**Motivation:** The historical rationale is not recorded. The structure indicates a preference for one deployable service with explicit code-level boundaries.

**Consequences:**
- Every domain shares configuration, deployment, middleware, filesystem state, and availability.
- Cross-domain validation can use an injected service or repository without network communication.
- Code-level dependency discipline is conventional rather than enforced by project references.

**Modification constraint:** Do not introduce repository access into controllers or transport dependencies into service models merely because all types share one assembly.

## JSON File Persistence Through NuciDAL

**Decision:** Persist eight aggregate record types in independently configured JSON files using scoped `OptimisticJsonRepository<T>` instances.

**Evidence:** Repository factories in [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs), store preparation in [Startup.cs](../NuciCraft.API/Startup.cs), and data objects in [DataAccess/DataObjects](../NuciCraft.API/DataAccess/DataObjects/).

**Motivation:** Not recorded. It removes a runtime database dependency and permits direct path configuration.

**Consequences:**
- There is no local schema migration, transaction across files, relational constraint, or query engine.
- Relative path correctness, permissions, durability, backups, and exclusive writer topology are operational concerns.
- Every mutation has an explicit `SaveChanges` point.

**Modification constraint:** Persisted shape changes require compatibility analysis and, when incompatible, an explicit migration mechanism before deployment.

## Store Preparation And Deferred Loading

**Decision:** Create missing store files before accepting requests, but defer repository construction and enumeration until a request operation needs the repository.

**Evidence:** `CreateStoreIfMissing` and scoped repository registrations in [Startup.cs](../NuciCraft.API/Startup.cs) and [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs).

**Motivation:** Keep startup responsible for filesystem preparation while request scopes own repository state.

**Consequences:**
- Initialisation is synchronous and all stores participate in process readiness.
- One malformed or inaccessible store prevents the complete API from starting.
- Existing data is never automatically repaired or migrated.

**Modification constraint:** A novel file-backed domain requires path creation and scoped repository registration; loading occurs when the domain is used.

## Container-Resolved Settings For Repository Factories

**Decision:** Resolve `DataStoreSettings` from the dependency-injection container when each repository instance is created, instead of retaining it in a static field.

**Evidence:** `AddConfigurations` and `AddJsonRepository<T>` in [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs).

**Motivation:** The former static field was process-global registration state. It bound every subsequently constructed repository to whichever assignment happened last, which broke test isolation and prevented concurrent service-collection construction.

**Consequences:**
- Concurrent and sequential service collections resolve their own store paths.
- Repository creation order no longer influences which paths are bound.
- `AddCustomServices` still presumes `AddConfigurations` executed first, because it resolves `DataStoreSettings` from the container.

**Modification constraint:** Preserve container resolution for settings consumed by registration helpers. Do not reintroduce static fields that retain registration state.

## Scoped Services And Repositories

**Decision:** Register application services, file repositories, and the Universal Name Generator client as scoped services; retain singleton lifetime only for process-wide generators and text utilities.

**Evidence:** [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs).

**Motivation:** Isolate request state and repository instances while preserving process-wide stateless utilities.

**Consequences:**
- Each request receives its own application-service and repository instances.
- File coordination across requests and processes remains dependent on repository and filesystem semantics.
- `ILogger` is resolved within the same request scope as application services.

**Modification constraint:** Keep request-specific state scoped and review file-write coordination when changing repository behaviour.

## Thin Controllers And Explicit Service Interfaces

**Decision:** Controllers depend on service interfaces and pass requests through the package-supplied `ProcessRequest` boundary.

**Evidence:** Every class under [Controllers](../NuciCraft.API/Controllers/) and every `I*Service` file under [Service](../NuciCraft.API/Service/).

**Motivation:** Not recorded. The arrangement isolates routing and response construction from domain decisions.

**Consequences:** Controller tests can mock services; service tests can execute without an HTTP host.

**Modification constraint:** Place novel validation in the service when it must remain true for non-controller callers. Reserve data annotations for transport validity.

## Separate Transport, Service, And Persistence Representations

**Decision:** Maintain request/response DTOs, service models, and persistence data objects as distinct representations.

**Evidence:** [Requests](../NuciCraft.API/Requests/), [Responses](../NuciCraft.API/Responses/), [Service/Models](../NuciCraft.API/Service/Models/), and [DataAccess/DataObjects](../NuciCraft.API/DataAccess/DataObjects/).

**Motivation:** Not recorded. Distinct types permit response projection, typed timestamps, persistence strings, and patch-specific null semantics.

**Consequences:** A field addition frequently requires coordinated revisions across several representations and mapping tests.

**Modification constraint:** Do not expose data objects directly as a shortcut. Review every representation and conversion when changing a shared concept.

## HMAC Ordering As Contract Metadata

**Decision:** Annotate request and response properties with NuciSecurity `HmacOrder` values and exclude derived collection counts with `HmacIgnore`.

**Evidence:** Classes under [Requests](../NuciCraft.API/Requests/) and [Responses](../NuciCraft.API/Responses/).

**Motivation:** HMAC implementation is external, but the annotations indicate canonical property ordering is compatibility-sensitive.

**Consequences:** Reordering or duplicating values can affect external signing behaviour. `GetPlayerResponse` presently assigns order 27 to both `Gender` and `Settings`; source does not establish whether duplicate orders are supported.

**Modification constraint:** Treat order values as public contract data and test package behaviour before renumbering existing properties.

## Selective Patch Merge

**Decision:** Null reference-type patch fields preserve persisted values, localised strings merge language by language, and player-setting booleans retain explicit JSON-presence flags.

**Evidence:** [LocalisedStringDataObjectExtensions.cs](../NuciCraft.API/DataAccess/DataObjects/LocalisedStringDataObjectExtensions.cs), [PatchPlayerSettingsRequest.cs](../NuciCraft.API/Requests/PatchPlayerSettingsRequest.cs), and [PlayerSettingsDataObjectExtensions.cs](../NuciCraft.API/DataAccess/DataObjects/PlayerSettingsDataObjectExtensions.cs).

**Motivation:** The design differentiates omission from replacement and, for non-nullable booleans, omission from explicit `false`.

**Consequences:**
- A localised property cannot be cleared by submitting null; null means preserve.
- Collection patch properties replace the complete collection when supplied.
- String properties generally cannot be cleared to null through patch methods because null means preserve; an empty string is applied where validation does not reject it.

**Modification constraint:** New non-nullable patch fields require an equivalent presence mechanism or nullable wrapper.

## Home-Specific Process Lock

**Decision:** Serialise every `HomeService` operation through a private instance lock.

**Evidence:** The private `Execute` methods in [HomeService.cs](../NuciCraft.API/Service/HomeService.cs).

**Motivation:** Not recorded. The lock makes uniqueness checks and saves atomic relative to other Home operations in the identical process.

**Consequences:**
- Reads and mutations in the same request scope cannot execute concurrently through the scoped Home service instance.
- The lock does not coordinate another process or direct repository consumer.
- Player validation occurs while the Home lock is held.

**Modification constraint:** Preserve the lock boundary around uniqueness validation and persistence unless replacing it with a stronger consistency mechanism.

## Zone Bounds Canonicalisation

**Decision:** Represent a zone cuboid with two corners and canonicalise them to minimum X, maximum Y, minimum Z and maximum X, minimum Y, maximum Z. Pitch and yaw become zero.

**Evidence:** `GetNormalisedBounds` in [ZoneService.cs](../NuciCraft.API/Service/ZoneService.cs).

**Motivation:** Not recorded. Canonical ordering makes containment checks deterministic regardless of submitted corner order.

**Consequences:** Read operations can return canonical values different from old persisted ordering. A bounds patch merges the supplied corner with the other persisted corner before validation and canonicalisation.

**Modification constraint:** Preserve canonical orientation in additions, patches, single reads, and collections; coordinate containment relies on it.

## Read-Time Zone Enrichment

**Decision:** Derive a zone map URL during reads only when no explicit map link exists, a teleportation point exists, the zone world exists, and that world permits a web map.

**Evidence:** `EnrichZone` in [ZoneService.cs](../NuciCraft.API/Service/ZoneService.cs).

**Motivation:** Not recorded. The method preserves explicit links while supplying a derived destination for eligible records.

**Consequences:** The derived link is not persisted. Configuration or world metadata changes affect subsequent reads. Failure to resolve the zone world can fail a read that would otherwise retrieve the zone.

**Modification constraint:** Do not persist derived links unless intentionally changing the state contract and invalidation semantics.

## Enum-Like Classes With Fallbacks

**Decision:** Model `Gender`, `Localisation`, `MobType`, and `WorldType` as sealed registry classes rather than C# enumerations.

**Evidence:** Corresponding files under [Service/Models](../NuciCraft.API/Service/Models/).

**Motivation:** Not recorded. The classes provide external string names, value equality, and conversion helpers.

**Consequences:**
- String parsing is case-insensitive.
- Unknown Gender becomes `Other`.
- Unknown WorldType becomes `Overworld`.
- Unknown MobType becomes `Unsupported` and is later rejected by `MobService`.
- Unknown Localisation becomes `Unsupported`.

**Modification constraint:** Add a value to the registry, public accessor, parsing tests, serialisation expectations, and every exhaustive service branch.

## Synchronous Service Contracts

**Decision:** Keep every application service method synchronous, including persistence, server status, and mob-name generation.

**Evidence:** Interfaces under [Service](../NuciCraft.API/Service/) and the blocking wait in [MobService.cs](../NuciCraft.API/Service/MobService.cs).

**Motivation:** Not recorded.

**Consequences:** Request threads remain occupied during filesystem and network operations. Cancellation tokens do not propagate. External latency directly increases HTTP request duration.

**Modification constraint:** An asynchronous conversion crosses interfaces, controllers, tests, logging, and package boundaries; do not convert one isolated call without tracing the complete request path.

## Package-Owned Cross-Cutting Conduct

**Decision:** Delegate authorisation processing, request envelopes, exception translation, scanner protection, request logging, persistence adapters, HMAC handling, and structured logging primitives to versioned Nuci packages.

**Evidence:** [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj), [Startup.cs](../NuciCraft.API/Startup.cs), and controller base classes.

**Motivation:** Not recorded. The approach centralises organisation-wide infrastructure outside this repository.

**Consequences:** Local source cannot establish every error mapping, authentication detail, serialisation rule, or repository consistency guarantee. Package upgrades have architectural impact.

**Modification constraint:** Inspect release notes and execute integration tests for package upgrades, especially NuciAPI, NuciDAL, NuciLog, and NuciSecurity.
