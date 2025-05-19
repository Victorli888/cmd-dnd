using DndRpg.Core;
using DndRpg.Core.Enums;
using DndRpg.Core.Interfaces;
using DndRpg.Core.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace DndRpg.Console.Modules;

public class CharacterCreationModule
{
    private readonly ICharacterCreationService _characterService;
    private readonly IAbilityScoreGenerator _pointBuyGenerator;
    private readonly IAbilityScoreGenerator _randomGenerator;
    private readonly IRaceService _raceService;

    public CharacterCreationModule(
        ICharacterCreationService characterService,
        IAbilityScoreGenerator pointBuyGenerator,
        IAbilityScoreGenerator randomGenerator,
        IRaceService raceService)
    {
        _characterService = characterService;
        _pointBuyGenerator = pointBuyGenerator;
        _randomGenerator = randomGenerator;
        _raceService = raceService;
    }

    public async Task<Character> CreateCharacterAsync()
    {
        System.Console.WriteLine("\n=== Character Creation ===");
        
        var name = GetCharacterName();
        if (string.IsNullOrEmpty(name)) return null;

        var race = SetCharacterRace();
        var characterClass = SetCharacterClass();
        
        // Get base ability scores
        var abilityScores = ChooseAbilityScoreMethod();
        

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

    private Dictionary<Abilities, int> ChooseAbilityScoreMethod()
    {
        System.Console.WriteLine("\nChoose ability score generation method:");
        System.Console.WriteLine("1. Point Buy (27 points)");
        System.Console.WriteLine("2. Random Roll (4d6, drop lowest)");

        while (true)
        {
            System.Console.Write("\nSelect method (1-2): ");
            var input = System.Console.ReadLine();

            switch (input)
            {
                case "1":
                    return _pointBuyGenerator.GenerateAbilityScores();
                case "2":
                    return _randomGenerator.GenerateAbilityScores();
                default:
                    System.Console.WriteLine("Invalid selection. Please try again.");
                    break;
            }
        }
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
            System.Console.WriteLine($"{score.Ability}: {score.TotalScore}");
        }
    }
}
