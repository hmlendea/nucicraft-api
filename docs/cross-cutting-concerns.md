# Cross-Cutting Concerns

| Metadata | Value |
|----------|-------|
| Purpose | Explain repository-wide implementation patterns that cross controllers, services, persistence, configuration, and operations. |
| Scope | Logging, validation, serialisation, HMAC, mapping, dependency injection, authentication, localisation, time, identifiers, HTTP, filesystem, caching, telemetry, and disposal. |
| Primary sources | [Startup.cs](../NuciCraft.API/Startup.cs), [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs), [Requests](../NuciCraft.API/Requests/), [Responses](../NuciCraft.API/Responses/), and [Service](../NuciCraft.API/Service/) |
| Related documents | [Architecture](architecture.md), [error handling](error-handling.md), [security](security.md), [configuration](configuration.md), and [concurrency](concurrency-and-scheduling.md) |

## Logging

### Request Logging

`UseNuciApiRequestLogging` is the third middleware after exception handling and scanner protection. Content, redaction, timing, and sink semantics belong to the package.

### Service Operation Logging

Services use NuciLog operations and statuses:
1. `OperationStatus.Started` before principal logic.
2. `OperationStatus.Success` after completion.
3. `OperationStatus.Failure` with exception in catch blocks.

[MyOperation.cs](../NuciCraft.API/Logging/MyOperation.cs) defines 31 operation names. [MyLogInfoKey.cs](../NuciCraft.API/Logging/MyLogInfoKey.cs) defines Biome, Category, Count, CreatedDT, Identifier, LastIpAddress, MobType, OfflineUUID, OnlineUUID, PlayerID, SkinUrl, UpdatedDT, Username, World, X, Y, and Z.

Logging placement is not uniform. Some argument checks precede Started or occur outside catch blocks. ServerStatusService does not log directly.

### Sensitive Context

Logs can contain personal and location data. Local calls do not add passwords or API keys, but package request logging requires separate scrutiny. Logging is diagnostic, not a durable audit transaction.

## Validation

Validation has three levels:
- ASP.NET Core parsing and data annotations for shape, required values, and mob count range.
- Package-owned `ProcessRequest` authorisation and potentially additional request validation.
- Service checks for domain and persistence invariants.

Service validation is intentionally required for rules that must hold outside HTTP, such as Home uniqueness and Zone references. Some transport-required values are not independently revalidated by services, so direct callers can observe different exception types.

There is no FluentValidation, common validation pipeline, or custom validator abstraction.

## Serialisation

### HTTP JSON

ASP.NET Core controllers use default configured System.Text.Json behaviour; no local `AddJsonOptions` call changes naming, enum, number, cycle, or null settings.

Explicit aliases include:
- Identifier as `id` in selected requests and responses.
- MobType as `type`.
- Count as `count`.
- Generator Schema and Count as lower-case names.
- Localised data-object fields as explicit lower-case names.

Enum-like classes are reference types with public Name and ExternalName properties, not System.Text.Json string enums. Public response serialisation behaviour therefore depends on their exposed properties and framework defaults.

### Persistence JSON

NuciDAL controls serialisation. Data objects expose public properties and selected `JsonPropertyName` attributes. There is no local serialiser configuration or schema version.

## HMAC Contract Metadata

NuciSecurity attributes annotate canonical order. Collection Count is generally `HmacIgnore` because it derives from the collection. Changing orders, aliases, nested representations, or ignored fields can alter client signature compatibility.

No local HMAC implementation or end-to-end signature fixture establishes exact package behaviour.

## Dependency Injection

The composition root registers concrete infrastructure once. Key conventions are:
- Controller dependencies use service interfaces.
- Service dependencies use repository or client interfaces where available.
- Settings are concrete singleton objects rather than options wrappers.
- Application services and repositories are singleton.
- Logger is scoped despite singleton consumers.

Constructor injection uses C# primary constructors throughout much of the code. A novel dependency requires registration and service-collection resolution coverage.

## Mapping

Internal extension methods convert data objects and service models. They centralise:
- Timestamp parsing and formatting.
- Nested coordinates and bounds.
- Localised strings.
- Player settings.
- Enum-like external names.

Mapping directories are inconsistent: ten files use singular `Mapping`, while Home uses plural `Mappings`. This is structural history, not a documented semantic distinction.

## Authentication And Authorisation

Every action passes the identical API-key descriptor to `ProcessRequest`. There is no user principal, role, scope, or per-record ownership authorisation. `UseAuthorization` adds no visible local policy.

Outbound generator bearer authentication is separate and explicit.

## Localisation

Two distinct concepts use similar names:
- `LocalisedString` stores up to eleven language-specific text values for domain labels.
- `Localisation` is an enum-like Player preference with Unsupported, English, and Romanian.

Localised field patches merge non-null languages. No translation fallback selects a preferred language for responses; clients receive the complete object.

Player Localisation parsing is case-insensitive and unsupported input becomes the Unsupported sentinel.

## Time And Date

### Persistence Timestamps

`TimestampFormats.Full` defines seven fractional digits and zone offset. Generated values use UTC. Player registration validates supplied values exactly; patch does not.

### Zone Domain Date

Zone CreationDate is a free string separate from CreatedDT. When omitted on add, the service obtains current time in `Europe/Bucharest`, formats the calendar date, and appends ` (?)` to express uncertainty.

### Time Abstraction

No clock interface exists. Services call `DateTimeOffset.UtcNow` directly, and ZoneService resolves system time-zone data directly. Tests must compare ranges or control inputs rather than inject time.

## Identifier Generation And Equality

- Player, Home, and RTP use `Guid.NewGuid().ToString()`.
- Player offline UUID uses deterministic version-3-compatible MD5 conversion.
- Country, World, ZoneType, and Zone IDs come from clients.
- Most selector equality is ordinal and case-sensitive.
- Home Name and some type filters are case-insensitive.
- Zone category join is case-sensitive while ZoneType category filter is case-insensitive.

No common identifier value type or comparer centralises these decisions.

## HTTP Behaviour

Routes are unversioned. Controller actions are synchronous. Success envelopes and errors are package-owned. HTTPS redirection, default files, and static files are active; no `wwwroot` content is tracked.

No local CORS, rate limit, response compression, OpenAPI, forwarded headers, HSTS, or health-check registration exists.

## Filesystem Access

Startup uses `System.IO` directly to create directories and files. Services use repositories rather than direct file APIs. Relative paths depend on working directory.

No file abstraction supports isolated unit testing of startup; tests instead use temporary real directories and mocked repositories.

## Caching

No application cache is configured. Server status and mob names are obtained per request. Zone MapLink derives per read. Repository-internal retention is external and must not be described as an application cache without package evidence.

## Telemetry

No metrics, distributed traces, analytics, or telemetry export is implemented. Logs are the only repository-configured observability channel.

## Retry And Resilience

No retry library or policy is configured. Minecraft status alone has explicit degraded fallback. Mob HTTP and persistence fail immediately.

## Resource Disposal

- The host and service provider own singleton disposal.
- No service implements `IDisposable` locally.
- Repository, logger, and HTTP client disposal are package conduct.
- ServerStatusService creates MineStat without a visible local disposal contract.
- Home lock release follows language monitor semantics.

## Background Execution

None. Store preparation is synchronous startup work; all other operations are request-driven.

## Modification Checklist

For any cross-cutting revision, examine:
- Middleware order.
- Singleton and scoped lifetime interactions.
- Public serialisation and HMAC compatibility.
- Service and request logging privacy.
- Unit and integration test boundaries.
- Configuration defaults and protected deployment overrides.
- Persisted shape compatibility.
- Documentation in the relevant concern-specific reference.
