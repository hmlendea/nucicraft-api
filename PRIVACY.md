# Privacy and Personal Data

This document describes how NuciCraft API at https://github.com/hmlendea/nucicraft-api handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

**Information reviewed:** 2026-10-13

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Self-Hosted Deployments](#self-hosted-deployments)
- [Data We Handle](#data-we-handle)
  - [Data Provided to the Application](#data-provided-to-the-application)
  - [Data Generated or Collected by the Application](#data-generated-or-collected-by-the-application)
  - [Data Received from Integrations](#data-received-from-integrations)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how NuciCraft API at https://github.com/hmlendea/nucicraft-api handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

NuciCraft API is a self-hosted ASP.NET Core application. The instance operator controls the deployment, configuration, local storage, logs, backups, access controls, retention, and request handling. The project maintainers do not operate user instances and do not receive data from self-hosted deployments unless the operator configures external integrations that transmit data.

The application sends data to two external services when configured:
- **Universal Name Generator API**: Outbound bearer-authenticated GET requests for mob name generation. The operator controls the base URL and API key via `universalNameGeneratorSettings`. This integration can be disabled by not configuring the settings or by not calling the `/Mobs` endpoints.
- **Minecraft Java server**: Outbound TCP server-list status queries to the configured hostname and Java port for live online player count. The operator controls the hostname and port via `serverSettings`. This integration can be disabled by not calling the `/Server` endpoint.

No telemetry, update checks, crash reports, email, authentication, reverse-proxy, object-storage, or monitoring integrations are built into the application.

## 📥 Data We Handle

### Data Provided to the Application

- **Player registration data**: username, display name, gender (male/female/other), online UUID, creation timestamp, password, last IP address, skin URL, last slept location (world, coordinates)
- **Player patch data**: email address, Discord ID, gender, ban status (flag, timestamp, reason, actor), mute status (flag, timestamp, reason, actor), last slept location, teleportation request preference
- **Home data**: player identifier, localised names (per-language), location (world, coordinates, pitch, yaw)
- **RTP location data**: username, biome, world, coordinates
- **Country data**: identifier, localised names, localised leader titles, leader name
- **World data**: identifier, localised names, web-map availability flag, spawn point (world, coordinates), world type
- **Zone type data**: identifier, categories, localised names
- **Zone data**: identifier, world, zone type, localised names, bounds (two corners with world and coordinates), population, creation date
- **Item data**: identifier, Minecraft ID, NuciCraft ID, Bukkit ID, sign IDs

### Data Generated or Collected by the Application

- **Generated identifiers**: random GUIDs for homes and items
- **Timestamps**: creation and update timestamps (UTC) for homes, items, and other entities
- **Sign IDs**: generated from NuciCraft IDs using custom replacements and length limits (e.g., `minecraft:diamond_sword` → `DiamondSword`)
- **Operational logs**: structured NuciLog records containing usernames, UUIDs, IP addresses, coordinates, identifiers, and operation context (when file output is enabled via `nuciLoggerSettings`)

### Data Received from Integrations

- **Universal Name Generator API**: generated mob names (no personal data)
- **Minecraft Java server**: online player count integer (no personal data)

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- Player registration, retrieval, listing, and patching — player registration and patch data
- Home creation, retrieval, filtering, patching, and deletion — home data and player identifiers
- RTP location addition and random selection — RTP location data
- Country, world, zone type, and zone metadata management — respective metadata
- Item metadata management and sign ID generation — item data
- Server information retrieval with live player count — server settings and Java server status
- Mob name generation — mob type and count (no personal data)
- Operation logging — all data categories above when logging is enabled

## 🗄️ Storage, Retention, and Deletion

| Data Category | Storage Location | Retention and Deletion |
|---------------|------------------|------------------------|
| Player records | JSON file at `dataStoreSettings.playersStorePath` (default `Data/players.json`) | Persisted until explicitly deleted via API; no automatic retention policy |
| Home records | JSON file at `dataStoreSettings.homesStorePath` (default `Data/homes.json`) | Persisted until explicitly deleted via API; no automatic retention policy |
| RTP location records | JSON file at `dataStoreSettings.rtpLocationsStorePath` (default `Data/rtp_locations.json`) | Append-only; no deletion endpoint; no automatic retention policy |
| Country records | JSON file at `dataStoreSettings.countriesStorePath` (default `Data/countries.json`) | Persisted until explicitly deleted via API; no automatic retention policy |
| World records | JSON file at `dataStoreSettings.worldsStorePath` (default `Data/worlds.json`) | Persisted until explicitly deleted via API; no automatic retention policy |
| Zone type records | JSON file at `dataStoreSettings.zoneTypesStorePath` (default `Data/zone_types.json`) | Persisted until explicitly deleted via API; no automatic retention policy |
| Zone records | JSON file at `dataStoreSettings.zonesStorePath` (default `Data/zones.json`) | Persisted until explicitly deleted via API; no automatic retention policy |
| Item records | JSON file at `dataStoreSettings.itemsStorePath` (default `Data/items.json`) | Persisted until explicitly deleted via API; no automatic retention policy |
| Operational logs | File at `nuciLoggerSettings.logFilePath` (default `logfile.log`) when `nuciLoggerSettings.isFileOutputEnabled` is `true` | Controlled by operator; no automatic rotation or retention policy |

For self-hosted deployments, the instance operator controls storage paths, file permissions, backups, and deletion. The project does not manage or access operator storage.

## 🔗 External Processing and Integrations

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| Universal Name Generator API | Generate random mob names | Mob type schema, requested count, bearer token (API key) | `universalNameGeneratorSettings.baseUrl`, `universalNameGeneratorSettings.apiKey` in [appsettings.json](./NuciCraft.API/appsettings.json) |
| Minecraft Java server | Query live online player count via server-list status protocol | Configured hostname and Java port; returns integer count | `serverSettings.hostname`, `serverSettings.javaEditionPort` in [appsettings.json](./NuciCraft.API/appsettings.json) |

The application has no other built-in external data transfers.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at [PRIVACY.md](./PRIVACY.md).

## 📬 Contact

For questions about application data handling, contact the project maintainers via [GitHub Issues](https://github.com/hmlendea/nucicraft-api/issues). For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the deployment identifier or repository reference; do not send passwords, access tokens, or other secrets.