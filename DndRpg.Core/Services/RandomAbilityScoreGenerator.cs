using DndRpg.Console;

namespace DndRpg.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public class RandomAbilityScoreGenerator : IAbilityScoreGenerator
{
    private readonly Random _random;

    public RandomAbilityScoreGenerator()
    {
        _random = new Random();
    }

    public Dictionary<Abilities, int> GenerateAbilityScores()
    {
        System.Console.WriteLine("\nRolling ability scores (4d6, drop lowest)...");
        var abilityScores = new Dictionary<Abilities, int>();
        
        foreach (Abilities ability in Enum.GetValues(typeof(Abilities)))
        {
            var rolls = Roll4d6DropLowest();
            abilityScores[ability] = rolls.Sum();
            System.Console.WriteLine($"{ability}: {rolls.Sum()} (Rolls: {string.Join(", ", rolls)})");
        }

        return abilityScores;
    }

    private List<int> Roll4d6DropLowest()
    {
        var rolls = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            rolls.Add(_random.Next(1, 7));
        }
        rolls.Sort();
        rolls.RemoveAt(0); // Remove lowest roll
        return rolls;
    }
}
