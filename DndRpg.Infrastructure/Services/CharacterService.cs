using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DndRpg.Core.Interfaces;
using DndRpg.Core.Models;
using Microsoft.Extensions.Logging;

namespace DndRpg.Infrastructure.Services
{
    /// <summary>
    /// Service class for creating and managing characters.
    /// </summary>
    public class CharacterService : ICharacterService
    {
        private readonly IDndApiClient _apiClient;
        private readonly ILogger<CharacterService> _logger;
        private readonly Dictionary<Guid, Character> _characters;

        /// <summary>
        /// Initializes a new instance of the CharacterService class.
        /// </summary>
        /// <param name="apiClient">The API client to use.</param>
        /// <param name="logger">The logger to use.</param>
        public CharacterService(IDndApiClient apiClient, ILogger<CharacterService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _characters = new Dictionary<Guid, Character>();
        }

        /// <summary>
        /// Creates a new character.
        /// </summary>
        /// <param name="name">The name of the character.</param>
        /// <param name="characterClass">The class of the character.</param>
        /// <param name="race">The race of the character.</param>
        public async Task<Character> CreateCharacterAsync(string name, string characterClass, string race)
        {
            try
            {
                // Get class and race details from the API
                var classDetails = await _apiClient.GetAsync<object>($"classes/{characterClass.ToLower()}");
                var raceDetails = await _apiClient.GetAsync<object>($"races/{race.ToLower()}");

                var character = new Character
                {
                    Name = name,
                    Class = characterClass,
                    Race = race,
                    Level = 1,
                    CurrentHitPoints = 10,
                    MaxHitPoints = 10
                };

                _characters.Add(character.Id, character);
                return character;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating character {Name}", name);
                throw;
            }
        }

        /// <summary>
        /// Gets a character by ID. Later Refactor to save peristent data to a database.
        /// </summary>
        /// <param name="id">The ID of the character.</param>
        /// <returns>The character.</returns>
        public Task<Character> GetCharacterAsync(Guid id)
        {
            if (_characters.TryGetValue(id, out var character))
            {
                return Task.FromResult(character);
            }
            return Task.FromResult<Character>(null);
        }

        public Task<IEnumerable<Character>> GetAllCharactersAsync()
        {
            return Task.FromResult<IEnumerable<Character>>(_characters.Values);
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

            if (!_characters.ContainsKey(character.Id))
                throw new KeyNotFoundException($"Character with ID {character.Id} not found");

            _characters[character.Id] = character;
            return Task.FromResult(character);
        }

        /// <summary>
        /// Deletes a character, later refactor to save persistent data to a database.
        /// </summary>
        /// <param name="id">The ID of the character.</param>
        /// <returns>True if the character was deleted, false otherwise.</returns>
        public Task<bool> DeleteCharacterAsync(Guid id)
        {
            return Task.FromResult(_characters.Remove(id));
        }

        /// <summary>
        /// Rolls an ability check. TODO: Should this be refactored to its own class?
        /// </summary>
        /// <param name="character">The character to roll the ability check for.</param>
        /// <param name="ability">The ability to roll the check for.</param>
        /// <returns>The result of the ability check.</returns>
        public Task<int> RollAbilityCheckAsync(Character character, string ability)
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
        public Task<int> RollSavingThrowAsync(Character character, string ability)
        {
            var random = new Random();
            var roll = random.Next(1, 21); // d20 roll
            var modifier = character.GetAbilityModifier(ability);
            return Task.FromResult(roll + modifier);
        }
    }
} 