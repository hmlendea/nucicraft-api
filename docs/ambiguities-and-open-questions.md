# Ambiguities And Open Questions

| Metadata | Value |
|----------|-------|
| Purpose | Record material matters that repository evidence does not establish confidently, preventing speculation from becoming architectural fact. |
| Scope | External package semantics, historical rationale, deployment assumptions, inconsistent contracts, and unresolved design intent. |
| Primary sources | Current production code, tests, project manifests, and existing root documentation. |
| Related documents | [Design decisions](design-decisions.md), [invariants](invariants.md), [dependencies](dependencies.md), and [documentation maintenance](documentation-maintenance.md) |

## NuciApiController Processing Semantics

**Ambiguity:** The complete order and mechanics of request validation, API-key comparison, HMAC processing, response wrapping, and exception translation inside `ProcessRequest` are external.

**Relevant sources:** Controllers under [Controllers](../NuciCraft.API/Controllers/), [ApiErrorResponseTests.cs](../NuciCraft.API.IntegrationTests/ApiErrorResponseTests.cs), and NuciAPI package references in [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj).

**Confirmed:** Every action passes a request, delegate, and API-key descriptor. Selected HTTP outcomes are integration-tested.

**Indeterminate:** Complete status mapping, error schema, constant-time key comparison, HMAC verification details, and validation sequence.

**Why it matters:** Controller or package changes can alter public security and error contracts without local source changes.

## NuciDAL Consistency And File Semantics

**Ambiguity:** Repository internals are not local.

**Relevant sources:** Repository use throughout [Service](../NuciCraft.API/Service/) and adapter creation in [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs).

**Confirmed:** Local code uses Get, GetAll, Add, Update, Remove, and SaveChanges over singleton JSON repositories; ordinary restart persistence works in selected tests.

**Indeterminate:** Atomic writes, locks, duplicate IDs, in-memory retention, snapshots, file-watch conduct, external-edit visibility, recovery, and durability.

**Why it matters:** These semantics control concurrency, scale-out, corruption recovery, and persistence-adapter upgrades.

## Scoped Logger Captured By Singletons

**Ambiguity:** `ILogger` is scoped while every application service is singleton.

**Relevant source:** [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs).

**Confirmed:** Dependency injection can resolve services in current tests.

**Indeterminate:** Intended logger scope, whether NuciLogger carries request-specific state, and whether the lifetime mismatch is intentional.

**Why it matters:** Request correlation, disposal, and state leakage assumptions may be incorrect.

## Duplicate Player HMAC Order

**Ambiguity:** `GetPlayerResponse.Gender` and `GetPlayerResponse.Settings` both use `HmacOrder(27)`.

**Relevant source:** [GetPlayerResponse.cs](../NuciCraft.API/Responses/GetPlayerResponse.cs).

**Confirmed:** The duplicate exists and response tests verify property values.

**Indeterminate:** Whether NuciSecurity permits duplicates, how it orders them, and whether clients depend on present behaviour.

**Why it matters:** Renumbering may repair a defect or create a signature compatibility break.

## Static DataStoreSettings During Registration

**Ambiguity:** `ServiceCollectionExtensions` stores DataStoreSettings in a private static field.

**Relevant source:** [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs).

**Confirmed:** Normal production startup assigns it before repository registration. Tests construct several service collections sequentially.

**Indeterminate:** Whether concurrent host construction is supported or intended and whether repository delegates can observe a later assignment.

**Why it matters:** Parallel test hosts or multiple in-process hosts may bind repositories to unexpected paths.

## Unused NuciText Registrations

**Ambiguity:** Normaliser and obfuscator are registered but not consumed by local production classes.

**Relevant source:** [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs).

**Confirmed:** Resolution is tested.

**Indeterminate:** Historical reason, external reflection-based consumption, or planned capability.

**Why it matters:** Removing apparently unused dependencies can violate an external expectation; retaining them adds maintenance surface.

## Mapping Versus Mappings Directories

**Ambiguity:** Home mapping resides under plural `Service/Mappings`; every other mapping resides under singular `Service/Mapping`.

**Relevant paths:** [Mapping](../NuciCraft.API/Service/Mapping/) and [Mappings](../NuciCraft.API/Service/Mappings/).

**Confirmed:** Both compile and are used.

**Indeterminate:** Whether this distinction is intentional or historical.

**Why it matters:** New mapping placement and any consolidation can affect namespaces, reflection tests, and agent discovery.

## Zone World Relationships

**Ambiguity:** Zone.World, Bounds corner World, and TeleportationPoint.World can differ.

**Relevant source:** [ZoneService.cs](../NuciCraft.API/Service/ZoneService.cs).

**Confirmed:** Zone.World and Type references are validated; bounds corners only validate versus one another; teleportation World is not cross-validated. Enrichment retrieves Zone.World but places TeleportationPoint.World in the URL.

**Indeterminate:** Whether cross-world values are valid use cases or omitted validation.

**Why it matters:** Tightening validation could reject existing records; retaining it can produce links whose selected map metadata and query world differ.

## Zone Category Case Sensitivity

**Ambiguity:** ZoneType direct category filtering is case-insensitive, while Zone category joins are case-sensitive.

**Relevant sources:** [ZoneTypeService.cs](../NuciCraft.API/Service/ZoneTypeService.cs) and [ZoneService.cs](../NuciCraft.API/Service/ZoneService.cs).

**Confirmed:** The comparers differ.

**Indeterminate:** Whether this is deliberate contract design or inconsistency.

**Why it matters:** A normalisation change alters query results and requires compatibility tests.

## Direct Query Parameters Outside Request DTOs

**Ambiguity:** Zone `type` and ZoneType `category` filters are direct controller parameters while empty request objects pass to `ProcessRequest`.

**Relevant sources:** [ZonesController.cs](../NuciCraft.API/Controllers/ZonesController.cs), [ZoneTypesController.cs](../NuciCraft.API/Controllers/ZoneTypesController.cs), [GetZonesRequest.cs](../NuciCraft.API/Requests/GetZonesRequest.cs), and [GetZoneTypesRequest.cs](../NuciCraft.API/Requests/GetZoneTypesRequest.cs).

**Confirmed:** The service receives filters, but request DTOs do not contain them.

**Indeterminate:** Whether authorisation or HMAC processing is intended to include those query values.

**Why it matters:** Moving fields into DTOs may alter signatures and client compatibility.

## Player Password Purpose

**Ambiguity:** Password is accepted, stored, patched, and returned without local hash or validation.

**Relevant sources:** [PlayerService.cs](../NuciCraft.API/Service/PlayerService.cs), [PlayerDataObject.cs](../NuciCraft.API/DataAccess/DataObjects/PlayerDataObject.cs), and [GetPlayerResponse.cs](../NuciCraft.API/Responses/GetPlayerResponse.cs).

**Confirmed:** Current plaintext-equivalent conduct and response exposure.

**Indeterminate:** Intended authentication protocol, whether the value is independently transformed by clients, and historical compatibility requirements.

**Why it matters:** A secure redesign requires purpose clarification, migration, compatibility, and response-contract changes.

## Configuration Placeholder Substitution

**Ambiguity:** Committed secret and URL values use double-bracket deployment marker syntax.

**Relevant source:** [appsettings.json](../NuciCraft.API/appsettings.json).

**Confirmed:** Standard ASP.NET Core configuration does not interpolate this syntax. Environment and command-line overrides are available.

**Indeterminate:** The deployment system that replaces or overrides placeholders.

**Why it matters:** A deployment that omits override can start with literal placeholder credentials or invalid destinations.

## External HTTP Timeout

**Ambiguity:** MobService configures no HTTP timeout.

**Relevant sources:** [MobService.cs](../NuciCraft.API/Service/MobService.cs) and NuciAPI.Client package.

**Confirmed:** Local code blocks synchronously on `SendRequestAsync` and adds no cancellation token.

**Indeterminate:** Effective NuciApiClient timeout, handler lifetime, DNS refresh, proxy, and connection pool policy.

**Why it matters:** Request resource consumption and outage duration depend on external defaults.

## Read-Time Zone Data-Object Mutation

**Ambiguity:** Zone reads assign canonical Bounds back to data objects returned by repositories without SaveChanges.

**Relevant source:** [ZoneService.cs](../NuciCraft.API/Service/ZoneService.cs).

**Confirmed:** Single and collection reads mutate the returned object's Bounds property and do not explicitly save.

**Indeterminate:** Whether NuciDAL returns tracked references whose in-memory repository state changes, copies, or fresh deserialised objects.

**Why it matters:** A nominal read may have process-visible state effects even without file persistence.

## Static And Default File Middleware

**Ambiguity:** Default-file and static-file middleware are active, but no tracked `wwwroot` content exists.

**Relevant source:** [Startup.cs](../NuciCraft.API/Startup.cs).

**Confirmed:** Middleware is registered.

**Indeterminate:** Historical assets, deployment-injected static content, or redundant configuration.

**Why it matters:** Removing it can affect externally supplied content; retaining it adds an HTTP surface whose content root is deployment-dependent.

## Release Process

**Ambiguity:** The repository delegates release conduct to mutable remote code.

**Relevant source:** [release.sh](../release.sh).

**Confirmed:** The wrapper downloads and executes a .NET 10 script from `master`.

**Indeterminate:** Current packaging, versioning, publication, credentials, and rollback conduct.

**Why it matters:** Local documentation cannot guarantee release results, reproducibility, or supply-chain integrity.

## Intended Deployment Topology

**Ambiguity:** Architecture and persistence strongly favour one writable process, but no explicit runtime guard prohibits several instances.

**Relevant sources:** Singleton repositories, lack of distributed coordination, and file stores.

**Confirmed:** No cross-process lock or invalidation exists.

**Probable intent:** One process with modest data volume.

**Indeterminate:** Whether operators currently run another topology with external filesystem guarantees.

**Why it matters:** Scale-out can introduce lost updates, stale state, or corruption.

## Resolution Protocol

When resolving an item:
1. Obtain direct source, package, deployment, or maintainer evidence.
2. Add an executable regression test where possible.
3. Revise canonical documents.
4. Record a design decision when the result constrains future modifications.
5. Remove the item only when uncertainty is materially resolved.
