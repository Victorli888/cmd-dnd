using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DndRpg.Core.Enums;
using DndRpg.Core.Models;

namespace DndRpg.Core.Interfaces
{
    /// <summary>
    /// Interface for the character service.
    /// </summary>
    public interface ICharacterService
    {
        /// <summary>
        /// Creates a new character.
        /// </summary>
        /// <param name="name">The name of the character.</param>
        Task<Character> CreateCharacterAsync(
            string name, 
            CharacterClass characterClass, 
            CharacterRace race,
            Dictionary<AbilityScore, int> abilityScores);
        Task<Character> GetCharacterAsync(Guid id);
        Task<IEnumerable<Character>> GetAllCharactersAsync();
        Task<Character> UpdateCharacterAsync(Character character);
        Task<bool> DeleteCharacterAsync(Guid id);
        Task<int> RollAbilityCheckAsync(Character character, AbilityScore ability);
        Task<int> RollSkillCheckAsync(Character character, string skill);
        Task<int> RollSavingThrowAsync(Character character, AbilityScore ability);
    }
} 