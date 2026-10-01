# External Dependencies And Integrations

| Metadata | Value |
|----------|-------|
| Purpose | Explain every external system and substantial infrastructure dependency, including initialisation, exchanged data, failure policy, lifecycle, and substitution boundaries. |
| Scope | Runtime network systems, filesystem infrastructure, Nuci package boundaries, MineStat, NuciLog, and the remote release helper. |
| Primary sources | [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj), [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs), [Startup.cs](../NuciCraft.API/Startup.cs), [MobService.cs](../NuciCraft.API/Service/MobService.cs), and [ServerStatusService.cs](../NuciCraft.API/Service/ServerStatusService.cs) |
| Related documents | [Architecture](architecture.md), [external-service component](components/external-services.md), [interfaces](interfaces.md), and [configuration](configuration.md) |

## Integration Classification

| Integration | Category | Direction | Required For |
|-------------|----------|-----------|--------------|
| JSON filesystem stores | Infrastructure system | Read and write | Startup and every file-backed capability. |
| Universal Name Generator API | External service | Outbound HTTP | Mob-name generation only. |
| Minecraft Java server | External service | Outbound TCP | Live count in `GET /Server`. |
| NuciAPI package family | Runtime framework | In-process | Controller processing, envelopes, clients, and middleware. |
| NuciDAL | Persistence library | In-process to filesystem | Seven JSON repositories. |
| NuciLog | Logging library | In-process to configured sink | Request and service diagnostics. |
| NuciSecurity.HMAC | Contract/security library | In-process | Property ordering metadata. |
| NuciText libraries | Utility libraries | In-process | Registered extension utilities; no local production caller. |
| MineStat | Protocol library | In-process to TCP | Java legacy status query. |
| Remote release helper | Maintainer automation | Outbound HTTPS during release | Packaging or publication delegated by `release.sh`. |

## JSON Filesystem Stores

### Purpose

Provide durable state without a database service.

### Initialisation

Seven `JsonRepository<T>` singleton instances are constructed lazily by dependency injection. During startup, [Startup.cs](../NuciCraft.API/Startup.cs) creates absent paths and enumerates each repository.

### Data Exchange

The process sends serialisable data objects to NuciDAL and receives materialised data objects or enumerables. NuciDAL exchanges JSON bytes with configured files. Exact serialiser settings and write strategy are package-owned.

### Failure And Resilience

- Path, permission, deserialisation, or eager-load failures prevent startup.
- Request-time read or write failures are logged by services and rethrown.
- No retry, backup, repair, transaction, or alternate store exists.
- No timeout or cancellation exists for filesystem operations.

### Security

No local encryption at rest or field protection exists. File permissions and storage encryption are deployment responsibilities.

### Lifecycle And Substitution

Repositories live for the process lifetime. Services depend on `IFileRepository<T>`, which is the substitution boundary. Replacing NuciDAL requires preserving the used Get, GetAll, Add, Update, Remove, and SaveChanges semantics or revising every service.

## Universal Name Generator API

### Purpose

Generate names from compiled schemas for supported Minecraft mobs.

### Initialisation

[ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs) constructs singleton `NuciApiClient` from `UniversalNameGeneratorSettings.BaseUrl`. `MobService` receives that client and settings.

### Data Sent

- HTTP method GET.
- Relative endpoint `Names`.
- `schema` string selected from mob type.
- `count` integer.
- Bearer token from generator-specific ApiKey.

No Player, Home, RTP, Zone, IP address, or inbound client credential is sent.

### Data Received

The expected runtime type is `GenerateNamesResponse`, extending `NuciApiSuccessResponse`, with a top-level Names collection.

### Authentication

Outbound bearer authentication is explicit through `NuciApiRequestAuthorisationInfo`. It is separate from the inbound NuciCraft API key.

### Failure, Retry, Timeout, And Cache

- Whitespace BaseUrl or ApiKey fails locally.
- Unsuccessful API response becomes `InvalidOperationException` with code and message.
- Unexpected response type or invalid Names fails locally.
- Client exceptions propagate after logging.
- No application retry, timeout override, circuit breaker, rate-limit handling, fallback, or cache exists.
- Effective HTTP timeout and connection management belong to `NuciApiClient`.

### Lifecycle And Substitution

The client and MobService are singletons. `INuciApiClient` is the substitution boundary and is replaced by a Moq object in the integration host.

## Minecraft Java Server

### Purpose

Supply the live online-player count returned by `GET /Server`.

### Initialisation

No persistent connection is initialised. Each `ServerStatusService.GetOnlinePlayersCount` call creates MineStat with configured Hostname, Java port, timeout five, and legacy protocol.

### Data Sent And Received

MineStat sends the Java legacy server-list status handshake. The dedicated integration test confirms initial bytes `0xfe 0x01`. The service consumes `ServerUp` and `CurrentPlayersInt` from MineStat's parsed response.

### Authentication

No Minecraft server credential is transmitted. Network access to the status port is the external boundary.

### Failure, Retry, Timeout, And Cache

- Five-second timeout is compiled into the service.
- I/O and socket failures return zero.
- Unavailable status returns zero.
- Invalid settings, negative count, and some parse failures propagate.
- No retry, cache, backoff, health state, or Bedrock fallback exists.

### Lifecycle And Substitution

Each query has independent MineStat state. `IServerStatusService` is the controller substitution boundary and is mocked in ordinary API integration tests. The real service is verified separately through loopback TCP.

## NuciAPI Package Family

### Purpose

Supply shared request and response abstractions, controller processing, outbound HTTP client, scanner protection, exception handling, and request logging.

### Initialisation

- Controllers derive from `NuciApiController`.
- Startup registers and activates package middleware.
- DI constructs `NuciApiClient`.

### Owned Conduct

The package family owns details not visible locally:
- `ProcessRequest` authorisation and request validation order.
- Success and error envelope serialisation.
- Complete exception-to-status mapping.
- Scanner detection rules and response.
- Request log content.
- HTTP client handler and timeout policy.

### Upgrade Consequences

Package upgrades can change public HTTP compatibility, error semantics, authentication handling, or outbound serialisation. Controller and integration suites are the principal local regression boundary.

No local abstraction isolates controllers from `NuciApiController` or response types. Outbound calls are isolated through `INuciApiClient`.

## NuciDAL

### Purpose

Provide repository interfaces, base entity identity, and JSON repository implementation.

### Encapsulation

Services depend on `IFileRepository<T>`, while only composition references `JsonRepository<T>` directly. Data objects derive indirectly from NuciDAL `EntityBase`.

### Unknown Package Semantics

Local source does not establish:
- Duplicate identifier policy.
- Atomic file replacement or in-place writes.
- File lock and concurrent writer policy.
- Whether `GetAll` is a snapshot, live collection, or disk read.
- External file modification detection.
- Serialiser options.

These unknowns are material to replacement, upgrades, scale-out, and recovery.

## NuciLog

### Purpose

Provide structured operation and request diagnostics, optional file output, and operation status types.

### Initialisation

Package settings bind through `AddNuciLoggerSettings`. `ILogger` maps to scoped `NuciLogger`; singleton services consume it.

### Data

Services can log identifiers, usernames, UUIDs, last IP addresses, worlds, biomes, coordinates, counts, timestamps, and exception details. They do not intentionally log passwords in local calls.

### Failure And Lifecycle

Sink failure, buffering, rotation, flush, and retention semantics are external. File output is enabled by committed defaults. Operators must protect log storage.

## NuciSecurity.HMAC

The repository applies `HmacOrder` and `HmacIgnore` to transport and nested types. HMAC calculation and verification are external. Ordering changes are therefore contract changes even though no local cryptographic method is visible.

The duplicated order value on Player response Gender and Settings is an unresolved compatibility question rather than a locally proven defect.

## NuciText Utilities

`INuciTextNormaliser` and `INuciTextObfuscator` are registered as singletons. No production class in this repository injects them. Their registration may support package resolution, anticipated use, or historical compatibility; intent is not recorded.

Removing them requires registration-test and runtime dependency verification.

## Remote Release Helper

[release.sh](../release.sh) selects .NET version 10.0, downloads a script from a mutable `master` URL, and pipes it to Bash with forwarded arguments.

Consequences are:
- Release requires outbound HTTPS and `wget` plus Bash.
- Executed content is not pinned by commit or checksum.
- Packaging, tagging, publication, and credentials are controlled by remote content unavailable in this repository.
- Maintainers must inspect the downloaded script before execution.

This integration is not used by runtime requests.

## Web Map Non-Integration

`WebMapSettings.BaseUrl` may suggest an integration, but the API performs no web-map request. `ZoneService` only constructs a URL for clients. Availability, authentication, and response formats are outside this process.
