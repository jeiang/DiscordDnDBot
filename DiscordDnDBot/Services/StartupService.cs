using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;

namespace DiscordDnDBot.Services
{
    public class StartupService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfigurationRoot _config;

        // DiscordSocketClient, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            DiscordSocketClient discord,
            IConfigurationRoot config)
        {
            _config = config;
            _discord = discord;
        }

        public async Task StartAsync()
        {
            string discordToken = _config["Token"];     // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace(discordToken))
            {
                throw new Exception("Please enter your bot's token into the `config.json` " +
                    "file found in the applications root directory.");
            }
            await _discord.LoginAsync(TokenType.Bot, discordToken);     // Login to discord
            await _discord.StartAsync();                                // Connect to the websocket
        }
    }
}
