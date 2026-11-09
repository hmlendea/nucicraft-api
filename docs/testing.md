# Testing Architecture

| Metadata | Value |
|----------|-------|
| Purpose | Document test projects, levels, fixtures, doubles, data isolation, verified conduct, commands, and confirmed coverage limitations. |
| Scope | Architecture, unit, and integration projects plus CI execution. |
| Primary sources | [NuciCraft.API.UnitTests](../NuciCraft.API.UnitTests/), [NuciCraft.API.IntegrationTests](../NuciCraft.API.IntegrationTests/), their project files, and [.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml) |
| Related documents | [Architecture](architecture.md), [repository structure](repository-structure.md), [error handling](error-handling.md), and [invariants](invariants.md) |

## Test Projects

| Project | Level | Frameworks | Production Host |
|---------|-------|------------|-----------------|
| [NuciCraft.API.ArchitectureTests](../NuciCraft.API.ArchitectureTests/) | Structural dependency and boundary rules | NUnit, ArchUnit.NET | No HTTP host. |
| [NuciCraft.API.UnitTests](../NuciCraft.API.UnitTests/) | Isolated unit and component tests | NUnit 4.6.1, Moq 4.20.72, Microsoft.NET.Test.Sdk 18.10.1 | No HTTP host. |
| [NuciCraft.API.IntegrationTests](../NuciCraft.API.IntegrationTests/) | In-memory HTTP, restart persistence, and loopback TCP integration | NUnit, Moq, `WebApplicationFactory`, TestHost | ASP.NET Core test host. |

All three target .NET 10 and reference the production project. The architecture and integration projects activate nullable reference analysis and latest language version; the production and unit project do not declare those identical properties.

## Execution Commands

Complete solution:

```bash
dotnet test NuciCraft.API.slnx
```

Unit only:

```bash
dotnet test NuciCraft.API.UnitTests/NuciCraft.API.UnitTests.csproj
```

Integration only:

```bash
dotnet test NuciCraft.API.IntegrationTests/NuciCraft.API.IntegrationTests.csproj
```

CI restores and compiles first, then executes `dotnet test --no-build --verbosity normal`. The separate [security workflow](../.github/workflows/security.yml) reports vulnerable packages and uploads CodeQL results; it does not execute tests.

Architecture only:

```bash
dotnet test NuciCraft.API.ArchitectureTests/NuciCraft.API.ArchitectureTests.csproj
```

No coverage collector, threshold, public report, snapshot framework, benchmark suite, or mutation-test configuration is tracked.

## Architecture Tests

The [architecture test project](../NuciCraft.API.ArchitectureTests/NuciCraft.API.ArchitectureTests.csproj) participates in solution-wide test execution. It loads the single production assembly once through ArchUnit.NET and excludes test assemblies from the analysed architecture. Every source selector must match existing types; internal layer targets must also be nonempty. Forbidden external namespace targets may be absent because those dependencies are prohibited rather than required.

The [dependency rules](../NuciCraft.API.ArchitectureTests/LayerDependencyTests.cs) enforce these forbidden directions:

| Source | Forbidden Dependencies |
|--------|------------------------|
| Controllers | Repositories, concrete services, outbound API clients, service helpers/mappings, persistence frameworks, and persistence records except the coordinate exception below. |
| Services | Controllers, response DTOs, ASP.NET Core MVC, Entity Framework, and DynamoDB. |
| Service models | Persistence, requests, responses, controllers, and configuration. |
| Persistence | Controllers, responses, services, and requests except the merge exception below. |
| Responses | Persistence and controllers. |
| Configuration and logging | Controllers, services, and persistence. |

These are namespace boundaries inside one production project, not separate Domain, Application, Infrastructure, and API assemblies. A project-level layer matrix cannot exist until those layers are split into projects. Independent [reference checks](../NuciCraft.API.ArchitectureTests/ProductionReferenceTests.cs) inspect both compiled assembly references and a build-copied production project file, rejecting test dependencies and unapproved persistence frameworks. They also prohibit production friend-assembly declarations. [HTTP boundary checks](../NuciCraft.API.ArchitectureTests/HttpBoundaryTests.cs) verify controller placement, the shared Nuci controller base, routing metadata, and interface-based service injection.

Current compatibility exceptions are deliberately narrow:
- The zone controller may use the persistence-owned coordinate type, but no other persistence record.
- The player-settings merge extension may consume the player-settings patch request, but no other request contract.
- Existing requests may expose persistence-owned nested values; services may consume request contracts directly.

The request/persistence cycle is therefore intentional legacy coupling, not an acyclic boundary. The suite does not assert that all namespaces are cycle-free. These exceptions record existing implementation constraints and do not endorse additional coupling. Structural checks complement behavioural tests; they cannot prove authorisation, correct persistence, or runtime dependency lifetimes.

## Unit-Test Architecture

### Host And Composition

| Fixture | Verification |
|---------|--------------|
| [ProgramTests.cs](../NuciCraft.API.UnitTests/ProgramTests.cs) | Host builder can compile from command-line settings; invalid store path fails before server loop. |
| [StartupTests.cs](../NuciCraft.API.UnitTests/StartupTests.cs) | Configuration retention, service registration, directory and file creation, and verification that startup does not eagerly call `GetAll`. |
| [ServiceCollectionExtensionsTests.cs](../NuciCraft.API.UnitTests/ServiceCollectionExtensionsTests.cs) | All settings values and every custom registration resolve; two service collections with different store paths each read their own store. |

`TestConfigurationFactory` constructs in-memory and command-line settings with temporary paths. Startup tests replace every repository with Moq and verify each eager enumeration.

### Anonymisation Detection Tests

| Fixture | Verification |
|---------|--------------|
| [CachedAnonymisationServiceDetectorTests.cs](../NuciCraft.API.UnitTests/Service/Anonymisation/CachedAnonymisationServiceDetectorTests.cs) | Cache reuse, two-hour and six-hour lifetimes, expiry refresh, expired-result fallback, and failure propagation without a cache entry. |
| [AnonymisationServiceDetectorFactoryTests.cs](../NuciCraft.API.UnitTests/Service/Anonymisation/AnonymisationServiceDetectorFactoryTests.cs) | Provider selection, cached-decorator construction, and rejection of unsupported providers. |
| [AnonymisationServiceDetectionSettingsTests.cs](../NuciCraft.API.UnitTests/Configuration/AnonymisationServiceDetectionSettingsTests.cs) | Committed default cache durations, provider, and flags. |

The cached-detector fixture drives expiry through a `FakeTimeProvider` instead of a wall-clock wait, so the assertions remain deterministic.

### Controller Tests

The fixtures under [Controllers](../NuciCraft.API.UnitTests/Controllers/) mirror the eleven production controllers, including [ItemsControllerTests.cs](../NuciCraft.API.UnitTests/Controllers/ItemsControllerTests.cs). [ControllerTestContext.cs](../NuciCraft.API.UnitTests/Controllers/ControllerTestContext.cs) creates a `DefaultHttpContext`, inserts an authorised bearer header, and assigns it to the controller.

These tests verify:
- Service method selection and request construction.
- Route selector propagation into patch objects.
- Conditional Home query branches.
- Collection and single response wrappers.
- Zone type and coordinate delegates.
- Server response composition.
- Selected route attributes.

Controller tests mock service interfaces and do not verify persistence or package middleware internals.

### Service Tests

Service fixtures use mocked repositories with callback-backed lists or explicit return values. They verify successful mutations, save calls, logging, exceptions, merge semantics, selectors, defaults, and domain branches.

Notable coverage includes:
- Deterministic Player offline UUIDs and strict registration timestamps.
- Player GetAll, alternate selectors, patch presence, settings merge, and failures.
- Home CRUD, localisation uniqueness, owner transfer, finite coordinates, and concurrent duplicate addition.
- RTP distance thresholds, world and biome distinctions, random filters, and error paths.
- Item selectors by identifier, Minecraft ID, Bukkit ID, and NuciCraft ID; patch application and sign-identifier generation.
- Mob schema selection, outbound bearer authorisation, response validation, and HMAC request ordering.
- World type fallback and patching.
- Zone references, bounds canonicalisation, partial bounds, containment, category filtering, creation defaults, and non-persisted web-map enrichment.
- Server settings validation in unit tests and protocol parsing in integration tests.
- Access-control IP checks covering the registered-player exemption, its configuration toggle, banned and unregistered usernames, casing, whitespace, banned-player IP denial, and both denial messages.
- Access-control registration semantics covering the registered, banned, and unregistered username combinations for both the IP check and the username check, including ban-reason projection and the absence of any anonymisation-service request on the short-circuit paths.

### Models, Mappings, Responses, And Logging

- Enum-like model fixtures verify parsing, fallback, equality, hash, string, and operator conduct to varying degrees.
- [MappingExtensionsTests.cs](../NuciCraft.API.UnitTests/Service/Mapping/MappingExtensionsTests.cs) verifies bidirectional values through internal methods.
- [MappingMethodInvoker.cs](../NuciCraft.API.UnitTests/Service/Mapping/MappingMethodInvoker.cs) uses reflection to invoke internal static mapping extensions by exact type and method name.
- Response fixtures verify projections and collection counts.
- `GetPlayerResponseTests` explicitly verifies Password projection and response property shape.
- `MyLogInfoKeyTests` verifies logging-key identity.

Reflection-based mapping tests are sensitive to namespace, type, method, and parameter signature changes, even when runtime use would compile after a rename.

## Integration-Test Host

[ApiTestHost.cs](../NuciCraft.API.IntegrationTests/ApiTestHost.cs) derives from `WebApplicationFactory<Program>`.

### Per-Host State

The default constructor creates a unique path beneath the system temporary directory, deletes it on disposal, and substitutes every `IFileRepository<T>` with [InMemoryRepository.cs](../NuciCraft.API.IntegrationTests/InMemoryRepository.cs), a JSON clone-backed in-memory double implementing the full repository contract. Ordinary HTTP fixtures therefore share no on-disk state. A second constructor accepts a retained directory, keeps the real file-backed repositories, and uses the same directory for restart-persistence tests.

Every host configures all eight store paths beneath its own temporary directory, so no host resolves the application-wide `Data/` defaults and no test writes to shared runtime state.

In-memory configuration supplies:
- Eight isolated store paths beneath the host's own temporary directory.
- Test-specific RTP thresholds.
- Example-invalid external URLs.
- Deterministic server metadata.
- Test API keys.
- Deactivated file logging.

### Substitutions

- All eight `IFileRepository<T>` registrations are replaced with in-memory repositories by default, giving clones on read and write isolation between fixtures.
- `INuciApiClient` is replaced with a mock returning the requested count of `Ilarion` names.
- `IServerStatusService` is replaced with a mock returning 42.
- `IAccessControlService` is replaced with a mock allowing all IP addresses and usernames except the fixture-specific denied cases.

Ordinary HTTP integration tests therefore contact neither the filesystem stores nor the internet or Minecraft. The dedicated server-status fixture constructs the real service separately.

The anonymisation caching fixture uses a separate host, [AccessControlApiTestHost.cs](../NuciCraft.API.IntegrationTests/AccessControlApiTestHost.cs), which keeps the real `IAccessControlService` and replaces `HttpClient`, `TimeProvider`, and `ILogger` instead. It configures the same eight isolated store paths beneath its own temporary directory. Its stubbed HTTP handler records every requested URL, so the fixture asserts how many detection requests reached the provider, and its `FakeTimeProvider` advances the clock past the cache lifetime.

### Authorised Client

`CreateAuthorisedClient` creates an HttpClient, adds bearer credentials, and adds `X-Forwarded-For: 127.0.0.1`. [ApiRequestExtensions.cs](../NuciCraft.API.IntegrationTests/ApiRequestExtensions.cs) creates UTF-8 `application/json` content.

## Integration Fixtures

| Fixture | Principal Coverage |
|---------|--------------------|
| [ApiErrorResponseTests.cs](../NuciCraft.API.IntegrationTests/ApiErrorResponseTests.cs) | `401`, required-field `400`, malformed JSON problem response, absent-record `404`. |
| [AccessControlApiTests.cs](../NuciCraft.API.IntegrationTests/AccessControlApiTests.cs) | Substituted access-control decisions across the country, anonymisation, and username paths. |
| [AccessControlIpUsernameApiTests.cs](../NuciCraft.API.IntegrationTests/AccessControlIpUsernameApiTests.cs) | Registered-player exemption and banned-player IP denial across HTTP query binding, including unregistered, banned, absent, and differently cased usernames. |
| [AccessControlRegistrationApiTests.cs](../NuciCraft.API.IntegrationTests/AccessControlRegistrationApiTests.cs) | Registered, banned, and unregistered username combinations for the IP and username checks, including ban-reason projection and anonymisation-request counts. |
| [AnonymisationDetectionCachingApiTests.cs](../NuciCraft.API.IntegrationTests/AnonymisationDetectionCachingApiTests.cs) | Detection cache reuse per IP address, cache expiry, successful refresh, and expired-result fallback on provider failure. |
| [CountriesApiTests.cs](../NuciCraft.API.IntegrationTests/CountriesApiTests.cs) | Country create, read, list, patch. |
| [HomesApiTests.cs](../NuciCraft.API.IntegrationTests/HomesApiTests.cs) | Home CRUD, selectors, uniqueness, ownership conflicts, and unauthorised commands. |
| [ItemsApiTests.cs](../NuciCraft.API.IntegrationTests/ItemsApiTests.cs) | Item create, every selector read, list, patch, duplicate conflict, absent `404`, required-field `400`, and unauthorised `401`. |
| [MobsApiTests.cs](../NuciCraft.API.IntegrationTests/MobsApiTests.cs) | Supported types, count range, and unsupported `501`. |
| [PersistenceApiTests.cs](../NuciCraft.API.IntegrationTests/PersistenceApiTests.cs) | Home state and uniqueness plus World state across host restart. |
| [PlayersApiTests.cs](../NuciCraft.API.IntegrationTests/PlayersApiTests.cs) | Registration, selector reads, list, and patch settings. |
| [RtpLocationsApiTests.cs](../NuciCraft.API.IntegrationTests/RtpLocationsApiTests.cs) | Addition, configured thresholds, and random filters. |
| [ServersApiTests.cs](../NuciCraft.API.IntegrationTests/ServersApiTests.cs) | Configured response plus substituted count and authorisation. |
| [ServerStatusServiceTests.cs](../NuciCraft.API.IntegrationTests/ServerStatusServiceTests.cs) | Real MineStat legacy TCP protocol, counts, unavailability, timeout, and invalid data. |
| [WorldsApiTests.cs](../NuciCraft.API.IntegrationTests/WorldsApiTests.cs) | World create, read, list, and patch. |
| [ZonesApiTests.cs](../NuciCraft.API.IntegrationTests/ZonesApiTests.cs) | Zone add, reference and bounds behaviour, category, type, and coordinate queries. |
| [ZoneTypesApiTests.cs](../NuciCraft.API.IntegrationTests/ZoneTypesApiTests.cs) | ZoneType create, read, list, category, and patch. |

Both test projects declare assembly-level `Parallelizable(ParallelScope.Fixtures)` with `LevelOfParallelism(4)`, so fixtures execute concurrently and no fixture is marked `NonParallelizable`. Isolation relies on per-host state rather than serialisation: every `ApiTestHost` and `AccessControlApiTestHost` instance owns a unique temporary store directory and substitutes its own in-memory repositories, and unit fixtures that touch the filesystem create a `Guid`-suffixed directory per test. Tests inside a fixture still run sequentially. The file-backed restart fixtures are the primary source of store-level concurrency and persistence assertions; fixtures using the default in-memory host exercise the full middleware, binding, and authorisation pipeline over isolated process state.

## Test Data

Tests construct data in code and use temporary files. They do not read the ignored local operational `Data/` directory. Test credentials, addresses, and identities are synthetic fixtures.

The integration host's remote URLs use the reserved `.invalid` domain and mocked clients, preventing accidental external HTTP calls.

## Production Architecture Correlation

| Production Boundary | Verification Boundary |
|---------------------|-----------------------|
| Composition root | Program, Startup, and service-collection unit tests. |
| Controller delegation | Controller unit tests. |
| Domain rules | Service unit tests. |
| Mapping | Reflection-based mapping tests. |
| Response projection | Response unit tests. |
| Middleware and HTTP binding | In-memory HTTP integration tests. |
| File persistence | In-memory repositories for ordinary HTTP isolation; file-backed restart tests for persistence and startup store preparation. |
| External name service | Mocked client contract tests, not live service. |
| Minecraft protocol | Loopback TCP integration tests. |

## Confirmed Test Gaps And Residual Risk

- No multi-process or two-repository-instance file-write test establishes NuciDAL coordination.
- No fault-injection test establishes atomicity after interrupted SaveChanges or malformed existing JSON recovery.
- No live Universal Name Generator compatibility test exists; local tests mock `INuciApiClient`.
- No deployed TLS, reverse proxy, filesystem permission, secret-provider, or process-supervisor test exists.
- No package-level test establishes behaviour for duplicate HMAC order 27 in `GetPlayerResponse`.
- No load, stress, rate-limit, or sustained-concurrency suite exists beyond focused Home service concurrency.
- No cancellation test exists because production contracts do not accept cancellation.
- No release-script verification pins or tests remote release behaviour.
- No automated Markdown or Mermaid validator is tracked by the repository.
- No measured coverage artefact is configured, so completeness must not be inferred from test count.

## Adding Tests

- Place isolated service rules in the corresponding unit fixture.
- Add controller tests for route-to-request translation and response construction.
- Add integration tests when model binding, middleware, serialisation, persistence, or restart behaviour matters.
- Use temporary paths and never local operational stores.
- Replace external network dependencies unless the test is explicitly a bounded loopback protocol integration.
- Add regression coverage for both success and the exact failure branch that motivated a modification.
