using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using CustomerService.Clients;
using CustomerService.Models;
using Xunit;

namespace CustomerService.Tests;

public class VehicleServiceClientTests
{
    private readonly Mock<ILogger<VehicleServiceClient>> _loggerMock;

    public VehicleServiceClientTests()
    {
        _loggerMock = new Mock<ILogger<VehicleServiceClient>>();
    }

    private VehicleServiceClient CreateClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        return new VehicleServiceClient(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetVehicleAsync_ValidRegnr_ReturnsVehicle()
    {
        // Arrange
        var vehicle = new Vehicle("WBA3A5C55DF123456", "ABC123", "BMW", "320i", 2019);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(vehicle))
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var client = CreateClient(response);

        // Act
        var result = await client.GetVehicleAsync("ABC123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WBA3A5C55DF123456", result.Vin);
        Assert.Equal("ABC123", result.Regnr);
        Assert.Equal("BMW", result.Make);
    }

    [Fact]
    public async Task GetVehicleAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(response);

        // Act
        var result = await client.GetVehicleAsync("NOTFOUND");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetVehicleAsync_ServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var client = CreateClient(response);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetVehicleAsync("ABC123"));
    }
}
