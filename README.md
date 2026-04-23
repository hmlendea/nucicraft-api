[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Latest Release](https://img.shields.io/github/v/release/hmlendea/nucicraft-api)](https://github.com/hmlendea/nucicraft-api/releases/latest) [![Build Status](https://github.com/hmlendea/nucicraft-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nucicraft-api/actions/workflows/dotnet.yml)

# NuciCraft API

A lightweight REST API that provides functionality and facilitates the management of the NuciCraft Minecraft server.

The API stores RTP locations and players in JSON files and exposes endpoints to:

- add a new RTP location
- register a player
- retrieve a random RTP location (optionally filtered by world and/or biome)

## Requirements

- .NET SDK/runtime with support for `net10.0`

## API Overview

Base URL (local):

```text
http://localhost:5000
```

Controller route prefix:

```text
/RtpLocations
```

```text
/Players
```

### Authentication

Requests are validated using the API key configured in `securitySettings.apiKey`.

This project also uses `NuciSecurity.HMAC` attributes on request/response models to support ordered signing semantics in the Nuci ecosystem.

### Endpoints

#### Register player

- Method: `POST`
- Path: `/Players`
- Body:

```json
{
	"username": "PlayerName",
	"onlineUUID": "6f6f5f2d-6f7e-4f6c-8e1d-03a9b8d939f0",
	"createdDT": "2026-04-01T12:00:00+00:00",
	"password": "<password>",
	"ipAddress": "127.0.0.1",
	"skinUrl": "https://example.com/skin.png"
}
```

Behavior:

- computes and stores the offline UUID from `username`
- persists player data to the players data store
- defaults `createdDT` to the current timestamp when not provided

#### Add RTP location

- Method: `POST`
- Path: `/RtpLocations`
- Body:

```json
{
	"username": "PlayerName",
	"biome": "plains",
	"world": "world",
	"x": 123,
	"y": 70,
	"z": -456
}
```

Behavior:

- creates a new location with a generated ID
- appends it to the JSON data store

#### Get random RTP location

- Method: `GET`
- Path: `/RtpLocations/random`
- Query parameters:
	- `username` (required)
	- `world` (optional)
	- `biome` (optional)

Example:

```text
GET /RtpLocations/random?world=world&biome=plains
```

Example response payload:

```json
{
	"biome": "plains",
	"world": "world",
	"x": 123,
	"y": 70,
	"z": -456
}
```

Behavior:

- loads all locations
- applies `world` and `biome` filters when provided
- returns one random matching location

## Configuration

Runtime settings are loaded from `appsettings.json`.

Default configuration:

```json
{
	"dataStoreSettings": {
		"playersStorePath": "Data/players.json",
		"rtpLocationsStorePath": "Data/rtp_locations.json"
	},
	"securitySettings": {
		"apiKey": "[[NUCICRAFT_API_KEY]]"
	},
	"nuciLoggerSettings": {
		"logFilePath": "logfile.log",
		"isFileOutputEnabled": true
	}
}
```

Notes:

- at startup, the API creates both data store directories/files automatically if missing
- `rtpLocationsStorePath` and `playersStorePath` can be changed to point to other JSON files
- replace `[[NUCICRAFT_API_KEY]]` with your actual API key

## Development

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from: `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

## Contributing

Contributions are welcome.

Please:

- keep the changes cross-platform
- keep the existing public API intact, unless a breaking change is intentional
- keep the pull requests focused and consistent with the existing style
- update the documentation when the behaviour changes
- add or update the tests for any new behaviour

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.