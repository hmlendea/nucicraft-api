# RTP And External Capabilities

| Metadata | Value |
|----------|-------|
| Purpose | Describe RTP addition and selection, Universal Name Generator requests, and Minecraft Java status queries as runtime capabilities. |
| Scope | Initiation, validation, algorithms, external exchanges, outputs, retries, fallback, cleanup, and postconditions. |
| Primary sources | [RtpLocationService.cs](../../NuciCraft.API/Service/RtpLocationService.cs), [MobService.cs](../../NuciCraft.API/Service/MobService.cs), and [ServerStatusService.cs](../../NuciCraft.API/Service/ServerStatusService.cs) |
| Related documents | [RTP component](../components/rtp-locations.md), [external services](../components/external-services.md), [external request flow](../flows/external-requests.md), and [invariants](../invariants.md) |

## RTP Location Addition

### Purpose And Entry Point

`POST /RtpLocations` records a candidate that future random-teleport queries may return.

### Preconditions And Inputs

The body requires Biome, World, X, Y, and Z at the transport boundary. The service presumes a non-null request and does not independently reject whitespace or non-finite coordinates.

### Execution Sequence

1. Authorise the request.
2. Construct Biome, World, X, Y, and Z log context.
3. Log Started.
4. Enumerate every location for the general World-distance check.
5. Reject when any identical-World location is at or within the configured horizontal threshold.
6. Enumerate same-Biome locations for the second World-distance check.
7. Reject when any identical-World and identical-Biome location is at or within the biome threshold.
8. Generate GUID Id and UTC CreatedDT.
9. Construct nested Coordinates with World, X, Y, and Z.
10. Add and save.
11. Log Success and return command success.

### Decisions

- World and Biome comparisons are case-sensitive.
- Different worlds never conflict.
- Y does not affect distance.
- Equality with either threshold rejects the candidate.
- The general check executes first, so a candidate violating both reports the general error.

### Failure And Postconditions

On proximity failure, no record is added. Repository errors are logged and rethrown. No retry or conflict recheck occurs after add. On success, one append-style record exists.

Concurrent services or processes can both validate versus a prior state and then introduce a conflicting pair.

## Random RTP Selection

### Purpose And Entry Point

`GET /RtpLocations/random` returns one persisted candidate, optionally constrained by World and Biome.

### Execution Sequence

1. Bind optional query values and authorise.
2. Log filter context.
3. Obtain all repository records.
4. Apply World filter only when non-whitespace.
5. Apply Biome filter only when non-whitespace.
6. Test whether any candidate exists.
7. Select one through `GetRandomElement`.
8. Map the record and add selected coordinates to success log context.
9. Return Id, Biome, and Coordinates.

No match raises `KeyNotFoundException`. There is no fallback that relaxes a filter and no reservation preventing two callers from receiving the identical location.

## Mob-Name Generation

### Purpose And Entry Point

`GET /Mobs/{mobType}/random-name?count={n}` obtains generated names from the configured Universal Name Generator.

### Preconditions

- Valid inbound API key.
- Count omitted or from 1 through 100000.
- Non-whitespace configured generator BaseUrl and ApiKey.
- Supported case-insensitive MobType.

### Execution Sequence

1. Controller creates `GetMobNameRequest`, retaining default count one unless query count exists.
2. `ProcessRequest` performs package-owned checks.
3. `MobService` validates request, MobType text, and settings.
4. Log Started with type and count.
5. Convert MobType text to registry value.
6. Select the compiled generator schema.
7. For Villager, randomly select male or female Romanian-person schema.
8. Construct outbound schema and count request.
9. Construct bearer authorisation from the generator API key.
10. Invoke asynchronous client GET against `Names` and block synchronously.
11. Require successful response and exact expected response type.
12. Materialise Names and reject null, vacant, or whitespace records.
13. Log Success and return every generated name.

### External Data Exchange

Transmitted data consists of schema, count, and outbound bearer credential. Received data is a Nuci API response expected to contain top-level Names. Player or server state is not transmitted.

### Failure Paths

- Unsupported type: `NotImplementedException`, observed as HTTP `501`.
- Invalid local settings: argument exception before operation logging.
- Remote unsuccessful response: `InvalidOperationException` containing remote code and message.
- Unexpected response type or invalid Names: `InvalidOperationException`.
- Transport, timeout, deserialisation, or client exception: logged and rethrown.

There is no local timeout override, retry, schema fallback after transport failure, cache, or partial result. No cleanup beyond normal HTTP client internals is locally controlled.

## Server Information And Live Count

### Purpose And Entry Point

`GET /Server` combines configured Minecraft server identity with a request-time Java online-player count.

### Preconditions

- Valid inbound API key.
- Non-whitespace configured Hostname.
- JavaEditionPort from 1 through 65535.

Name and Bedrock port have no local service validation.

### Execution Sequence

1. Controller passes an empty `GetServerRequest` through `ProcessRequest`.
2. The response delegate reads Name, Hostname, JavaEditionPort, and BedrockEditionPort from singleton settings.
3. `ServerStatusService` validates Hostname and Java port.
4. Construct MineStat with legacy protocol and a five-second timeout.
5. MineStat opens TCP communication and parses the legacy status response.
6. If construction raises I/O or socket failure, return zero.
7. If `ServerUp` is false, return zero.
8. If current count is negative, raise `InvalidOperationException`.
9. Return count and construct `GetServerResponse`.

### Branches And Observable Results

| Condition | Result |
|-----------|--------|
| Server reports a non-negative count | Return that count. |
| Connection refused or related socket/I/O failure | Return zero. |
| Timeout or no useful status | MineStat path resolves to zero in integration tests. |
| `ServerUp` false | Return zero. |
| Negative count | Fail request. |
| Non-numeric count | Parsing failure propagates. |
| Invalid host or port | Argument failure propagates. |

Zero is deliberately ambiguous between unavailable server and available server without players. The Bedrock port is not queried.

## Concurrency And Scheduling

- All three capabilities execute only in response to HTTP requests.
- There are no timers, workers, queues, or scheduled refreshes.
- RTP repository scans and writes can interleave between requests.
- Mob requests share the singleton HTTP client but have independent request objects.
- Server status creates an independent MineStat instance per call.
- No service accepts a cancellation token.

## Retries And Cleanup

No capability performs an application-level retry. RTP failures leave the store unchanged when they occur before repository add. Name and status queries do not persist partial output. Resource disposal inside NuciApiClient and MineStat is external library conduct.

## Capability Invariants

Threshold, schema, response, and fallback rules are consolidated in [invariants.md](../invariants.md).
