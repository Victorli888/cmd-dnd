using System;
using System.Threading.Tasks;
using DndRpg.Core.Interfaces;
using DndRpg.Infrastructure.Clients;
using DndRpg.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DndRpg.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            var characterService = serviceProvider.GetRequiredService<ICharacterService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                await RunGameAsync(characterService);
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

            // Add HttpClient
            services.AddHttpClient<IDndApiClient, DndApiClient>();

            // Add services
            services.AddScoped<ICharacterService, CharacterService>();

            return services;
        }

        /// <summary>
        /// Runs the game to get started.
        /// </summary>
        /// <param name="characterService">The character service.</param>
        private static async Task RunGameAsync(ICharacterService characterService)
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
                        await CreateCharacterAsync(characterService);
                        break;
                    case "2":
                        await ListCharactersAsync(characterService);
                        break;
                    case "3":
                        await ViewCharacterDetailsAsync(characterService);
                        break;
                    case "4":
                        await RollAbilityCheckAsync(characterService);
                        break;
                    case "5":
                        await RollSkillCheckAsync(characterService);
                        break;
                    case "6":
                        await RollSavingThrowAsync(characterService);
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
        /// Creates a new character.
        /// </summary>
        /// <param name="characterService">The character service.</param>
        private static async Task CreateCharacterAsync(ICharacterService characterService)
        {
            System.Console.Write("Enter character name: ");
            var name = System.Console.ReadLine();

            System.Console.Write("Enter character class: ");
            var characterClass = System.Console.ReadLine();

            System.Console.Write("Enter character race: ");
            var race = System.Console.ReadLine();

            try
            {
                var character = await characterService.CreateCharacterAsync(name, characterClass, race);
                System.Console.WriteLine($"Character created successfully! ID: {character.Id}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error creating character: {ex.Message}");
            }
        }

        /// <summary>
        /// Lists all characters.
        /// </summary>
        /// <param name="characterService">The character service.</param>
        private static async Task ListCharactersAsync(ICharacterService characterService)
        {
            var characters = await characterService.GetAllCharactersAsync();
            foreach (var character in characters)
            {
                System.Console.WriteLine($"ID: {character.Id}, Name: {character.Name}, Class: {character.Class}, Race: {character.Race}");
            }
        }

        /// <summary>
        /// Views the details of a character.
        /// </summary>
        /// <param name="characterService">The character service.</param>
        private static async Task ViewCharacterDetailsAsync(ICharacterService characterService)
        {
            System.Console.Write("Enter character ID: ");
            if (Guid.TryParse(System.Console.ReadLine(), out var id))
            {
                var character = await characterService.GetCharacterAsync(id);
                if (character != null)
                {
                    System.Console.WriteLine($"Name: {character.Name}");
                    System.Console.WriteLine($"Class: {character.Class}");
                    System.Console.WriteLine($"Race: {character.Race}");
                    System.Console.WriteLine($"Level: {character.Level}");
                    System.Console.WriteLine($"Hit Points: {character.CurrentHitPoints}/{character.MaxHitPoints}");
                }
                else
                {
                    System.Console.WriteLine("Character not found.");
                }
            }
            else
            {
                System.Console.WriteLine("Invalid ID format.");
            }
        }

        /// <summary>
        /// Rolls an ability check for a character.
        /// </summary>
        /// <param name="characterService">The character service.</param>
        private static async Task RollAbilityCheckAsync(ICharacterService characterService)
        {
            System.Console.Write("Enter character ID: ");
            if (Guid.TryParse(System.Console.ReadLine(), out var id))
            {
                var character = await characterService.GetCharacterAsync(id);
                if (character != null)
                {
                    System.Console.Write("Enter ability (strength, dexterity, constitution, intelligence, wisdom, charisma): ");
                    var ability = System.Console.ReadLine().ToLower();
                    var result = await characterService.RollAbilityCheckAsync(character, ability);
                    System.Console.WriteLine($"Roll result: {result}");
                }
                else
                {
                    System.Console.WriteLine("Character not found.");
                }
            }
            else
            {
                System.Console.WriteLine("Invalid ID format.");
            }
        }

        /// <summary>
        /// Rolls a skill check for a character.
        /// </summary>
        /// <param name="characterService">The character service.</param>
        private static async Task RollSkillCheckAsync(ICharacterService characterService)
        {
            System.Console.Write("Enter character ID: ");
            if (Guid.TryParse(System.Console.ReadLine(), out var id))
            {
                var character = await characterService.GetCharacterAsync(id);
                if (character != null)
                {
                    System.Console.Write("Enter skill: ");
                    var skill = System.Console.ReadLine();
                    var result = await characterService.RollSkillCheckAsync(character, skill);
                    System.Console.WriteLine($"Roll result: {result}");
                }
                else
                {
                    System.Console.WriteLine("Character not found.");
                }
            }
            else
            {
                System.Console.WriteLine("Invalid ID format.");
            }
        }

        /// <summary>
        /// Rolls a saving throw for a character.
        /// </summary>
        /// <param name="characterService">The character service.</param>
        private static async Task RollSavingThrowAsync(ICharacterService characterService)
        {
            System.Console.Write("Enter character ID: ");
            if (Guid.TryParse(System.Console.ReadLine(), out var id))
            {
                var character = await characterService.GetCharacterAsync(id);
                if (character != null)
                {
                    System.Console.Write("Enter ability (strength, dexterity, constitution, intelligence, wisdom, charisma): ");
                    var ability = System.Console.ReadLine().ToLower();
                    var result = await characterService.RollSavingThrowAsync(character, ability);
                    System.Console.WriteLine($"Roll result: {result}");
                }
                else
                {
                    System.Console.WriteLine("Character not found.");
                }
            }
            else
            {
                System.Console.WriteLine("Invalid ID format.");
            }
        }
    }
}
