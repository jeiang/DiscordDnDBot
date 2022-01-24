using System.Text;

using Discord;
using Discord.Net;

namespace DiscordDnDBot.Services
{
    public static class LogHandler
    {
        public static LogSeverity LogLevel { get; set; } = LogSeverity.Info;

        private static readonly StreamWriter _writer = new FileInfo($"./log-{DateTime.Now:yyyy'-'MM'-'dd'-'HH'-'mm'-'ss}.log").AppendText();

        private static string GenerateLogString(LogSeverity Severity, string Source, string Message, Exception? Exception = null)
        {
            string output = $"[{DateTime.Now:G}] {Severity.ToString().ToUpper()}:\t{Source}\t";
            if (Exception == null)
            {
                output += Message;
            }
            else if (Exception is RateLimitedException rateLimitedException)
            {
                output += $"Rate limiting occurred. {rateLimitedException.Message}.\nRequest: {rateLimitedException.Request}\n{rateLimitedException}";
            }
            else if (Exception is WebSocketClosedException webSocketClosedException)
            {
                output += $"The websocket was closed because {webSocketClosedException.Reason}. Code: {webSocketClosedException.CloseCode}\n{webSocketClosedException}";
            }
            else if (Exception is HttpException httpException)
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
                output += $"{Message}\n{Exception}";
            }
            return output.ReplaceLineEndings("\n\t");
        }

        public static async Task Log(LogSeverity severity, string source, string message, Exception? exception = null)
        {
            if ((int)severity > (int)LogLevel)
            {
                return;
            }

            TextWriter outputStream = Console.Out;

            switch (severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    outputStream = Console.Error;
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

            string output = GenerateLogString(severity, source, message, exception);
            await outputStream.WriteLineAsync(output);
            await _writer.WriteLineAsync(output);
            await _writer.FlushAsync();
        }

        public static async Task Log(LogMessage message)
        {
            await Log(message.Severity, message.Source, message.Message, message.Exception);
        }
    }
}
