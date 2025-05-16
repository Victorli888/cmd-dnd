namespace DndRpg.Core.Interfaces;

public interface IRaceService
{
    Task<Dictionary<Abilities, int>> GetAbilityBonusesAsync(CharacterRace race);
    Task<RaceApiResponse> GetRaceCharacteristicsAsync(CharacterRace race);
}