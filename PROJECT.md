# PLCDataCollector — PLC / Production Data Architecture

## Purpose
.NET 8 ASP.NET Core application for PLC/industrial data collection, production-data synchronization, APIs and real-time graph/display data.

## Stack
.NET 8, NModbus, Oracle + SQLite, EF Core, Dapper, FluentMigrator, FTP, CSV, WebSockets, Serilog, Swagger, health checks and rate limiting.

## Architecture
PLC / source DB / FTP -> PLC + parsing + production services -> source database -> synchronization -> target SQLite/API -> REST + WebSocket clients.

## Key Services
PLCService, PlcDataService, ProductionService, DataParsingService, FTPService, GraphDataService, DataSyncBackgroundService, PLCDataCollectorBackgroundService, HealthCheckBackgroundService, AlertService and WebSocketService.

## Database
Source can be Oracle or SQLite depending on environment. Target is SQLite. Do not assume identical responsibilities between source and target.

## Runtime
Program.cs configures DI, database providers, background services, Swagger, health checks, CORS, compression, WebSockets, rate limiting and middleware.

## Industrial Safety
Do not change PLC addresses/register mappings, polling intervals, retry behavior or write operations without understanding production impact. Never put PLC credentials in source code.

## AI Rules
Trace PLC/source -> parser -> domain service -> database -> API/WebSocket before changes. Preserve idempotency and synchronization semantics.

## Validation
Prefer non-production PLC/source testing. Test connectivity failures, malformed/duplicate data, database outages and service restart/recovery.