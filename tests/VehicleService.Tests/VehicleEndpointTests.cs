using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VehicleService.Clients;
using VehicleService.Models;
using Xunit;

namespace VehicleService.Tests;

public class VehicleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public VehicleEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithMock(Mock<IVehicleDatabaseClient> mockClient)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleDatabaseClient));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddSingleton(mockClient.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetVehicle_ValidRegnr_Returns200WithVehicle()
    {
        // Arrange
        var expectedVehicle = new Vehicle("WBA3A5C55DF123456", "ABC123", "BMW", "320i", 2019);
        var mockClient = new Mock<IVehicleDatabaseClient>();
        mockClient
            .Setup(c => c.GetVehicleAsync("ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVehicle);

        var client = CreateClientWithMock(mockClient);

        // Act
        var response = await client.GetAsync("/vehicles/ABC123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var vehicle = await response.Content.ReadFromJsonAsync<Vehicle>();
        Assert.NotNull(vehicle);
        Assert.Equal(expectedVehicle.Regnr, vehicle.Regnr);
    }

    [Fact]
    public async Task GetVehicle_NotFound_Returns404()
    {
        // Arrange
        var mockClient = new Mock<IVehicleDatabaseClient>();
        mockClient
            .Setup(c => c.GetVehicleAsync("NOTFOUND", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var client = CreateClientWithMock(mockClient);

        // Act
        var response = await client.GetAsync("/vehicles/NOTFOUND");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetVehicle_LegacyServiceUnavailable_Returns503()
    {
        // Arrange
        var mockClient = new Mock<IVehicleDatabaseClient>();
        mockClient
            .Setup(c => c.GetVehicleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var client = CreateClientWithMock(mockClient);

        // Act
        var response = await client.GetAsync("/vehicles/ABC123");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
