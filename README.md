[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucicraft-api)](https://github.com/hmlendea/nucicraft-api/releases/latest)
[![Build Status](https://github.com/hmlendea/nucicraft-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucicraft-api/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/github/license/hmlendea/nucicraft-api)](https://github.com/hmlendea/nucicraft-api/blob/master/LICENSE)

# NuciCraft API

NuciCraft API is a lightweight ASP.NET Core REST service for NuciCraft Minecraft server operations, including player registration and updates, RTP location management, world, country, and zone management, and mob name generation.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Usage](#usage)
  - [Get Server Information](#get-server-information)
  - [Register a Player](#register-a-player)
  - [Get a Player by ID](#get-a-player-by-id)
  - [Get a Player by Username](#get-a-player-by-username)
  - [Get a Player by Offline UUID](#get-a-player-by-offline-uuid)
  - [Get a Player by Online UUID](#get-a-player-by-online-uuid)
  - [Get All Players](#get-all-players)
  - [Patch a Player](#patch-a-player)
  - [Manage Homes](#manage-homes)
  - [Add an RTP Location](#add-an-rtp-location)
  - [Get a Random RTP Location](#get-a-random-rtp-location)
  - [Get Random Mob Names](#get-random-mob-names)
  - [Manage Countries](#manage-countries)
  - [Manage Worlds](#manage-worlds)
  - [Manage Zone Types](#manage-zone-types)
  - [Manage Zones](#manage-zones)
  - [Manage Items](#manage-items)
    - [Sign ID Generation](#sign-id-generation)
  - [Access Control](#access-control)
- [Known Limitations](#known-limitations)
- [Installation](#installation)
  - [Installation from Source](#installation-from-source)
- [Configuration](#configuration)
- [Development](#development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
  - [Test](#test)
  - [Release](#release)
  - [Dependencies](#dependencies)
- [GitHub Actions](#github-actions)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [Code of Conduct](#code-of-conduct)
- [Security](#security)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

- Registers, retrieves, and updates players via protected API endpoints
- Retrieves the configured server name, hostname, Java and Bedrock edition ports, and live Java online player count
- Persists independently patchable player preferences, including teleportation-request reception
- Creates, retrieves, and patches player homes with localised names, coordinates, generated identifiers, and per-player name uniqueness
- Stores and retrieves RTP locations with distance constraints and biome/world filtering
- Generates random mob names via Universal Name Generator integration
- Stores, retrieves, and updates world metadata, including type, web-map availability, and spawn points
- Stores, retrieves, and updates localised zone type metadata
- Stores, retrieves, and updates country metadata
- Stores, retrieves, updates, and deletes zone metadata
- Validates zone bounds so both corners share the identical world and are necessary on zone creation
- Assigns a default zone creation date when one is not provided by the caller
- Stores, retrieves, and updates item metadata with Minecraft, Bukkit, and NuciCraft identifiers
- Generates sign-compatible identifiers from NuciCraft IDs with custom replacements and length limits
- Validates IP addresses against denied countries using real-time geolocation lookup
- Validates usernames against denied patterns with case-sensitive regex matching and minimum length enforcement

## 🚀 Usage

All endpoints are rooted at your configured host, for example `http://localhost:5000`.

Requests are protected by API-key authorisation configured through `securitySettings.apiKey`.

### Get Server Information

```bash
curl "http://localhost:5000/Server" \
	-H "Authorization: Bearer ${NUCICRAFT_API_KEY}"
```

The response contains `name`, `hostname`, `javaEditionPort`, `bedrockEditionPort`, and the integer `onlinePlayersCount` within the standard `content` envelope. These values describe the Minecraft server, not the API listener.

Each authorised request queries the configured Java hostname and port using the Java legacy server-list status protocol, supported by modern Java servers. `onlinePlayersCount` is the live server-reported count, not a configured value, registered-player total, player-list sample, or Bedrock query result. No count is cached. Proxies or plugins that customise status responses may alter the reported count.

The API requires TCP access to that Java port and server-list status responses must be active. Connection and socket operations use a five-second timeout. If the Java server is unavailable, the connection fails, or no status is available, the endpoint still succeeds and returns `onlinePlayersCount: 0` alongside the configured server details. A successful response reporting zero players also returns `0`, so this value does not distinguish an offline server from an online server without players. Invalid configuration and player-count parsing errors still fail the request.

### Register a Player

```bash
curl -X POST "http://localhost:5000/Players" \
	-H "Content-Type: application/json" \
	-d '{
		"username": "PlayerName",
		"displayName": "Player Name",
		"gender": "female",
		"onlineUUID": "6f6f5f2d-6f7e-4f6c-8e1d-03a9b8d939f0",
		"createdDT": "2026-04-01T12:00:00+00:00",
		"password": "example-password",
		"lastIpAddress": "127.0.0.1",
		"skinUrl": "https://example.com/skin.png",
		"lastSleptLocation": {
			"world": "world",
			"x": 64,
			"y": 72,
			"z": 128
		}
	}'
```

`gender` accepts `male`, `female`, or `other`. Omitted or empty values default to `other`.

### Get a Player by ID

```bash
curl "http://localhost:5000/Players/8b9d0f3e-2cc2-4d67-93dd-5c5270b19d4c"
```

### Get a Player by Username

```bash
curl "http://localhost:5000/Players/by-username/PlayerName"
```

### Get a Player by Offline UUID

```bash
curl "http://localhost:5000/Players/by-offline-uuid/61300000-0000-3000-8000-000000000000"
```

### Get a Player by Online UUID

```bash
curl "http://localhost:5000/Players/by-online-uuid/87300000-0000-0000-0000-000000000000"
```

### Get All Players

```bash
curl "http://localhost:5000/Players"
```

### Patch a Player

```bash
curl -X PATCH "http://localhost:5000/Players/by-username/PlayerName" \
  -H "Content-Type: application/json" \
  -d '{
    "emailAddress": "player@example.com",
    "discordId": "1234567890",
		"gender": "male",
		"isBanned": true,
		"bannedDT": "2026-09-18T16:53:36.0000649+03:00",
		"bannedReason": "Repeated rule violations",
		"bannedBy": "ModeratorName",
		"isMuted": true,
		"mutedDT": "2026-09-18T16:53:36.0000649+03:00",
		"mutedReason": "Excessive chat messages",
		"mutedBy": "ModeratorName",
		"lastSleptLocation": {
			"world": "world",
			"x": 64,
			"y": 72,
			"z": 128
		},
    "settings": {
      "teleportationRequestsAreEnabled": false
    }
  }'
```

`teleportationRequestsAreEnabled` defaults to `true` for newly registered players. Omitting it from a settings patch preserves the persisted value.

### Manage Homes

All Homes routes require API-key authorisation:

| Method | Route | Result |
|--------|-------|--------|
| `POST` | `/Homes` | Create a home; `player` in the JSON body is an existing player's identifier. |
| `DELETE` | `/Homes/{id}` | Delete a home by its generated identifier. |
| `PATCH` | `/Homes/{id}` | Patch a home; an optional `player` in this body is an existing player's identifier. |
| `GET` | `/Homes/{id}` | Retrieve one home by its generated identifier. |
| `GET` | `/Homes` | Retrieve every home. |
| `GET` | `/Homes/by-player/{playerIdentifier}` | Retrieve a player's homes using the player identifier. |
| `GET` | `/Homes?player={playerIdentifier}` | Retrieve a player's homes using a query parameter. |
| `GET` | `/Homes?player={playerIdentifier}&name={name}` | Retrieve one home by any of its localised names. URL-encode the name. |

```bash
curl -X POST "http://localhost:5000/Homes" \
	-H "Authorization: Bearer ${NUCICRAFT_API_KEY}" \
	-H "Content-Type: application/json" \
	-d '{
		"player": "'"${PLAYER_ID}"'",
		"name": { "english": "Astora", "romanian": "Anor Londo" },
		"location": { "world": "world", "x": 42, "y": 64, "z": 613, "pitch": 0, "yaw": 179.9 }
	}'
```

The API validates and stores `player` as the player's identifier, never their username or Minecraft UUID. It generates the home's random GUID (`id`) and UTC `createdDT`; `updatedDT` is null until the first patch and is generated by the API thereafter. Creation and patch request contracts contain no writable timestamps, and clients cannot supply or replace the home identifier.

POST and PATCH return the resulting home within `content`. DELETE returns the standard success response and permanently removes the home. Single GET responses use the identical home contract; collection responses contain `content.homes` and `content.count`. Home properties are `id`, `createdDT`, `updatedDT`, `name`, `player`, and `location`.

```bash
curl -X PATCH "http://localhost:5000/Homes/${HOME_ID}" \
	-H "Authorization: Bearer ${NUCICRAFT_API_KEY}" \
	-H "Content-Type: application/json" \
	-d '{"name":{"romanian":"Firelink Shrine"},"location":{"world":"world","x":96,"y":64,"z":128}}'
```

Name patches merge supplied translations and preserve omitted translations. A supplied location replaces the complete coordinates object; `world` is required and numeric coordinates and orientation must be finite. Omitted or null patch properties preserve the stored values. Changing `player` requires a player identifier and rechecks name uniqueness for that player.

Each home requires at least one non-whitespace localised name. Comparisons ignore case and surrounding whitespace across every translation: two homes owned by one player cannot share any localised name, even in different languages. Identical names are permitted for different players, and each player may possess multiple differently named homes. Duplicate creation, renaming, and transfers return `400` without altering the existing home. Absent homes or referenced players return `404`; list queries without results return an empty collection.

### Add an RTP Location

```bash
curl -X POST "http://localhost:5000/RtpLocations" \
	-H "Content-Type: application/json" \
	-d '{
		"username": "PlayerName",
		"biome": "plains",
		"world": "world",
		"x": 123,
		"y": 70,
		"z": -456
	}'
```

### Get a Random RTP Location

```bash
curl "http://localhost:5000/RtpLocations/random?username=PlayerName&world=world&biome=plains"
```

### Get Random Mob Names

The optional `count` query parameter defaults to `1`.

```bash
curl "http://localhost:5000/Mobs/wandering_trader/random-name?count=4"
```

### Manage Countries

```bash
curl -X POST "http://localhost:5000/Countries" \
	-H "Content-Type: application/json" \
	-d '{
		"identifier": "nucilandia",
		"name": {
			"english": "Nucilandia",
			"romanian": "Nucilandia"
		},
		"leaderTitle": {
			"english": "Great Walnut",
			"romanian": "Marele Nuc"
		},
		"leader": "Hori"
	}'
```

```bash
curl "http://localhost:5000/Countries/nucilandia"
curl "http://localhost:5000/Countries"
```

```bash
curl -X PATCH "http://localhost:5000/Countries/nucilandia" \
	-H "Content-Type: application/json" \
	-d '{
		"leader": "Horațiu"
	}'
```

### Manage Worlds

```bash
curl -X POST "http://localhost:5000/Worlds" \
	-H "Content-Type: application/json" \
	-d '{
		"id": "main",
		"name": {
			"english": "Main World",
			"romanian": "Lumea Principala"
		},
		"hasWebMap": true,
		"spawnPoint": {
			"world": "main",
			"x": 64,
			"y": 72,
			"z": 128
		},
		"type": "overworld"
	}'
```

`type` accepts `overworld`, `nether`, or `end` and defaults to `overworld`. `hasWebMap` defaults to `false`, while `spawnPoint` is optional.

```bash
curl "http://localhost:5000/Worlds/main"
curl "http://localhost:5000/Worlds"
```

```bash
curl -X PATCH "http://localhost:5000/Worlds/main" \
	-H "Content-Type: application/json" \
	-d '{
		"name": {
			"romanian": "Lumea Centrala"
		},
		"hasWebMap": false,
		"spawnPoint": {
			"world": "main",
			"x": 96,
			"y": 64,
			"z": 128
		}
	}'
```

### Manage Zone Types

```bash
curl -X POST "http://localhost:5000/ZoneTypes" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "city",
    "categories": [
      "settlement",
      "civilian"
    ],
    "name": {
      "english": "City",
      "romanian": "Oraș"
    }
  }'
```

```bash
curl "http://localhost:5000/ZoneTypes/city"
curl "http://localhost:5000/ZoneTypes"
curl "http://localhost:5000/ZoneTypes?category=settlement"
```

```bash
curl -X PATCH "http://localhost:5000/ZoneTypes/city" \
  -H "Content-Type: application/json" \
  -d '{
    "categories": [
      "capital",
      "fortified"
    ],
    "name": {
      "romanian": "Oraș Mare"
    }
  }'
```

Zone types include string categories and localised names. The optional `category` query parameter filters the collection case-insensitively and returns zone types containing that category. Patch requests replace supplied categories and merge supplied localisations with persisted values.

### Manage Zones

When creating a zone, `world` is necessary and must reference an existing world identifier from the worlds store.

When creating a zone, `type` is necessary and must reference an existing zone type identifier from the zone types store. On patch requests, a supplied `type` must also reference an existing zone type.

The optional `type` query parameter on `GET /Zones` filters the collection by zone type identifier, case-insensitively. For example, `GET /Zones?type=city` returns only zones whose type is `city`.

When creating a zone, `bounds` is necessary and must contain both opposite corners of the zone volume. Both corners must use the identical `world` value. On patch requests, `bounds` remains optional, and a partial bounds update preserves the untouched corner before validation.

When creating a zone, `creationDate` is optional. If omitted or whitespace, the service sets it automatically to the current Romania date with an uncertainty suffix in the format `yyyy-MM-dd (?)`.

```bash
curl -X POST "http://localhost:5000/Zones" \
	-H "Content-Type: application/json" \
	-d '{
		"id": "spawn-city",
		"world": "main",
		"type": "city",
		"name": {
			"en": "Spawn City"
		},
		"bounds": {
			"firstCorner": {
				"world": "world",
				"x": 32,
				"y": 64,
				"z": -48
			},
			"secondCorner": {
				"world": "world",
				"x": 96,
				"y": 96,
				"z": 24
			}
		},
		"population": 120
	}'
```

```bash
curl "http://localhost:5000/Zones/spawn-city"
curl "http://localhost:5000/Zones"
curl "http://localhost:5000/Zones/by-category/settlement"
```

The category route returns zones whose zone type contains the supplied category.

```bash
curl "http://localhost:5000/Zones/by-coordinates?world=world&x=64&y=72&z=128"
```

```bash
curl -X PATCH "http://localhost:5000/Zones/spawn-city" \
	-H "Content-Type: application/json" \
	-d '{
		"population": 121,
		"bounds": {
			"firstCorner": {
				"world": "world",
				"x": 40,
				"y": 64,
				"z": -40
			}
		}
	}'
```

```bash
curl -X DELETE "http://localhost:5000/Zones/spawn-city"
```

### Manage Items

All Items routes require API-key authorisation:

| Method | Route | Result |
|--------|-------|--------|
| `POST` | `/Items` | Create an item. |
| `GET` | `/Items/{itemIdentifier}` | Retrieve an item by its identifier (NuciCraft ID). |
| `GET` | `/Items/by-minecraft-id/{minecraftId}` | Retrieve an item by its Minecraft ID (e.g., `minecraft:diamond_sword`). |
| `GET` | `/Items/by-bukkit-id/{bukkitId}` | Retrieve an item by its Bukkit ID (e.g., `DIAMOND_SWORD`). |
| `GET` | `/Items/by-nucicraft-id/{nuciCraftId}` | Retrieve an item by its NuciCraft ID. |
| `GET` | `/Items` | Retrieve all items. |
| `PATCH` | `/Items/{itemIdentifier}` | Patch an item by its identifier. |

```bash
curl -X POST "http://localhost:5000/Items" \
	-H "Authorization: Bearer ${NUCICRAFT_API_KEY}" \
	-H "Content-Type: application/json" \
	-d '{
		"id": "diamond_sword",
		"minecraftId": "minecraft:diamond_sword",
		"nucicraftId": "diamond_sword",
		"bukkitId": "DIAMOND_SWORD",
		"signIds": ["DiamondSword"]
	}'
```

The `id` field is the item's primary identifier (NuciCraft ID). `minecraftId` and `bukkitId` are required. `nucicraftId` is optional and defaults to the `id` value if not provided. `signIds` is optional; if omitted or empty, the service generates a default sign ID from the NuciCraft ID using the sign ID generator (see [Sign ID Generation](#sign-id-generation)).

```bash
curl "http://localhost:5000/Items/diamond_sword"
curl "http://localhost:5000/Items/by-minecraft-id/minecraft:diamond_sword"
curl "http://localhost:5000/Items/by-bukkit-id/DIAMOND_SWORD"
curl "http://localhost:5000/Items/by-nucicraft-id/diamond_sword"
curl "http://localhost:5000/Items"
```

```bash
curl -X PATCH "http://localhost:5000/Items/diamond_sword" \
	-H "Authorization: Bearer ${NUCICRAFT_API_KEY}" \
	-H "Content-Type: application/json" \
	-d '{
		"bukkitId": "DIAMOND_SWORD",
		"signIds": ["DiamondSword", "DiaSword"]
	}'
```

Patch requests merge supplied `signIds` and replace other provided fields. Omitted fields preserve their stored values.

#### Sign ID Generation

When `signIds` is not provided on creation, the service generates a default sign ID from the `nucicraftId` (or `id` if `nucicraftId` is not set) using the following rules:

1. If the ID contains a namespace prefix (e.g., `minecraft:diamond_sword`), the prefix is removed.
2. Underscores are replaced with spaces, and each word is capitalised (e.g., `diamond_sword` → `Diamond Sword` → `DiamondSword`).
3. Custom replacements are applied (e.g., `Axe` → `Hatchet`, `Bamboo` → `Bambus`, `Cherry` → `Sakura`, `Cobblestone` → `Cobble`, `Concrete` → `Cement`, `Deepslate` → `Deep`, `Propagule` → `Sapling`, `Leaves` → `Foliage`, `Stained` → removed, `Sulfur` → `Sulphur`).
4. If the result exceeds 16 characters, word shortenings are applied in order until it fits (e.g., `Netherite` → `Nether`, `Chiselled` → `Chisel`, `Polished` → `Polish`, `Pickaxe` → `Pick`, `Stairs` → `Stair`, `Cobble` → `Cbbl`, etc.).

Examples:
- `minecraft:diamond_sword` → `DiamondSword`
- `minecraft:copper_axe` → `CopperHatchet`
- `minecraft:iron_pickaxe` → `IronPickaxe`
- `minecraft:netherite_upgrade_smithing_template` → `NethUpgSmithTemp`
- `polished_cinnabar_stairs` → `PoliCinbarStair`

### Access Control

All Access Control routes require API-key authorisation:

| Method | Route | Result |
|--------|-------|--------|
| `POST` | `/AccessControl/check-ip` | Check if an IP address is allowed based on geolocation and denied countries. |
| `POST` | `/AccessControl/check-username` | Check if a username is allowed based on denied patterns and minimum length. |

```bash
curl -X POST "http://localhost:5000/AccessControl/check-ip" \
	-H "Authorization: Bearer ${NUCICRAFT_API_KEY}" \
	-H "Content-Type: application/json" \
	-d '{
		"ipAddress": "8.8.8.8"
	}'
```

The IP check performs a real-time geolocation lookup using the configured geolocation API (default: ip-api.com) to determine the country code, then checks if that country is in the denied countries list. The response contains `isAllowed`, `deniedReason` (if denied), and `countryCode`.

```bash
curl -X POST "http://localhost:5000/AccessControl/check-username" \
	-H "Authorization: Bearer ${NUCICRAFT_API_KEY}" \
	-H "Content-Type: application/json" \
	-d '{
		"username": "PlayerName"
	}'
```

The username check validates the username against the configured minimum length (default: 4) and checks if it matches any of the denied username patterns (case-sensitive regex). The response contains `isAllowed` and `deniedReason` (if denied).

## ⚠️ Known Limitations

- The service uses JSON-file persistence and is not optimised for high-concurrency multi-instance deployments.
- There is no embedded Swagger/OpenAPI UI in the current host configuration.

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/nucicraft-api/releases)

### Installation from Source

```bash
git clone https://github.com/hmlendea/nucicraft-api.git
cd nucicraft-api
dotnet restore NuciCraft.API.slnx
```

## ⚙️ Configuration

Settings are loaded from [appsettings.json](./NuciCraft.API/appsettings.json), with overrides from the standard ASP.NET Core environment-variable and command-line configuration providers. The subsequent keys are recognised:

| Section | Key | Description |
|---------|-----|-------------|
| `dataStoreSettings` | `countriesStorePath` | Path to the countries JSON store. |
| `dataStoreSettings` | `homesStorePath` | Path to the homes JSON store, created automatically at startup using the default in [appsettings.json](./NuciCraft.API/appsettings.json). |
| `dataStoreSettings` | `itemsStorePath` | Path to the items JSON store. |
| `dataStoreSettings` | `playersStorePath` | Path to the players JSON store. |
| `dataStoreSettings` | `rtpLocationsStorePath` | Path to the RTP locations JSON store. |
| `dataStoreSettings` | `worldsStorePath` | Path to the worlds JSON store. |
| `dataStoreSettings` | `zonesStorePath` | Path to the zones JSON store. |
| `dataStoreSettings` | `zoneTypesStorePath` | Path to the zone types JSON store. |
| `serverSettings` | `name` | Advertised server name. Defaults to `NuciCraft`. |
| `serverSettings` | `hostname` | Advertised Minecraft hostname, also used for live Java status queries. Must be resolvable and accessible from the API host. |
| `serverSettings` | `javaEditionPort` | Advertised Java edition port, also queried directly over TCP. Defaults to `25565`; must be between `1` and `65535`. |
| `serverSettings` | `bedrockEditionPort` | Advertised Bedrock edition port. Defaults to `19132`. |
| `rtpLocationSettings` | `minimumLocationDistance` | Minimum distance permitted between any two RTP locations. |
| `rtpLocationSettings` | `minimumBiomeLocationDistance` | Minimum distance permitted between RTP locations in the identical biome. |
| `securitySettings` | `apiKey` | API key used for endpoint authorisation. |
| `universalNameGeneratorSettings` | `baseUrl` | Universal Name Generator API base URL. |
| `universalNameGeneratorSettings` | `apiKey` | Universal Name Generator API key. |
| `nuciLoggerSettings` | `logFilePath` | Path for file-based logs. |
| `nuciLoggerSettings` | `isFileOutputEnabled` | Enables or disables file log output. |
| `accessControlSettings` | `deniedCountries` | Array of ISO 3166-1 alpha-2 country codes to deny access from. |
| `accessControlSettings` | `deniedUsernamePatterns` | Array of case-sensitive regex patterns to deny usernames matching. |
| `accessControlSettings` | `geolocationApiUrl` | Base URL for the geolocation API (default: http://ip-api.com/json/). |
| `accessControlSettings` | `minimumUsernameLength` | Minimum allowed username length (default: 4). |

Environment-variable overrides for server information are `serverSettings__name`, `serverSettings__hostname`, `serverSettings__javaEditionPort`, and `serverSettings__bedrockEditionPort`. Settings are bound at startup, so restart the API after modifying them. These values do not configure Minecraft listeners or activate Bedrock support.

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

All NuGet dependencies are restored automatically by `dotnet restore`.

### Build

```bash
dotnet build NuciCraft.API/NuciCraft.API.csproj
```

### Run

```bash
dotnet run --project NuciCraft.API/NuciCraft.API.csproj
```

### Test

```bash
dotnet test NuciCraft.API.slnx
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `MineStat` | 3.1.2 | Runtime | Java server-list status queries for the live online player count. |
| `NuciAPI` | 3.6.1 | Runtime | Base API abstractions and request/response contracts. |
| `NuciAPI.Client` | 1.2.3 | Runtime | Outbound HTTP client for the Universal Name Generator integration. |
| `NuciAPI.Controllers` | 2.3.1 | Runtime | Shared controller infrastructure and request processing helpers. |
| `NuciAPI.Middleware` | 2.0.3 | Runtime | Shared middleware abstractions. |
| `NuciAPI.Middleware.ExceptionHandling` | 1.0.2 | Runtime | Exception-handling middleware. |
| `NuciAPI.Middleware.Logging` | 1.0.1 | Runtime | Request-logging middleware. |
| `NuciAPI.Middleware.Security` | 1.0.6 | Runtime | Scanner-protection and security middleware. |
| `NuciDAL` | 3.2.1 | Runtime | JSON-file-backed repository abstractions and persistence. |
| `NuciLog` | 1.2.1 | Runtime | Structured operation logging. |
| `NuciLog.Core` | 3.1.0 | Runtime | Logging core and diagnostic context. |
| `NuciSecurity.HMAC` | 4.1.3 | Runtime | Request/response signing metadata via ordered HMAC fields. |
| `NuciText.Normalisation` | 1.0.1 | Runtime | Text normalisation utilities. |
| `NuciText.Obfuscation` | 1.1.1 | Runtime | Text obfuscation utilities. |
| `NuciWeb.HTTP` | 1.7.3 | Runtime | HTTP utilities for outbound requests. |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ .NET ](https://github.com/hmlendea/nucicraft-api/blob/master/.github/workflows/dotnet.yml) | Continuous integration | Sets up .NET 10, restores dependencies, builds the solution, and runs all tests on pushes and pull requests targeting `master`. |

## 🗂️ Project Structure

The solution contains the subsequent projects:

- [NuciCraft.API](NuciCraft.API): ASP.NET Core API host, controllers, services, configuration, and data access
- [NuciCraft.API.IntegrationTests](NuciCraft.API.IntegrationTests): NUnit-based in-process HTTP integration tests with isolated temporary JSON stores and a stubbed name-generator client
- [NuciCraft.API.UnitTests](NuciCraft.API.UnitTests): NUnit-based unit tests for controllers, services, responses, and mappings

The key directories inside `NuciCraft.API/` are:

| Directory | Purpose |
|-----------|---------|
| `Configuration` | Strongly typed settings models bound from `appsettings.json`. |
| `Controllers` | REST endpoint definitions and HTTP request handling. |
| `Data` | JSON data stores for countries, homes, items, worlds, players, RTP locations, zones, and zone types. |
| `DataAccess` | Data objects and repository mappings for persistence. |
| `Logging` | Operation and log metadata keys used for diagnostics. |
| `Requests` | API request DTOs and validation attributes. |
| `Responses` | API response DTO wrappers. |
| `Service` | Core application services and domain logic. |

## 🏗️ Architecture

NuciCraft API is a layered modular monolith with interface-based adapters at its persistence and outbound HTTP boundaries.

See the [Architecture Documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 📚 Documentation

Start with the [documentation index](./docs/README.md) for the implementation-grounded architecture, behaviour, process flows, data model, operational guidance, change map, and coverage audit.

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/nucicraft-api/issues) for problems or suggestions

See the [Contributing Guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [Code of Conduct](./CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔒 Security

NuciCraft API uses API-key authorisation and self-hosted storage; operators are responsible for protecting configuration, persisted data, and logs.

For information on reporting security vulnerabilities, see the [Security Policy](./SECURITY.md).

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/nucicraft-api/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for additional information.
