# Player And Home Capabilities

| Metadata | Value |
|----------|-------|
| Purpose | Describe Player and Home conduct as externally initiated capabilities, including complete normal and exceptional paths. |
| Scope | Registration, retrieval, Player patching, Home creation, retrieval, filtering, patching, and deletion. |
| Primary sources | [PlayersController.cs](../../NuciCraft.API/Controllers/PlayersController.cs), [PlayerService.cs](../../NuciCraft.API/Service/PlayerService.cs), [HomesController.cs](../../NuciCraft.API/Controllers/HomesController.cs), and [HomeService.cs](../../NuciCraft.API/Service/HomeService.cs) |
| Related documents | [Player and Home components](../components/player-and-homes.md), [request flow](../flows/player-and-home-requests.md), [persistence](../components/persistence-and-mapping.md), and [invariants](../invariants.md) |

## Player Registration

### Purpose And Initiator

An authorised client initiates `POST /Players` to create one persisted Player record. The operation does not return the created record; clients ordinarily retrieve it through a selector route afterwards.

### Preconditions And Inputs

- A valid service-wide bearer API key.
- A JSON object bindable to [RegisterPlayerRequest.cs](../../NuciCraft.API/Requests/RegisterPlayerRequest.cs).
- Username is transport-required.
- Any supplied CreatedDT, BannedDT, MutedDT, LastLoginDT, LastLogoutDT, or BackDT must use the exact seven-fractional-digit timestamp format.

### Execution Sequence

1. ASP.NET Core binds and validates the request.
2. `PlayersController.Register` passes it to `ProcessRequest` with API-key authorisation.
3. `PlayerService.Register` rejects a null request and starts operation logging.
4. The service generates a random identifier.
5. The service computes the deterministic Minecraft offline UUID from Username.
6. Gender is parsed case-insensitively; unsupported input becomes Other.
7. CreatedDT becomes supplied exact timestamp or current UTC.
8. Optional supported timestamps are parsed or remain null.
9. Supplied coordinate data objects map into service coordinates.
10. Player settings are initialised to service defaults.
11. The complete Player maps to `PlayerDataObject`, including string timestamps.
12. The repository adds the record and saves.
13. The service logs success and the processor returns the standard command success response.

### Decisions And Transformations

- No existing-record search occurs.
- DisplayName remains null when omitted; retrieval later substitutes Username.
- OfflineUUID ignores any client value because registration does not expose that input.
- OnlineUUID and password are stored as supplied.
- New settings cannot be supplied by registration; defaults are mandatory.

### Side Effects And Postconditions

- One Player record exists with generated Identifier and OfflineUUID.
- The player store is saved synchronously.
- Operation logs can include Username, UUID values, timestamps, and last IP address.

### Failure Paths

- Missing Username at HTTP boundary: `400` through model validation.
- Invalid supported timestamp: `ArgumentException`, translated by outer middleware.
- Repository add or save failure: logged and rethrown.
- No retry, duplicate reconciliation, compensation, or cleanup occurs.

## Player Retrieval

### Entry Points

- `GET /Players/{identifier}`
- `GET /Players/by-username/{username}`
- `GET /Players/by-offline-uuid/{offlineUUID}`
- `GET /Players/by-online-uuid/{onlineUUID}`
- `GET /Players`

### Single-Player Sequence

1. Controller constructs `GetPlayerRequest` with exactly one route selector.
2. `PlayerService.Get` logs all four selector slots.
3. Repository `GetAll` is scanned with one OR predicate.
4. The first case-sensitive match maps into a `Player` model.
5. Persisted timestamps are parsed and settings or coordinates are mapped.
6. `GetPlayerResponse` projects the Player and defaults null DisplayName to Username.
7. Content is returned through the Nuci API envelope.

If no record matches, the service raises `KeyNotFoundException`. Multiple duplicate records are not detected; repository order determines the result.

### Collection Sequence

1. Controller sends an empty `GetPlayersRequest` through `ProcessRequest`.
2. `PlayerService.GetAll` maps the repository enumerable.
3. Controller projects every model into `GetPlayerResponse`.
4. `GetPlayersResponse.Count` enumerates the resulting collection for its derived count.

The response includes sensitive stored fields. No field-level filtering is applied.

## Player Patch

### Purpose And Entry Points

Four PATCH routes revise a Player selected by identifier, username, offline UUID, or online UUID. The route assigns its selector into the body request before processing.

### Validation

`PlayerService.ValidatePatchSelectors` requires exactly one non-whitespace selector across all four properties. A body that supplies another selector in addition to the route selector fails.

Unlike registration, supplied patch timestamp strings are not format-validated before persistence.

### Execution Sequence

1. Bind body and assign route selector.
2. Authorise and invoke `PlayerService.Update`.
3. Validate exactly one selector.
4. Scan repository records and obtain the first matching Player.
5. Replace every supplied non-null mutable field.
6. Merge Settings according to explicit presence flags.
7. Generate current UTC UpdatedDT.
8. Call repository Update and SaveChanges.
9. Log success and return a command success response.

### Patch Branches

- Nullable booleans apply when they contain either true or false.
- Nested coordinate objects replace complete persisted nested objects.
- Settings booleans apply only when their JSON setters executed.
- Null settings leaves all persisted settings unchanged.
- Null persisted settings plus a supplied settings patch creates defaults before selective application.
- Identifier, Username, OfflineUUID, OnlineUUID, and CreatedDT cannot be mutated by available patch logic.

### Failure And Postconditions

On success, exactly one selected record has a novel UpdatedDT and saved fields. On selector, lookup, mapping, or persistence failure, the service logs and rethrows. There is no rollback beyond the repository's own unknown internal conduct.

## Home Creation

### Purpose And Initiator

An authorised client calls `POST /Homes` to associate a localised saved location with an existing Player identifier.

### Preconditions

- Body Name, Player, and Location are transport-required.
- Player must be the generated identifier of an existing record.
- Location World must be non-whitespace.
- Every coordinate and orientation value must be finite.
- At least one localised Name value must contain non-whitespace characters.
- No Home for the identical Player may share any trimmed Name value, regardless of language property or case.

### Execution Sequence

1. Controller delegates through `ProcessRequest`.
2. `HomeService.Execute` acquires the process-local Home lock and logs Started.
3. Validate request, Player selector, and Location.
4. Resolve Player through `IPlayerService.Get` and retain its generated identifier.
5. Generate Home identifier and typed UTC CreatedDT.
6. Map the Home into a data object.
7. Materialise candidate localised names and scan every persisted Home for an overlap under the identical Player.
8. Add and save the Home.
9. Map the persisted data object back into a Home model.
10. Log Success, release the lock, and return `GetHomeResponse`.

### Failure Conduct

Validation, missing Player, duplicate Name, mapping, repository, and save failures are logged by `Execute`, rethrown, and release the monitor lock automatically. No Home write occurs before all local validation passes.

## Home Retrieval And Filtering

### Direct Identifier

`GET /Homes/{id}` performs repository direct lookup and mapping under the Home lock.

### All Homes

`GET /Homes` without `name` or `player` keys materialises all mapped Homes as an array while holding the lock, then returns a collection response.

### Player Filter

`GET /Homes?player={id}` and `GET /Homes/by-player/{id}` validate a non-whitespace Player selector, compare stored owner identifiers ordinally, and return a materialised array. They do not verify that the Player still exists.

### Player And Name Lookup

`GET /Homes?player={id}&name={name}`:
1. Takes precedence whenever the `name` query key is present.
2. Requires non-whitespace Player and Name in service logic.
3. Trims the requested Name.
4. Scans all Homes for exact Player and ordinal case-insensitive membership among trimmed localised names.
5. Returns the first match or raises `KeyNotFoundException`.

## Home Patch

### Execution Sequence

1. Controller assigns route identifier to the body request; the property is ignored in JSON.
2. `HomeService.Execute` acquires the Home lock.
3. Repository retrieves the Home and maps it to a service model, then back to a data object.
4. A supplied Name merges each non-null localisation.
5. A supplied Player is resolved and replaced with its generated identifier.
6. A supplied Location is validated and replaces the complete nested object.
7. The complete candidate is checked for per-player name uniqueness, excluding its own identifier.
8. UpdatedDT is generated.
9. Repository update and save execute.
10. The revised Home maps into response content.

This order ensures a transfer to another Player revalidates Name uniqueness in the destination owner's namespace.

## Home Deletion

`DELETE /Homes/{id}` validates the non-whitespace identifier inside the Home lock, calls repository Remove, saves, logs success, and returns a command success response. There is no soft delete or recovery store.

## Retries, Cleanup, And Idempotency

- No Player or Home operation retries.
- GET operations are observational except for any unknown repository materialisation effects.
- PATCH with identical values still generates a novel UpdatedDT and saves, so it is not state-idempotent.
- DELETE repeat behaviour depends on NuciDAL absent-record semantics and is not normalised locally.
- POST operations always generate novel identifiers and are not idempotent.
- Cleanup consists of ordinary exception unwinding and Home lock release.

## Capability Invariants

The canonical rule list appears in [invariants.md](../invariants.md). Modifications must especially preserve deterministic offline UUID generation, selector immutability, settings presence semantics, canonical Home owner identifiers, and the lock-protected Home uniqueness check.
