using DndRpg.Core;
using DndRpg.Core.Enums;
using DndRpg.Core.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace DndRpg.Console.Modules;

public class CharacterCreationModule
{
    private readonly ICharacterCreationService _characterService;
    private readonly Random _random;
    private const int POINT_BUY_POINTS = 27;

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
        var abilityScores = PointBuyAbilityScores();

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

    private Dictionary<Abilities, int> PointBuyAbilityScores()
    {
        var abilityScores = new Dictionary<Abilities, int>();
        var remainingPoints = POINT_BUY_POINTS;

        // Initialize all abilities with 8 (minimum score)
        foreach (Abilities ability in Enum.GetValues(typeof(Abilities)))
        {
            abilityScores[ability] = 8;
        }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold blue]=== Point Buy Ability Scores ===[/]");
        AnsiConsole.MarkupLine($"You have [bold green]{POINT_BUY_POINTS}[/] points to spend.");

        // Create a table for the point cost reference
        var costTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Score")
            .AddColumn("Cost");

        for (int score = 8; score <= 15; score++)
        {
            costTable.AddRow(
                score.ToString(),
                GetPointCost(score).ToString()
            );
        }

        AnsiConsole.Write(costTable);
        AnsiConsole.WriteLine();

        while (remainingPoints > 0)
        {
            // Create the ability scores table
            var abilityTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Ability")
                .AddColumn("Score")
                .AddColumn("Modifier")
                .AddColumn("Cost");

            // Populate the table
            foreach (var (abil, score) in abilityScores)
            {
                var modifier = (score - 10) / 2;
                var scoreCost = GetPointCost(score);
                abilityTable.AddRow(
                    abil.ToString(),
                    score.ToString(),
                    $"{modifier:+#;-#;0}",
                    scoreCost.ToString()
                );
            }
            abilityTable.AddRow(
                "Remaining Points",
                remainingPoints.ToString(),
                "",
                ""
            );

            // Display the current state
            AnsiConsole.Write(abilityTable);
            AnsiConsole.WriteLine();

            // Get user input
            var selectedAbility = AnsiConsole.Prompt(
                new SelectionPrompt<Abilities>()
                    .Title("Select ability to modify:")
                    .AddChoices(Enum.GetValues<Abilities>())
            );

            var currentScore = abilityScores[selectedAbility];
            var newScore = AnsiConsole.Prompt(
                new TextPrompt<int>($"Enter new score for {selectedAbility} (8-15):")
                    .DefaultValue(currentScore)
                    .ValidationErrorMessage("[red]Invalid score! Must be between 8 and 15.[/]")
                    .Validate(proposedScore =>
                    {
                        if (proposedScore < 8 || proposedScore > 15)
                            return ValidationResult.Error("Score must be between 8 and 15");
                        
                        var pointCost = GetPointCost(proposedScore) - GetPointCost(currentScore);
                        if (pointCost > remainingPoints)
                            return ValidationResult.Error($"Not enough points! Need {pointCost} more points.");
                        
                        return ValidationResult.Success();
                    })
            );

            var pointCost = GetPointCost(newScore) - GetPointCost(currentScore);
            remainingPoints -= pointCost;
            abilityScores[selectedAbility] = newScore;

            AnsiConsole.Clear();
        }

        AnsiConsole.MarkupLine("\n[bold green]Ability scores assigned![/]");
        return abilityScores;
    }

    private int GetPointCost(int score)
    {
        return score switch
        {
            8 => 0,
            9 => 1,
            10 => 2,
            11 => 3,
            12 => 4,
            13 => 5,
            14 => 7,
            15 => 9,
            _ => throw new ArgumentException("Invalid score for point buy")
        };
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
