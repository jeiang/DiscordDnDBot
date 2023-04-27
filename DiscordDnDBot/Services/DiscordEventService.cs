
using Discord.WebSocket;
using DiscordDnDBot.DiscordEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordDnDBot.Services
{
    public class DiscordEventService
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;
        private readonly LoggingService _loggingService;
        private readonly DatabaseService _databaseService;
        private readonly IEnumerable<IEventHandler> _handlers; // used to prevent them from being garbage collected.

        public DiscordEventService(IConfigurationRoot config, DiscordSocketClient client,
            LoggingService loggingService, DatabaseService databaseService)
        {
            _config = config;
            _client = client;
            _loggingService = loggingService;
            _databaseService = databaseService;

            var serviceCollection =
                new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton(_databaseService)
                .AddSingleton(_loggingService);

            var eventHandlers =
                AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => typeof(IEventHandler).IsAssignableFrom(t) && !t.IsInterface);
            foreach (var handler in eventHandlers)
            {
                _ = serviceCollection.AddSingleton(handler);
            }

            var provider = serviceCollection.BuildServiceProvider();

            foreach (var handler in eventHandlers)
            {
                IEventHandler? eventHandler = provider.GetRequiredService(handler) as IEventHandler;
                eventHandler?.HookEvents();
            }

            _handlers =
                eventHandlers
                .Select(eventHandler => provider.GetRequiredService(eventHandler))
                .Where(handler => handler is IEventHandler)
                .Select(handler => (IEventHandler)handler);

            _handlers.AsParallel().ForAll(handler => handler.HookEvents());
        }
    }
}
