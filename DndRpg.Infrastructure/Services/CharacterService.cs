using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DndRpg.Core;
using DndRpg.Core.Interfaces;
using DndRpg.Core.Models;
using DndRpg.Core.Enums;
using Microsoft.Extensions.Logging;

namespace DndRpg.Infrastructure.Services
{
    /// <summary>
    /// Service class for creating and managing characters.
    /// </summary>
    public class CharacterService : ICharacterCreationService
    {
        private readonly IDndApiClient _apiClient;
        private readonly ILogger<CharacterService> _logger;
        private readonly ICharacterRepository _characterRepository;

        /// <summary>
        /// Initializes a new instance of the CharacterService class.
        /// </summary>
        /// <param name="apiClient">The API client to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="characterRepository">The character repository to use.</param>
        public CharacterService(
            IDndApiClient apiClient,
            ICharacterRepository characterRepository)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _characterRepository = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));
        }

        /// <summary>
        /// Creates a new character.
        /// </summary>
        /// <param name="name">The name of the character.</param>
        /// <param name="characterClass">The class of the character.</param>
        /// <param name="race">The race of the character.</param>
        /// <param name="abilityScores">The ability scores of the character.</param>
        public async Task<Character> CreateCharacterAsync(
            string name,
            CharacterClass characterClass,
            CharacterRace race,
            Dictionary<AbilityScore, int> abilityScores)
        {
            var character = new Character
            {
                Id = Guid.NewGuid(),
                Name = name,
                Class = characterClass,
                Race = race,
                AbilityScores = abilityScores,
                Level = 1,
                MaxHitPoints = CalculateStartingHitPoints(characterClass, abilityScores),
                CurrentHitPoints = CalculateStartingHitPoints(characterClass, abilityScores)
            };

            return await _characterRepository.CreateAsync(character);
        }

        private int CalculateAbilityModifier(int abilityScore)
        {
            // D&D 5e ability modifier formula: (score - 10) / 2, rounded down
            return (abilityScore - 10) / 2;
        }

        /// <summary>
        /// Gets a character by ID. Later Refactor to save peristent data to a database.
        /// </summary>
        /// <param name="id">The ID of the character.</param>
        /// <returns>The character.</returns>
        public Task<Character> GetCharacterAsync(Guid id)
        {
            return _characterRepository.GetByIdAsync(id);
        }

        public Task<IEnumerable<Character>> GetAllCharactersAsync()
        {
            return _characterRepository.GetAllAsync();
        }

        /// <summary>
        /// Updates a character, later refactor to save persistent data to a database.
        /// </summary>
        /// <param name="character">The character to update.</param>
        /// <returns>The updated character.</returns>
        public Task<Character> UpdateCharacterAsync(Character character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            return _characterRepository.UpdateAsync(character);
        }

        /// <summary>
        /// Deletes a character, later refactor to save persistent data to a database.
        /// </summary>
        /// <param name="id">The ID of the character.</param>
        /// <returns>True if the character was deleted, false otherwise.</returns>
        public Task<bool> DeleteCharacterAsync(Guid id)
        {
            return _characterRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Rolls an ability check. TODO: Should this be refactored to its own class?
        /// </summary>
        /// <param name="character">The character to roll the ability check for.</param>
        /// <param name="ability">The ability to roll the check for.</param>
        /// <returns>The result of the ability check.</returns>
        public Task<int> RollAbilityCheckAsync(Character character, AbilityScore ability)
        {
            var random = new Random();
            var roll = random.Next(1, 21); // d20 roll
            var modifier = character.GetAbilityModifier(ability);
            return Task.FromResult(roll + modifier);
        }

        /// <summary>
        /// Rolls a skill check. TODO: Should this be refactored to its own class?
        /// </summary>
        /// <param name="character">The character to roll the skill check for.</param>
        /// <param name="skill">The skill to roll the check for.</param>
        /// <returns>The result of the skill check.</returns>
        public Task<int> RollSkillCheckAsync(Character character, string skill)
        {
            var random = new Random();
            var roll = random.Next(1, 21); // d20 roll
            var modifier = character.GetSkillModifier(skill);
            return Task.FromResult(roll + modifier);
        }

        /// <summary>
        /// Rolls a saving throw. TODO: Should this be refactored to its own class?
        /// </summary>
        /// <param name="character">The character to roll the saving throw for.</param>
        /// <param name="ability">The ability to roll the throw for.</param>
        /// <returns>The result of the saving throw.</returns>
        public Task<int> RollSavingThrowAsync(Character character, AbilityScore ability)
        {
            var random = new Random();
            var roll = random.Next(1, 21); // d20 roll
            var modifier = character.GetAbilityModifier(ability);
            return Task.FromResult(roll + modifier);
        }

        private int CalculateStartingHitPoints(CharacterClass characterClass, Dictionary<AbilityScore, int> abilityScores)
        {
            // Implement the logic to calculate starting hit points based on character class and ability scores
            // This is a placeholder and should be replaced with the actual implementation
            return 10; // Placeholder return, actual implementation needed
        }
    }
} 