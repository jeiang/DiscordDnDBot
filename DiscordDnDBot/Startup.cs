using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDnDBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordDnDBot
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public Startup(string[] _)
        {
            var builder = new ConfigurationBuilder()    // Create a new instance of the config builder
                .SetBasePath(AppContext.BaseDirectory)  // Specify the default location for the config file
                .AddJsonFile("config.json", false);    // Add this (json encoded) file to the configuration
            Configuration = builder.Build();            // Build the configuration
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();     // Create a new instance of a service collection
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();                     // Build the service provider
            _ = provider.GetRequiredService<LoggingService>();                      // Start the logging service
            _ = provider.GetRequiredService<ApplicationCommandService>(); 		    // Start the command handler service
            _ = provider.GetRequiredService<DiscordEventService>();
            await provider.GetRequiredService<StartupService>().StartAsync();   // Start the startup service
            await Task.Delay(-1);                                               // Keep the program alive
        }

        private void ConfigureServices(IServiceCollection services)
        {
            if (!Enum.TryParse<LogSeverity>(Configuration["LogLevel"], out var logLevel))
            {
                logLevel = LogSeverity.Info;
            }

            DiscordSocketConfig clientConfig = new();
            Configuration.GetSection(nameof(DiscordSocketConfig)).Bind(clientConfig);
            clientConfig.LogLevel = logLevel;
            // NOTE: This fixes bug with config changing null to string.empty on string (not string?)
            clientConfig.GatewayHost =
                string.IsNullOrEmpty(clientConfig.GatewayHost)
                ? null
                : clientConfig.GatewayHost;

            InteractionServiceConfig interactionServiceConfig = new();
            Configuration.GetSection(nameof(InteractionServiceConfig)).Bind(interactionServiceConfig);
            interactionServiceConfig.LogLevel = logLevel;

            DiscordSocketClient client = new(clientConfig);
            InteractionService interactionService = new(client, interactionServiceConfig);

            _ = services
                .AddSingleton(client)
                .AddSingleton(interactionService)
                .AddSingleton<ApplicationCommandService>()  // Add the command handler to the collection
                .AddSingleton<StartupService>()             // Add startupservice to the collection
                .AddSingleton<LoggingService>()             // Add loggingservice to the collection
                .AddSingleton<DatabaseService>()            // Add databaseservice to the collection
                .AddSingleton<DiscordEventService>()        // Add discordEventService to the collection
                .AddSingleton(Configuration);               // Add the configuration to the collection
        }
    }
}
