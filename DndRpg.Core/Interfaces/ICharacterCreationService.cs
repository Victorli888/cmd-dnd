using DndRpg.Core.Enums;
using DndRpg.Core.Models;

namespace DndRpg.Core;

public interface ICharacterCreationService
{
    Task<Character> CreateCharacterAsync(
        string name, 
        CharacterClass characterClass, 
        CharacterRace race,
        Dictionary<Abilities, int> abilityScores);
}