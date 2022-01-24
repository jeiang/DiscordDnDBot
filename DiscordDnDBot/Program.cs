using DiscordDnDBot.Services;
using DiscordDnDBot.Types;

using Discord;

using Newtonsoft.Json;

namespace DiscordDnDBot
{
    public class Program
    {
        private const string CONFIG_FILE_PATH = "./config.json";

        public static async Task Main(string[] _)
        {
            Config config;
            try
            {
                string configTxt = await File.ReadAllTextAsync(CONFIG_FILE_PATH);
                config = JsonConvert.DeserializeObject<Config>(configTxt) ?? Config.Default;
            }
            catch (FileNotFoundException fnfe)
            {
                await LogHandler.Log(LogSeverity.Warning, "Configuration", 
                    $"Failed to find config.json at {Path.GetFullPath(CONFIG_FILE_PATH)}. Using Defaults.", fnfe);
                config = Config.Default;
                try
                {
                    await File.WriteAllTextAsync(CONFIG_FILE_PATH, 
                        JsonConvert.SerializeObject(config, Formatting.Indented));
                } 
                catch (Exception ex)
                {
                    await LogHandler.Log(LogSeverity.Warning, "Configuration",
                        $"Failed to save default config file to {Path.GetFullPath(CONFIG_FILE_PATH)}.", ex);
                }
            }
            catch (Exception ex)
            {
                await LogHandler.Log(new LogMessage(LogSeverity.Critical, "Configuration", 
                    $"Error when trying to load config from {Path.GetFullPath(CONFIG_FILE_PATH)}.", ex));
                return;
            }

            if (string.IsNullOrWhiteSpace(config.Token))
            {
                await LogHandler.Log(LogSeverity.Critical, "Initialization", 
                    "Missing token in config.json. Please enter a token. This token will NOT be saved to disk.");
                string? tempToken = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(tempToken))
                {
                    config.Token = tempToken;
                }
                else
                {
                    await LogHandler.Log(LogSeverity.Critical, "Initialization", 
                        "Missing Token. Cannot start Bot. Exiting.");
                    return;
                }
            }

            await new Bot(config).StartBot();
        }
    }
}