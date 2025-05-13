namespace DndRpg.Core.Models;

public class AbilityScore
{
    public Guid CharacterId { get; set; }
    public Abilities Ability { get; set; }
    public int Score { get; set; }
    
    // Navigation property
    public Character Character { get; set; }
}