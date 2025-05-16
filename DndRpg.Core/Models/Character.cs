using System;
using System.Collections.Generic;
using DndRpg.Core.Enums;

namespace DndRpg.Core.Models
{
    /// <summary>
    public class Character
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public CharacterClass Class { get; set; }
        public CharacterRace Race { get; set; }
        public int CurrentHitPoints { get; set; }
        public int MaxHitPoints { get; set; }
        public ICollection<AbilityScore> AbilityScores { get; set; } = new List<AbilityScore>();
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public List<string> Proficiencies { get; set; }
        public List<string> Expertises { get; set; }
        public List<string> Spells { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Character"/> class.
        /// </summary>
        public Character()
        {
            Id = Guid.NewGuid();
            Proficiencies = new List<string>();
            Expertises = new List<string>();
            Spells = new List<string>();
            MaxHitPoints = 0;
        }

        /// <summary>
        /// Gets the ability modifier for a given ability.
        /// </summary>
        /// <param name="ability">The ability to get the modifier for.</param>
        /// <returns>The ability modifier.</returns>
        public int GetAbilityModifier(AbilityScore ability)
        {
            if (!AbilityScores.Contains(ability))
                throw new ArgumentException($"Ability {ability} not found!");
            int modifier = (ability.TotalScore - 10) / 2;
            return modifier;
        }

        /// <summary>
        /// Checks if the character is proficient in a skill.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the character is proficient, false otherwise.</returns>
        public bool IsProficient(string skill)
        {
            //TODO: Calculate proficiency bonus, in DND5e it's based on the level of the character and applies a bonus according to the character level
            return Proficiencies.Contains(skill);
        }

        /// <summary>
        /// Checks if the character is an expert in a skill.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the character is an expert, false otherwise.</returns>
        public bool IsExpert(string skill)
        {
            //TODO: if a skill is in the expertises list, double the proficiency bonus (DEX + (2 * Proficiency Bonus))
            return Expertises.Contains(skill);
        }

        /// <summary>
        public int GetSkillModifier(Skill skill)
        {
            if (!Skills.Contains(skill))
                throw new ArgumentException($"Skill {skill} not found");
            //TODO: Calculate skill modifier

            return skill.Value;
        }
    }
} 