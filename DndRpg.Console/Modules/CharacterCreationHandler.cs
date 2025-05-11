using DndRpg.Core;
using DndRpg.Core.Enums;
using DndRpg.Core.Models;

namespace DndRpg.Console.Modules;

public class CharacterCreationHandler
{
    private readonly ICharacterCreationService _characterCreationService;
    private readonly Random _random;

    public CharacterCreationHandler(ICharacterCreationService characterCreationService)
    {
        _characterCreationService = characterCreationService;
        _random = new Random();
    }

    public async Task CreateCharacterAsync()
    {
        System.Console.WriteLine("\n=== Character Creation ===");
        
        var name = GetCharacterName();
        if (string.IsNullOrEmpty(name)) return;

        var selectedRace = GetCharacterRace();
        var selectedClass = GetCharacterClass();
        var abilityScores = RollAbilityScores();

        try
        {
            var character = await _characterCreationService.CreateCharacterAsync(
                name: name,
                characterClass: selectedClass,
                race: selectedRace,
                abilityScores: abilityScores
            );

            DisplayCharacterSummary(character);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error creating character: {ex.Message}");
        }
    }

    private string GetCharacterName()
    {
        System.Console.Write("Enter character name: ");
        var name = System.Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            System.Console.WriteLine("Name cannot be empty.");
            return null;
        }
        return name;
    }

    private CharacterRace GetCharacterRace()
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

    private CharacterClass GetCharacterClass()
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

    private Dictionary<AbilityScore, int> RollAbilityScores()
    {
        System.Console.WriteLine("\nRolling ability scores...");
        var abilityScores = new Dictionary<AbilityScore, int>();
        
        foreach (AbilityScore ability in Enum.GetValues(typeof(AbilityScore)))
        {
            // Roll 4d6, drop lowest
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
        System.Console.WriteLine("\nCharacter created successfully!");
        System.Console.WriteLine($"ID: {character.Id}");
        System.Console.WriteLine($"Name: {character.Name}");
        System.Console.WriteLine($"Race: {character.Race}");
        System.Console.WriteLine($"Class: {character.Class}");
        System.Console.WriteLine("\nAbility Scores:");
        foreach (var score in character.AbilityScores)
        {
            System.Console.WriteLine($"{score.Key}: {score.Value}");
        }
    }
}
