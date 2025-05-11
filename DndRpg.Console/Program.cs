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

namespace DndRpg.Console
{
    class Program
    {
        private static CharacterCreationHandler _characterCreationHandler;
        private static ICharacterCreationService _characterService;

        static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            _characterService = serviceProvider.GetRequiredService<ICharacterCreationService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            _characterCreationHandler = new CharacterCreationHandler(_characterService);

            try
            {
                await RunGameAsync();
            }
            catch (Exception ex)
            {
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

            // Add HttpClient with configuration
            services.AddHttpClient<IDndApiClient, DndApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://www.dnd5eapi.co/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

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
                System.Console.WriteLine("2. List all characters");
                System.Console.WriteLine("3. View character details");
                System.Console.WriteLine("4. Roll ability check");
                System.Console.WriteLine("5. Roll skill check");
                System.Console.WriteLine("6. Roll saving throw");
                System.Console.WriteLine("7. Exit");

                var choice = System.Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await _characterCreationHandler.CreateCharacterAsync();
                        break;
                    case "2":
                        await ListCharactersAsync();
                        break;
                    case "3":
                        await ViewCharacterDetailsAsync();
                        break;
                    case "4":
                        await RollAbilityCheckAsync();
                        break;
                    case "5":
                        await RollSkillCheckAsync();
                        break;
                    case "6":
                        await RollSavingThrowAsync();
                        break;
                    case "7":
                        return;
                    default:
                        System.Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        /// <summary>
        /// Lists all characters.
        /// </summary>
        private static async Task ListCharactersAsync()
        {
            throw new NotImplementedException()
        }

        /// <summary>
        /// Views the details of a character.
        /// </summary>
        private static async Task ViewCharacterDetailsAsync()
        {
            throw new NotImplementedException();
        }

        private static async Task RollAbilityCheckAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rolls a skill check for a character.
        /// </summary>
        private static async Task RollSkillCheckAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rolls a saving throw for a character.
        /// </summary>
        private static async Task RollSavingThrowAsync()
        {
           throw new NotImplementedException();
        }
    }
}
