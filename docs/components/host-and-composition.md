# Host And Composition Component

| Metadata | Value |
|----------|-------|
| Purpose | Document process creation, dependency injection, startup preparation, middleware order, and lifecycle ownership. |
| Scope | `Program`, `Startup`, `ServiceCollectionExtensions`, settings binding, and process-level infrastructure selection. |
| Primary sources | [Program.cs](../../NuciCraft.API/Program.cs), [Startup.cs](../../NuciCraft.API/Startup.cs), [ServiceCollectionExtensions.cs](../../NuciCraft.API/ServiceCollectionExtensions.cs), and [Configuration](../../NuciCraft.API/Configuration/) |
| Related documents | [Architecture](../architecture.md), [configuration](../configuration.md), [startup flow](../flows/startup.md), and [build and deployment](../build-and-deployment.md) |

## Purpose

The host and composition component turns repository types and deployment configuration into one runnable ASP.NET Core process. It selects concrete infrastructure, defines dependency lifetimes, verifies the seven persistence adapters can load, and establishes the inbound middleware pipeline.

## Scope

This component owns:
- Default host construction and command-line argument hand-off.
- Controller and middleware service registration.
- Strongly typed settings construction and binding.
- Repository, client, service, utility, and logger registrations.
- Initial filesystem creation and repository enumeration.
- Middleware ordering and endpoint mapping.

It does not own:
- Domain validation or state mutation.
- HTTP route definitions.
- Nuci package internals.
- Deployment provisioning, process supervision, backups, or TLS termination.
- Custom shutdown conduct.

## Position In The Architecture

This is the composition root. It is permitted to reference concrete services and adapters because it decides which implementations satisfy controller and service dependencies. Runtime dependency direction proceeds away from this component into controllers, services, repositories, and external clients.

## Dependencies

| Dependency | Use |
|------------|-----|
| ASP.NET Core hosting | Process construction, configuration, dependency injection, middleware, routing, static files, and environment detection. |
| Nuci API middleware | Exception handling, scanner protection, and request logging. |
| NuciDAL | `IFileRepository<T>` and `JsonRepository<T>`. |
| NuciAPI.Client | Singleton outbound API client. |
| NuciLog | Bound logger settings and `NuciLogger`. |
| NuciText | Normaliser and obfuscator singleton registrations. |
| Filesystem | Directory and vacant JSON store creation. |

Package versions are fixed in [NuciCraft.API.csproj](../../NuciCraft.API/NuciCraft.API.csproj).

## Dependants

- Every controller depends on registrations created here.
- Every application service depends on its repository, settings, logger, or external adapter registrations.
- Startup and composition unit tests depend on stable registration and store-preparation conduct.
- Deployment depends on the configuration section names and path interpretation defined here.

## Internal Structure

### Program

[Program.cs](../../NuciCraft.API/Program.cs) contains:
- `Main`, which constructs and runs the host.
- `CreateHostBuilder`, which delegates to `Host.CreateDefaultBuilder` and selects `Startup`.

No custom host lifetime, configuration provider, web server, content root, or logging provider is installed locally.

### Startup

[Startup.cs](../../NuciCraft.API/Startup.cs) contains:
- `ConfigureServices`, the high-level registration sequence.
- `Configure`, store preparation and middleware construction.
- `PrepareRepositories`, orchestration for all seven stores.
- `CreateStoreIfMissing`, filesystem creation.
- `EagerlyLoadRepositories` and its generic helper, repository resolution and enumeration.

### Service Collection Extensions

[ServiceCollectionExtensions.cs](../../NuciCraft.API/ServiceCollectionExtensions.cs) contains:
- `AddConfigurations`, which constructs, binds, and registers six application settings instances plus NuciLog settings.
- `AddCustomServices`, which registers repositories, clients, application services, text utilities, and logging.
- `AddJsonRepository<T>`, the generic singleton adapter factory.

The private static `dataStoreSettings` field connects `AddConfigurations` to later repository factories. `AddCustomServices` therefore presumes `AddConfigurations` executed first.

### Settings Types

The [Configuration](../../NuciCraft.API/Configuration/) directory defines:
- `DataStoreSettings`
- `RtpLocationSettings`
- `SecuritySettings`
- `ServerSettings`
- `UniversalNameGeneratorSettings`
- `WebMapSettings`

They are mutable property containers without local validation methods or options validation.

## Behaviour

### Service Construction

`AddConfigurations` binds sections using each local variable's C# name. The effective lower-camel section names in [appsettings.json](../../NuciCraft.API/appsettings.json) bind case-insensitively through Microsoft configuration.

`AddCustomServices` creates singleton repository factories that read paths from the static settings object. The Universal Name Generator client is constructed with the bound base URL; other settings validation is deferred to consuming operations.

### Store Preparation

For every configured store:
1. Reject a null or whitespace path.
2. Derive its parent directory.
3. Create that directory when it does not exist.
4. Create the file with `[]` when it does not exist.
5. Resolve the repository and enumerate `GetAll()` into a list.

Existing file content is neither rewritten nor validated independently of repository deserialisation. `Path.GetDirectoryName` can return a null or vacant value for a filename without a directory component; the current code passes that value to directory APIs.

### Pipeline Construction

The exact middleware sequence is documented in [architecture.md](../architecture.md). Request routing occurs after exception, scanner, request-logging, HTTPS, default-file, and static-file middleware. `UseAuthorization` runs before mapped controller endpoints, but API-key checks are controller-processing descriptors rather than local ASP.NET Core policies.

## Inputs And Outputs

Inputs are:
- Process arguments accepted by the default host.
- Standard default-host configuration sources.
- Six application sections and the external NuciLog section.
- Filesystem state at seven paths.
- ASP.NET Core environment name.

Outputs are:
- A service provider containing every runtime dependency.
- Seven present and deserialisable stores.
- A configured request pipeline.
- A running Kestrel-hosted process.

No application-domain data is transformed directly by this component.

## State

- Strongly typed settings instances persist for the process lifetime.
- `dataStoreSettings` is also held in static process state.
- Singleton repositories, services, clients, and text utilities persist for the process lifetime.
- Logger registration is scoped, although singleton consumers can retain a resolved logger.
- Files created during startup persist beyond process termination.

Configuration objects are not registered through `IOptionsMonitor`; the repository defines no supported hot-reload behaviour for bound instances.

## Error Handling

No startup preparation exception is caught locally. Representative failures include:
- Null or whitespace store paths.
- Invalid or inaccessible directories.
- Denied write access.
- Invalid JSON or incompatible persisted fields.
- Missing repository registrations.
- Adapter construction errors.

Any such failure aborts host initialisation. There is no retry, fallback path, partial capability mode, or store recreation for existing malformed files.

Runtime exceptions entering the pipeline reach package-supplied exception middleware. Exact mappings not covered by integration tests remain external behaviour.

## Configuration

This component consumes every key in [appsettings.json](../../NuciCraft.API/appsettings.json). The default host provides layered JSON, environment, and command-line configuration. Configuration details and environment-key forms are centralised in [configuration.md](../configuration.md).

## Extension And Modification Guidance

### Adding A File-Backed Domain

Revise all of the subsequent locations:
1. Add a path property to `DataStoreSettings` and a default key to the configuration file.
2. Register `IFileRepository<TDataObject>` in `AddCustomServices`.
3. Create the missing store in `PrepareRepositories`.
4. Add the record type to `EagerlyLoadRepositories`.
5. Register the service interface and implementation.
6. Expand startup and service-collection tests.

Omitting either preparation step creates inconsistent startup conduct.

### Adding Middleware

Place middleware according to its desired observation and exception boundary. Changes before or after exception handling, request logging, routing, or authorisation can materially alter visible errors and diagnostics.

### Changing Lifetimes

Trace the complete dependency graph. A singleton cannot safely depend on ordinary scoped request state. Repository lifetime changes can alter state visibility and file write coordination.

## Relevant Processes

- [Process startup](../flows/startup.md)
- [File-backed requests](../flows/file-backed-request.md)
- [External requests](../flows/external-requests.md)
