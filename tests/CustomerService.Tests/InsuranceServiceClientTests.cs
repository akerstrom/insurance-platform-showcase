using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using CustomerService.Clients;
using CustomerService.Models;
using Xunit;

namespace CustomerService.Tests;

public class InsuranceServiceClientTests
{
    private readonly Mock<ILogger<InsuranceServiceClient>> _loggerMock;
    private readonly JsonSerializerOptions _jsonOptions;

    public InsuranceServiceClientTests()
    {
        _loggerMock = new Mock<ILogger<InsuranceServiceClient>>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private InsuranceServiceClient CreateClient(HttpResponseMessage response)
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
            BaseAddress = new Uri("http://localhost:5002")
        };

        return new InsuranceServiceClient(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetInsurancesAsync_ValidPid_ReturnsInsurances()
    {
        // Arrange
        var insurances = new[]
        {
            new { Id = Guid.NewGuid(), Pid = "199001011234", Type = "Car", Status = "Active", Premium = 30m, Regnr = "ABC123" },
            new { Id = Guid.NewGuid(), Pid = "199001011234", Type = "Pet", Status = "Active", Premium = 10m, Regnr = (string?)null }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(insurances, _jsonOptions))
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var client = CreateClient(response);

        // Act
        var result = await client.GetInsurancesAsync("199001011234");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(InsuranceType.Car, result[0].Type);
        Assert.Equal("ABC123", result[0].Regnr);
        Assert.Equal(InsuranceType.Pet, result[1].Type);
        Assert.Null(result[1].Regnr);
    }

    [Fact]
    public async Task GetInsurancesAsync_NotFound_ReturnsEmptyList()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(response);

        // Act
        var result = await client.GetInsurancesAsync("NOTFOUND");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetInsurancesAsync_ServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var client = CreateClient(response);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetInsurancesAsync("199001011234"));
    }
}
