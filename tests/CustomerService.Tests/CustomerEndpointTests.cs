using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using CustomerService.Clients;
using CustomerService.Models;
using Xunit;

namespace CustomerService.Tests;

public class CustomerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public CustomerEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private HttpClient CreateClientWithMocks(
        Mock<IInsuranceServiceClient> mockInsuranceClient,
        Mock<IVehicleServiceClient> mockVehicleClient)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var insuranceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IInsuranceServiceClient));
                if (insuranceDescriptor != null)
                    services.Remove(insuranceDescriptor);

                var vehicleDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleServiceClient));
                if (vehicleDescriptor != null)
                    services.Remove(vehicleDescriptor);

                services.AddSingleton(mockInsuranceClient.Object);
                services.AddSingleton(mockVehicleClient.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetCustomerInsurances_ValidPid_Returns200WithInsurances()
    {
        // Arrange
        var insurances = new List<InsuranceInfo>
        {
            new(Guid.NewGuid(), "199001011234", InsuranceType.Pet, "Active", 10m),
            new(Guid.NewGuid(), "199001011234", InsuranceType.Health, "Active", 20m)
        };

        var mockInsuranceClient = new Mock<IInsuranceServiceClient>();
        mockInsuranceClient
            .Setup(c => c.GetInsurancesAsync("199001011234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        var mockVehicleClient = new Mock<IVehicleServiceClient>();

        var client = CreateClientWithMocks(mockInsuranceClient, mockVehicleClient);

        // Act
        var response = await client.GetAsync("/customers/199001011234/insurances");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CustomerInsurance>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCustomerInsurances_CarInsuranceIncludesVehicle()
    {
        // Arrange
        var insurances = new List<InsuranceInfo>
        {
            new(Guid.NewGuid(), "199001011234", InsuranceType.Car, "Active", 30m, "ABC123")
        };

        var vehicle = new Vehicle("WBA3A5C55DF123456", "ABC123", "BMW", "320i", 2019);

        var mockInsuranceClient = new Mock<IInsuranceServiceClient>();
        mockInsuranceClient
            .Setup(c => c.GetInsurancesAsync("199001011234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        var mockVehicleClient = new Mock<IVehicleServiceClient>();
        mockVehicleClient
            .Setup(c => c.GetVehicleAsync("ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var client = CreateClientWithMocks(mockInsuranceClient, mockVehicleClient);

        // Act
        var response = await client.GetAsync("/customers/199001011234/insurances");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CustomerInsurance>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Single(result);
        var resultVehicle = result[0].Vehicle;
        Assert.NotNull(resultVehicle);
        Assert.Equal("ABC123", resultVehicle.Regnr);
        Assert.Equal("BMW", resultVehicle.Make);
    }

    [Fact]
    public async Task GetCustomerInsurances_VehicleNotFound_ReturnsInsuranceWithoutVehicle()
    {
        // Arrange
        var insurances = new List<InsuranceInfo>
        {
            new(Guid.NewGuid(), "199001011234", InsuranceType.Car, "Active", 30m, "UNKNOWN")
        };

        var mockInsuranceClient = new Mock<IInsuranceServiceClient>();
        mockInsuranceClient
            .Setup(c => c.GetInsurancesAsync("199001011234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        var mockVehicleClient = new Mock<IVehicleServiceClient>();
        mockVehicleClient
            .Setup(c => c.GetVehicleAsync("UNKNOWN", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var client = CreateClientWithMocks(mockInsuranceClient, mockVehicleClient);

        // Act
        var response = await client.GetAsync("/customers/199001011234/insurances");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CustomerInsurance>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Null(result[0].Vehicle);
    }

    [Fact]
    public async Task GetCustomerInsurances_VehicleServiceFails_ReturnsInsuranceWithoutVehicle()
    {
        // Arrange
        var insurances = new List<InsuranceInfo>
        {
            new(Guid.NewGuid(), "199001011234", InsuranceType.Car, "Active", 30m, "ABC123")
        };

        var mockInsuranceClient = new Mock<IInsuranceServiceClient>();
        mockInsuranceClient
            .Setup(c => c.GetInsurancesAsync("199001011234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        var mockVehicleClient = new Mock<IVehicleServiceClient>();
        mockVehicleClient
            .Setup(c => c.GetVehicleAsync("ABC123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var client = CreateClientWithMocks(mockInsuranceClient, mockVehicleClient);

        // Act
        var response = await client.GetAsync("/customers/199001011234/insurances");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<CustomerInsurance>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Null(result[0].Vehicle);
    }

    [Fact]
    public async Task GetCustomerInsurances_CustomerNotFound_Returns404()
    {
        // Arrange
        var mockInsuranceClient = new Mock<IInsuranceServiceClient>();
        mockInsuranceClient
            .Setup(c => c.GetInsurancesAsync("NOTFOUND", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InsuranceInfo>());

        var mockVehicleClient = new Mock<IVehicleServiceClient>();

        var client = CreateClientWithMocks(mockInsuranceClient, mockVehicleClient);

        // Act
        var response = await client.GetAsync("/customers/NOTFOUND/insurances");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerInsurances_InsuranceServiceUnavailable_Returns503()
    {
        // Arrange
        var mockInsuranceClient = new Mock<IInsuranceServiceClient>();
        mockInsuranceClient
            .Setup(c => c.GetInsurancesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var mockVehicleClient = new Mock<IVehicleServiceClient>();

        var client = CreateClientWithMocks(mockInsuranceClient, mockVehicleClient);

        // Act
        var response = await client.GetAsync("/customers/199001011234/insurances");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        // Arrange
        var mockInsuranceClient = new Mock<IInsuranceServiceClient>();
        var mockVehicleClient = new Mock<IVehicleServiceClient>();
        var client = CreateClientWithMocks(mockInsuranceClient, mockVehicleClient);

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
