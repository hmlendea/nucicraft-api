# Testing Architecture

| Metadata | Value |
|----------|-------|
| Purpose | Document test projects, levels, fixtures, doubles, data isolation, verified conduct, commands, and confirmed coverage limitations. |
| Scope | Unit and integration projects plus CI execution. |
| Primary sources | [NuciCraft.API.UnitTests](../NuciCraft.API.UnitTests/), [NuciCraft.API.IntegrationTests](../NuciCraft.API.IntegrationTests/), their project files, and [.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml) |
| Related documents | [Architecture](architecture.md), [repository structure](repository-structure.md), [error handling](error-handling.md), and [invariants](invariants.md) |

## Test Projects

| Project | Level | Frameworks | Production Host |
|---------|-------|------------|-----------------|
| [NuciCraft.API.UnitTests](../NuciCraft.API.UnitTests/) | Isolated unit and component tests | NUnit 4.6.1, Moq 4.20.72, Microsoft.NET.Test.Sdk 18.10.1 | No HTTP host. |
| [NuciCraft.API.IntegrationTests](../NuciCraft.API.IntegrationTests/) | In-memory HTTP, restart persistence, and loopback TCP integration | NUnit, Moq, `WebApplicationFactory`, TestHost | ASP.NET Core test host. |

Both target .NET 10 and reference the production project. The integration project activates nullable reference analysis and latest language version; the production and unit project do not declare those identical properties.

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

CI restores and compiles first, then executes `dotnet test --no-build --verbosity normal`.

No coverage collector, threshold, public report, snapshot framework, benchmark suite, or mutation-test configuration is tracked.

## Unit-Test Architecture

### Host And Composition

| Fixture | Verification |
|---------|--------------|
| [ProgramTests.cs](../NuciCraft.API.UnitTests/ProgramTests.cs) | Host builder can compile from command-line settings; invalid store path fails before server loop. |
| [StartupTests.cs](../NuciCraft.API.UnitTests/StartupTests.cs) | Configuration retention, service registration, directory and file creation, and eager GetAll calls in Development and Production. |
| [ServiceCollectionExtensionsTests.cs](../NuciCraft.API.UnitTests/ServiceCollectionExtensionsTests.cs) | All settings values and every custom registration resolve. |

`TestConfigurationFactory` constructs in-memory and command-line settings with temporary paths. Startup tests replace every repository with Moq and verify each eager enumeration.

### Controller Tests

Nine fixtures under [Controllers](../NuciCraft.API.UnitTests/Controllers/) mirror the nine production controllers. [ControllerTestContext.cs](../NuciCraft.API.UnitTests/Controllers/ControllerTestContext.cs) creates a `DefaultHttpContext`, inserts an authorised bearer header, and assigns it to the controller.

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
- Mob schema selection, outbound bearer authorisation, response validation, and HMAC request ordering.
- World type fallback and patching.
- Zone references, bounds canonicalisation, partial bounds, containment, category filtering, creation defaults, and non-persisted web-map enrichment.
- Server settings validation in unit tests and protocol parsing in integration tests.

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

The default constructor creates a unique path beneath the system temporary directory and deletes it on disposal. A second constructor accepts a retained directory for restart-persistence tests.

In-memory configuration supplies:
- Seven isolated store paths.
- Test-specific RTP thresholds.
- Example-invalid external URLs.
- Deterministic server metadata.
- Test API keys.
- Deactivated file logging.

### Substitutions

- `INuciApiClient` is replaced with a mock returning the requested count of `Ilarion` names.
- `IServerStatusService` is replaced with a mock returning 42.

Ordinary HTTP integration tests therefore do not contact the internet or Minecraft. The dedicated server-status fixture constructs the real service separately.

### Authorised Client

`CreateAuthorisedClient` creates an HttpClient, adds bearer credentials, and adds `X-Forwarded-For: 127.0.0.1`. [ApiRequestExtensions.cs](../NuciCraft.API.IntegrationTests/ApiRequestExtensions.cs) creates UTF-8 `application/json` content.

## Integration Fixtures

| Fixture | Principal Coverage |
|---------|--------------------|
| [ApiErrorResponseTests.cs](../NuciCraft.API.IntegrationTests/ApiErrorResponseTests.cs) | `401`, required-field `400`, malformed JSON problem response, absent-record `404`. |
| [CountriesApiTests.cs](../NuciCraft.API.IntegrationTests/CountriesApiTests.cs) | Country create, read, list, patch. |
| [HomesApiTests.cs](../NuciCraft.API.IntegrationTests/HomesApiTests.cs) | Home CRUD, selectors, uniqueness, ownership conflicts, and unauthorised commands. |
| [MobsApiTests.cs](../NuciCraft.API.IntegrationTests/MobsApiTests.cs) | Supported types, count range, and unsupported `501`. |
| [PersistenceApiTests.cs](../NuciCraft.API.IntegrationTests/PersistenceApiTests.cs) | Home state and uniqueness plus World state across host restart. |
| [PlayersApiTests.cs](../NuciCraft.API.IntegrationTests/PlayersApiTests.cs) | Registration, selector reads, list, and patch settings. |
| [RtpLocationsApiTests.cs](../NuciCraft.API.IntegrationTests/RtpLocationsApiTests.cs) | Addition, configured thresholds, and random filters. |
| [ServersApiTests.cs](../NuciCraft.API.IntegrationTests/ServersApiTests.cs) | Configured response plus substituted count and authorisation. |
| [ServerStatusServiceTests.cs](../NuciCraft.API.IntegrationTests/ServerStatusServiceTests.cs) | Real MineStat legacy TCP protocol, counts, unavailability, timeout, and invalid data. |
| [WorldsApiTests.cs](../NuciCraft.API.IntegrationTests/WorldsApiTests.cs) | World create, read, list, and patch. |
| [ZonesApiTests.cs](../NuciCraft.API.IntegrationTests/ZonesApiTests.cs) | Zone add, reference and bounds behaviour, category, type, and coordinate queries. |
| [ZoneTypesApiTests.cs](../NuciCraft.API.IntegrationTests/ZoneTypesApiTests.cs) | ZoneType create, read, list, category, and patch. |

Fixtures touching the shared in-memory host or filesystem commonly use `NonParallelizable` to prevent test interference.

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
| File persistence | Integration host and restart tests. |
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
