using System;
using System.Threading.Tasks;
using DndRpg.Core.Interfaces;

using DndRpg.Infrastructure.Clients;
using DndRpg.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using DndRpg.Console.Modules;
using DndRpg.Core;
using DndRpg.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace DndRpg.Console
{
    class Program
    {
        private static CharacterCreationModule _characterCreation;

        static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            _characterCreation = new CharacterCreationModule(
                serviceProvider.GetRequiredService<ICharacterCreationService>()
            );

            try
            {
                await RunGameAsync();
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while running the game");
            }
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        /// <returns>The configured service collection.</returns>
        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add HttpClient
            services.AddHttpClient<IDndApiClient, DndApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://www.dnd5eapi.co/api/");
            });

            // Add SQLite with better connection string and logging
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "DndRpg",
                "dnd_characters.db"
            );
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            }.ToString();

            services.AddSingleton<ICharacterRepository>(sp => 
                new SqliteCharacterRepository(
                    connectionString,
                    sp.GetRequiredService<ILogger<SqliteCharacterRepository>>()
                )
            );

            // Add services
            services.AddScoped<ICharacterCreationService, CharacterService>();

            return services;
        }

        /// <summary>
        /// Runs the game to get started.
        /// </summary>
        private static async Task RunGameAsync()
        {
            System.Console.WriteLine("Welcome to D&D Character Manager!");
            System.Console.WriteLine("--------------------------------");

            while (true)
            {
                System.Console.WriteLine("\nWhat would you like to do?");
                System.Console.WriteLine("1. Create a new character");
                System.Console.WriteLine("2. Exit");

                var choice = System.Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await _characterCreation.CreateCharacterAsync();
                        break;
                    case "2":
                        return;
                    default:
                        System.Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }
}
