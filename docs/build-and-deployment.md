# Build, Deployment, And Operations

| Metadata | Value |
|----------|-------|
| Purpose | Document restoration, compilation, test, publication, CI, release, runtime requirements, startup, shutdown, and operator responsibilities. |
| Scope | Repository-provided development and delivery mechanisms plus evidenced deployment assumptions. |
| Primary sources | [NuciCraft.API.slnx](../NuciCraft.API.slnx), project files, [.github/workflows/dotnet.yml](../.github/workflows/dotnet.yml), [.github/workflows/security.yml](../.github/workflows/security.yml), [.github/workflows/github-release.yml](../.github/workflows/github-release.yml), [.vscode/tasks.json](../.vscode/tasks.json), and [.vscode/launch.json](../.vscode/launch.json) |
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

CI does not publish artefacts, produce coverage, lint Markdown, validate Mermaid, deploy, create releases, or run against multiple operating systems.

## Security Scanning

[.github/workflows/security.yml](../.github/workflows/security.yml) is named `Security` and executes on:
- Push to `master`.
- Pull request targeting `master`.
- A weekly schedule at `0 0 * * 0`.

One Ubuntu job performs:
1. `actions/checkout@v7`.
2. `actions/setup-dotnet@v5` with `10.0.x`.
3. `dotnet restore`.
4. `dotnet list package --vulnerable --include-transitive`.
5. CodeQL initialisation for `csharp` through `github/codeql-action/init@v4`.
6. `dotnet build --no-restore`.
7. CodeQL analysis through `github/codeql-action/analyze@v4`.

The job grants `security-events: write` and `contents: read`. The write scope is required because the analysis step uploads SARIF results to GitHub Code Scanning. With a read scope, the upload fails on push and schedule runs with `Resource not accessible by integration`, while pull request runs still complete.

The security workflow does not publish artefacts, produce coverage, execute tests, lint Markdown, validate Mermaid, deploy, create releases, or run against multiple operating systems.

## Release Automation

No repository-local release command exists. [.github/workflows/github-release.yml](../.github/workflows/github-release.yml) is named `GitHub Release`, is the sole release entry point, and executes on:
- A published GitHub release.

One Ubuntu job performs:
1. `actions/checkout@v7` with full history.
2. Reading `<TargetFramework>` from [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj) and stripping the `net` prefix, yielding the major and minor .NET version. The job fails when the element is absent.
3. `actions/setup-dotnet@v5` with `<derived version>.x`.
4. Version extraction from `github.event.release.tag_name` with a leading `v` stripped.
5. Rewriting the `origin` URL to `https://github.com/<owner>/<repository>.git`, because the downloaded helper derives its artefact name prefix from `git remote -v` and expects a `.git` suffix, while `actions/checkout` records a suffix-free HTTPS URL.
6. Downloading `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/<derived version>.sh` with `wget` and piping it to Bash with the version as its argument. For the tracked `net10.0` target framework, the URL is `.../dotnet/10.0.sh`.
7. The helper publishing self-contained output for `linux-arm`, `linux-arm64`, `linux-x64`, `osx-arm64`, `osx-x64`, `win-arm64`, and `win-x64`, and packaging each runtime identifier into `NuciCraft.API/bin/Release/`.
8. Upload of those artefacts to the published release through `softprops/action-gh-release@v2`.

The job grants `contents: write` and uses the automatic `GITHUB_TOKEN`; no additional secret is configured. The workflow requires an existing published release because it does not create tags or releases. The .NET version is not hard-coded: changing `<TargetFramework>` in [NuciCraft.API.csproj](../NuciCraft.API/NuciCraft.API.csproj) changes both the installed SDK and the helper URL, provided a matching helper exists in the external repository.

The workflow does not test, produce coverage, lint Markdown, validate Mermaid, deploy, or run against multiple operating systems.

### Release Trust Boundary

The helper is fetched from the external `dotnet-release-scripts` repository's mutable `master` branch, is not pinned by commit or checksum, and is piped directly into Bash while forwarding script arguments. Repository-local evidence does not establish what that remote content presently does. Release compilation arguments, packaging layout, artefact naming, and destination are external and can change independently, and the workflow's artefact glob only matches output while the remote naming convention remains unchanged. Reviewers must retrieve and inspect the exact remote content and assess integrity before relying on a release.

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
| Eight writable persistent paths | Store API state. |
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
