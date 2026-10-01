# Agent-Oriented Change Guide

| Metadata | Value |
|----------|-------|
| Purpose | Map common modifications to affected implementation layers, registrations, mappings, tests, compatibility concerns, and documentation. |
| Scope | Practical repository modifications likely to be performed by future coding agents. |
| Primary sources | [Repository structure](repository-structure.md), [architecture](architecture.md), [testing](testing.md), and current implementation patterns |
| Related documents | [Invariants](invariants.md), [data model](data-model.md), [interfaces](interfaces.md), and [documentation maintenance](documentation-maintenance.md) |

## Start Here

Before a modification:
1. Identify the owning capability in the [documentation index](README.md).
2. Read its component and behavioural document.
3. Read the relevant execution flow.
4. Review [invariants.md](invariants.md) and [ambiguities-and-open-questions.md](ambiguities-and-open-questions.md).
5. Inspect only the concrete source and tests named by those documents.
6. Confirm worktree state and preserve unrelated changes.

## Adding A New HTTP Endpoint

### Starting Point

Start in the owning controller under [Controllers](../NuciCraft.API/Controllers/) and its service interface under [Service](../NuciCraft.API/Service/).

### Commonly Affected Layers

- Request DTO under [Requests](../NuciCraft.API/Requests/).
- Response content under [Responses](../NuciCraft.API/Responses/) when data is returned.
- Controller action, route, binding, and `ProcessRequest` delegate.
- Service interface and implementation.
- Logging operation and context keys when novel.
- Repository, mapping, or external adapter only when the capability requires them.

### Registrations

No registration is necessary when extending an existing service. A novel service requires `ServiceCollectionExtensions.AddCustomServices` and registration tests.

### Mappings

Add mapping only when crossing service and persistence representations. Do not map a response directly from a data object as a shortcut.

### Tests

- Controller unit test for binding-to-service translation.
- Service unit tests for each decision branch.
- HTTP integration success test.
- Authorisation, invalid input, and error status tests when novel semantics arise.

### Documentation

Revise [interfaces.md](interfaces.md), the owning behavioural document, the relevant flow, [data-model.md](data-model.md) for novel DTOs, and [documentation-coverage.md](documentation-coverage.md).

### Compatibility And Common Omissions

- Routes are unversioned.
- Every endpoint currently passes API-key authorisation.
- Assign route selectors into patch DTOs deliberately.
- Select unique HMAC order values and preserve existing order.
- Decide whether a command returns base success or content.
- Add the endpoint to public [README.md](../README.md) when user-facing usage changes materially.

## Adding A New File-Backed Domain

### Starting Point

Define the domain record, public capability, and ownership boundary first. Then use Country or World as the basic pattern, or Home/Zone when richer invariants apply.

### Affected Layers

1. Settings path in [DataStoreSettings.cs](../NuciCraft.API/Configuration/DataStoreSettings.cs) and [appsettings.json](../NuciCraft.API/appsettings.json).
2. Root data object extending `NuciCraftEntityBase`.
3. Service model when persistence and application representations differ.
4. Mapping extensions.
5. Service interface and implementation.
6. Requests, responses, and controller.
7. Logging operation and keys.
8. Repository registration.
9. Startup path creation and eager load.

### Tests

- Settings registration.
- Repository resolution.
- Startup creates and enumerates the store.
- Mapping round trip.
- Service success and failures.
- Controller delegation.
- HTTP integration CRUD or query paths.
- Restart persistence when durability is material.

### Documentation

Revise repository overview, architecture, repository structure, data model, configuration, state, component, behaviour, flow, interfaces, dependencies if novel, invariants, testing, change guide, and coverage map.

### Common Omissions

- Adding repository registration but omitting store creation or eager load.
- Committing runtime JSON data.
- Omitting explicit SaveChanges.
- Exposing persistence objects publicly.
- Ignoring old-file compatibility and migration.
- Assuming multi-process safety.

## Adding Or Changing A Persisted Field

### Starting Point

Start with the root or nested data object under [DataAccess/DataObjects](../NuciCraft.API/DataAccess/DataObjects/) and identify every representation from [data-model.md](data-model.md).

### Review Map

- Add or patch request.
- Service model.
- Data object.
- Both mapping directions.
- Response content.
- Service creation and patch logic.
- HMAC ordering and JSON aliases.
- Old persisted records and defaults.
- Personal-data and logging implications.

### Tests

Mapping, service creation, omitted patch, explicit patch, response projection, HTTP serialisation, and restart persistence.

### Compatibility

Adding an optional field with a safe default can be backward compatible. Renaming, changing type, making a field required, or altering timestamp representation requires migration. No migration engine exists.

### Common Omissions

- Updating only request and response while persistence discards the value.
- Updating persistence but omitting response projection.
- Losing patch omission semantics for booleans.
- Using a duplicate HMAC order.
- Failing to consider whether null means clear or preserve.

## Adding A Configuration Option

### Starting Point

Add a property to the appropriate class under [Configuration](../NuciCraft.API/Configuration/) or create a cohesive settings class.

### Affected Areas

- Default or placeholder in [appsettings.json](../NuciCraft.API/appsettings.json).
- Binding and singleton registration for a novel settings class.
- Consumer constructor injection.
- Validation timing and failure conduct.
- Test configuration factory and integration-host overrides.
- Service-collection resolution tests.

### Documentation

Revise [configuration.md](configuration.md), security for secrets, integrations for external destinations, build and deployment for operational requirements, and the owning component.

### Common Omissions

- Presuming placeholder interpolation.
- Omitting integration-test overrides and accidentally contacting external systems.
- Adding mutable reload expectations without `IOptionsMonitor`.
- Using relative paths without documenting working-directory dependence.

## Adding An External Integration

### Starting Point

Define a narrow interface at the application boundary and identify request-time versus process-lifetime state.

### Affected Areas

- Client or adapter interface and concrete implementation.
- Settings and secret source.
- DI registration and lifetime.
- Application service orchestration.
- Authentication, timeout, cancellation, retry, fallback, and rate limits.
- Request and response models.
- Sensitive data transmitted and received.

### Tests

- Unit tests with a mock adapter.
- Contract validation for method, endpoint, authorisation, and shape.
- Loopback or fake-server integration where protocol behaviour matters.
- Failure, timeout, malformed response, and unavailable dependency branches.

### Documentation

Revise [integrations.md](integrations.md), [dependencies.md](dependencies.md), [security.md](security.md), [error-handling.md](error-handling.md), [configuration.md](configuration.md), relevant behaviour and flow, and coverage.

### Common Omissions

- Reusing the inbound API key as an outbound credential.
- Omitting a timeout or cancellation design.
- Retrying non-idempotent operations.
- Logging credentials or remote personal data.
- Allowing integration tests to contact real remote systems unintentionally.

## Adding A New Application Service

### Starting Point

Define one responsibility and an `I*Service` interface under [Service](../NuciCraft.API/Service/).

### Required Revisions

- Implementation with constructor-injected dependencies.
- Singleton registration unless a different lifetime is justified and its dependency graph permits it.
- Service-collection resolution test.
- Controller or consuming service injection.
- Logging operation convention.
- Unit tests with isolated dependencies.

### Compatibility

Review singleton thread safety and scoped dependency capture. Avoid request-specific mutable fields.

## Modifying Validation

### Starting Point

Determine whether the rule is transport shape or domain invariant:
- Transport shape belongs in request annotations.
- Domain invariant belongs in the service so non-controller callers cannot bypass it.

### Required Review

- Existing persisted records that might violate the novel rule.
- Addition and patch parity.
- Read behaviour for historical invalid state.
- Exception type and observable status.
- Validation placement relative to logging and mutation.
- Tests for exact threshold, null, whitespace, case, and partial patch edges.

### Common Omissions

- Validating creation but not ownership transfer or patch.
- Tightening write validation while reads still fail on old records.
- Changing case sensitivity inadvertently.
- Mutating an object before full candidate validation.

## Modifying Serialisation Or HMAC Contracts

### Starting Point

Inspect request, response, nested types, and package attributes. Determine whether the change affects HTTP, persistence, outbound integration, or several boundaries.

### Required Review

- `JsonPropertyName` values.
- HMAC order uniqueness and compatibility.
- `HmacIgnore` for derived values.
- System.Text.Json handling of enum-like classes.
- NuciDAL persisted property names.
- Remote generator contract.
- Client and old-file compatibility.

### Tests

Reflection contract tests, HTTP integration JSON assertions, mapping tests, persisted old-record reads, and external request verification.

## Modifying Authentication Or Authorisation

### Starting Point

Read [security.md](security.md) and inspect every controller because authorisation is passed explicitly per action.

### Required Revisions

- Security settings and secret provisioning.
- Controller processing or a deliberate central replacement.
- Integration tests for absent, invalid, and valid credentials.
- Per-record policy design if introducing caller identity.
- Request logging and error disclosure.
- Public README and security documentation.

### Common Omissions

- Treating the Home Player selector as authenticated identity.
- Protecting only novel endpoints while existing routes retain another model.
- Committing test or production credentials.
- Presuming `UseAuthorization` supplies authentication by itself.

## Replacing Persistence

### Starting Point

Define required semantics currently left to NuciDAL: unique IDs, query snapshots, transactions, concurrency, atomic writes, migration, retries, and exceptions.

### Affected Areas

- DI adapters.
- Startup preparation.
- Data-object base identity.
- Every service repository operation.
- Configuration and deployment infrastructure.
- Migration and rollback.
- Integration and restart tests.

The service boundary can remain stable if a compatible `IFileRepository<T>` adapter is practical, but a database design may merit domain-specific repositories rather than emulating file semantics.

## Adding A Background Process

No pattern presently exists. A deliberate addition requires:
- `IHostedService` or `BackgroundService` registration.
- Start and stop lifecycle.
- Cancellation.
- Scheduling and overlap policy.
- Singleton repository coordination.
- Failure and retry policy.
- Health and observability.
- Deterministic tests.

Revise architecture, concurrency, state, error handling, build and deployment, testing, and coverage. Do not place an infinite loop in a singleton application service invoked by controllers.

## Modifying Deployment Or CI

### Starting Point

Use [.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml), [.vscode/tasks.json](../.vscode/tasks.json), project manifests, and [release.sh](../release.sh) as evidence.

### Required Review

- .NET version parity.
- Solution versus runtime-only compilation.
- Restore, compile, and test separation.
- Secret handling.
- Store backup and schema compatibility.
- Single-writer deployment topology.
- Health and rollback.
- Remote release script trust.

Revise [build-and-deployment.md](build-and-deployment.md), configuration, security, testing, dependencies, and public README where user procedures change.

## Final Change Checklist

- Owning component and process documents are revised.
- All changed public and persisted fields are mapped.
- DI registration and startup preparation remain complete.
- Tests cover success, failure, edge, and persistence paths proportionate to risk.
- Security and personal-data exposure are assessed.
- Existing runtime data remains readable or a migration exists.
- Documentation links pass validation.
- Complete solution compiles and tests pass.
