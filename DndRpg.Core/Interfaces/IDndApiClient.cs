namespace DndRpg.Core.Interfaces;

/// <summary>
/// Interface for the D&D API client.
/// </summary>
public interface IDndApiClient
{
    /// <summary>
    /// Gets data from the API.
    /// </summary>
    Task<T> GetAsync<T>(string endpoint) where T : class;
    Task<IEnumerable<T>> GetListAsync<T>(string endpoint) where T : class;
    Task<T> SearchAsync<T>(string endpoint, string query) where T : class;
}