# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY src/CustomerService/CustomerService.csproj ./CustomerService/
RUN dotnet restore CustomerService/CustomerService.csproj

# Copy source and build
COPY src/CustomerService/ ./CustomerService/
WORKDIR /src/CustomerService
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

LABEL org.opencontainers.image.title="ips-customer-service" \
      org.opencontainers.image.description="Customer API - Orchestrator aggregating insurance and vehicle data" \
      org.opencontainers.image.source="https://github.com/peterakerstrom/insurance-platform-showcase"

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --chown=appuser:appuser --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CustomerService.dll"]
