using DndRpg.Core;
using DndRpg.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DndRpg.Infrastructure.Services;

public class RaceService : IRaceService
{
    private readonly IDndApiClient _apiClient;
    private readonly ILogger<RaceService> _logger;
    private readonly Dictionary<CharacterRace, string> _raceToApiIndex;
    
    private static readonly Dictionary<string, Abilities> _abilityScoreMapping = new()
    {
        { "str", Abilities.Strength },
        { "dex", Abilities.Dexterity },
        { "con", Abilities.Constitution },
        { "int", Abilities.Intelligence },
        { "wis", Abilities.Wisdom },
        { "cha", Abilities.Charisma }
    };

    public RaceService(IDndApiClient apiClient, ILogger<RaceService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
        
        //TODO: Is this the correct way to implement CharacterRaceEnum? since they are constant cant we directly use them
        // Rather than map them to a string?
        _raceToApiIndex = new Dictionary<CharacterRace, string>
        {
            { CharacterRace.Human, "human" },
            { CharacterRace.Elf, "elf" },
            { CharacterRace.Dwarf, "dwarf" },
            // Add other races as needed
        };
    }

    public async Task<Dictionary<Abilities, int>> GetAbilityBonusesAsync(CharacterRace race)
    {
        try
        {
            var raceDetails = await GetRaceCharacteristicsAsync(race);
            _logger.LogInformation("Raw API Response: {Response}", 
                JsonSerializer.Serialize(raceDetails));
            
            _logger.LogInformation("Ability Bonuses from API: {Bonuses}", 
                JsonSerializer.Serialize(raceDetails.AbilityBonuses));
            
            var bonuses = new Dictionary<Abilities, int>();

            foreach (var abilityBonus in raceDetails.AbilityBonuses)
            {
                var abilityIndex = abilityBonus.AbilityScore.Index.ToLower();
                _logger.LogInformation("Processing ability bonus: {Index} with bonus {Bonus}", 
                    abilityIndex, abilityBonus.Bonus);
                
                if (_abilityScoreMapping.TryGetValue(abilityIndex, out var ability))
                {
                    bonuses[ability] = abilityBonus.Bonus;
                    _logger.LogInformation("Successfully mapped {Index} to {Ability} with bonus {Bonus}", 
                        abilityIndex, ability, abilityBonus.Bonus);
                }
                else
                {
                    _logger.LogWarning("Failed to map ability score {Index}", abilityIndex);
                }
            }

            _logger.LogInformation("Final bonuses dictionary: {Bonuses}", 
                string.Join(", ", bonuses.Select(kv => $"{kv.Key}: {kv.Value}")));
            return bonuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ability bonuses for race {Race}", race);
            throw;
        }
    }

    public async Task<RaceApiResponse> GetRaceCharacteristicsAsync(CharacterRace race)
    {
        if (!_raceToApiIndex.TryGetValue(race, out var apiIndex))
        {
            throw new ArgumentException($"No API index found for race {race}");
        }

        try
        {
            var response = await _apiClient.GetAsync<RaceApiResponse>($"races/{apiIndex}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching race details for {Race}", race);
            throw;
        }
    }
}