using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<Character> CreateCharacterAsync(string name, string characterClass, string race);
        Task<Character> GetCharacterAsync(Guid id);
        Task<IEnumerable<Character>> GetAllCharactersAsync();
        Task<Character> UpdateCharacterAsync(Character character);
        Task<bool> DeleteCharacterAsync(Guid id);
        Task<int> RollAbilityCheckAsync(Character character, string ability);
        Task<int> RollSkillCheckAsync(Character character, string skill);
        Task<int> RollSavingThrowAsync(Character character, string ability);
    }
} 