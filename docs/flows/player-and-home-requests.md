# Player And Home Request Flows

| Metadata | Value |
|----------|-------|
| Purpose | Preserve method-level Player and Home call graphs, transformations, lock boundaries, and state changes. |
| Scope | Player registration and patch, Home addition and patch, plus selector reads. |
| Primary sources | [PlayerService.cs](../../NuciCraft.API/Service/PlayerService.cs), [HomeService.cs](../../NuciCraft.API/Service/HomeService.cs), [PlayersController.cs](../../NuciCraft.API/Controllers/PlayersController.cs), and [HomesController.cs](../../NuciCraft.API/Controllers/HomesController.cs) |
| Related documents | [Player and Home behaviour](../behaviour/player-and-home-capabilities.md), [Player and Home components](../components/player-and-homes.md), and [invariants](../invariants.md) |

## Player Registration Trace

```mermaid
sequenceDiagram
    actor Client
    participant Controller as PlayersController.Register
    participant Processor as ProcessRequest
    participant Service as PlayerService.Register
    participant UUID as GetOfflineUuid
    participant Time as Timestamp Parsing
    participant Mapping as PlayerMappingExtensions
    participant Repo as Player Repository
    participant Logger

    Client->>Controller: POST /Players with RegisterPlayerRequest
    Controller->>Processor: request, Register delegate, API key
    Processor->>Service: Register(request)
    Service->>Logger: RegisterPlayer Started
    Service->>Service: Guid.NewGuid()
    Service->>UUID: MD5 OfflinePlayer:username and UUID bits
    UUID-->>Service: OfflineUUID
    Service->>Time: Parse supplied timestamps or generate CreatedDT
    Time-->>Service: Typed DateTimeOffset values
    Service->>Service: new PlayerSettings defaults
    Service->>Mapping: player.ToDataObject()
    Mapping-->>Service: PlayerDataObject and string timestamps
    Service->>Repo: Add(dataObject)
    Service->>Repo: SaveChanges()
    Service->>Logger: RegisterPlayer Success
    Service-->>Processor: Completion
    Processor-->>Client: Success response
```

Failure after Started enters the service catch, records Failure, and rethrows. Request-null rejection occurs before log context construction; HTTP binding does not pass null for a valid body.

## Player Patch Trace

1. One of four controller actions receives a route selector and body.
2. Controller writes route selector into `PatchPlayerRequest`.
3. `ProcessPatchRequest` passes the request to `ProcessRequest`.
4. `PlayerService.Update` logs selector context.
5. `ValidatePatchSelectors` counts non-whitespace selector values and requires one.
6. `FindPlayerDataObject` calls repository `GetAll`, OR-matches selectors, and takes first match.
7. `ApplyPatchValues` examines every mutable field in source order.
8. A non-null Settings object invokes `PlayerSettingsDataObject.MergeWith`.
9. Boolean setting setters have previously marked which JSON properties were present.
10. Service generates string UpdatedDT.
11. Repository Update and SaveChanges execute.
12. Service records Success; controller returns no content record.

If a patch stores an invalid timestamp string, this operation can succeed. A later Player mapping parses that value and can fail.

## Player Retrieval Trace

The four selector controllers converge on `ProcessGetRequest`:

```mermaid
flowchart LR
    Id[GET by identifier] --> Request[GetPlayerRequest]
    Username[GET by username] --> Request
    Offline[GET by offline UUID] --> Request
    Online[GET by online UUID] --> Request
    Request --> Find[PlayerService.FindPlayerDataObject]
    Find --> First[First matching PlayerDataObject]
    First --> Map[ToDomainModel]
    Map --> Response[GetPlayerResponse]
    Response --> Envelope[NuciApiContentResponse]
```

`ToDomainModel` parses all timestamps, maps coordinates, converts Gender and Settings, and exposes Password. `GetPlayerResponse` applies only DisplayName fallback.

## Home Addition Trace

```mermaid
sequenceDiagram
    actor Client
    participant Controller as HomesController.Add
    participant Processor as ProcessRequest
    participant Home as HomeService.Add
    participant Lock as persistenceLock
    participant Player as IPlayerService.Get
    participant Repo as Home Repository
    participant Logger

    Client->>Controller: POST /Homes
    Controller->>Processor: AddHomeRequest and API key
    Processor->>Home: Add(request)
    Home->>Lock: Enter through Execute
    Home->>Logger: AddHome Started
    Home->>Home: Validate request, player text, location
    Home->>Player: Get(identifier selector)
    Player-->>Home: Canonical Player model
    Home->>Home: Generate id and CreatedDT
    Home->>Home: Map, extract names, validate at least one
    Home->>Repo: GetAll() for duplicate scan
    Repo-->>Home: Existing Homes
    alt Duplicate overlapping name for Player
        Home->>Logger: AddHome Failure
        Home-->>Processor: ArgumentException
    else Unique
        Home->>Repo: Add(dataObject)
        Home->>Repo: SaveChanges()
        Home->>Logger: AddHome Success
        Home-->>Processor: Mapped Home
        Processor-->>Client: GetHomeResponse
    end
    Home->>Lock: Exit during return or unwind
```

Because Player lookup occurs inside the lock, a slow or failing Player repository operation blocks every concurrent Home operation in this process.

## Home Patch Trace

1. Controller sets `PatchHomeRequest.Identifier` from route; JSON cannot set it due to `JsonIgnore`.
2. `HomeService.Update` enters `Execute`, acquires the lock, and logs Started.
3. Validate request and identifier.
4. Repository `Get(identifier)` returns a data object.
5. Map to Home service model and back to a data object.
6. Merge a supplied Name language by language.
7. Resolve a supplied Player through `IPlayerService.Get` and store canonical identifier.
8. Validate and replace a supplied Location.
9. Extract all resulting names and scan repository records, excluding the identical Home id.
10. Reject a destination-player conflict before repository update.
11. Generate UpdatedDT.
12. Update and save.
13. Map result, log Success, release lock, and return content.

The map-out and map-back step creates a detached candidate under ordinary object semantics. It preserves Identifier and creation metadata before patching.

## Home Query Branch Trace

`HomesController.GetAll` branches on both bound values and raw query-key presence:

```mermaid
flowchart TD
    Start[GET /Homes] --> Name{Bound Name non-null or name key present?}
    Name -- Yes --> Single[service.Get Player, Name]
    Name -- No --> Player{Bound Player non-null or player key present?}
    Player -- Yes --> Filtered[GetAllByPlayer]
    Player -- No --> All[GetAll]
    Single --> OneResponse[GetHomeResponse]
    Filtered --> ManyResponse[GetHomesResponse]
    All --> ManyResponse
```

An empty `name` key selects the single-Home path and then fails service whitespace validation rather than falling through to a collection.

## State And Side Effects

- Registration adds and saves one Player record.
- Player patch updates and saves one Player record.
- Home addition or patch scans and then saves under one lock.
- Home deletion removes and saves under the identical lock.
- Retrieval logs but performs no explicit save.
- No emitted events, external calls, or cross-store transactions exist.

## Final Conditions

Successful Home writes preserve per-player name uniqueness relative to every operation passing through the identical singleton instance. This guarantee does not extend to another process, direct repository use, or external file modifications.
