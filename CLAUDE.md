# Claude Code Guidelines

## Project Overview

This is an **insurance integration layer** connecting ThreadPilot (a new core insurance system) to legacy backend systems. Built with .NET 8 and ASP.NET Core minimal APIs.

## Architecture

```
ThreadPilot -> Integration Layer -> Legacy Systems
                    |
    +---------------+---------------+
    |               |               |
VehicleService  InsuranceService  CustomerService
  (5001)          (5002)           (5003)
    |               |
LegacyVehicleDB  LegacyMainframe
  (5101)          (5102)
```

### Service Responsibilities

| Service | Pattern | Status |
|---------|---------|--------|
| VehicleService | Anti-corruption layer | Implemented |
| InsuranceService | Anti-corruption layer | Implemented |
| CustomerService | Orchestrator | Not started |

## Code Conventions

### Project Structure

```
src/
  {ServiceName}/
    Program.cs              # Minimal API setup and endpoints
    {ServiceName}.csproj
    appsettings.json
    Models/                 # Domain models (records)
    Clients/                # HTTP clients for legacy systems
      I{LegacySystem}Client.cs
      {LegacySystem}Client.cs

tests/
  {ServiceName}.Tests/
    {Component}Tests.cs     # Unit tests with Moq
```

### Implementation Patterns

1. **Minimal APIs** - Use `app.MapGet()` style endpoints, not controllers
2. **Records for models** - Immutable domain models as C# records
3. **HttpClient via DI** - Use `AddHttpClient<TInterface, TImplementation>()` for typed clients
4. **Anti-corruption layer** - Each client translates legacy models to domain models internally

### Error Handling

All endpoints follow this pattern:

```csharp
try
{
    var result = await client.GetDataAsync(id, ct);
    return result is not null
        ? Results.Ok(result)
        : Results.NotFound(new { error = "Not found", message = "..." });
}
catch (TaskCanceledException)
{
    return Results.StatusCode(504);  // Gateway Timeout
}
catch (HttpRequestException)
{
    return Results.StatusCode(503);  // Service Unavailable
}
```

### Testing

- Use **xUnit** for test framework
- Use **Moq** for mocking HttpMessageHandler
- Use **WebApplicationFactory** for integration tests
- Test both client translation and endpoint behavior separately

Example test file structure:
```
{ServiceName}.Tests/
  {LegacyClient}Tests.cs     # Tests for ACL translation
  {ServiceName}EndpointTests.cs  # Integration tests for endpoints
```

## Build and Run

```bash
# Run all tests
dotnet test

# Run specific service
dotnet run --project src/VehicleService
dotnet run --project src/InsuranceService

# Run legacy simulators (needed for manual testing)
dotnet run --project legacy/VehicleDatabase
dotnet run --project legacy/InsuranceMainframe
```

## Configuration

Services read legacy system URLs from `appsettings.json`:

```json
{
  "LegacyServices": {
    "VehicleDatabase": "http://localhost:5101",
    "InsuranceMainframe": "http://localhost:5102"
  }
}
```

Fallback to localhost defaults if not configured.

## Next Steps

CustomerService (port 5003) needs implementation:
- Orchestrates calls to InsuranceService and VehicleService
- Enriches car insurances with vehicle details
- Should handle partial failures gracefully (return data even if vehicle lookup fails)
