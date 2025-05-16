using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DndRpg.Core.Interfaces;
using DndRpg.Core.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using DndRpg.Core;
using DndRpg.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DndRpg.Infrastructure.Data
{
    public class SqliteCharacterRepository : ICharacterRepository
    {
        private readonly DndDbContext _context;
        private readonly ILogger<SqliteCharacterRepository> _logger;

        public SqliteCharacterRepository(DndDbContext context, ILogger<SqliteCharacterRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Character> CreateAsync(Character character)
        {
            await _context.Characters.AddAsync(character);
            await _context.SaveChangesAsync();
            return character;
        }

        public async Task<Character> GetByIdAsync(Guid id)
        {
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id);

            if (character != null)
            {
                await _context.Entry(character)
                    .Collection(c => c.AbilityScores)
                    .LoadAsync();
                    
                await _context.Entry(character)
                    .Collection(c => c.Skills)
                    .LoadAsync();
            }

            return character;
        }

        public async Task<IEnumerable<Character>> GetAllAsync()
        {
            // First get all characters
            var characters = await _context.Characters.ToListAsync();
            
            // Then load the related collections for each character
            foreach (var character in characters)
            {
                await _context.Entry(character)
                    .Collection(c => c.AbilityScores)
                    .LoadAsync();
                    
                await _context.Entry(character)
                    .Collection(c => c.Skills)
                    .LoadAsync();
            }
            
            return characters;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character == null) return false;
            
            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Character> UpdateAsync(Character character)
        {
            _logger.LogInformation("Updating character {Id} with ability scores: {Scores}", 
                character.Id, 
                string.Join(", ", character.AbilityScores.Select(a => 
                    $"{a.Ability}: {a.BaseScore} + {a.BonusScore} = {a.TotalScore}")));

            _context.Characters.Update(character);
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Database update completed with {Count} changes", result);

            // Reload the character to ensure we have the latest data
            var updatedCharacter = await GetByIdAsync(character.Id);
            _logger.LogInformation("Character reloaded. Current scores: {Scores}", 
                string.Join(", ", updatedCharacter.AbilityScores.Select(a => 
                    $"{a.Ability}: {a.BaseScore} + {a.BonusScore} = {a.TotalScore}")));
            
            return updatedCharacter;
        }
    }
}
