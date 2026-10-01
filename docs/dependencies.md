# Dependencies

| Metadata | Value |
|----------|-------|
| Purpose | Explain significant dependencies by architectural role, use locations, encapsulation, and upgrade or replacement consequences. |
| Scope | Direct runtime and test package references plus materially used transitive APIs. |
| Primary sources | [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj), [unit-test project](../NuciCraft.API.UnitTests/NuciCraft.API.UnitTests.csproj), [integration-test project](../NuciCraft.API.IntegrationTests/NuciCraft.API.IntegrationTests.csproj), and production usages |
| Related documents | [Architecture](architecture.md), [integrations](integrations.md), [build and deployment](build-and-deployment.md), and [testing](testing.md) |

## Platform Dependencies

### .NET 10 And ASP.NET Core

**Role:** Host, dependency injection, configuration, middleware, routing, model binding, JSON transport, filesystem and networking primitives, cryptographic hashing, and process lifetime.

**Use locations:** [Program.cs](../NuciCraft.API/Program.cs), [Startup.cs](../NuciCraft.API/Startup.cs), controllers, services, and project manifests.

**Encapsulation:** Not encapsulated; the entire application is an ASP.NET Core web process.

**Upgrade consequences:** Target-framework upgrades affect hosting defaults, serialisation, model validation, dependency injection, runtime APIs, CI, editor launch paths, release automation, and package compatibility.

## Runtime Package Matrix

| Package | Version | Architectural Role |
|---------|---------|--------------------|
| MineStat | 3.1.2 | Minecraft Java legacy server-list status client. |
| NuciAPI | 3.6.1 | Shared API requests, responses, and authorisation primitives. |
| NuciAPI.Client | 1.2.3 | Typed outbound Universal Name Generator client. |
| NuciAPI.Controllers | 2.3.1 | `NuciApiController.ProcessRequest` boundary. |
| NuciAPI.Middleware | 2.0.3 | Base middleware package dependency. |
| NuciAPI.Middleware.ExceptionHandling | 1.0.2 | HTTP exception translation. |
| NuciAPI.Middleware.Logging | 1.0.1 | Request logging. |
| NuciAPI.Middleware.Security | 1.0.6 | Scanner-protection registration and middleware. |
| NuciDAL | 3.2.1 | Repository contracts, entity base, and JSON adapter. |
| NuciLog | 1.2.1 | Logger implementation and settings registration. |
| NuciLog.Core | 3.1.0 | Logger interfaces, operations, statuses, and context types. |
| NuciSecurity.HMAC | 4.1.3 | HMAC order and ignore annotations. |
| NuciText.Normalisation | 1.0.1 | Registered normaliser utility. |
| NuciText.Obfuscation | 1.1.1 | Registered obfuscator utility. |

Versions are explicit and not centralised through shared package properties.

## MineStat

**Why it exists:** Encodes and parses the Minecraft Java server-list protocol so local code does not implement packet parsing.

**Use:** [ServerStatusService.cs](../NuciCraft.API/Service/ServerStatusService.cs) constructs `MineStat` with Legacy protocol and reads `ServerUp` and `CurrentPlayersInt`.

**Encapsulation:** Isolated behind `IServerStatusService` for controllers. The concrete service itself directly references MineStat.

**Upgrade or replacement consequences:** Reverify handshake bytes, timeout interpretation, exception types, unavailable-server behaviour, integer parsing, and legacy protocol compatibility. [ServerStatusServiceTests.cs](../NuciCraft.API.IntegrationTests/ServerStatusServiceTests.cs) is the protocol regression boundary.

## NuciAPI Core, Controllers, And Middleware

**Why they exist:** Standardise organisation-level API contracts, controller processing, authorisation descriptors, response envelopes, exception handling, scanner protection, and request logging.

**Use:** Every request and response type, every controller, and [Startup.cs](../NuciCraft.API/Startup.cs).

**Encapsulation:** Low. Controllers inherit package classes and public responses contain package envelopes. Middleware extensions are composition-level dependencies.

**Upgrade consequences:** Potential changes to public JSON, HTTP statuses, API-key processing, HMAC handling, model validation sequencing, request logs, or scanner behaviour. Execute all controller and integration tests and add focused tests for any release-note change.

Replacing these packages would require a new controller base, authorisation implementation, response envelopes, exception middleware, scanner policy, and request logging.

## NuciAPI.Client

**Why it exists:** Sends typed outbound Nuci API requests to the Universal Name Generator.

**Use:** Composition constructs `NuciApiClient`; `MobService` calls `INuciApiClient.SendRequestAsync` with explicit bearer information.

**Encapsulation:** Interface-isolated inside `MobService` and replaceable in tests.

**Upgrade consequences:** Reverify overload selection, query serialisation, BaseUrl combination, response runtime type, top-level Names, unsuccessful response data, HTTP handler lifetime, and timeout. The explicit authorisation overload is required for the protected remote endpoint.

## NuciDAL

**Why it exists:** Supplies generic repository operations and JSON-file persistence.

**Use:** Every file-backed service, startup eager loading, data-object inheritance, and composition.

**Encapsulation:** Services depend on `IFileRepository<T>`, but data objects derive from NuciDAL base types and composition constructs `JsonRepository<T>`.

**Upgrade consequences:** Highest persistence risk. Reverify serialised field names, duplicate IDs, Get and GetAll semantics, SaveChanges durability, concurrent writes, object identity, exception types, and old-file compatibility. Restart-persistence integration tests cover only selected ordinary cases.

Replacing it requires implementing the complete locally used repository surface and defining currently external consistency semantics.

## NuciLog And NuciLog.Core

**Why they exist:** Provide structured operation logging, statuses, context fields, settings, and optional file sink.

**Use:** All principal file-backed and Mob services, request middleware, [MyOperation.cs](../NuciCraft.API/Logging/MyOperation.cs), and [MyLogInfoKey.cs](../NuciCraft.API/Logging/MyLogInfoKey.cs).

**Encapsulation:** Services directly reference core interfaces and types; implementation selection is in composition.

**Upgrade consequences:** Reverify DI lifetime, settings section, file output, exception logging, context serialisation, and sink failure policy. Changes can expose sensitive data or alter diagnostic completeness.

## NuciSecurity.HMAC

**Why it exists:** Annotates canonical property order and properties excluded from HMAC processing.

**Use:** Requests, responses, coordinates, zone bounds, localised or settings data objects, and outbound generator request.

**Encapsulation:** Attributes are embedded in public and persisted contract types.

**Upgrade consequences:** Reverify canonicalisation, duplicate order handling, nested object ordering, ignored Count fields, and client interoperability. There is no local end-to-end HMAC compatibility test.

## NuciText Libraries

**Why they exist:** The composition root registers `INuciTextNormaliser` and `INuciTextObfuscator`.

**Use:** No local production class injects either interface.

**Encapsulation:** Registration only.

**Upgrade or removal consequences:** Current service-collection tests expect both to resolve. The historical or external reason for registration is not evident. Search package and downstream expectations before removal.

## NuciExtensions Transitive Use

[RtpLocationService.cs](../NuciCraft.API/Service/RtpLocationService.cs) imports `NuciExtensions` and calls `GetRandomElement`, but the runtime project does not declare a direct `NuciExtensions` PackageReference. The API arrives transitively through another package.

Consequences:
- A transitive dependency revision can remove or change the API without a manifest edit here.
- Dependency intent is less explicit.
- Random selection implementation is external.

If this use remains intentional, a direct package reference can make ownership explicit, subject to repository package policy.

## Framework Cryptography

`PlayerService` uses framework MD5 solely for deterministic Minecraft offline UUID compatibility, not password security. Replacing the algorithm would change identities for every username and violate compatibility. Player passwords are not hashed by this use.

## Test Dependencies

| Package | Version | Role |
|---------|---------|------|
| Microsoft.NET.Test.Sdk | 18.10.1 | Test discovery and execution. |
| NUnit | 4.6.1 | Test framework and assertions. |
| NUnit3TestAdapter | 6.3.0 | NUnit integration with .NET test tooling. |
| Moq | 4.20.72 | Repository, service, client, environment, and context doubles. |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.0 | `WebApplicationFactory<Program>`. |
| Microsoft.AspNetCore.TestHost | 10.0.0 | In-memory ASP.NET Core server. |

The unit-test project also references NuciDAL and NuciLog.Core directly because test source uses their interfaces and types.

Upgrade consequences include assertion or mocking changes, host behaviour changes, test-discovery compatibility, and nullable warnings in integration tests.

## Dependency Replacement Priorities

| Dependency | Replacement Scope |
|------------|-------------------|
| MineStat | One concrete service plus protocol tests. |
| NuciApiClient | One adapter registration and MobService interface usage. |
| NuciDAL | Every persisted domain, startup, data-object base, migration, and consistency design. |
| NuciLog | Every service logging call, settings, operations, middleware integration. |
| NuciAPI Controllers and Middleware | Complete inbound HTTP boundary and public error/success contracts. |
| NuciSecurity.HMAC | Every signed contract and client compatibility. |
| NuciText | Registration and unknown downstream expectations. |

## Dependency Review Rules

- Treat package upgrades as behavioural changes when the package owns a boundary.
- Read release notes and inspect transitive changes.
- Execute solution tests plus focused boundary tests.
- Preserve old JSON fixtures outside production data when testing NuciDAL compatibility.
- Never infer external package conduct not demonstrated by source or tests.
- Document novel direct or transitive dependencies and their trust implications.
