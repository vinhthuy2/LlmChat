# Technical Context

## Technology Stack
- **Framework**: .NET 8.0
- **Database**: SQLite with Entity Framework Core 9.0.6
- **LLM Integration**: OllamaSharp 5.2.2
- **Logging**: Serilog 8.0.1
- **API Documentation**: Swashbuckle.AspNetCore 6.4.0
- **Development Tools**: Visual Studio/Rider

## Development Setup
1. .NET 8.0 SDK
2. Ollama running locally
3. SQLite database (ChatLlm.db)
4. IDE with .NET support
5. Logs directory for file logging

## Project Structure
```
LlmChat/
├── Agents/           # LLM agent implementations
├── Chat/            # Chat session management
├── Infra/           # Infrastructure concerns
│   └── Logging/     # Logging system
├── Migrations/      # Database migrations
└── Program.cs       # Application entry point
```

## Dependencies
- Microsoft.AspNetCore.OpenApi
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.InMemory
- Microsoft.EntityFrameworkCore.Sqlite
- OllamaSharp
- Serilog.AspNetCore
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Serilog.Enrichers.Environment
- Serilog.Enrichers.Thread
- Swashbuckle.AspNetCore

## Technical Constraints
1. Requires Ollama running locally
2. SQLite for data persistence
3. .NET 8.0 runtime
4. Cross-origin resource sharing enabled
5. Log file rotation enabled

## Development Workflow
1. Local development with Ollama
2. Database migrations for schema changes
3. API testing via Swagger UI
4. Cross-origin requests enabled for web client development
5. Log monitoring via console and files 