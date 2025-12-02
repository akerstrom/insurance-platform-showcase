# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY legacy/VehicleDatabase/VehicleDatabase.csproj ./VehicleDatabase/
RUN dotnet restore VehicleDatabase/VehicleDatabase.csproj

# Copy source and build
COPY legacy/VehicleDatabase/ ./VehicleDatabase/
WORKDIR /src/VehicleDatabase
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

LABEL org.opencontainers.image.title="ips-legacy-vehicle-db" \
      org.opencontainers.image.description="Legacy vehicle database simulator" \
      org.opencontainers.image.source="https://github.com/peterakerstrom/insurance-platform-showcase"

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --chown=appuser:appuser --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "VehicleDatabase.dll"]
