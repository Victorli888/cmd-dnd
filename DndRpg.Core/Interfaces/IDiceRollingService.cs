﻿using DndRpg.Core.Models;

namespace DndRpg.Core;

public interface IDiceRollingService
{
    Task<int> RollAbilityCheckAsync(Character character, Abilities abilities);
    Task<int> RollSavingThrowAsync(Character character, Abilities abilities);
    //TODO: Create an ActionFactory Rather than a method for each type of roll
    Task<int> RollAttackRollAsync(Character character);
    Task<int> RollDamageRollAsync(Character character, int diceCount, int diceSize);
    Task<int> RollHealingRollAsync(Character character, int diceCount, int diceSize);
    Task<int> RollInitiativeRollAsync(Character character);
}
