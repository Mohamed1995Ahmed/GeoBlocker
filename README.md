# GeoBlocker API

A .NET 8 Web API for managing country-based IP blocking using third-party geolocation. All data is stored in-memory — no database required.

## Features

- Block and unblock countries permanently
- Temporarily block countries for a configurable duration (auto-expires)
- Look up geolocation details for any IP address
- Automatically detect and check if a caller's country is blocked
- Log all access attempts with IP, country, timestamp, and user agent
- Paginated and searchable responses across all list endpoints

## Tech Stack

- **ASP.NET Core 8** — Web API
- **ipgeolocation.io** — Third-party geolocation provider
- **ConcurrentDictionary / ConcurrentQueue** — Thread-safe in-memory storage
- **Swagger / Swashbuckle** — Interactive API documentation
- **Hosted Background Service** — Cleans up expired temporal blocks every 5 minutes

## Project Structure

```
GeoBlocker.sln
├── GeoBlocker/             # Main API project (controllers, Program.cs, config)
├── Models/                 # Domain models and DTOs
├── Services/               # Business logic and geolocation service
├── Repositories/           # In-memory data store (IInMemoryRepository)
├── BackgroundServices/     # TemporalBlockCleanupService (runs every 5 min)
└── Controllers/            # (reserved class library project)
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- A free API key from [ipgeolocation.io](https://ipgeolocation.io)

### Configuration

Add your API key to `GeoBlocker/appsettings.json`:

```json
{
  "GeoLocation": {
    "Provider": "ipgeolocation",
    "IpApiBaseUrl": "https://api.ipgeolocation.io",
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### Run

```bash
dotnet run --project GeoBlocker
```

Then open the Swagger UI at:

```
https://localhost:{port}/swagger
```

## API Endpoints

### Countries

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/countries/block` | Permanently block a country by ISO code |
| `DELETE` | `/api/countries/block/{countryCode}` | Remove a country from the block list |
| `GET` | `/api/countries/blocked` | List all blocked countries (paginated + search) |
| `POST` | `/api/countries/temporal-block` | Temporarily block a country for 1–1440 minutes |

### IP

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/ip/lookup?ipAddress={ip}` | Look up geolocation for an IP (uses caller IP if omitted) |
| `GET` | `/api/ip/check-block` | Check if the caller's country is blocked and log the attempt |

### Logs

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/logs/blocked-attempts` | Paginated list of all access attempts |

## Request & Response Examples

**Block a country**
```http
POST /api/countries/block
Content-Type: application/json

{ "countryCode": "US" }
```

**Temporarily block a country**
```http
POST /api/countries/temporal-block
Content-Type: application/json

{ "countryCode": "RU", "durationMinutes": 120 }
```

**Get blocked countries with search and pagination**
```http
GET /api/countries/blocked?page=1&pageSize=10&search=eg
```

**Log entry fields**

```json
{
  "ipAddress": "1.2.3.4",
  "timestamp": "2026-04-09T10:00:00Z",
  "countryCode": "CN",
  "countryName": "China",
  "isBlocked": true,
  "userAgent": "Mozilla/5.0 ..."
}
```

## Validation Rules

- Country codes must be valid ISO 3166-1 alpha-2 codes (e.g. `US`, `GB`, `EG`)
- `durationMinutes` for temporal blocks must be between **1 and 1440** (24 hours)
- Duplicate blocks return `409 Conflict`
- Missing countries on delete return `404 Not Found`
- Invalid IP formats return `400 Bad Request`

## Notes

- All storage is in-memory and resets on application restart
- The background cleanup service removes expired temporal blocks every 5 minutes
- When running locally, loopback IPs (`127.0.0.1`, `::1`) are substituted with a public test IP (`8.8.8.8`) for development convenience
