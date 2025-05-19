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
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;

namespace DndRpg.Console
{
    class Program
    {
        private static CharacterCreationModule _characterCreation;
        private static CharacterLoadingModule _characterLoading;

        static async Task Main(string[] args)
        {
            // Set up the base directory for logs and database
            var baseDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "DndRpg"
            );
            
            // Ensure directories exist
            Directory.CreateDirectory(baseDirectory);
            Directory.CreateDirectory(Path.Combine(baseDirectory, "logs"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .Build();

            // Override the log file path in configuration
            var logPath = Path.Combine(baseDirectory, "logs", "log-.txt");
            configuration["Serilog:WriteTo:1:Args:path"] = logPath;

            var services = ConfigureServices(configuration);
            var serviceProvider = services.BuildServiceProvider();

            _characterCreation = new CharacterCreationModule(
                serviceProvider.GetRequiredService<ICharacterCreationService>(),
                serviceProvider.GetRequiredService<PointBuyAbilityScoreGenerator>(),
                serviceProvider.GetRequiredService<RandomAbilityScoreGenerator>(),
                serviceProvider.GetRequiredService<IRaceService>()
            );

            _characterLoading = new CharacterLoadingModule(
                serviceProvider.GetRequiredService<ICharacterRepository>(),
                serviceProvider.GetRequiredService<ILogger<CharacterLoadingModule>>()
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
        private static IServiceCollection ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            // Configure Serilog from appsettings.json
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Add logging to services
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(logger);
            });

            // Add HttpClient
            services.AddHttpClient<IDndApiClient, DndApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://www.dnd5eapi.co/api/2014/");
            });

            // Register DndApiService
            services.AddHttpClient<DndApiService>(client =>
            {
                client.BaseAddress = new Uri("https://www.dnd5eapi.co/api/2014/");
            });
            services.AddScoped<DndApiService>();

            // Set up the database path
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "DndRpg",
                "dnd_characters.db"
            );
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            
            // Create connection string
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            }.ToString();

            // Register DbContext with the connection string
            services.AddDbContext<DndDbContext>(options =>
                options.UseSqlite(connectionString)
            );

            // Register the repository
            services.AddScoped<ICharacterRepository, SqliteCharacterRepository>();

            // Add other services
            services.AddScoped<ICharacterCreationService, CharacterService>();
            services.AddScoped<IRaceService, RaceService>();

            // Register ability score generators
            services.AddScoped<PointBuyAbilityScoreGenerator>();
            services.AddScoped<RandomAbilityScoreGenerator>();

            return services;
        }

        /// <summary>
        /// Runs the game to get started.
        /// </summary>
        private static async Task RunGameAsync()
        {
            System.Console.WriteLine("Welcome to D&D Character Manager!");
            System.Console.WriteLine("--------------------------------");

            // Load existing characters
            await _characterLoading.LoadCharactersAsync();

            while (true)
            {
                System.Console.WriteLine("\nWhat would you like to do?");
                System.Console.WriteLine("1. Create a new character");
                System.Console.WriteLine("2. View all characters");
                System.Console.WriteLine("3. Exit");

                var choice = System.Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await _characterCreation.CreateCharacterAsync();
                        break;
                    case "2":
                        await _characterLoading.LoadCharactersAsync();
                        break;
                    case "3":
                        return;
                    default:
                        System.Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }
}
