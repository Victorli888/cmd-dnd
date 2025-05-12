using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DndRpg.Core.Interfaces;
using DndRpg.Core.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using DndRpg.Core;
using DndRpg.Core.Enums;
using Microsoft.Extensions.Logging;

namespace DndRpg.Infrastructure.Data
{
    public class SqliteCharacterRepository : ICharacterRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<SqliteCharacterRepository> _logger;

        public SqliteCharacterRepository(string connectionString, ILogger<SqliteCharacterRepository> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation($"Database path: {new SqliteConnectionStringBuilder(connectionString).DataSource}");
            _logger.LogInformation($"Operating System: {Environment.OSVersion}");
            _logger.LogInformation($"Current Directory: {Environment.CurrentDirectory}");

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Characters (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Class TEXT NOT NULL,
                    Race TEXT NOT NULL,
                    Level INTEGER NOT NULL,
                    HitPoints INTEGER NOT NULL,
                    AbilityScores TEXT NOT NULL
                );";
            command.ExecuteNonQuery();
        }

        public async Task<Character> CreateAsync(Character character)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Characters (Id, Name, Class, Race, Level, HitPoints, AbilityScores)
                VALUES (@Id, @Name, @Class, @Race, @Level, @HitPoints, @AbilityScores);";

            command.Parameters.AddWithValue("@Id", character.Id.ToString());
            command.Parameters.AddWithValue("@Name", character.Name);
            command.Parameters.AddWithValue("@Class", character.Class.ToString());
            command.Parameters.AddWithValue("@Race", character.Race.ToString());
            command.Parameters.AddWithValue("@Level", character.Level);
            command.Parameters.AddWithValue("@HitPoints", character.MaxHitPoints);
            command.Parameters.AddWithValue("@AbilityScores", JsonSerializer.Serialize(character.AbilityScores));

            await command.ExecuteNonQueryAsync();
            return character;
        }

        public async Task<Character> GetByIdAsync(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Characters WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", id.ToString());

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapCharacterFromReader(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Character>> GetAllAsync()
        {
            var characters = new List<Character>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Characters;";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                characters.Add(MapCharacterFromReader(reader));
            }

            return characters;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Characters WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", id.ToString());

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<Character> UpdateAsync(Character character)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogInformation($"Updating character: {character.Name}");

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Characters 
                    SET Name = @Name,
                        Class = @Class,
                        Race = @Race,
                        Level = @Level,
                        HitPoints = @HitPoints,
                        AbilityScores = @AbilityScores
                    WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", character.Id.ToString());
                command.Parameters.AddWithValue("@Name", character.Name);
                command.Parameters.AddWithValue("@Class", character.Class.ToString());
                command.Parameters.AddWithValue("@Race", character.Race.ToString());
                command.Parameters.AddWithValue("@Level", character.Level);
                command.Parameters.AddWithValue("@HitPoints", character.MaxHitPoints);
                command.Parameters.AddWithValue("@AbilityScores", JsonSerializer.Serialize(character.AbilityScores));

                var rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected == 0)
                {
                    _logger.LogWarning($"No character found with Id: {character.Id}");
                    return null;
                }

                _logger.LogInformation($"Successfully updated character: {character.Name}");
                return character;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating character: {character.Name}");
                throw;
            }
        }

        private Character MapCharacterFromReader(SqliteDataReader reader)
        {
            return new Character
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.GetString(1),
                Class = Enum.Parse<CharacterClass>(reader.GetString(2)),
                Race = Enum.Parse<CharacterRace>(reader.GetString(3)),
                Level = reader.GetInt32(4),
                MaxHitPoints = reader.GetInt32(5),
                AbilityScores = JsonSerializer.Deserialize<Dictionary<AbilityScore, int>>(reader.GetString(6))
            };
        }

    }
}
