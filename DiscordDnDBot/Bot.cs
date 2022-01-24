using DiscordDnDBot.Services;
using DiscordDnDBot.Types;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

namespace DiscordDnDBot
{
    internal class Bot
    {
        private readonly InteractionService _interactionService;
        private readonly DiscordSocketClient _client;
        private readonly Config _config;

        private IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_interactionService)
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();

        private async Task ClientReady()
        {

        }

        public Bot(Config config)
        {
            _config = config;
            _client = new DiscordSocketClient(config.WebSocketConfig);
            _interactionService = new InteractionService(_client, config.InteractionServiceConfig);

            _client.Log += LogHandler.Log;
            _interactionService.Log += LogHandler.Log;
        }

        public async Task StartBot()
        {
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            TextReader input = Console.In;

            string? inputCmd = string.Empty;

            while (inputCmd != "kill")
            {
                inputCmd = await input.ReadLineAsync();
            }

            await _client.StopAsync();
            await _client.LogoutAsync();
        }
    }
}
