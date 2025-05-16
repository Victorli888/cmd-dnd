using System.Text.Json;
using DndRpg.Core;
using DndRpg.Core.Models;

namespace DndRpg.Infrastructure.Services;

public class DndApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.dnd5eapi.co/api/2014";

    public DndApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<Dictionary<string, int>> GetRaceAbilityBonuses(string race)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/races/{race.ToLower()}");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var raceResponse = JsonSerializer.Deserialize<RaceApiResponse>(content);

        return raceResponse?.AbilityBonuses
            .ToDictionary(
                ab => ab.AbilityScore.Index.ToLower(),
                ab => ab.Bonus
            ) ?? new Dictionary<string, int>();
    }
} 