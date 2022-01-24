using System.Text;

using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;

namespace DiscordDnDBot.Services
{
    public class LoggingService
    {
        private string LogDirectory { get; }
        private readonly FileInfo _logFile;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly int _logLevel;

        private static string GenerateLogString(LogMessage msg)
        {
            string output = $"[{DateTime.Now:G}] {msg.Severity.ToString().ToUpper()}:\t{msg.Source}\t";
            if (msg.Exception == null)
            {
                output += msg.Message;
            }
            else if (msg.Exception is RateLimitedException rateLimitedException)
            {
                output += $"Rate limiting occurred. {rateLimitedException.Message}.\nRequest: " +
                    $"{rateLimitedException.Request}\n{rateLimitedException}";
            }
            else if (msg.Exception is WebSocketClosedException webSocketClosedException)
            {
                output += $"The websocket was closed because {webSocketClosedException.Reason}. Code: " +
                    $"{webSocketClosedException.CloseCode}\n{webSocketClosedException}";
            }
            else if (msg.Exception is HttpException httpException)
            {
                output +=
                    new StringBuilder()
                    .Append("Error occurred while processing a Discord HTTP Request.\n" +
                        $"Discord Error Code: {httpException.DiscordCode}\n" +
                        $"HTTP Error Code: {httpException.HttpCode}\n" +
                        $"Reason: {httpException.Reason}\n" +
                        $"Request: {httpException.Request}\n" +
                        $"Discord Errors:\n\t")
                    .AppendJoin("\n\t", httpException.Errors)
                    .ToString();
            }
            else
            {
                output += $"{msg.Message}\n{msg.Exception}";
            }
            return output.ReplaceLineEndings("\n\t");
        }

        // DiscordSocketClient and InteractionService are injected automatically from the IServiceProvider
        public LoggingService(DiscordSocketClient client, InteractionService interactionService, 
            IConfigurationRoot configurationRoot)
        {
            LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            _logFile = 
                new FileInfo(Path.Combine(LogDirectory, $"{DateTime.Now:yyyy'-'MM'-'dd'-'HH'-'mm'-'ss}.log"));

            _client = client;
            _interactionService = interactionService;
            _configurationRoot = configurationRoot;

            if (!int.TryParse(_configurationRoot["severity"], out _logLevel))
            {
                _logLevel = (int)LogSeverity.Info;
            }

            _client.Log += LogAsync;
            _interactionService.Log += LogAsync;
        }

        public async Task LogAsync(LogMessage message)
        {
            if ((int)message.Severity < _logLevel)
            {
                return;
            }

            if (!_logFile.Exists)
            {
                // Create the log directory if it doesn't exist
                if (!Directory.Exists(LogDirectory))
                {
                    _ = Directory.CreateDirectory(LogDirectory);
                }
                await _logFile.Create().DisposeAsync();
            }

            TextWriter consoleOutput = Console.Out;

            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    consoleOutput = Console.Error;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
            }

            string output = GenerateLogString(message);
            await consoleOutput.WriteLineAsync(output);
            using StreamWriter writer = _logFile.AppendText();
            await writer.WriteLineAsync(output);
        }
    }
}
