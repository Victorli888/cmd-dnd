namespace DndRpg.Core.Interfaces;

public interface ICharacterRepository
{
    Task<Character> CreateCharacterAsync(Character character);
    Task<Character> GetCharacterAsync(Guid id);
    Task<IEnumerable<Character>> GetAllCharactersAsync();
    Task<Character> UpdateCharacterAsync(Character character);
    Task<bool> DeleteCharacterAsync(Guid id);
}
