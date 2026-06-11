using System.Net;
using System.Net.Http.Json;
using cesar.Features.DesignTemplates.Entities;
using cesar.Features.DesignTemplates.Models;
using cesar.Features.JsonKeyStats.Models;
using cesar.Features.LeadIntelligence.Models;
using cesar.Features.RawLead.Models;
using cesar.Features.Weather.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace cesar.Tests;

public class ApiCrudTests
{
    [Fact]
    public async Task RawLeadApi_CoversCrudSearchMissingAndValidationScenarios()
    {
        using var factory = new CesarWebApplicationFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/rawleads", new
        {
            sourceSystem = "web_form",
            externalId = "LEAD-API-1",
            rawJsonData = """{"fullName":"Ana","score":91}"""
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<RawLeadDto>(createResponse);

        var searchResults = await client.GetFromJsonAsync<List<RawLeadDto>>("/api/rawleads?q=LEAD-API-1");
        Assert.Contains(searchResults!, item => item.Id == created.Id);

        var getResponse = await client.GetAsync($"/api/rawleads/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var missingGet = await client.GetAsync("/api/rawleads/999999");
        Assert.Equal(HttpStatusCode.NotFound, missingGet.StatusCode);

        var invalidPost = await client.PostAsJsonAsync("/api/rawleads", new
        {
            sourceSystem = "web_form",
            externalId = "BAD-JSON",
            rawJsonData = "not-json"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidPost.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/rawleads/{created.Id}", new
        {
            id = created.Id,
            sourceSystem = "crm_sync",
            externalId = "LEAD-API-2",
            rawJsonData = """{"fullName":"Ana Updated","score":95}"""
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<RawLeadDto>(updateResponse);
        Assert.Equal("crm_sync", updated.SourceSystem);

        var missingPut = await client.PutAsJsonAsync("/api/rawleads/999999", new
        {
            id = 999999,
            sourceSystem = "crm_sync",
            externalId = "MISSING",
            rawJsonData = """{"fullName":"Missing"}"""
        });
        Assert.Equal(HttpStatusCode.NotFound, missingPut.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/rawleads/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingDelete = await client.DeleteAsync($"/api/rawleads/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingDelete.StatusCode);
    }

    [Fact]
    public async Task JsonKeyStatApi_CoversCrudSearchMissingAndValidationScenarios()
    {
        using var factory = new CesarWebApplicationFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/jsonkeystats", new
        {
            key = "apiKey",
            occurrences = 3
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<JsonKeyStatDto>(createResponse);

        var searchResults = await client.GetFromJsonAsync<List<JsonKeyStatDto>>("/api/jsonkeystats?q=apiKey");
        Assert.Contains(searchResults!, item => item.Id == created.Id);

        var getResponse = await client.GetAsync($"/api/jsonkeystats/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var missingGet = await client.GetAsync("/api/jsonkeystats/999999");
        Assert.Equal(HttpStatusCode.NotFound, missingGet.StatusCode);

        var invalidPost = await client.PostAsJsonAsync("/api/jsonkeystats", new
        {
            key = "",
            occurrences = -1
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidPost.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/jsonkeystats/{created.Id}", new
        {
            id = created.Id,
            key = "apiKeyUpdated",
            occurrences = 8
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<JsonKeyStatDto>(updateResponse);
        Assert.Equal("apiKeyUpdated", updated.Key);

        var missingPut = await client.PutAsJsonAsync("/api/jsonkeystats/999999", new
        {
            id = 999999,
            key = "missing",
            occurrences = 1
        });
        Assert.Equal(HttpStatusCode.NotFound, missingPut.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/jsonkeystats/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingDelete = await client.DeleteAsync($"/api/jsonkeystats/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingDelete.StatusCode);
    }

    [Fact]
    public async Task LeadIntelligenceApi_CoversCrudSearchMissingAndValidationScenarios()
    {
        using var factory = new CesarWebApplicationFactory();
        using var client = factory.CreateClient();
        var lead = await CreateRawLeadAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/leadintelligence", new
        {
            leadId = lead.Id,
            contentHash = "abc123",
            familiarityIndex = 0.4,
            dataDensityScore = 0.7
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<LeadIntelligenceDto>(createResponse);

        var searchResults = await client.GetFromJsonAsync<List<LeadIntelligenceDto>>("/api/leadintelligence?q=abc123");
        Assert.Contains(searchResults!, item => item.Id == created.Id);

        var getResponse = await client.GetAsync($"/api/leadintelligence/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var missingGet = await client.GetAsync("/api/leadintelligence/999999");
        Assert.Equal(HttpStatusCode.NotFound, missingGet.StatusCode);

        var invalidPost = await client.PostAsJsonAsync("/api/leadintelligence", new
        {
            leadId = 0,
            contentHash = "",
            familiarityIndex = 2.0,
            dataDensityScore = -1.0
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidPost.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/leadintelligence/{created.Id}", new
        {
            id = created.Id,
            leadId = lead.Id,
            contentHash = "def456",
            familiarityIndex = 0.6,
            dataDensityScore = 0.8
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<LeadIntelligenceDto>(updateResponse);
        Assert.Equal("def456", updated.ContentHash);

        var missingPut = await client.PutAsJsonAsync("/api/leadintelligence/999999", new
        {
            id = 999999,
            leadId = lead.Id,
            contentHash = "missing",
            familiarityIndex = 0.5,
            dataDensityScore = 0.5
        });
        Assert.Equal(HttpStatusCode.NotFound, missingPut.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/leadintelligence/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingDelete = await client.DeleteAsync($"/api/leadintelligence/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingDelete.StatusCode);
    }

    [Fact]
    public async Task DesignTemplateApi_CoversCrudSearchMissingAndValidationScenarios()
    {
        using var factory = new CesarWebApplicationFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/designtemplates", new
        {
            name = "API Mail",
            contentType = ContentType.Mail,
            htmlMarkup = "<p>Hello {{fullName}}</p>",
            placeholderSchema = """{"fullName":""}"""
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<DesignTemplateDto>(createResponse);

        var searchResults = await client.GetFromJsonAsync<List<DesignTemplateDto>>("/api/designtemplates?q=API%20Mail");
        Assert.Contains(searchResults!, item => item.Id == created.Id);

        var getResponse = await client.GetAsync($"/api/designtemplates/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var missingGet = await client.GetAsync("/api/designtemplates/999999");
        Assert.Equal(HttpStatusCode.NotFound, missingGet.StatusCode);

        var invalidPost = await client.PostAsJsonAsync("/api/designtemplates", new
        {
            name = "Invalid",
            contentType = ContentType.Mail,
            htmlMarkup = "<p>Invalid</p>",
            placeholderSchema = "not-json"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidPost.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/designtemplates/{created.Id}", new
        {
            id = created.Id,
            name = "API Mail Updated",
            contentType = ContentType.HTML,
            htmlMarkup = "<strong>{{fullName}}</strong>",
            placeholderSchema = """{"fullName":"Ana"}"""
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<DesignTemplateDto>(updateResponse);
        Assert.Equal("API Mail Updated", updated.Name);

        var missingPut = await client.PutAsJsonAsync("/api/designtemplates/999999", new
        {
            id = 999999,
            name = "Missing",
            contentType = ContentType.Mail,
            htmlMarkup = "<p>Missing</p>",
            placeholderSchema = "{}"
        });
        Assert.Equal(HttpStatusCode.NotFound, missingPut.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/designtemplates/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingDelete = await client.DeleteAsync($"/api/designtemplates/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingDelete.StatusCode);
    }

    [Fact]
    public async Task WeatherForecastApi_CoversCrudSearchMissingAndValidationScenarios()
    {
        using var factory = new CesarWebApplicationFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/weatherforecasts", new
        {
            date = new DateOnly(2026, 6, 11),
            temperatureC = 22,
            summary = "Clear API"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<WeatherForecastDto>(createResponse);

        var searchResults = await client.GetFromJsonAsync<List<WeatherForecastDto>>("/api/weatherforecasts?q=Clear");
        Assert.Contains(searchResults!, item => item.Id == created.Id);

        var getResponse = await client.GetAsync($"/api/weatherforecasts/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var missingGet = await client.GetAsync("/api/weatherforecasts/999999");
        Assert.Equal(HttpStatusCode.NotFound, missingGet.StatusCode);

        var invalidPost = await client.PostAsJsonAsync("/api/weatherforecasts", new
        {
            date = new DateOnly(2026, 6, 11),
            temperatureC = 120,
            summary = "Too hot"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidPost.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/weatherforecasts/{created.Id}", new
        {
            id = created.Id,
            date = new DateOnly(2026, 6, 12),
            temperatureC = 18,
            summary = "Cloudy API"
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<WeatherForecastDto>(updateResponse);
        Assert.Equal("Cloudy API", updated.Summary);

        var missingPut = await client.PutAsJsonAsync("/api/weatherforecasts/999999", new
        {
            id = 999999,
            date = new DateOnly(2026, 6, 12),
            temperatureC = 18,
            summary = "Missing"
        });
        Assert.Equal(HttpStatusCode.NotFound, missingPut.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/weatherforecasts/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingDelete = await client.DeleteAsync($"/api/weatherforecasts/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingDelete.StatusCode);
    }

    [Fact]
    public async Task ProtectedApiWriteEndpoint_ReturnsUnauthorized_WhenUserIsAnonymous()
    {
        using var factory = new CesarWebApplicationFactory(authenticated: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.PostAsJsonAsync("/api/weatherforecasts", new
        {
            date = new DateOnly(2026, 6, 11),
            temperatureC = 22,
            summary = "Anonymous"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<RawLeadDto> CreateRawLeadAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/rawleads", new
        {
            sourceSystem = "web_form",
            externalId = $"LEAD-{Guid.NewGuid():N}",
            rawJsonData = """{"fullName":"Lead","score":42}"""
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadJsonAsync<RawLeadDto>(response);
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>();
        Assert.NotNull(value);
        return value;
    }
}
