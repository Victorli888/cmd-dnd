namespace DndRpg.Core.Models;

public class Skill
{
    public Guid CharacterId { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    
    // Navigation property
    public Character Character { get; set; }
}