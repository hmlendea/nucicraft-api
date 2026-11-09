# Configuration

| Metadata | Value |
|----------|-------|
| Purpose | Inventory every configuration source, key, default, precedence rule, consumer, and runtime consequence. |
| Scope | ASP.NET Core host configuration, six local settings classes, NuciLog settings, environment naming, and operational path semantics. |
| Primary sources | [appsettings.json](../NuciCraft.API/appsettings.json), [Configuration](../NuciCraft.API/Configuration/), [ServiceCollectionExtensions.cs](../NuciCraft.API/ServiceCollectionExtensions.cs), and [Program.cs](../NuciCraft.API/Program.cs) |
| Related documents | [Host component](components/host-and-composition.md), [interfaces](interfaces.md), [security](security.md), and [build and deployment](build-and-deployment.md) |

## Configuration Sources And Precedence

`Program.CreateHostBuilder` uses `Host.CreateDefaultBuilder`, so the standard ASP.NET Core providers apply. From lower to higher precedence, the material providers are:
1. [appsettings.json](../NuciCraft.API/appsettings.json).
2. `appsettings.{Environment}.json` when present. No environment-specific file is tracked.
3. Environment variables.
4. Command-line arguments.

The host can include additional framework sources depending on environment, such as development user secrets only when separately configured by the framework and project metadata. This repository does not declare a user-secrets identifier.

Configuration keys are case-insensitive. Environment variables use double underscores for section separators, for example `serverSettings__javaEditionPort`.

## Binding And Lifetime

`ServiceCollectionExtensions.AddConfigurations`:
1. Constructs each local settings class.
2. Calls `configuration.Bind(nameof(localVariable), instance)`.
3. Registers the instance as a singleton.

NuciLog settings are registered through package extension `AddNuciLoggerSettings(configuration)`.

The project does not use `IOptions<T>`, validation attributes, `ValidateOnStart`, or `IOptionsMonitor`. Consumers receive mutable singleton objects, but no supported reload path revises them after registration.

## Data Store Settings

Consumer type: [DataStoreSettings.cs](../NuciCraft.API/Configuration/DataStoreSettings.cs).

| Key | Type | Committed Default | Required | Consumers And Consequence |
|-----|------|-------------------|----------|---------------------------|
| `dataStoreSettings.countriesStorePath` | string | `Data/countries.json` | Yes at startup | Country repository, store creation, eager load. |
| `dataStoreSettings.homesStorePath` | string | `Data/homes.json` | Yes at startup | Home repository, store creation, eager load. |
| `dataStoreSettings.itemsStorePath` | string | `Data/items.json` | Yes at startup | Item repository, store creation, eager load. |
| `dataStoreSettings.playersStorePath` | string | `Data/players.json` | Yes at startup | Player repository, store creation, eager load. |
| `dataStoreSettings.rtpLocationsStorePath` | string | `Data/rtp_locations.json` | Yes at startup | RTP repository, store creation, eager load. |
| `dataStoreSettings.worldsStorePath` | string | `Data/worlds.json` | Yes at startup | World repository, store creation, eager load. |
| `dataStoreSettings.zonesStorePath` | string | `Data/zones.json` | Yes at startup | Zone repository, store creation, eager load. |
| `dataStoreSettings.zoneTypesStorePath` | string | `Data/zone_types.json` | Yes at startup | ZoneType repository, store creation, eager load. |

Relative paths resolve from the process working directory. Every path must include a usable parent-directory result for current startup logic. All stores must be readable and writable for full capability operation.

The default `Data/` directory is excluded by [.gitignore](../.gitignore). Runtime state must not be committed.

## Server Settings

Consumer type: [ServerSettings.cs](../NuciCraft.API/Configuration/ServerSettings.cs).

| Key | Type | Committed Default | Required | Consequence |
|-----|------|-------------------|----------|-------------|
| `serverSettings.name` | string | `NuciCraft` | Not locally validated | Returned by `GET /Server`. |
| `serverSettings.hostname` | string | `mc.nucilandia.ro` | Required for status query | Returned to clients and used as Java TCP destination. |
| `serverSettings.javaEditionPort` | int | `25565` | Must be 1 through 65535 when queried | Returned and queried through MineStat. |
| `serverSettings.bedrockEditionPort` | int | `19132` | Not locally validated | Returned only; no Bedrock query. |

These values describe the Minecraft server. They do not configure Kestrel listening addresses or ports.

## RTP Location Settings

Consumer type: [RtpLocationSettings.cs](../NuciCraft.API/Configuration/RtpLocationSettings.cs).

| Key | Type | Committed Default | Runtime Use |
|-----|------|-------------------|-------------|
| `rtpLocationSettings.minimumLocationDistance` | int | `200` | General same-World horizontal exclusion radius. |
| `rtpLocationSettings.minimumBiomeLocationDistance` | int | `500` | Same-Biome and same-World horizontal exclusion radius. |

No startup validation checks positive values or relative ordering. The service squares values, so a negative value behaves like its absolute magnitude. Existing records are not revalidated when thresholds change.

## Web Map Settings

Consumer type: [WebMapSettings.cs](../NuciCraft.API/Configuration/WebMapSettings.cs).

| Key | Type | Committed Default | Runtime Use |
|-----|------|-------------------|-------------|
| `webMapSettings.baseUrl` | string | Deployment placeholder | Prefix for read-time Zone MapLink derivation. |

The service does not validate BaseUrl, insert a separator, or call the resulting URL. Configuration must include the intended path form. The service appends `?worldname=...&x=...&z=...` directly.

## Security Settings

Consumer type: [SecuritySettings.cs](../NuciCraft.API/Configuration/SecuritySettings.cs).

| Key | Type | Committed Default | Runtime Use |
|-----|------|-------------------|-------------|
| `securitySettings.apiKey` | string | Deployment placeholder | Expected service-wide API key passed by every controller to `ProcessRequest`. |

The double-bracket deployment marker is an ordinary literal string. ASP.NET Core does not interpolate that marker syntax. Deployments must override it through a protected provider. The repository performs no local key-strength, vacancy, rotation, or multi-key validation.

## Universal Name Generator Settings

Consumer type: [UniversalNameGeneratorSettings.cs](../NuciCraft.API/Configuration/UniversalNameGeneratorSettings.cs).

| Key | Type | Committed Default | Required | Runtime Use |
|-----|------|-------------------|----------|-------------|
| `universalNameGeneratorSettings.baseUrl` | string | Deployment placeholder | Yes for Mob calls | Constructs singleton `NuciApiClient` and is checked for whitespace by `MobService`. |
| `universalNameGeneratorSettings.apiKey` | string | Deployment placeholder | Yes for Mob calls | Outbound bearer token; checked for whitespace. |

Literal placeholders are non-whitespace and can pass local service validation. A genuine deployment must override both. Base URL validity can fail during client construction or request execution depending on external client behaviour.

## Access Control Settings

Consumer type: [AccessControlSettings.cs](../NuciCraft.API/Configuration/AccessControlSettings.cs).

| Key | Type | Committed Default | Required | Runtime Use |
|-----|------|-------------------|----------|-------------|
| `accessControlSettings.deniedCountries` | string[] | See [appsettings.json](../NuciCraft.API/appsettings.json) | No | Checked by `AccessControlService.PerformIpCheck` against geolocation country code. |
| `accessControlSettings.deniedUsernamePatterns` | string[] | See [appsettings.json](../NuciCraft.API/appsettings.json) | No | Case-sensitive regex patterns checked by `AccessControlService.PerformUsernameCheck`. |
| `accessControlSettings.geolocationApiUrl` | string | `http://ip-api.com/json/` | No | Base URL for geolocation API calls; IP address and query fields appended. |
| `accessControlSettings.minimumUsernameLength` | int | `4` | No | Minimum username length enforced by `AccessControlService.PerformUsernameCheck`. |
| `accessControlSettings.isRegisteredPlayerExemptionEnabled` | bool | `true` | No | When enabled, an IP check that supplies the username of a registered, non-banned player is permitted without the geolocation, denied-country, and anonymisation-service checks. |
| `accessControlSettings.anonymisationServiceDetection.provider` | string | `proxycheck` | No | Selects the upstream detection provider (`proxycheck` or `funkemunky`). An unsupported value raises `NotSupportedException` when a detector is created. |
| `accessControlSettings.anonymisationServiceDetection.isEnabled` | bool | `true` | No | When disabled, `AccessControlService` skips the detection request entirely and permits the request. |
| `accessControlSettings.anonymisationServiceDetection.isCacheEnabled` | bool | `true` | No | Enables the in-process detection result cache. When disabled, every check performs a live detection request. |
| `accessControlSettings.anonymisationServiceDetection.cacheDurationHoursWhenDetected` | int | `6` | No | Lifetime of a cached detection result that reported an anonymisation service. |
| `accessControlSettings.anonymisationServiceDetection.cacheDurationHoursWhenNotDetected` | int | `2` | No | Lifetime of a cached detection result that reported no anonymisation service. |
| `accessControlSettings.anonymisationServiceDetection.cacheDurationHoursForExpiredFallback` | int | `6` | No | Additional retention of an expired detection result, so that it remains available as a fallback when a live refresh fails. |

The geolocation API is called with `fields=status,countryCode,country` appended to the constructed URL. A non-success status or missing country code results in an allowed response (fail-open). Denied country comparison is case-insensitive. Username pattern matching uses `Regex.IsMatch` without `IgnoreCase`.

### Registered Player Exemption

When `accessControlSettings.isRegisteredPlayerExemptionEnabled` is `true` and the IP check supplies a non-whitespace `username` that matches a registered, non-banned player, the check is permitted immediately, with `countryCode` and `deniedReason` both `null`, and no geolocation or anonymisation-service request is made.

The same setting also governs the username check. When enabled, a registered, non-banned username is permitted without evaluating the minimum length or denied-pattern rules. When disabled, a registered username is not permitted early, so those rules apply. A registered, banned username is denied in both cases.

When the username is absent or whitespace, no player record matches, or the matching record is banned, the IP check proceeds to the banned-player address denial and then the regular checks. Player username comparison is ordinal and case-sensitive, so a differently cased username does not match a registered player.

The lookup reads the player store through the same repository used by [PlayerService](components/player-and-homes.md), so an unregistered username does not produce a failure log.

### Banned Player Denial

An IP address recorded as the last IP address of any banned player is denied by default. The denial runs after the registered-username checks and before the geolocation, denied-country, and anonymisation-service checks, so no outbound request is performed for such an address.

The denial reason is `This IP address is banned on {serverName}` when the request supplies no username, and `This IP address is associated with a banned user on {serverName}` when it supplies one. The server name comes from `serverSettings.name`.

A request that supplies the username of a registered, non-banned player is permitted before the denial applies, even when the same address is recorded for a banned player. A request that supplies the username of a registered, banned player is denied earlier, with the associated-with-a-banned-user reason.

The comparison uses the persisted `LastIpAddress` field with ordinal, case-sensitive equality.

### Banned Username Denial

The username check consults the player store before the pattern and length rules. A registered, banned username is denied with `This user is banned. {banReason}`, where the ban reason comes from the persisted `BannedReason` field. A missing or whitespace ban reason becomes `No ban reason was recorded.`

An unregistered username is then evaluated against `accessControlSettings.minimumUsernameLength` and `accessControlSettings.deniedUsernamePatterns`. A registered, non-banned username is permitted unless `isRegisteredPlayerExemptionEnabled` is disabled.

### Anonymisation Service Detection Cache

`CachedAnonymisationServiceDetector` stores one entry per IP address in the in-process `IMemoryCache` under the `anonymisation:` key prefix. A cached result is served without a live request until its lifetime elapses.

| Detection outcome | Cache lifetime |
|------------------|----------------|
| Anonymisation service detected | `cacheDurationHoursWhenDetected` hours. |
| No anonymisation service detected | `cacheDurationHoursWhenNotDetected` hours. |

When a cached result has expired, a live detection request is made. A successful request replaces the stale entry with a new result and a new lifetime. A failed request leaves the stale entry in place and serves it as a fallback, so that a provider outage does not change the access decision. When no entry exists, a failed request is propagated to `AccessControlService`, which logs the failure and permits the request.

## NuciLog Settings

The settings type belongs to NuciLog. Locally configured keys are:

| Key | Type | Committed Default | Runtime Use |
|-----|------|-------------------|-------------|
| `nuciLoggerSettings.logFilePath` | string | `logfile.log` | File destination when enabled. |
| `nuciLoggerSettings.isFileOutputEnabled` | bool | `true` | Activates package file output. |

The repository does not configure retention, rotation, redaction, structured sink transport, or flush policy. The process must possess write permission when file output is active.

## ASP.NET Core Environment

`ASPNETCORE_ENVIRONMENT=Development` causes `Startup.Configure` to register the Developer Exception Page. Other values omit it. The integration host selects `IntegrationTesting` and disables file logger output through in-memory overrides.

No capability feature flags exist. The only behaviour toggle is `accessControlSettings.isRegisteredPlayerExemptionEnabled`, which switches the registered-player exemption of the IP check.

## Environment Variable Examples

The complete environment-key forms are:

```text
dataStoreSettings__countriesStorePath
dataStoreSettings__homesStorePath
dataStoreSettings__itemsStorePath
dataStoreSettings__playersStorePath
dataStoreSettings__rtpLocationsStorePath
dataStoreSettings__worldsStorePath
dataStoreSettings__zonesStorePath
dataStoreSettings__zoneTypesStorePath
serverSettings__name
serverSettings__hostname
serverSettings__javaEditionPort
serverSettings__bedrockEditionPort
rtpLocationSettings__minimumLocationDistance
rtpLocationSettings__minimumBiomeLocationDistance
webMapSettings__baseUrl
securitySettings__apiKey
universalNameGeneratorSettings__baseUrl
universalNameGeneratorSettings__apiKey
nuciLoggerSettings__logFilePath
nuciLoggerSettings__isFileOutputEnabled
accessControlSettings__deniedCountries
accessControlSettings__deniedUsernamePatterns
accessControlSettings__geolocationApiUrl
accessControlSettings__minimumUsernameLength
accessControlSettings__isRegisteredPlayerExemptionEnabled
accessControlSettings__anonymisationServiceDetection__provider
accessControlSettings__anonymisationServiceDetection__isEnabled
accessControlSettings__anonymisationServiceDetection__isCacheEnabled
accessControlSettings__anonymisationServiceDetection__cacheDurationHoursWhenDetected
accessControlSettings__anonymisationServiceDetection__cacheDurationHoursWhenNotDetected
accessControlSettings__anonymisationServiceDetection__cacheDurationHoursForExpiredFallback
```

This list documents names only and intentionally contains no secret values.

## Command-Line Overrides

The default provider permits key-value overrides using framework syntax, for example:

```bash
dotnet run --project NuciCraft.API/NuciCraft.API.csproj -- --serverSettings:javaEditionPort=25565
```

No custom command-line parser or application-specific options exist.

## Configuration Validation Timing

| Setting Group | First Effective Validation |
|---------------|----------------------------|
| Store paths | Startup `CreateStoreIfMissing` and repository eager load. |
| Server host and Java port | First `GetOnlinePlayersCount` call. |
| Bedrock port and server name | None locally. |
| RTP distances | None locally; arithmetic at first addition. |
| Web map BaseUrl | None locally; concatenation during eligible Zone read. |
| Inbound API key | Package-owned request processing. |
| Generator BaseUrl | External client construction or request, plus whitespace check in MobService. |
| Generator API key | MobService whitespace check. |
| Logger settings | External package registration or first write. |

## Interactions

- Zone MapLink enrichment requires both valid WebMap BaseUrl and a World record with HasWebMap true.
- Server Hostname and Java port are both advertised and queried, so an override changes public metadata and network destination together.
- RTP biome threshold is evaluated only after the general threshold; a larger biome default creates the intended stronger same-biome spacing.
- Logger path can share the process working-directory sensitivity of relative data paths.
- Store path changes point repositories to completely different state; no data transfer occurs automatically.

## Reload And Restart

Bound instances are constructed during service registration. The supported interpretation is that configuration changes require process restart. No monitor, callback, or mutable refresh service is registered.

## Secret Handling

API keys must originate from a protected deployment source and must not be committed. The repository's placeholders are not credentials. Avoid command-line secret values because process listings and shell history can expose them. Environment variables or an external protected configuration provider are more appropriate, subject to deployment policy.
