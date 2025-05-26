using CMAPTask.Domain.Entities.OB;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CMAPTask.Infrastructure.Services;

public class OBTokenService
{
    private readonly HttpClient _httpClient;
    private readonly OBSettings _settings;

    public OBTokenService(HttpClient httpClient, IOptions<OBSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<OBTokenResponse> GetTokenAsync()
    {
        var requestBody = new
        {
            secret_id = _settings.SecretID,
            secret_key = _settings.SecretKey
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("token/new/", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        var token = JsonSerializer.Deserialize<OBTokenResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return token ?? throw new Exception("Failed to deserialize OB token response.");
    }
}
