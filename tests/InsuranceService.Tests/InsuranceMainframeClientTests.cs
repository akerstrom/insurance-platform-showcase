using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using InsuranceService.Clients;
using InsuranceService.Models;
using Xunit;

namespace InsuranceService.Tests;

public class InsuranceMainframeClientTests
{
    private readonly Mock<ILogger<InsuranceMainframeClient>> _loggerMock;
    private readonly JsonSerializerOptions _jsonOptions;

    public InsuranceMainframeClientTests()
    {
        _loggerMock = new Mock<ILogger<InsuranceMainframeClient>>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private InsuranceMainframeClient CreateClient(HttpResponseMessage response)
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
            BaseAddress = new Uri("http://localhost:5102")
        };

        return new InsuranceMainframeClient(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetInsurancesAsync_ValidPid_ReturnsInsurances()
    {
        // Arrange
        var policies = new[]
        {
            new { Id = Guid.NewGuid(), Pid = "199001011234", Type = "Car", Status = "Active", Premium = 30m, Regnr = "ABC123" },
            new { Id = Guid.NewGuid(), Pid = "199001011234", Type = "Pet", Status = "Active", Premium = 10m, Regnr = (string?)null }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(policies, _jsonOptions))
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var client = CreateClient(response);

        // Act
        var result = await client.GetInsurancesAsync("199001011234");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Type == InsuranceType.Car && i.Regnr == "ABC123");
        Assert.Contains(result, i => i.Type == InsuranceType.Pet && i.Regnr == null);
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

    [Fact]
    public async Task GetInsurancesAsync_TranslatesAllInsuranceTypes()
    {
        // Arrange
        var policies = new[]
        {
            new { Id = Guid.NewGuid(), Pid = "199001011234", Type = "Car", Status = "Active", Premium = 30m, Regnr = "ABC123" },
            new { Id = Guid.NewGuid(), Pid = "199001011234", Type = "Pet", Status = "Active", Premium = 10m, Regnr = (string?)null },
            new { Id = Guid.NewGuid(), Pid = "199001011234", Type = "Health", Status = "Active", Premium = 20m, Regnr = (string?)null }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(policies, _jsonOptions))
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var client = CreateClient(response);

        // Act
        var result = await client.GetInsurancesAsync("199001011234");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, i => i.Type == InsuranceType.Car && i.Premium == 30m);
        Assert.Contains(result, i => i.Type == InsuranceType.Pet && i.Premium == 10m);
        Assert.Contains(result, i => i.Type == InsuranceType.Health && i.Premium == 20m);
    }
}
