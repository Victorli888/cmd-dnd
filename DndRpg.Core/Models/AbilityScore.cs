namespace DndRpg.Core.Models;

public class AbilityScore
{
    public Guid CharacterId { get; set; }
    public Abilities Ability { get; set; }
    public int BaseScore { get; set; }  // From point buy or random roll
    public int BonusScore { get; set; } // Cumulative bonus from race, class, level, etc.
    
    public int TotalScore => BaseScore + BonusScore;
    
    // Navigation property
    public Character Character { get; set; }
}