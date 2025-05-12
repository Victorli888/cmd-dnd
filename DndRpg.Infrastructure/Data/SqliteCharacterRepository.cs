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
                    CurrentHitPoints INTEGER NOT NULL,
                    MaxHitPoints INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS AbilityScores (
                    CharacterId TEXT NOT NULL,
                    Ability TEXT NOT NULL,
                    Score INTEGER NOT NULL,
                    PRIMARY KEY (CharacterId, Ability),
                    FOREIGN KEY (CharacterId) REFERENCES Characters(Id)
                );";
            command.ExecuteNonQuery();
        }

        public async Task<Character> CreateAsync(Character character)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Insert character
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Characters (Id, Name, Class, Race, Level, MaxHitPoints)
                    VALUES (@Id, @Name, @Class, @Race, @Level, @MaxHitPoints);";

                command.Parameters.AddWithValue("@Id", character.Id.ToString());
                command.Parameters.AddWithValue("@Name", character.Name);
                command.Parameters.AddWithValue("@Class", character.Class.ToString());
                command.Parameters.AddWithValue("@Race", character.Race.ToString());
                command.Parameters.AddWithValue("@Level", character.Level);
                command.Parameters.AddWithValue("@MaxHitPoints", character.MaxHitPoints);
                command.Parameters.AddWithValue("@CurrentHitPoints", character.CurrentHitPoints);

                await command.ExecuteNonQueryAsync();

                // Insert ability scores
                foreach (var abilityScore in character.AbilityScores)
                {
                    command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO AbilityScores (CharacterId, Ability, Score)
                        VALUES (@CharacterId, @Ability, @Score);";

                    command.Parameters.AddWithValue("@CharacterId", character.Id.ToString());
                    command.Parameters.AddWithValue("@Ability", abilityScore.Key.ToString());
                    command.Parameters.AddWithValue("@Score", abilityScore.Value);

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return character;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Character> GetByIdAsync(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Characters WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", id.ToString());

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var character = new Character
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.GetString(1),
                Class = Enum.Parse<CharacterClass>(reader.GetString(2)),
                Race = Enum.Parse<CharacterRace>(reader.GetString(3)),
                Level = reader.GetInt32(4),
                CurrentHitPoints = reader.GetInt32(5),
                MaxHitPoints = reader.GetInt32(6),
                AbilityScores = new Dictionary<AbilityScore, int>()
            };

            // Get ability scores
            command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Ability, Score FROM AbilityScores WHERE CharacterId = @CharacterId;";
            command.Parameters.AddWithValue("@CharacterId", id.ToString());

            using var abilityReader = await command.ExecuteReaderAsync();
            while (await abilityReader.ReadAsync())
            {
                var ability = Enum.Parse<AbilityScore>(abilityReader.GetString(0));
                var score = abilityReader.GetInt32(1);
                character.AbilityScores[ability] = score;
            }

            return character;
        }

        public async Task<IEnumerable<Character>> GetAllAsync()
        {
            var characters = new List<Character>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Get all characters
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Characters;";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var character = new Character
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    Name = reader.GetString(1),
                    Class = Enum.Parse<CharacterClass>(reader.GetString(2)),
                    Race = Enum.Parse<CharacterRace>(reader.GetString(3)),
                    Level = reader.GetInt32(4),
                    MaxHitPoints = reader.GetInt32(5),
                    AbilityScores = new Dictionary<AbilityScore, int>()
                };

                // Get ability scores for this character
                var abilityCommand = connection.CreateCommand();
                abilityCommand.CommandText = @"
                    SELECT Ability, Score FROM AbilityScores WHERE CharacterId = @CharacterId;";
                abilityCommand.Parameters.AddWithValue("@CharacterId", character.Id.ToString());

                using var abilityReader = await abilityCommand.ExecuteReaderAsync();
                while (await abilityReader.ReadAsync())
                {
                    var ability = Enum.Parse<AbilityScore>(abilityReader.GetString(0));
                    var score = abilityReader.GetInt32(1);
                    character.AbilityScores[ability] = score;
                }

                characters.Add(character);
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
                        HitPoints = @MaxHitPoints
                    WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", character.Id.ToString());
                command.Parameters.AddWithValue("@Name", character.Name);
                command.Parameters.AddWithValue("@Class", character.Class.ToString());
                command.Parameters.AddWithValue("@Race", character.Race.ToString());
                command.Parameters.AddWithValue("@Level", character.Level);
                command.Parameters.AddWithValue("@HitPoints", character.MaxHitPoints);

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
    }
}
