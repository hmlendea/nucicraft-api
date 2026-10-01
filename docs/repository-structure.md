# Repository Structure

| Metadata | Value |
|----------|-------|
| Purpose | Map the physical repository to architectural responsibilities and modification boundaries. |
| Scope | Tracked source, tests, configuration, automation, and documentation; generated outputs and local runtime data are identified but not inventoried record by record. |
| Primary sources | [Solution](../NuciCraft.API.slnx), [runtime project](../NuciCraft.API/), [unit tests](../NuciCraft.API.UnitTests/), [integration tests](../NuciCraft.API.IntegrationTests/), and [GitHub workflow](../.github/workflows/dotnet.yml) |
| Related documents | [Architecture](architecture.md), [repository overview](repository-overview.md), and [design decisions](design-decisions.md) |

## Root Organisation

| Path | Responsibility | Preservation Guidance |
|------|----------------|-----------------------|
| [NuciCraft.API](../NuciCraft.API/) | Production ASP.NET Core project. | Runtime logic, HTTP contracts, persistence records, and settings belong here. |
| [NuciCraft.API.UnitTests](../NuciCraft.API.UnitTests/) | Isolated NUnit and Moq verification. | Mirror production component categories and isolate dependencies. |
| [NuciCraft.API.IntegrationTests](../NuciCraft.API.IntegrationTests/) | In-memory HTTP host, temporary stores, persistence restart, and TCP protocol verification. | Use isolated temporary paths and do not depend on local operational data. |
| [docs](./) | Persistent architectural and behavioural knowledge base. | Revise alongside implementation changes according to the maintenance guide. |
| [.github](../.github/) | Repository funding and continuous-integration metadata. | Workflow changes must preserve supported restore, compile, and test validation. |
| [.vscode](../.vscode/) | Local launch and task definitions. | Keep project paths and target framework outputs aligned with the project manifest. |
| [NuciCraft.API.slnx](../NuciCraft.API.slnx) | Solution membership for all three projects. | Add a project here when it must participate in solution-wide restore, compile, or tests. |
| [README.md](../README.md) | Public synopsis, usage, configuration, and contributor entry point. | Retain concise public guidance and link detailed documentation. |
| [ARCHITECTURE.md](../ARCHITECTURE.md) | Existing root architectural synopsis. | Preserve externally useful context; detailed agent-oriented documentation resides under `docs/`. |
| [SECURITY.md](../SECURITY.md) | Supported-version and vulnerability-disclosure policy. | Revise only for policy or security-scope changes. |
| [release.sh](../release.sh) | Maintainer wrapper for a remotely hosted .NET release helper. | Treat the remote script as an external trust and reproducibility boundary. |
| [.gitignore](../.gitignore) | Excludes runtime data, compilation products, test output, secrets, and IDE state. | Do not commit local `Data/` stores or secret-bearing environment files. |
| [LICENSE](../LICENSE) | GNU General Public License terms. | Licence policy is outside ordinary implementation modifications. |

## Production Project

### Composition And Configuration

| Path | Purpose |
|------|---------|
| [Program.cs](../NuciCraft.API/Program.cs) | Executable entry point and default host construction. |
| [Startup.cs](../NuciCraft.API/Startup.cs) | Controller registration, middleware order, store creation, and eager repository enumeration. |
| [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs) | Settings binding, concrete adapter selection, and dependency lifetimes. |
| [appsettings.json](../NuciCraft.API/appsettings.json) | Default paths, ports, thresholds, log settings, and deployment placeholders. |
| [Configuration](../NuciCraft.API/Configuration/) | Six plain settings types bound by section name. |
| [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj) | .NET 10 web project, direct package versions, and configuration-file output copying. |

Composition code must select infrastructure and lifetimes. Domain validation does not belong in settings registration or startup store preparation.

### HTTP Boundary

| Directory | Contents | Boundary Rule |
|-----------|----------|---------------|
| [Controllers](../NuciCraft.API/Controllers/) | Nine attribute-routed controllers. | Bind and delegate; do not access repositories directly. |
| [Requests](../NuciCraft.API/Requests/) | Thirty-one inbound request and patch structures. | Own transport names, required/range validation, query/body shape, and HMAC ordering. |
| [Responses](../NuciCraft.API/Responses/) | Sixteen success-content structures. | Own public response shape and derived collection counts, not persistence. |

Controller names determine most route roots through `[Route("[controller]")]`. `ServersController` deliberately uses singular `/Server`. A public contract modification commonly affects all three directories plus controller and integration tests.

### Application Services

The [Service](../NuciCraft.API/Service/) directory contains nine service interfaces and implementations:
- Country, Home, Mob, Player, RTP location, server status, World, Zone, and ZoneType.
- `GenerateNamesRequest` and `GenerateNamesResponse` define the outbound Universal Name Generator contract.
- `TimestampFormats` centralises generated persisted timestamps and strict player-registration parsing.

Subdirectories have distinct purposes:

| Directory | Purpose | Important Convention |
|-----------|---------|----------------------|
| [Service/Models](../NuciCraft.API/Service/Models/) | Fifteen service-layer models and four enum-like value classes. | These represent application values, not repository or request contracts. |
| [Service/Mapping](../NuciCraft.API/Service/Mapping/) | Ten internal conversion extension classes. | Keep nested and timestamp conversion centralised here. |
| [Service/Mappings](../NuciCraft.API/Service/Mappings/) | `HomeMappingExtensions` only. | This plural directory is an existing naming inconsistency; preserve references unless deliberately consolidated. |
| [Service/Helpers](../NuciCraft.API/Service/Helpers/) | Localised-string selective merge logic. | A null incoming language preserves the persisted language. |

Service interfaces remain controller-facing contracts. Infrastructure implementation selection belongs in the composition root, while domain decisions remain in concrete services.

### Persistence Representation

The [DataAccess/DataObjects](../NuciCraft.API/DataAccess/DataObjects/) directory contains:
- `NuciCraftEntityBase`, which extends NuciDAL `EntityBase` with string creation and revision timestamps.
- Seven root persisted record types corresponding to the seven stores.
- Nested coordinates, zone bounds, localised string, and player settings data objects.
- `PlayerSettingsDataObjectExtensions`, which merges patch settings according to explicit property-presence flags.

No repository implementation is local. NuciDAL supplies `IFileRepository<T>` and `JsonRepository<T>`.

### Runtime Data

The local [Data](../NuciCraft.API/Data/) directory is excluded by [.gitignore](../.gitignore). It can contain operational JSON stores created or modified by the application, but it is not source-controlled repository state. Future agents must not use local record values as stable fixtures, document personal records, or commit the directory.

Store schemas are defined by data-object classes, not by JSON Schema documents. Tests create independent temporary files rather than using the local directory.

### Logging

The [Logging](../NuciCraft.API/Logging/) directory contains:
- [MyOperation.cs](../NuciCraft.API/Logging/MyOperation.cs), the catalogue of 31 application operation names.
- [MyLogInfoKey.cs](../NuciCraft.API/Logging/MyLogInfoKey.cs), the catalogue of structured context keys.

These classes extend NuciLog types. Add a corresponding operation when a novel service operation requires the established started, success, and failure trace.

## Unit-Test Project

| Area | Path | Responsibility |
|------|------|----------------|
| Composition | [ProgramTests.cs](../NuciCraft.API.UnitTests/ProgramTests.cs), [StartupTests.cs](../NuciCraft.API.UnitTests/StartupTests.cs), and [ServiceCollectionExtensionsTests.cs](../NuciCraft.API.UnitTests/ServiceCollectionExtensionsTests.cs) | Host builder, store preparation, eager loads, settings, and registrations. |
| Controller boundary | [Controllers](../NuciCraft.API.UnitTests/Controllers/) | Request construction, route branch delegation, wrappers, and authorisation context. |
| Services and models | [Service](../NuciCraft.API.UnitTests/Service/) | Domain operations, validation branches, mappings, enum-like values, and settings merges. |
| Response contracts | [Responses](../NuciCraft.API.UnitTests/Responses/) | Property projection and collection count derivation. |
| Logging keys | [Logging](../NuciCraft.API.UnitTests/Logging/) | Key identity and naming. |
| Shared configuration | [TestConfigurationFactory.cs](../NuciCraft.API.UnitTests/TestConfigurationFactory.cs) | Repeatable in-memory settings and temporary store paths. |

[MappingMethodInvoker.cs](../NuciCraft.API.UnitTests/Service/Mapping/MappingMethodInvoker.cs) invokes internal extension methods through reflection so mapping tests can remain in a separate assembly without widening production visibility.

## Integration-Test Project

| Path | Responsibility |
|------|----------------|
| [ApiTestHost.cs](../NuciCraft.API.IntegrationTests/ApiTestHost.cs) | `WebApplicationFactory<Program>`, temporary store configuration, authorised client creation, and deterministic integration substitutions. |
| [ApiRequestExtensions.cs](../NuciCraft.API.IntegrationTests/ApiRequestExtensions.cs) | JSON HTTP content construction for tests. |
| [ApiErrorResponseTests.cs](../NuciCraft.API.IntegrationTests/ApiErrorResponseTests.cs) | Observable authorisation, model-binding, malformed JSON, and absent-record statuses. |
| [PersistenceApiTests.cs](../NuciCraft.API.IntegrationTests/PersistenceApiTests.cs) | State and Home uniqueness across process restarts. |
| [ServerStatusServiceTests.cs](../NuciCraft.API.IntegrationTests/ServerStatusServiceTests.cs) | Actual MineStat legacy-protocol exchange over a loopback `TcpListener`. |
| Domain API fixtures | [NuciCraft.API.IntegrationTests](../NuciCraft.API.IntegrationTests/) | Authorised HTTP capability tests for every controller domain. |

`ApiTestHost` replaces `INuciApiClient` and `IServerStatusService` for ordinary HTTP tests. The dedicated server-status integration fixture tests the real service separately.

## Automation And Developer Configuration

| Path | Conduct |
|------|---------|
| [.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml) | On pushes and pull requests to `master`, checks out source, installs .NET 10, restores, compiles without a second restore, and tests without a second compile. |
| [.vscode/tasks.json](../.vscode/tasks.json) | Defines runtime-project compile, publish, and watch tasks. |
| [.vscode/launch.json](../.vscode/launch.json) | Launches the Debug .NET 10 assembly in Development and detects the server URL. |
| [release.sh](../release.sh) | Downloads a version-selected remote release script and pipes it to Bash. |

No repository-local formatter, linter, coverage collector, container manifest, deployment manifest, package lock, or documentation generator is configured.

## Generated And Local Artefacts

The subsequent paths are not architectural source:
- `bin/` and `obj/` are compiler and restore outputs.
- `Data/` is ignored operational state.
- log files are ignored runtime output.
- test result and coverage files are ignored verification output.
- IDE caches and user settings are ignored workstation state.

Future agents must neither document these outputs as stable implementation nor revise them manually. Package dependency truth resides in project manifests, while effective resolved dependency data under `obj/` is disposable.

## Placement Guide

| Modification | Correct Location |
|--------------|------------------|
| HTTP route or binding | `Controllers` and `Requests` |
| Public success shape | `Responses` |
| Domain validation or orchestration | Concrete class under `Service` |
| Controller-facing capability contract | `I*Service` interface under `Service` |
| Application representation | `Service/Models` |
| Persisted representation | `DataAccess/DataObjects` |
| Representation conversion | `Service/Mapping`, respecting the existing Home exception |
| Selective localised merge | `Service/Helpers` |
| Concrete adapter or lifetime | `ServiceCollectionExtensions.cs` |
| Startup filesystem or middleware conduct | `Startup.cs` |
| Default setting | `appsettings.json` and the matching configuration class |
| Isolated regression test | Mirrored area under `NuciCraft.API.UnitTests` |
| HTTP or restart-persistence regression | `NuciCraft.API.IntegrationTests` |
