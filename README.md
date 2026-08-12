[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucicraft-api)](https://github.com/hmlendea/nucicraft-api/releases/latest)
[![Build Status](https://github.com/hmlendea/nucicraft-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucicraft-api/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# NuciCraft API

NuciCraft API is a lightweight ASP.NET Core REST service for NuciCraft Minecraft server operations, including player registration and updates, RTP location management, country and zone management, and mob name generation.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Usage](#usage)
  - [Register a Player](#register-a-player)
	- [Get a Player by ID](#get-a-player-by-id)
	- [Get a Player by Username](#get-a-player-by-username)
	- [Get a Player by Offline UUID](#get-a-player-by-offline-uuid)
	- [Get a Player by Online UUID](#get-a-player-by-online-uuid)
  - [Get All Players](#get-all-players)
	- [Patch a Player](#patch-a-player)
  - [Add an RTP Location](#add-an-rtp-location)
  - [Get a Random RTP Location](#get-a-random-rtp-location)
  - [Get a Random Mob Name](#get-a-random-mob-name)
	- [Manage Countries](#manage-countries)
  - [Manage Zones](#manage-zones)
- [Known Limitations](#known-limitations)
- [Installation](#installation)
  - [CLI Installation](#cli-installation)
- [Configuration](#configuration)
- [Development](#development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
  - [Test](#test)
  - [Release](#release)
  - [Dependencies](#dependencies)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [Security](#security)
- [Supporting the Project](#supporting-the-project)
- [License](#license)

## ✨ Capabilities

- Registers, retrieves, and updates players via protected API endpoints
- Stores and retrieves RTP locations with distance constraints and biome/world filtering
- Generates random mob names via Universal Name Generator integration
- Stores, retrieves, and updates country metadata
- Stores, retrieves, and updates zone metadata
- Assigns a default zone creation date when one is not provided by the caller

## 🚀 Usage

All endpoints are rooted at your configured host, for example `http://localhost:5000`.

Requests are protected by API-key authorisation configured through `securitySettings.apiKey`.

### Register a Player

```bash
curl -X POST "http://localhost:5000/Players" \
	-H "Content-Type: application/json" \
	-d '{
		"username": "PlayerName",
		"onlineUUID": "6f6f5f2d-6f7e-4f6c-8e1d-03a9b8d939f0",
		"createdDT": "2026-04-01T12:00:00+00:00",
		"password": "example-password",
		"ipAddress": "127.0.0.1",
		"skinUrl": "https://example.com/skin.png"
	}'
```

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
		"discordId": "1234567890"
	}'
```

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

### Get a Random Mob Name

```bash
curl "http://localhost:5000/Mobs/wandering_trader/random-name"
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

### Manage Zones

When creating a zone, `creationDate` is optional. If omitted or whitespace, the service sets it automatically to the current Romania date with an uncertainty suffix in the format `yyyy-MM-dd (?)`.

```bash
curl -X POST "http://localhost:5000/Zones" \
	-H "Content-Type: application/json" \
	-d '{
		"identifier": "spawn-city",
		"name": {
			"en": "Spawn City"
		},
		"population": 120
	}'
```

```bash
curl "http://localhost:5000/Zones/spawn-city"
curl "http://localhost:5000/Zones"
```

```bash
curl -X PATCH "http://localhost:5000/Zones/spawn-city" \
	-H "Content-Type: application/json" \
	-d '{
		"population": 121
	}'
```

## ⚠️ Known Limitations

- The service uses JSON-file persistence and is not optimised for high-concurrency multi-instance deployments.
- There is no embedded Swagger/OpenAPI UI in the current host configuration.

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/nucicraft-api/releases)

### CLI Installation

```bash
git clone https://github.com/hmlendea/nucicraft-api.git
cd nucicraft-api
dotnet restore NuciCraft.API.slnx
```

## ⚙️ Configuration

All settings are loaded from the configuration file. The subsequent keys are recognised:

| Section | Key | Description |
|---------|-----|-------------|
| `dataStoreSettings` | `countriesStorePath` | Path to the countries JSON store. |
| `dataStoreSettings` | `playersStorePath` | Path to the players JSON store. |
| `dataStoreSettings` | `rtpLocationsStorePath` | Path to the RTP locations JSON store. |
| `dataStoreSettings` | `zonesStorePath` | Path to the zones JSON store. |
| `rtpLocationSettings` | `minimumLocationDistance` | Minimum distance permitted between any two RTP locations. |
| `rtpLocationSettings` | `minimumBiomeLocationDistance` | Minimum distance permitted between RTP locations in the identical biome. |
| `securitySettings` | `apiKey` | API key used for endpoint authorisation. |
| `universalNameGeneratorSettings` | `baseUrl` | Universal Name Generator API base URL. |
| `universalNameGeneratorSettings` | `apiKey` | Universal Name Generator API key. |
| `nuciLoggerSettings` | `logFilePath` | Path for file-based logs. |
| `nuciLoggerSettings` | `isFileOutputEnabled` | Enables or disables file log output. |

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

| Package | Purpose |
|---------|---------|
| `NuciAPI` | Base API abstractions and request/response contracts. |
| `NuciAPI.Controllers` | Shared controller infrastructure and request processing helpers. |
| `NuciAPI.Middleware.*` | Exception handling, request logging, and scanner-protection middleware. |
| `NuciDAL` | JSON-file-backed repository abstractions and persistence. |
| `NuciLog` and `NuciLog.Core` | Structured operation logging and diagnostic context. |
| `NuciSecurity.HMAC` | Request/response signing metadata via ordered HMAC fields. |
| `NuciText.Normalisation` and `NuciText.Obfuscation` | Text normalisation and obfuscation utilities. |

## 🗂️ Project Structure

The solution contains the subsequent projects:

- `NuciCraft.API`: ASP.NET Core API host, controllers, services, configuration, and data access
- `NuciCraft.API.UnitTests`: NUnit-based unit tests for controllers, services, responses, and mappings

The key directories inside `NuciCraft.API/` are:

| Directory | Purpose |
|-----------|---------|
| `Configuration` | Strongly typed settings models bound from `appsettings.json`. |
| `Controllers` | REST endpoint definitions and HTTP request handling. |
| `Data` | JSON data stores for countries, players, RTP locations, and zones. |
| `DataAccess` | Data objects and repository mappings for persistence. |
| `Logging` | Operation and log metadata keys used for diagnostics. |
| `Requests` | API request DTOs and validation attributes. |
| `Responses` | API response DTO wrappers. |
| `Service` | Core application services and domain logic. |

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Maintain the existing public contract intact unless a breaking change is intentional
- Maintain the pull requests as focused and consistent with the existing code style
- Maintain your branch up-to-date with `master`
- Revise the documentation when behaviour changes
- Properly test all changes, including edge cases and error conditions
- Add unit tests for any new or changed functionality

## 🔒 Security

For information on reporting security vulnerabilities, see [SECURITY.md](./SECURITY.md).

## 💝 Supporting the Project

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/nucicraft-api/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for further information.