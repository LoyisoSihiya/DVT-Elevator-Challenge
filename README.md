# DVT Elevator Challenge

A production-ready elevator simulation system built with ASP.NET Core 8 and a C# console frontend. The system simulates real-time elevator movement across multiple buildings, dispatches elevators intelligently to passenger requests, and provides a live interactive console interface.

---

## Prerequisites

Before running the project, ensure the following are installed on your machine:

| Requirement | Version | Notes |
|---|---|---|
| .NET SDK | 8.0 or later | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server LocalDB | Any | Included with Visual Studio. Install separately via SQL Server Express if needed |
| Visual Studio | 2022 (recommended) | Or VS Code with C# extension |

To verify your .NET installation:
```bash
dotnet --version
```

To verify LocalDB is available:
```bash
sqllocaldb info
```

---

## Project Setup

### Step 1 — Clone or Copy the Source Code

```bash
git clone <repository-url>
cd DVT.Elevator
```

### Step 2 — Restore NuGet Packages

```bash
cd DVT.Elevator.API
dotnet restore
```

### Step 3 — Run the API

The database is created and seeded automatically on first run.

```bash
cd DVT.Elevator.API/DVT.Elevator.API
dotnet run
```

The API starts at:
- **HTTPS:** `https://localhost:7083`
- **Swagger UI:** `https://localhost:7083/swagger`

Wait until you see:
```
Now listening on: https://localhost:7083
```

### Step 4 — Run the Console Application

Open a second terminal:

```bash
cd DVT.Elevator.API/DVT.Elevator.Application
dotnet run
```

The console will wait for the API to be ready, then display the welcome screen.

---

## Configuration

No configuration changes are required for a standard setup. The defaults work out of the box on any Windows machine with LocalDB installed.

### DVT.Elevator.API/appsettings.json

| Setting | Default | Change When |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `(localdb)\mssqllocaldb` | Using full SQL Server instead of LocalDB |
| `ElevatorSimulation:MovementIntervalMs` | `2000` | Adjust elevator speed (milliseconds per floor) |
| `ElevatorSimulation:EnableSimulation` | `true` | Set to `false` to disable auto-movement |

### DVT.Elevator.Application/appsettings.json

| Setting | Default | Change When |
|---|---|---|
| `ApiSettings:BaseUrl` | `https://localhost:7083` | API runs on a different port |

---

## Seed Data

The database is automatically seeded on first run with:

- **2 Buildings** — DVT Tower (20 floors), Innovation Center (15 floors)
- **4 Elevator Types** — Passenger, Freight, Glass, High-Speed
- **5 Elevators** — distributed across both buildings

---

## Using the Console Application

The console menu provides the following options:

| Option | Description |
|---|---|
| 📊 View Real-Time Elevator Status | Live display of all elevators — floor, direction, status, passengers. Auto-refreshes every 500ms via SignalR |
| 🛗 Request an Elevator | Call an elevator from any floor to any floor with a specified passenger count |
| 📋 View Active Requests | Live view of all pending, assigned, and in-progress requests. Auto-refreshes every 2 seconds |
| 📜 View Request History | Last 20 completed requests |
| 🏢 View Building Information | Building details and elevator inventory |
| ➕ Create a Building | Add a new building with a specified number of floors |
| 🔼 Add an Elevator | Register a new elevator in the current building |
| 🔧 Set Elevator Maintenance Mode | Enable or disable maintenance mode for any elevator |
| 🔄 Change Building | Switch between buildings |
| ❌ Exit | Close the console application |

---

## Architecture

The solution follows a layered architecture with 5 projects:

```
┌─────────────────────────────────────────────────────┐
│              PRESENTATION LAYER                     │
│                                                     │
│  DVT.Elevator.Application                           │
│  Console Frontend — calls API via HTTP + SignalR    │
└──────────────────────┬──────────────────────────────┘
                       │ HTTP / REST + SignalR
┌──────────────────────▼──────────────────────────────┐
│                  BACKEND LAYER                      │
│                                                     │
│  DVT.Elevator.API                                   │
│  REST Controllers, Swagger, Middleware              │
│                                                     │
│  DVT.Elevator.Infrastructure                        │
│  EF Core, Repositories, SignalR, Simulation Engine  │
└──────────────────────┬──────────────────────────────┘
                       │ uses
┌──────────────────────▼──────────────────────────────┐
│              BUSINESS LOGIC LAYER                   │
│                                                     │
│  DVT.Elevator.Services                              │
│  BuildingService, ElevatorService, FloorService     │
│  PassengerRequestService, DispatchStrategy          │
│  AutoMapper MappingProfile                          │
└──────────────────────┬──────────────────────────────┘
                       │ uses
┌──────────────────────▼──────────────────────────────┐
│           DOMAIN CORE  (shared by all layers)       │
│                                                     │
│  DVT.Elevator.Domain                                │
│  Entities, Interfaces, DTOs, Enums                  │
│  No external dependencies                           │
└─────────────────────────────────────────────────────┘
                       │ persisted in
              ┌────────▼────────┐
              │  SQL Server     │
              │  DVTElevatorDb  │
              └─────────────────┘
```

### Project Responsibilities

#### DVT.Elevator.Application — Presentation Layer
The interactive console frontend. Communicates with the API exclusively via HTTP REST calls and SignalR for real-time updates. Contains no business logic or database access.

- `UI/ElevatorConsoleUI.cs` — All menu screens and user interaction
- `Services/ElevatorApiClient.cs` — HTTP client for all API calls
- `Services/ElevatorSignalRService.cs` — SignalR connection management
- `Models/` — Response models for API communication
- `Helpers/TimeZoneHelper.cs` — UTC to SAST conversion

#### DVT.Elevator.API — Backend Entry Point
Exposes the REST API. Controllers receive HTTP requests and delegate to the Services layer. No business logic lives here.

- `Controllers/` — BuildingsController, ElevatorsController, FloorsController, PassengerRequestsController, ElevatorTypesController
- `Middleware/ExceptionHandlingMiddleware.cs` — Global error handling
- `Validators/` — FluentValidation request validators

#### DVT.Elevator.Infrastructure — Backend Data Layer
All technical implementations — database, repositories, real-time communication, and the background simulation engine.

- `Data/ElevatorDbContext.cs` — EF Core database context
- `Data/Configurations/` — Table structure and FK constraints
- `Data/DbSeeder.cs` — Seed data on first run
- `Repositories/` — Repository pattern implementations
- `Hubs/ElevatorHub.cs` — SignalR hub for real-time client connections
- `Hubs/ElevatorHubService.cs` — Broadcasts elevator events to connected clients
- `Services/ElevatorSimulationService.cs` — Background service that moves elevators every tick

#### DVT.Elevator.Services — Business Logic Layer
All business rules, service implementations, dispatch algorithm, and object mapping.

- `Services/BuildingService.cs` — Building management
- `Services/ElevatorService.cs` — Elevator management
- `Services/FloorService.cs` — Floor status
- `Services/PassengerRequestService.cs` — Request processing and dispatch
- `Strategies/NearestElevatorDispatchStrategy.cs` — Elevator scoring and selection algorithm
- `Mappings/MappingProfile.cs` — AutoMapper entity-to-DTO mappings

#### DVT.Elevator.Domain — Shared Core
Pure business concepts with no external dependencies. Referenced by all other layers.

- `Entities/` — Building, Elevator, Floor, PassengerRequest, ElevatorType
- `Interfaces/` — IRepository, IUnitOfWork, IBuildingService, IElevatorService, IPassengerRequestService, IElevatorDispatchStrategy
- `DTOs/` — 12 individual DTO files (one class per file)
- `Enums/` — ElevatorDirection, ElevatorStatus, RequestStatus

---

## Design Patterns

| Pattern | Where Used | Purpose |
|---|---|---|
| Strategy | `IElevatorDispatchStrategy` | Swappable dispatch algorithms |
| Repository | `IRepository<T>` | Abstracts data access from business logic |
| Unit of Work | `IUnitOfWork` | Coordinates multiple repositories in one transaction |
| Observer | SignalR events in `ElevatorSignalRService` | Real-time push notifications |
| Dependency Injection | Throughout | Loose coupling, testability |
| Background Service | `ElevatorSimulationService` | Continuous elevator movement simulation |

---

## Dispatching Algorithm

The `NearestElevatorDispatchStrategy` uses a weighted scoring system to select the best elevator for each request.

Every eligible elevator starts with a base score of **100** and points are added or deducted:

| Factor | Points |
|---|---|
| Distance to source floor | -2 per floor |
| Elevator is idle | +20 |
| Moving same direction and on the way | +15 |
| Moving same direction but not on the way | +5 |
| Moving opposite direction | -10 |
| Available capacity ratio | up to +10 |
| Would exceed 80% capacity | -15 |

The elevator with the highest score is assigned. If no elevator is available, the request is queued and re-attempted on every simulation tick.

---

## Real-Time Communication

The system uses **SignalR** for server-to-client push notifications. The API broadcasts the following events:

| Event | Triggered When |
|---|---|
| `ElevatorMoved` | An elevator moves one floor |
| `ElevatorStatusChanged` | An elevator arrives, loads, or unloads passengers |
| `RequestCompleted` | Passengers are dropped off at their destination |
| `CapacityWarning` | An elevator cannot accept passengers due to capacity |

The console application connects to `/hubs/elevator`, subscribes to a building group, and updates its live display cache when events are received. If SignalR is unavailable, the console falls back to HTTP polling automatically.

---

## Database

The database is managed by Entity Framework Core using Code First migrations.

### Tables

| Table | Description |
|---|---|
| `Buildings` | Building name and total floors |
| `ElevatorTypes` | Passenger, Freight, Glass, High-Speed definitions |
| `Elevators` | Each elevator's current state |
| `Floors` | All floors per building |
| `PassengerRequests` | Every elevator request and its lifecycle |

### Referential Integrity

| Relationship | On Delete |
|---|---|
| Buildings → Floors | CASCADE |
| Buildings → Elevators | RESTRICT |
| Buildings → PassengerRequests | RESTRICT |
| ElevatorTypes → Elevators | RESTRICT |
| Elevators → PassengerRequests | SET NULL |

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/buildings` | Get all buildings |
| POST | `/api/buildings` | Create a building |
| GET | `/api/buildings/{id}` | Get building by ID |
| GET | `/api/elevators` | Get all elevators |
| GET | `/api/elevators/building/{id}` | Get elevators by building |
| GET | `/api/elevators/{id}/status` | Get real-time elevator status |
| GET | `/api/elevators/building/{id}/statuses` | Get all elevator statuses |
| POST | `/api/elevators` | Create an elevator |
| PUT | `/api/elevators/{id}/maintenance` | Set maintenance mode |
| GET | `/api/elevatortypes` | Get all elevator types |
| POST | `/api/passengerrequests` | Request an elevator |
| GET | `/api/passengerrequests/building/{id}/active` | Get active requests |
| GET | `/api/passengerrequests/building/{id}/history` | Get request history |
| GET | `/api/floors/building/{id}` | Get floors by building |
| GET | `/health` | Health check |

Full interactive documentation available at `https://localhost:7083/swagger` when the API is running.

---

## Technologies

| Technology | Version | Purpose |
|---|---|---|
| ASP.NET Core | 8.0 | Web API framework |
| Entity Framework Core | 8.0 | ORM and database migrations |
| SQL Server LocalDB | — | Database |
| SignalR | — | Real-time communication |
| AutoMapper | 12.0 | Entity to DTO mapping |
| FluentValidation | 11.0 | Request validation |
| Spectre.Console | 0.55 | Rich console UI |
| Swashbuckle | 6.6 | Swagger/OpenAPI documentation |
