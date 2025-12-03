# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY src/VehicleService/VehicleService.csproj ./VehicleService/
RUN dotnet restore VehicleService/VehicleService.csproj

# Copy source and build
COPY src/VehicleService/ ./VehicleService/
WORKDIR /src/VehicleService
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

LABEL org.opencontainers.image.title="ips-vehicle-service" \
      org.opencontainers.image.description="Vehicle API - Anti-corruption layer for legacy vehicle database" \
      org.opencontainers.image.source="https://github.com/peterakerstrom/insurance-platform-showcase"

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --chown=appuser:appuser --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "VehicleService.dll"]
