# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY src/InsuranceService/InsuranceService.csproj ./InsuranceService/
RUN dotnet restore InsuranceService/InsuranceService.csproj

# Copy source and build
COPY src/InsuranceService/ ./InsuranceService/
WORKDIR /src/InsuranceService
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

LABEL org.opencontainers.image.title="ips-insurance-service" \
      org.opencontainers.image.description="Insurance API - Anti-corruption layer for legacy mainframe" \
      org.opencontainers.image.source="https://github.com/peterakerstrom/insurance-platform-showcase"

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --chown=appuser:appuser --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InsuranceService.dll"]
