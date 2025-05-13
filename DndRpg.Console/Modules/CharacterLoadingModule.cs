using DndRpg.Core.Interfaces;
using DndRpg.Core.Models;
using Microsoft.Extensions.Logging;

namespace DndRpg.Console.Modules
{
    public class CharacterLoadingModule
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ILogger<CharacterLoadingModule> _logger;
        private List<Character> _loadedCharacters;

        public CharacterLoadingModule(
            ICharacterRepository characterRepository,
            ILogger<CharacterLoadingModule> logger)
        {
            _characterRepository = characterRepository;
            _logger = logger;
            _loadedCharacters = new List<Character>();
        }

        public async Task LoadCharactersAsync()
        {
            try
            {
                _loadedCharacters = (await _characterRepository.GetAllAsync()).ToList();
                DisplayLoadedCharacters();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading characters from database");
                System.Console.WriteLine("Error loading characters from database.");
            }
        }

        private void DisplayLoadedCharacters()
        {
            if (!_loadedCharacters.Any())
            {
                System.Console.WriteLine("\nNo characters found in database.");
                return;
            }

            System.Console.WriteLine("\n=== Loaded Characters ===");
            foreach (var character in _loadedCharacters)
            {
                System.Console.WriteLine($"{character.Name} - {character.Class}");
            }
        }

        public Character GetCharacterByName(string name)
        {
            return _loadedCharacters.FirstOrDefault(c => 
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public List<Character> GetAllCharacters()
        {
            return _loadedCharacters;
        }
    }
}