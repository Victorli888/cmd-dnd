using DndRpg.Console;
using Spectre.Console;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace DndRpg.Core;

public class PointBuyAbilityScoreGenerator : IAbilityScoreGenerator
{
    private const int POINT_BUY_POINTS = 27;

    public Dictionary<Abilities, int> GenerateAbilityScores()
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

        DisplayPointCostTable();

        while (remainingPoints > 0)
        {
            DisplayAbilityScoresTable(abilityScores, remainingPoints);
            
            var selectedAbility = PromptForAbility();
            var currentScore = abilityScores[selectedAbility];
            var newScore = PromptForNewScore(selectedAbility, currentScore, remainingPoints);

            var pointCost = GetPointCost(newScore) - GetPointCost(currentScore);
            remainingPoints -= pointCost;
            abilityScores[selectedAbility] = newScore;

            AnsiConsole.Clear();
        }

        AnsiConsole.MarkupLine("\n[bold green]Ability scores assigned![/]");
        return abilityScores;
    }

    private void DisplayPointCostTable()
    {
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
    }

    private void DisplayAbilityScoresTable(Dictionary<Abilities, int> abilityScores, int remainingPoints)
    {
        var abilityTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Ability")
            .AddColumn("Score")
            .AddColumn("Modifier")
            .AddColumn("Cost");

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

        AnsiConsole.Write(abilityTable);
        AnsiConsole.WriteLine();
    }

    private Abilities PromptForAbility()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<Abilities>()
                .Title("Select ability to modify:")
                .AddChoices(Enum.GetValues<Abilities>())
        );
    }

    private int PromptForNewScore(Abilities ability, int currentScore, int remainingPoints)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>($"Enter new score for {ability} (8-15):")
                .DefaultValue(currentScore)
                .ValidationErrorMessage("[red]Invalid score! Must be between 8 and 15.[/]")
                .Validate(proposedScore =>
                {
                    if (proposedScore < 8 || proposedScore > 15)
                    {
                        return false;
                    }
                    var pointCost = GetPointCost(proposedScore) - GetPointCost(currentScore);
                    if (pointCost > remainingPoints)
                    {
                        return false;
                    }
                    
                    return true;
                }, "Invalid score or not enough points")
        );
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
}
