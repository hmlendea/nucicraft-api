# External Service Components

| Metadata | Value |
|----------|-------|
| Purpose | Document mob-name HTTP generation and Minecraft Java server-status querying as outbound network components. |
| Scope | `MobService`, `ServerStatusService`, outbound contracts, client construction, settings, timeouts, fallbacks, and tests. |
| Primary sources | [MobService.cs](../../NuciCraft.API/Service/MobService.cs), [ServerStatusService.cs](../../NuciCraft.API/Service/ServerStatusService.cs), [GenerateNamesRequest.cs](../../NuciCraft.API/Service/GenerateNamesRequest.cs), and [GenerateNamesResponse.cs](../../NuciCraft.API/Service/GenerateNamesResponse.cs) |
| Related documents | [External behaviour](../behaviour/rtp-and-external-capabilities.md), [external request flow](../flows/external-requests.md), [integrations](../integrations.md), and [error handling](../error-handling.md) |

## Purpose

Two components supply request-time information unavailable from local stores:
- `MobService` translates a supported Minecraft mob type into a Universal Name Generator schema and retrieves names over HTTP.
- `ServerStatusService` asks the configured Minecraft Java server for its current online-player count over the legacy server-list protocol.

## Scope

These components own outbound request construction, local configuration validation, response interpretation, and component-specific failure policy.

They do not own the remote systems, DNS, network routing, TLS policy, HTTP handler configuration, Minecraft status implementation, retry infrastructure, caching, rate limiting, or circuit breaking.

## Position In The Architecture

`MobService` is an application service invoked by `MobsController` and backed by `INuciApiClient`. `ServerStatusService` is a narrow integration service invoked by `ServersController`, which combines its result with bound server metadata.

Neither component persists state.

## Dependencies

### Mob Name Generation

- `INuciApiClient`, implemented by singleton `NuciApiClient`.
- `UniversalNameGeneratorSettings`.
- `MobType` registry and `Random.Shared`.
- Nuci API request authorisation and response types.
- NuciLog `ILogger`.

### Server Status

- `ServerSettings`.
- MineStat 3.1.2 and `SlpProtocol.Legacy`.
- TCP networking and I/O exception types.

## Dependants

- `MobsController` and `ServersController`.
- API clients requiring generated names or live server population.
- Integration-host substitution logic for deterministic HTTP tests.
- Dedicated service tests for schema mapping and legacy protocol interpretation.

## Internal Structure

### Mob Service

| Type | Role |
|------|------|
| [IMobService.cs](../../NuciCraft.API/Service/IMobService.cs) | Synchronous name-generation contract. |
| [MobService.cs](../../NuciCraft.API/Service/MobService.cs) | Validation, schema selection, outbound invocation, and response checks. |
| [MobType.cs](../../NuciCraft.API/Service/Models/MobType.cs) | Case-insensitive external-name registry and Unsupported sentinel. |
| [GenerateNamesRequest.cs](../../NuciCraft.API/Service/GenerateNamesRequest.cs) | Outbound schema and count query. |
| [GenerateNamesResponse.cs](../../NuciCraft.API/Service/GenerateNamesResponse.cs) | Expected top-level successful response containing Names. |

### Server Status Service

| Type | Role |
|------|------|
| [IServerStatusService.cs](../../NuciCraft.API/Service/IServerStatusService.cs) | Integer count contract. |
| [ServerStatusService.cs](../../NuciCraft.API/Service/ServerStatusService.cs) | Settings validation, MineStat query, fallback, and count validation. |
| [GetServerResponse.cs](../../NuciCraft.API/Responses/GetServerResponse.cs) | Combines static metadata and live count. |

## Behaviour

### Mob Type Parsing

`MobType.FromString` compares case-insensitively with registered external names. Null, whitespace, or unknown values produce `Unsupported`; `MobService.GetMobType` then raises `NotImplementedException`.

### Schema Selection

| Mob | Generator Schema |
|-----|------------------|
| `wandering_trader` | `romanian-persons-male` |
| `ender_dragon` | `fantasy-dragons` |
| `cow` | `romanian-animals-cows` |
| `pig` | `animals-pigs` |
| `evoker`, `illusioner`, `pillager`, `vindicator` | `pinched-zaganian-persons-male` |
| `villager` | Randomly `romanian-persons-male` or `romanian-persons-female` |

Villager uses `Random.Shared.Next(2)`, giving two local branches without a deterministic request seed.

### Outbound Name Request

`GetRandomMobName`:
1. Requires a request and non-whitespace MobType.
2. Requires non-whitespace BaseUrl and ApiKey settings.
3. Logs MobType and Count.
4. Parses type and selects a schema.
5. Creates `GenerateNamesRequest` with schema and count.
6. Creates `NuciApiRequestAuthorisationInfo` with BearerToken from settings.
7. Calls `SendRequestAsync<GenerateNamesRequest, GenerateNamesResponse>` using GET and endpoint `Names`.
8. Blocks synchronously with `GetAwaiter().GetResult()`.
9. Requires a successful response of the exact expected type.
10. Materialises Names and rejects null, vacant, or whitespace elements.

Remote error Code and Message become part of a local `InvalidOperationException` message. The service returns the complete generated collection.

### Java Status Query

`GetOnlinePlayersCount`:
1. Requires a non-whitespace Hostname.
2. Requires JavaEditionPort from 1 through 65535.
3. Constructs MineStat with hostname, port, five-second timeout, and legacy protocol.
4. Returns zero when construction raises `IOException` or `SocketException`.
5. Returns zero when MineStat reports `ServerUp` false.
6. Rejects a negative parsed player count.
7. Returns the parsed count otherwise.

The dedicated integration test creates a loopback `TcpListener`, verifies MineStat transmits bytes `0xfe 0x01`, and responds with the encoded legacy status payload. Tests establish live re-query, timeout zero, closed-connection zero, negative-count failure, and non-numeric-count failure.

## Inputs And Outputs

### Name Generation

Input is one supported mob external name and count. Output is an enumerable of names from the remote response. The repository transmits schema, count, and bearer credential to the configured API. It does not transmit Player data.

### Server Status

Input is process configuration rather than request values. The outbound query transmits the legacy status handshake to the configured host and Java port. Output is a non-negative integer unless parsing or validation fails.

`GetServerResponse` also exposes configured Name, Hostname, JavaEditionPort, and BedrockEditionPort.

## State

- `NuciApiClient`, `MobService`, and `ServerStatusService` are singletons.
- Each mob request allocates an outbound request and response objects.
- Each server request creates a new MineStat instance.
- No local cache, persisted state, shared socket, or prior result exists.
- `Random.Shared` is process-global framework state used only for Villager schema selection.

## Error Handling

### Mob Generation

- Invalid local settings: `ArgumentException` before service logging begins.
- Unsupported mob: `NotImplementedException`, translated to `501` in integration tests.
- Unsuccessful remote response: `InvalidOperationException` including remote code and message.
- Unexpected response type: `InvalidOperationException`.
- Null, vacant, or partially whitespace names: `InvalidOperationException`.
- Client transport and deserialisation exceptions: logged and rethrown.

There is no retry, fallback schema after request failure, stale cache, or local name generation.

### Server Status

- Invalid host or port: argument exceptions propagate.
- `IOException`, `SocketException`, server unavailable, timeout, or absent reply: zero.
- Negative count: `InvalidOperationException`.
- Non-numeric count can propagate `FormatException` from MineStat.

The service does not log directly. Request middleware provides outer request logging.

## Configuration

| Setting | Consumer | Effect |
|---------|----------|--------|
| `universalNameGeneratorSettings.baseUrl` | `NuciApiClient` and Mob validation | Remote base address. |
| `universalNameGeneratorSettings.apiKey` | `MobService` | Outbound bearer token. |
| `serverSettings.name` | `ServersController` | Advertised server name. |
| `serverSettings.hostname` | Controller and status service | Advertised host and TCP query destination. |
| `serverSettings.javaEditionPort` | Controller and status service | Advertised port and TCP query destination. |
| `serverSettings.bedrockEditionPort` | Controller only | Advertised metadata; not queried. |

No configurable status timeout, protocol, endpoint path, HTTP timeout, retry count, or schema mapping exists.

## Extension And Modification Guidance

### Adding A Mob

Add the value to `MobType`, expose its static accessor, revise schema selection, add model equality and parsing tests, add service schema tests, and add an HTTP integration test. If the schema is configurable, revise settings validation and documentation.

### Changing The Name API

Review response inheritance, top-level Names placement, explicit bearer-authorisation overload, endpoint path, failure propagation, and synchronous wait. A client package upgrade can alter these contracts.

### Changing Server Status

Preserve the distinction between recoverable unavailability and invalid data deliberately. Adding Bedrock or caching requires a new observable contract because zero currently conflates unavailable and vacant servers.

## Relevant Processes

- [External request flow](../flows/external-requests.md)
- [External capability behaviour](../behaviour/rtp-and-external-capabilities.md)
