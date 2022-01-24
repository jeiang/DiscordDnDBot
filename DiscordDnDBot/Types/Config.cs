using DiscordDnDBot.Services;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Newtonsoft.Json;

namespace DiscordDnDBot.Types
{
    internal class Config
    {
        public static Config Default
        {
            get
            {
                return new Config(string.Empty, null, new InteractionServiceConfig(), new DiscordSocketConfig(),
                    LogSeverity.Info, Config.CommandInitializationOptions.None);
            }
        }

        [Flags]
        public enum CommandInitializationOptions
        {
            None,
            LoadCommands,
            ClearAllCommands
        }

        public string Token { get; set; } = String.Empty;

        public ulong? TestingGuild { get; private set; }

        public InteractionServiceConfig InteractionServiceConfig { get; private set; }

        public DiscordSocketConfig WebSocketConfig { get; private set; }

        public LogSeverity LogLevel { get; private set; } = Discord.LogSeverity.Info;

        public CommandInitializationOptions CommandInitialization { get; private set; } = CommandInitializationOptions.None;

        [JsonConstructor]
        public Config(string token, ulong? testingGuild, InteractionServiceConfig interactionServiceConfig,
            DiscordSocketConfig discordSocketConfig, LogSeverity logSeverity, 
            CommandInitializationOptions commandInitialization)
        {
            Token = token;
            TestingGuild = testingGuild;
            InteractionServiceConfig = interactionServiceConfig;
            WebSocketConfig = discordSocketConfig;
            LogLevel = logSeverity;
            CommandInitialization = commandInitialization;

            WebSocketConfig.LogLevel = LogLevel;
            InteractionServiceConfig.LogLevel = LogLevel;

            LogHandler.LogLevel = LogLevel;
        }
    }
}
