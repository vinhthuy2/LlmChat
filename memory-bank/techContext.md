# Technical Context

## Technology Stack
- **Framework**: .NET 8.0
- **Database**: SQLite with Entity Framework Core 9.0.6
- **Logging**: Serilog 8.0.1
- **API Documentation**: Swashbuckle.AspNetCore 6.4.0
- **Development Tools**: Visual Studio/Rider
- **LLM Integration**: OllamaSharp
- **Testing**: MSTest, NSubstitute
- **Streaming**: Server-Sent Events
- **Memory Management**: In-memory dictionaries

## Development Setup
1. .NET 8.0 SDK
2. SQLite database (ChatLlm.db)
3. IDE with .NET support
4. Logs directory for file logging
5. Ollama server running on localhost:11434
6. Ollama model: phi3
7. Streaming client support

## Project Structure
```
LlmChat/
├── Agents/           # LLM agent implementations
├── Chat/            # Chat session management
│   ├── Interfaces/  # Service and store interfaces
│   └── Services/    # Service implementations
├── Infra/           # Infrastructure concerns
│   ├── Data/        # Data access and repositories
│   └── Logging/     # Logging system
├── Migrations/      # Database migrations
├── Tests/           # Test projects
│   ├── Chat/        # Chat service tests
│   └── Infra/       # Infrastructure tests
└── Program.cs       # Application entry point
```

## Dependencies
- Microsoft.AspNetCore.OpenApi
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.InMemory
- Microsoft.EntityFrameworkCore.Sqlite
- Serilog.AspNetCore
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Serilog.Enrichers.Environment
- Serilog.Enrichers.Thread
- Swashbuckle.AspNetCore
- OllamaSharp
- NSubstitute
- MSTest.TestFramework
- MSTest.TestAdapter

## Technical Constraints
1. SQLite for data persistence
2. .NET 8.0 runtime
3. Cross-origin resource sharing enabled
4. Log file rotation enabled
5. Ollama server required for LLM features
6. Streaming response support required
7. Unit test coverage required
8. Integration test support required
9. In-memory conversation history
10. Deferred message processing

## Development Workflow
1. Local development with SQLite
2. Database migrations for schema changes
3. API testing via Swagger UI
4. Cross-origin requests enabled for web client development
5. Log monitoring via console and files
6. Ollama server must be running for LLM features
7. Unit testing with MSTest and NSubstitute
8. Integration testing with in-memory database
9. Test-driven development approach
10. Continuous integration support
11. Streaming response testing
12. Conversation history testing 