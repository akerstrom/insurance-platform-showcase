using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using InsuranceService.Clients;
using InsuranceService.Models;
using Xunit;

namespace InsuranceService.Tests;

public class InsuranceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public InsuranceEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private HttpClient CreateClientWithMock(Mock<IInsuranceMainframeClient> mockClient)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IInsuranceMainframeClient));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddSingleton(mockClient.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetInsurances_ValidPid_Returns200WithInsurances()
    {
        // Arrange
        var insurances = new List<Insurance>
        {
            new(Guid.NewGuid(), "199001011234", InsuranceType.Car, "Active", 30m, "ABC123"),
            new(Guid.NewGuid(), "199001011234", InsuranceType.Pet, "Active", 10m)
        };

        var mockClient = new Mock<IInsuranceMainframeClient>();
        mockClient
            .Setup(c => c.GetInsurancesAsync("199001011234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        var client = CreateClientWithMock(mockClient);

        // Act
        var response = await client.GetAsync("/insurances/199001011234");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<Insurance>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetInsurances_NotFound_Returns404()
    {
        // Arrange
        var mockClient = new Mock<IInsuranceMainframeClient>();
        mockClient
            .Setup(c => c.GetInsurancesAsync("NOTFOUND", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Insurance>());

        var client = CreateClientWithMock(mockClient);

        // Act
        var response = await client.GetAsync("/insurances/NOTFOUND");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInsurances_LegacyServiceUnavailable_Returns503()
    {
        // Arrange
        var mockClient = new Mock<IInsuranceMainframeClient>();
        mockClient
            .Setup(c => c.GetInsurancesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var client = CreateClientWithMock(mockClient);

        // Act
        var response = await client.GetAsync("/insurances/199001011234");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task GetInsurances_CarInsuranceIncludesRegnr()
    {
        // Arrange
        var insurances = new List<Insurance>
        {
            new(Guid.NewGuid(), "199001011234", InsuranceType.Car, "Active", 30m, "ABC123")
        };

        var mockClient = new Mock<IInsuranceMainframeClient>();
        mockClient
            .Setup(c => c.GetInsurancesAsync("199001011234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        var client = CreateClientWithMock(mockClient);

        // Act
        var response = await client.GetAsync("/insurances/199001011234");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<Insurance>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("ABC123", result[0].Regnr);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        // Arrange
        var mockClient = new Mock<IInsuranceMainframeClient>();
        var client = CreateClientWithMock(mockClient);

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
