# RTP Location Component

| Metadata | Value |
|----------|-------|
| Purpose | Document random-teleport location persistence, spatial separation, filtering, and random selection. |
| Scope | RTP controller, service, contracts, model, data object, mapping, settings, and tests. |
| Primary sources | [RtpLocationService.cs](../../NuciCraft.API/Service/RtpLocationService.cs), [RtpLocationsController.cs](../../NuciCraft.API/Controllers/RtpLocationsController.cs), and [RtpLocationServiceTests.cs](../../NuciCraft.API.UnitTests/Service/RtpLocationServiceTests.cs) |
| Related documents | [RTP behaviour](../behaviour/rtp-and-external-capabilities.md), [file-backed flow](../flows/file-backed-request.md), [state and persistence](../state-and-persistence.md), and [invariants](../invariants.md) |

## Purpose

The RTP component accumulates candidate random-teleport coordinates while enforcing configured horizontal separation. It later returns one uniformly selected record after optional exact World and Biome filters.

## Scope

The component owns:
- RTP addition and generated metadata.
- General and same-biome proximity checks.
- Optional selection filters.
- Random element selection through NuciExtensions.
- RTP-specific logging and persistence.

It does not own:
- Teleport execution.
- Coordinate safety, chunk loading, biome verification, or Minecraft world verification.
- Y-axis distance.
- Deletion, patching, weighting, reservation, or usage tracking.
- Deterministic random seeds.

## Position In The Architecture

`RtpLocationsController` exposes one command and one query. `IRtpLocationService` is the controller-facing contract. `RtpLocationService` owns validation and selection and uses one singleton file repository.

## Dependencies

- `IFileRepository<RtpLocationEntity>`.
- `RtpLocationSettings`.
- NuciLog `ILogger`.
- NuciExtensions `GetRandomElement`.
- RTP and coordinate mappings.

## Dependants

- API clients supplying safe candidate coordinates and consuming random destinations.
- Controller, service, mapping, and integration tests.
- Deployment configuration defining both thresholds.

## Internal Structure

| Concern | Location |
|---------|----------|
| Contract | [IRtpLocationService.cs](../../NuciCraft.API/Service/IRtpLocationService.cs) |
| Implementation | [RtpLocationService.cs](../../NuciCraft.API/Service/RtpLocationService.cs) |
| HTTP input | [AddRtpLocationRequest.cs](../../NuciCraft.API/Requests/AddRtpLocationRequest.cs) and [GetRtpLocationRequest.cs](../../NuciCraft.API/Requests/GetRtpLocationRequest.cs) |
| HTTP output | [GetRtpLocationResponse.cs](../../NuciCraft.API/Responses/GetRtpLocationResponse.cs) |
| Service model | [RtpLocation.cs](../../NuciCraft.API/Service/Models/RtpLocation.cs) |
| Persistence | [RtpLocationEntity.cs](../../NuciCraft.API/DataAccess/DataObjects/RtpLocationEntity.cs) |
| Mapping | [RtpLocationMappingExtensions.cs](../../NuciCraft.API/Service/Mapping/RtpLocationMappingExtensions.cs) |
| Settings | [RtpLocationSettings.cs](../../NuciCraft.API/Configuration/RtpLocationSettings.cs) |

## Behaviour

### Addition

The HTTP request requires Biome, World, X, Y, and Z. The service logs these values, then evaluates two constraints in order:
1. Compare with every persisted location in the identical ordinal World using `MinimumLocationDistance`.
2. Compare with persisted records whose Biome equals the candidate Biome, then require the identical ordinal World using `MinimumBiomeLocationDistance`.

Each comparison computes:

$$
d^2 = (x_1 - x_2)^2 + (z_1 - z_2)^2
$$

The candidate is too close when:

$$
d^2 \leq r^2
$$

Consequently, acceptance requires distance strictly greater than the configured threshold. Y is stored but absent from proximity logic.

After both checks, the service generates a random GUID identifier and current UTC CreatedDT, stores Biome plus nested coordinates, adds, and saves.

### Random Selection

The service obtains all records. A non-whitespace World filters through case-sensitive equality. A non-whitespace Biome applies another case-sensitive filter. If no candidates remain, the service raises `KeyNotFoundException`.

`GetRandomElement` selects one candidate. The selected data object maps to a service model and then to a response containing Id, Biome, and Coordinates.

### Complexity

Addition performs one complete scan for the general rule and another filtered scan for the biome rule. Selection filters an enumerable and checks it before random selection. There is no spatial index, per-world partition, or cache in local code.

## Inputs And Outputs

Addition input is one Biome, World, and floating-point X, Y, Z tuple. The service does not locally validate finite values; ordinary JSON serialisation normally rejects non-standard numeric literals, but direct service callers can bypass transport behaviour.

The query accepts optional exact filters. Output includes the generated identifier and complete Coordinates model, whose Pitch and Yaw use data-object defaults because addition sets only World, X, Y, and Z.

## State

- Records are append-only through the current API.
- The service and repository are singletons.
- No service lock protects the scan followed by add and save.
- Concurrent additions can both pass checks versus the same prior snapshot.
- Multiple processes have no shared coordination.

## Error Handling

- General proximity violation: `ArgumentException`.
- Same-biome proximity violation: a distinct `ArgumentException`.
- No random candidate: `KeyNotFoundException`.
- Repository failures: logged and rethrown.
- A direct null request can fail while constructing log context before the method enters its `try` block.

There is no retry or alternative destination when selection is vacant.

## Configuration

- `DataStoreSettings.RtpLocationsStorePath`
- `RtpLocationSettings.MinimumLocationDistance`, default 200 in committed settings
- `RtpLocationSettings.MinimumBiomeLocationDistance`, default 500 in committed settings
- `SecuritySettings.ApiKey`
- NuciLog settings

Settings classes do not reject zero or negative thresholds. Squaring means a negative threshold behaves like its absolute magnitude.

## Extension And Modification Guidance

When changing distance rules:
- Preserve world partition semantics deliberately.
- State whether the threshold remains inclusive.
- Decide whether Y enters the metric.
- Add threshold-edge, different-world, same-biome, different-biome, and concurrent-add tests.
- Assess existing stored points, which are not revalidated on startup.

When adding deletion or weighting, revise the service interface, transport contracts, logging operations, persistence tests, and random-selection semantics.

## Relevant Processes

- [RTP capability behaviour](../behaviour/rtp-and-external-capabilities.md)
- [Generic file-backed flow](../flows/file-backed-request.md)
