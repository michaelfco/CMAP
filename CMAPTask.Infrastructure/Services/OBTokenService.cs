using CMAPTask.Domain.Entities.OB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Domain.Interfaces;
using OpenBanking.Infrastructure.Extensions;
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
    private readonly IApiSettingsRepository _apiSettings;
    private readonly IDapperGenericRepository _repo;


    public OBTokenService(HttpClient httpClient, IOptions<OBSettings> options, IApiSettingsRepository apiSettings,IDapperGenericRepository repo)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _apiSettings = apiSettings;    
        _repo = repo;
    }

    public async Task<OBTokenResponse> GetTokenAsync(Guid? userId)
    {
        var user = await _repo.GetByIdAsync<User>("Users", "UserId", userId);
        var apiConfig = await _apiSettings.GetByEnvironment(user.UseCredentialId);

        var requestBody = new
        {
            secret_id = apiConfig?.SecretID ?? _settings.SecretID,
            secret_key = apiConfig?.SecretKey ?? _settings.SecretKey
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
