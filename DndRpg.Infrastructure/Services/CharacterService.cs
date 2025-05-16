using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DndRpg.Core;
using DndRpg.Core.Interfaces;
using DndRpg.Core.Models;
using DndRpg.Core.Enums;
using Microsoft.Extensions.Logging;
using System.Linq;

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
        private readonly IRaceService _raceService;

        /// <summary>
        /// Initializes a new instance of the CharacterService class.
        /// </summary>
        /// <param name="apiClient">The API client to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="characterRepository">The character repository to use.</param>
        /// <param name="raceService">The race service to use.</param>
        public CharacterService(
            IDndApiClient apiClient,
            ILogger<CharacterService> logger,
            ICharacterRepository characterRepository,
            IRaceService raceService)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _characterRepository = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));
            _raceService = raceService ?? throw new ArgumentNullException(nameof(raceService));
        }

        /// <summary>
        /// Creates a new character.
        /// </summary>
        /// <param name="name">The name of the character.</param>
        /// <param name="characterClass">The class of the character.</param>
        /// <param name="race">The race of the character.</param>
        /// <param name="baseAbilityScores">The base ability scores of the character.</param>
        public async Task<Character> CreateCharacterAsync(
            string name,
            CharacterClass characterClass,
            CharacterRace race,
            Dictionary<Abilities, int> baseAbilityScores)
        {
            var character = new Character
            {
                Id = Guid.NewGuid(),
                Name = name,
                Class = characterClass,
                Race = race,
                Level = 1,
                CurrentHitPoints = 10,
                MaxHitPoints = 10
            };

            // Create ability scores with base scores
            character.AbilityScores = baseAbilityScores.Select(kvp => new AbilityScore
            {
                CharacterId = character.Id,
                Ability = kvp.Key,
                BaseScore = kvp.Value,
                BonusScore = 0
            }).ToList();

            _logger.LogInformation("Creating character with base scores: {Scores}", 
                string.Join(", ", character.AbilityScores.Select(a => $"{a.Ability}: {a.BaseScore}")));

            // Save the character with base scores
            character = await _characterRepository.CreateAsync(character);
            _logger.LogInformation("Character created with ID: {Id}", character.Id);

            // Get racial bonuses and update
            var racialBonuses = await _raceService.GetAbilityBonusesAsync(race);
            _logger.LogInformation("Retrieved racial bonuses: {Bonuses}", 
                string.Join(", ", racialBonuses.Select(kv => $"{kv.Key}: {kv.Value}")));

            bool anyUpdates = false;
            foreach (var abilityScore in character.AbilityScores)
            {
                if (racialBonuses.TryGetValue(abilityScore.Ability, out int bonus))
                {
                    abilityScore.BonusScore = bonus;  // Simply set the bonus value
                    anyUpdates = true;
                    _logger.LogInformation("Updated {Ability} bonus score to {Bonus}", 
                        abilityScore.Ability, bonus);
                }
            }

            if (anyUpdates)
            {
                _logger.LogInformation("Updating character with new bonus scores");
                character = await _characterRepository.UpdateAsync(character);
                
                // Verify the update
                var updatedCharacter = await _characterRepository.GetByIdAsync(character.Id);
                _logger.LogInformation("Character updated. Final scores: {Scores}", 
                    string.Join(", ", updatedCharacter.AbilityScores.Select(a => 
                        $"{a.Ability}: {a.BaseScore} + {a.BonusScore} = {a.TotalScore}")));
            }
            else
            {
                _logger.LogWarning("No bonus scores were updated");
            }

            return character;
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
        public Task<int> RollSkillCheckAsync(Character character, Skill skill)
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