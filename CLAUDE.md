# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Nifty Options Algo** is a .NET 8.0-based algorithmic trading system for Nifty options. It's designed with a layered, multi-project architecture supporting strategy execution, risk management, backtesting, and real-time monitoring via a Blazor web dashboard.

## Technology Stack

- **Language**: C# with .NET 8.0
- **Testing Framework**: xUnit with coverlet for code coverage
- **Logging**: Serilog (structured logging with console and file sinks)
- **Job Scheduling**: Quartz 3.7.0 for worker tasks
- **Web Framework**: ASP.NET Core with Blazor for the dashboard
- **UI Components**: Microsoft AspNetCore Components QuickGrid

## Project Structure

The solution consists of two categories of projects:

### Core Libraries (Business Logic & Infrastructure)

1. **NiftyOptionsAlgo.Core** - Core domain models and interfaces
2. **NiftyOptionsAlgo.Infrastructure** - Data access and external service integrations
3. **NiftyOptionsAlgo.Engine** - Strategy execution engine with:
   - `StrategyEngine.cs` - Main strategy orchestration
   - `OrderExecutor.cs` - Order execution logic
   - `PositionMonitor.cs` - Position tracking and monitoring
4. **NiftyOptionsAlgo.RiskManager** - Risk controls and position limits
5. **NiftyOptionsAlgo.Backtester** - Historical backtesting engine

### Application & Presentation

6. **NiftyOptionsAlgo.Worker** - Background worker service (Windows Service/Docker container)
   - Uses Quartz for scheduled job execution
   - Serilog for structured logging
7. **NiftyOptionsAlgo.Dashboard** - ASP.NET Core Blazor web application for monitoring and control

### Test Projects

- `NiftyOptionsAlgo.Core.Tests`
- `NiftyOptionsAlgo.Engine.Tests`
- `NiftyOptionsAlgo.Backtester.Tests`

## Build & Development Commands

### Build the Entire Solution
```bash
dotnet build
```

### Run Unit Tests
```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test NiftyOptionsAlgo.Core.Tests

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestClassName"

# Run with code coverage report
dotnet test /p:CollectCoverage=true
```

### Run Specific Applications

```bash
# Start the Worker service
dotnet run --project NiftyOptionsAlgo.Worker

# Start the Dashboard web application
dotnet run --project NiftyOptionsAlgo.Dashboard
# Dashboard runs on https://localhost:7001 by default
```

### Clean Build
```bash
dotnet clean
dotnet build
```

## Development Workflow

1. **Make changes** to source files in the appropriate project directory
2. **Build locally**: `dotnet build`
3. **Run tests**: `dotnet test` (or specific test project)
4. **Run the application**: Use the commands above for Worker or Dashboard
5. **Check code coverage**: `dotnet test /p:CollectCoverage=true`

## Architecture Notes

### Dependency Flow

The architecture follows a layered approach with dependencies flowing inward:

- **Dashboard & Worker** depend on Core, Engine, Infrastructure, RiskManager, and Backtester
- **Engine** depends on Core and Infrastructure
- **RiskManager** and **Backtester** depend on Core
- **Core** has no internal dependencies (defines interfaces/contracts)
- **Infrastructure** implements Core interfaces

### Key Patterns

- **Dependency Injection**: Used throughout via Microsoft.Extensions.DependencyInjection
- **Logging**: Serilog is configured in Worker; use `ILogger<T>` for dependency injection
- **Async/Await**: Extensively used for I/O operations (API calls, database access)
- **Entity Dependencies**: Core uses Microsoft.Extensions.Logging.Abstractions for abstraction

### Configuration

- **Worker**: `appsettings.json` and `appsettings.Development.json` in the Worker directory
- **Dashboard**: `appsettings.json` and `appsettings.Development.json` in the Dashboard directory
- **User Secrets**: Worker has user secrets configured (ID: `dotnet-NiftyOptionsAlgo.Worker-60bbb580-7d23-4e00-bcde-05fada64371f`)

## Code Standards

Refer to the [global C# & .NET instructions](~/.claude/instructions/dotnet.instructions.md) for:
- Naming conventions and C# style guidelines
- Error handling and exception policies
- Async/await patterns
- DI and extension methods
- Testing practices with xUnit

## Nifty Options Trading Context

This system is specifically built for options trading on the Nifty index (India's NSE). Key considerations:

- **Options Greeks**: Engine may calculate/use IV, delta, gamma, theta, vega
- **Strike Selection**: Strike-based option selection logic likely in Engine
- **Intraday Nature**: Nifty options are daily expiry; backtester should account for this
- **Risk Controls**: RiskManager enforces position limits and Greeks-based controls

## Notable Patterns in Codebase

1. **Generics and Abstractions**: Core defines generic interfaces for flexibility
2. **Quartz Jobs**: Worker likely implements `IJob` interface for scheduled tasks
3. **Blazor Components**: Dashboard uses Blazor's component model and QuickGrid for data display
4. **Serilog Enrichment**: Worker may use correlation IDs for request tracing

## Common Issues & Troubleshooting

- **Tests Fail on Startup**: Ensure infrastructure dependencies (databases, APIs) are available or mocked
- **Worker Won't Start**: Check `appsettings.json` and user secrets configuration
- **Dashboard Connection Issues**: Verify Worker is running and accessible if Dashboard depends on it
- **Build Fails**: Run `dotnet clean` and retry; check for NuGet package restore issues
