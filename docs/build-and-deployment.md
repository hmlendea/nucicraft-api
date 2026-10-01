# Build, Deployment, And Operations

| Metadata | Value |
|----------|-------|
| Purpose | Document restoration, compilation, test, publication, CI, release, runtime requirements, startup, shutdown, and operator responsibilities. |
| Scope | Repository-provided development and delivery mechanisms plus evidenced deployment assumptions. |
| Primary sources | [NuciCraft.API.slnx](../NuciCraft.API.slnx), project files, [.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml), [.vscode/tasks.json](../.vscode/tasks.json), [.vscode/launch.json](../.vscode/launch.json), and [release.sh](../release.sh) |
| Related documents | [Repository structure](repository-structure.md), [configuration](configuration.md), [startup flow](flows/startup.md), and [testing](testing.md) |

## Toolchain

- Target framework: .NET 10.0.
- Runtime project SDK: `Microsoft.NET.Sdk.Web`.
- Test project SDK: `Microsoft.NET.Sdk`.
- Solution format: `.slnx` containing runtime, unit-test, and integration-test projects.
- Package source and lock policy: ordinary NuGet restoration; no repository lock file is tracked.

A compatible .NET 10 SDK is required for development and CI. A framework-dependent publication requires a compatible runtime on the host unless publish arguments select self-contained output.

## Dependency Restoration

From repository root:

```bash
dotnet restore NuciCraft.API.slnx
```

The CI workflow invokes `dotnet restore` without an explicit solution; repository-root discovery selects the solution.

NuGet assets under `obj/` are generated and excluded from source control.

## Compilation

Compile the complete solution:

```bash
dotnet build NuciCraft.API.slnx --no-restore
```

Compile only the web project:

```bash
dotnet build NuciCraft.API/NuciCraft.API.csproj
```

The VS Code `build` task targets only the web project and adds full-path and concise console logger properties. It does not compile test projects.

Generated outputs reside beneath each project's `bin/` and `obj/` directories and are ignored.

## Testing

Execute all tests:

```bash
dotnet test NuciCraft.API.slnx
```

The test architecture and narrower commands are in [testing.md](testing.md).

## Local Execution

Run from repository root:

```bash
dotnet run --project NuciCraft.API/NuciCraft.API.csproj
```

The application expects effective configuration for store paths and protected keys. Committed placeholders are not automatically interpolated.

The VS Code `watch` task runs:

```bash
dotnet watch run --project NuciCraft.API/NuciCraft.API.csproj
```

The VS Code launch configuration:
- Runs the Debug `net10.0` assembly.
- Uses the runtime project directory as working directory.
- Sets `ASPNETCORE_ENVIRONMENT=Development`.
- Executes the `build` task first.
- Opens the detected listening URL externally.

Working directory matters for default relative `Data/` and log paths.

## Publication

The VS Code `publish` task invokes:

```bash
dotnet publish NuciCraft.API/NuciCraft.API.csproj
```

No runtime identifier, self-contained selection, trimming, single-file option, output directory, archive format, or deployment target is configured. Ordinary .NET publish defaults therefore apply.

`appsettings.json` is marked `CopyToOutputDirectory=PreserveNewest` in the runtime project.

## Continuous Integration

[.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml) is named `.NET` and executes on:
- Push to `master`.
- Pull request targeting `master`.

One Ubuntu job performs:
1. `actions/checkout@v6`.
2. `actions/setup-dotnet@v5` with `10.0.x`.
3. `dotnet restore`.
4. `dotnet build --no-restore`.
5. `dotnet test --no-build --verbosity normal`.

CI does not publish artefacts, produce coverage, scan dependencies, lint Markdown, validate Mermaid, deploy, create releases, or run against multiple operating systems.

## Maintainer Release Process

[release.sh](../release.sh) declares .NET version 10.0, builds a URL under an external `deployment-scripts` repository's mutable `master` branch, downloads it with `wget`, and pipes it to Bash while forwarding script arguments.

Repository-local evidence does not establish what that remote script presently does. Release versioning, compilation, packaging, tagging, publication, credentials, rollback, and destination are external and can change independently.

Before execution, maintainers must retrieve and inspect the exact remote content and assess integrity. The wrapper has no checksum or commit pin.

## Deployment Unit

The deployment unit is one ASP.NET Core process containing every capability. The repository provides no:
- Container file or image definition.
- Docker Compose file.
- Kubernetes manifest.
- Systemd or other service unit.
- Reverse-proxy configuration.
- Cloud infrastructure template.
- Database service or migration package.

Deployment packaging and process supervision are operator decisions.

## Runtime Requirements

| Requirement | Reason |
|-------------|--------|
| Compatible .NET 10 runtime | Execute the framework-dependent web application. |
| Seven writable persistent paths | Store API state. |
| Optional writable log path | Required when file logging is enabled. |
| Protected configuration provider | Supply genuine inbound and outbound API keys. |
| Outbound HTTPS or HTTP to generator | Mob-name capability. |
| Outbound TCP to Minecraft Java port | Live server count. |
| Time-zone data containing `Europe/Bucharest` | Default Zone creation date. |
| Correct working directory or absolute overrides | Resolve relative default paths. |

## Startup And Readiness

Startup creates absent stores and enumerates all repositories before endpoint operation. Failure in any store prevents the entire process from becoming useful.

There is no health, readiness, or liveness endpoint. Process running state alone does not verify generator access, Minecraft access, file writability after startup, or valid credentials.

Operators can infer initial storage readiness from successful process startup, but complete dependency readiness requires external probes.

## Shutdown

No custom graceful-shutdown code exists. ASP.NET Core host lifetime stops the process and disposes the service provider. Mutations save within each request, so no local final flush is scheduled.

In-flight synchronous filesystem or network operations have no local cancellation token. Host shutdown behaviour for them depends on framework and external adapters.

## Environment Differences

Development registers Developer Exception Page in addition to the outer Nuci exception middleware. Other environments omit it. The integration environment uses in-memory overrides, temporary stores, mocked generator and server-status services, and deactivated file logging.

No environment-specific application settings file is tracked.

## Operational Observability

Available:
- Package request logging middleware.
- Service Started, Success, and Failure records.
- Optional log file output.
- HTTP result and process exit state.

Absent:
- Health endpoints.
- Metrics.
- Distributed traces.
- Alert definitions.
- Dashboards.
- Audit store.

Operators must protect logs because context can contain personal and location data.

## Persistent-State Operations

The operator owns:
- Backup and restoration.
- File permission and encryption policy.
- Capacity monitoring.
- Corruption recovery.
- Migration for incompatible schema changes.
- Coordination during deployment and rollback.

Rolling deployments with multiple writable instances are not supported by any local coordination mechanism. A cautious deployment stops writers, preserves stores, deploys one compatible process, and verifies repository materialisation.

## Health And Smoke Verification

No canonical script is tracked. A deployment can verify, with protected credentials and non-destructive requests:
- Process accepts HTTPS requests.
- `GET /Worlds` can read persistence.
- `GET /Server` can return static metadata and live or fallback count.
- A Mob request only when generator connectivity is intentionally included in the smoke scope.

Avoid mutation endpoints for health checks because POST and PATCH are not idempotent.

## Rollback Considerations

Application rollback is uncomplicated only when persisted schemas remain backward compatible. Once a novel version writes incompatible JSON, an older binary can fail startup or reads. No automated migration rollback exists.

Preserve a coordinated data duplicate before schema-changing deployment and validate both forward and reverse compatibility explicitly.
