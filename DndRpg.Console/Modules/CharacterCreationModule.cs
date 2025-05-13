using DndRpg.Core;
using DndRpg.Core.Enums;
using DndRpg.Core.Models;

namespace DndRpg.Console.Modules;

public class CharacterCreationModule
{
    private readonly ICharacterCreationService _characterService;
    private readonly Random _random;

    public CharacterCreationModule(ICharacterCreationService characterService)
    {
        _characterService = characterService;
        _random = new Random();
    }

    public async Task<Character> CreateCharacterAsync()
    {
        System.Console.WriteLine("\n=== Character Creation ===");
        
        var name = GetCharacterName();
        if (string.IsNullOrEmpty(name)) return null;

        var race = SetCharacterRace();
        var characterClass = SetCharacterClass();
        var abilityScores = RollForRandomAbilityScores();

        try
        {
            var character = await _characterService.CreateCharacterAsync(
                name,
                characterClass,
                race,
                abilityScores
            );

            DisplayCharacterSummary(character);
            return character;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error creating character: {ex.Message}");
            return null;
        }
    }

    private string GetCharacterName()
    {
        System.Console.Write("\nEnter character name: ");
        var name = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            System.Console.WriteLine("Name cannot be empty.");
            return null;
        }
        return name;
    }

    private CharacterRace SetCharacterRace()
    {
        System.Console.WriteLine("\nAvailable Races:");
        var races = Enum.GetNames(typeof(CharacterRace));
        for (int i = 0; i < races.Length; i++)
        {
            System.Console.WriteLine($"{i + 1}. {races[i]}");
        }
        
        while (true)
        {
            System.Console.Write($"\nSelect race (1-{races.Length}): ");
            if (int.TryParse(System.Console.ReadLine(), out int raceChoice) && 
                raceChoice > 0 && raceChoice <= races.Length)
            {
                return (CharacterRace)(raceChoice - 1);
            }
            System.Console.WriteLine("Invalid selection. Please try again.");
        }
    }

    private CharacterClass SetCharacterClass()
    {
        System.Console.WriteLine("\nAvailable Classes:");
        var classes = Enum.GetNames(typeof(CharacterClass));
        for (int i = 0; i < classes.Length; i++)
        {
            System.Console.WriteLine($"{i + 1}. {classes[i]}");
        }
        
        while (true)
        {
            System.Console.Write($"\nSelect class (1-{classes.Length}): ");
            if (int.TryParse(System.Console.ReadLine(), out int classChoice) && 
                classChoice > 0 && classChoice <= classes.Length)
            {
                return (CharacterClass)(classChoice - 1);
            }
            System.Console.WriteLine("Invalid selection. Please try again.");
        }
    }

    private Dictionary<Abilities, int> RollForRandomAbilityScores()
    {
        System.Console.WriteLine("\nRolling ability scores...");
        var abilityScores = new Dictionary<Abilities, int>();
        
        foreach (Abilities ability in Enum.GetValues(typeof(Abilities)))
        {
            // Roll 4d6, drop lowest
            //TODO: Create a Dice Roller Class rather than writing a specific dice roll for each method.
            var rolls = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                rolls.Add(_random.Next(1, 7));
            }
            rolls.Sort();
            rolls.RemoveAt(0); // Remove lowest roll
            int score = rolls.Sum();
            
            abilityScores[ability] = score;
            System.Console.WriteLine($"{ability}: {score}");
        }

        return abilityScores;
    }

    private void DisplayCharacterSummary(Character character)
    {
        System.Console.WriteLine("\n=== Character Created Successfully! ===");
        System.Console.WriteLine($"Name: {character.Name}");
        System.Console.WriteLine($"Race: {character.Race}");
        System.Console.WriteLine($"Class: {character.Class}");
        System.Console.WriteLine("\nAbility Scores:");
        foreach (var score in character.AbilityScores)
        {
            System.Console.WriteLine($"{score.Ability}: {score.Score}");
        }
    }
}
