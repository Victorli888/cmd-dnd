using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DndRpg.Core.Models;

namespace DndRpg.Core.Interfaces;

public interface ICharacterRepository
{
    Task<Character> GetByIdAsync(Guid id);
    Task<IEnumerable<Character>> GetAllAsync();
    Task<Character> CreateAsync(Character character);
    Task<Character> UpdateAsync(Character character);
    Task<bool> DeleteAsync(Guid id);
}
