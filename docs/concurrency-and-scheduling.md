# Concurrency And Scheduling

| Metadata | Value |
|----------|-------|
| Purpose | Explain request concurrency, singleton shared state, synchronisation, race risks, blocking operations, ordering, cancellation, and the absence of schedulers. |
| Scope | Production code and framework assumptions visible from the repository. |
| Primary sources | [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs), [HomeService.cs](../NuciCraft.API/Service/HomeService.cs), [MobService.cs](../NuciCraft.API/Service/MobService.cs), and all other services under [Service](../NuciCraft.API/Service/) |
| Related documents | [Architecture](architecture.md), [state and persistence](state-and-persistence.md), [design decisions](design-decisions.md), and [invariants](invariants.md) |

## Execution Model

ASP.NET Core can process multiple HTTP requests concurrently. The repository does not configure a restricted request executor or global application lock. Controller actions invoke synchronous service interfaces on request threads.

No background worker, hosted service, timer, scheduler, queue consumer, producer loop, or periodic refresh is implemented.

## Shared Lifetimes

| Dependency | Lifetime | Concurrent Access Consequence |
|------------|----------|-------------------------------|
| Application services | Singleton | Every request for a domain shares one service instance. |
| File repositories | Singleton | Reads and writes for a record type share one adapter instance. |
| Settings | Singleton | Effectively read-only after startup but publicly mutable objects. |
| NuciApiClient | Singleton | Concurrent name requests share client infrastructure. |
| ServerStatusService | Singleton | Method creates independent MineStat state per call. |
| NuciText utilities | Singleton | No local production consumer. |
| ILogger | Scoped registration | Singleton service construction captures a resolved instance beyond ordinary request scope expectations. |
| Controllers | Framework activated | Hold only dependencies and immutable authorisation descriptor. |

Singleton services contain no mutable request fields except `HomeService.persistenceLock`. Their local variables are invocation-specific.

## Home Synchronisation

Every public Home operation delegates through a private `Execute` method that executes:

```csharp
lock (persistenceLock)
{
    // Started log, operation, success or failure log
}
```

The lock covers:
- Owner validation through PlayerService.
- Repository reads and collection materialisation.
- Name extraction and duplicate scans.
- Add, update, remove, and SaveChanges.
- Service logging.

### Guarantees

For requests entering the identical singleton HomeService instance:
- No two Home operations execute their critical sections concurrently.
- A name uniqueness check and its subsequent save cannot interleave with another Home operation.
- Returned collection arrays are materialised before lock release.

### Limits

The lock does not protect:
- Another API process.
- Direct Home repository consumers.
- External file modifications.
- Player modifications executed independently.
- Package operations that continue asynchronously after returning, if any.

[HomeServiceTests.cs](../NuciCraft.API.UnitTests/Service/HomeServiceTests.cs) includes concurrent duplicate-add verification for the service instance.

## Unsynchronised Read-Check-Write Paths

### RTP Addition

RTP addition scans general locations, scans same-Biome locations, then adds and saves without a lock. Two requests can both validate against the identical prior state and add mutually conflicting records.

### Player Registration

Registration generates identifiers and writes without scanning alternate selectors or acquiring a lock. Concurrent or sequential duplicate Username registrations are permitted by local logic.

### Metadata Addition And Patch

Country, World, ZoneType, Player, RTP, and Zone services rely on repository concurrency semantics. Concurrent updates can overwrite one another or contend at file-write time depending on NuciDAL behaviour.

### Zone References

Zone add or patch validates World and ZoneType in separate reads before saving the Zone. Metadata can change between validation and save. Current interfaces cannot delete those references, which reduces but does not eliminate external-file or multi-process races.

## Repository Concurrency

The local project neither wraps NuciDAL with locks nor documents NuciDAL's internal locking. Therefore these properties are indeterminate from repository evidence:
- Whether simultaneous SaveChanges calls are serialised.
- Whether writes use atomic file replacement.
- Whether GetAll enumeration is stable during mutation.
- Whether each repository retains an in-memory collection.
- Whether separate repository instances coordinate on one path.

Do not assert thread safety from singleton registration alone.

## Synchronous And Asynchronous Conduct

### Filesystem

Repository methods and SaveChanges are invoked synchronously. Request threads remain occupied until they return.

### Mob Names

`INuciApiClient.SendRequestAsync` is asynchronous, but `MobService` immediately calls `GetAwaiter().GetResult()`. The service and controller interface remain synchronous. This blocks the invoking request thread while preserving the original exception rather than wrapping it in `AggregateException`.

### Server Status

MineStat construction and query are synchronous with a five-second timeout. Concurrent `/Server` requests create independent network operations.

### Tests

Integration tests use asynchronous `HttpClient`, and the server-status fixture uses Tasks to operate a loopback listener. These do not introduce production background tasks.

## Ordering Guarantees

Confirmed:
- One Home operation completes its locked section before another starts that section.
- Within one service method, validation, mutation, and SaveChanges occur in source order.
- Middleware executes in registration order for an inbound request.

Not guaranteed locally:
- Ordering among requests for any non-Home service.
- Ordering of repository enumeration beyond adapter behaviour.
- Ordering of collection response items.
- Fair acquisition of the Home monitor.
- Ordering among concurrent outbound requests.
- Durable write ordering across distinct stores.

## Randomness

- Identifiers use `Guid.NewGuid` and can be generated concurrently.
- Villager schema uses process-global `Random.Shared`, designed by the framework for concurrent access.
- RTP selection delegates randomness to NuciExtensions; implementation and concurrent selection properties are external.

No deterministic seed is configurable.

## Cancellation

No service method accepts `CancellationToken`. Request cancellation therefore does not propagate through local service contracts to:
- Repository I/O.
- Mob-name HTTP requests.
- MineStat TCP queries.
- Collection scans.

Framework and external packages may independently observe cancellation, but local source does not pass it.

## Scheduling

There is no application scheduler. All work is demand-driven:
- Store preparation occurs once during startup.
- Persistence occurs within mutation requests.
- Server status is queried within each `/Server` request.
- Names are generated within each mob request.
- Map links are derived within each Zone read.

There is no deferred cleanup, retry queue, reconciliation, compaction, backup, health probe, or cache refresh.

## Race-Risk Matrix

| Scenario | Protection | Residual Risk |
|----------|------------|---------------|
| Two duplicate Home additions in one process | Home lock | Protected through HomeService. |
| Duplicate Home additions in two processes | None | Both can pass and save. |
| Two close RTP additions | None | Both can pass prior-state scans. |
| Two patches to identical record | Repository-dependent | Last or failed writer semantics unknown. |
| Read during non-Home save | Repository-dependent | Snapshot and file visibility unknown. |
| Zone validation versus metadata change | No transaction | Time-of-check to time-of-save divergence. |
| Configuration mutation at runtime | No monitor, object remains mutable | Accidental mutation can affect later calls inconsistently. |
| Concurrent release script use | Outside application | Remote automation semantics unknown. |

## Modification Guidance

- Add synchronisation at the ownership boundary that covers both validation and save.
- Do not add mutable request state to singleton fields.
- Prefer an atomic persistence constraint over an application lock when supporting multiple processes.
- Convert full call paths to async when introducing asynchronous service methods; include cancellation propagation.
- Add concurrency tests for any read-check-write invariant.
- Define ordering explicitly if clients begin depending on collection order.
