using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using VehicleService.Clients;
using VehicleService.Models;
using Xunit;

namespace VehicleService.Tests;

public class VehicleDatabaseClientTests
{
    private readonly Mock<ILogger<VehicleDatabaseClient>> _loggerMock;

    public VehicleDatabaseClientTests()
    {
        _loggerMock = new Mock<ILogger<VehicleDatabaseClient>>();
    }

    private VehicleDatabaseClient CreateClient(HttpResponseMessage response)
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
            BaseAddress = new Uri("http://localhost:5101")
        };

        return new VehicleDatabaseClient(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetVehicleAsync_ValidRegnr_ReturnsVehicle()
    {
        // Arrange
        var expectedVehicle = new Vehicle("WBA3A5C55DF123456", "ABC123", "BMW", "320i", 2019);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expectedVehicle))
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var client = CreateClient(response);

        // Act
        var result = await client.GetVehicleAsync("ABC123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedVehicle.Vin, result.Vin);
        Assert.Equal(expectedVehicle.Regnr, result.Regnr);
        Assert.Equal(expectedVehicle.Make, result.Make);
        Assert.Equal(expectedVehicle.Model, result.Model);
        Assert.Equal(expectedVehicle.Year, result.Year);
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
