using DndRpg.Core;

namespace DndRpg.Console;

public interface IAbilityScoreGenerator
{
    Dictionary<Abilities, int> GenerateAbilityScores();
}
