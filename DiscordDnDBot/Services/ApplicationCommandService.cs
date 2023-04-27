using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDnDBot.Helpers;
using DiscordDnDBot.Types.Converters;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text;

namespace DiscordDnDBot.Services
{
    public class ApplicationCommandService
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly LoggingService _loggingService;

        private async Task ClientReady()
        {
            _interactions.AddTypeConverter<DateTime>(new DateTimeConverter());

            ModuleInfo[] modulesToLoad = (await _interactions.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services)).ToArray();

            if (modulesToLoad.Length == 0)
            {
                await _loggingService.LogAsync("CommandService", "No modules detected in current assembly.",
                    LogSeverity.Critical);
                Environment.Exit(-1);
            }

            Dictionary<ulong, HashSet<IApplicationCommand>> loadedCommands = new();

            bool GlobalDelete = _config.GetSection("ApplicationCommands:Global:DeleteOldCommands").Get<bool>();
            bool GlobalLoad = _config.GetSection("ApplicationCommands:Global:LoadNewCommands").Get<bool>();

            HashSet<IApplicationCommand> globalCommands = new();
            globalCommands.UnionWith(await _client.GetGlobalApplicationCommandsAsync());
            if (GlobalDelete)
            {
                foreach (SocketApplicationCommand cmd in globalCommands)
                {
                    await _loggingService.LogAsync("CommandService", $"Unloading Global Command {cmd.Name}.");
                    await cmd.DeleteAsync();
                }
                globalCommands.Clear();
            }
            if (GlobalLoad)
            {
                await _loggingService.LogAsync("CommandService", "Loading global commands on Discord.");
                globalCommands.UnionWith(
                    await _interactions.AddModulesGloballyAsync(deleteMissing: true, modules: modulesToLoad));
            }
            loadedCommands.Add(0, globalCommands);

            var guildConfigs = _config.GetSection("ApplicationCommands:Guilds").GetChildren();

            foreach (var guildConfig in guildConfigs)
            {
                if (!ulong.TryParse(guildConfig.Key, out ulong guildId))
                {
                    await _loggingService.LogAsync("CommandService",
                        $"Could not parse {guildConfig} as guild id.", LogSeverity.Error);
                    continue;
                }

                SocketGuild? guild = _client.GetGuild(guildId);

                if (guild == null)
                {
                    await _loggingService.LogAsync("CommandService",
                        $"Could not find guild with id {guildId}.", LogSeverity.Error);
                    continue;
                }

                HashSet<IApplicationCommand> guildCommands = new();
                guildCommands.UnionWith(await guild.GetApplicationCommandsAsync());

                if (guildConfig.GetSection("DeleteOldCommands").Get<bool>())
                {
                    foreach (SocketApplicationCommand cmd in guildCommands)
                    {
                        await _loggingService.LogAsync("CommandService",
                            $"Unloading Command {cmd.Name} in guild \"{cmd.Guild}\".");
                        await cmd.DeleteAsync();
                    }
                    guildCommands.Clear();
                }
                if (guildConfig.GetSection("LoadNewCommands").Get<bool>())
                {
                    await _loggingService.LogAsync("CommandService",
                        $"Loading commands on Discord for guild {guild.Name}.");
                    guildCommands.UnionWith(
                        await _interactions.AddModulesToGuildAsync(guild, deleteMissing: true, modules: modulesToLoad));
                }

                loadedCommands.Add(guildId, guildCommands);
            }

            // TODO: Create recursive function to walk through IApplicationCommand Groups
            StringBuilder sb = new("Currently active commands:\n");
            foreach (var guildCommandPair in loadedCommands)
            {
                SocketGuild? guild = _client.GetGuild(guildCommandPair.Key);

                _ = guild == null
                    ? sb.AppendLine("Global Commands:")
                    : sb.AppendLine($"Guild \"{guild.Name}\" Commands:");

                foreach (IApplicationCommand command in guildCommandPair.Value)
                {
                    _ = sb.AppendLine($"Command:{command.Name}:\n\tDescription: {command.Description}");
                }
            }

            await _loggingService.LogAsync("CommandService", sb.ToString(), LogSeverity.Verbose);
        }

        private async Task HandleSlashCommand(SocketSlashCommand msg)
        {
            SocketInteractionContext<SocketSlashCommand> ctx = new(_client, msg);
            _ = await _interactions.ExecuteCommandAsync(ctx, _services);
        }

        private async Task SlashCommandExecuted(SlashCommandInfo info, IInteractionContext ctx, IResult result)
        {
            if (!result.IsSuccess)
            {
                await ctx.Interaction.DeleteOriginalResponseAsync();

                string output = result.Error switch
                {
                    InteractionCommandError.UnmetPrecondition => "Unmet Precondition",
                    InteractionCommandError.UnknownCommand => "Unknown command",
                    InteractionCommandError.BadArgs => "Invalid number of arguments",
                    InteractionCommandError.Exception => "Command Exception",
                    InteractionCommandError.Unsuccessful => "Unsuccessful Command Execution",
                    InteractionCommandError.ConvertFailed => "Conversion Failed",
                    InteractionCommandError.ParseFailed => "Parameter Parsing Failed",
                    _ => $"{result.Error}"
                };

                await _loggingService.LogAsync("CommandService", $"Command \"{info.Name}\" was " +
                    $"executed unsuccessfully by user \"{ctx.User.Username} in {ctx.Guild.Name}\".\n\t{output}",
                    LogSeverity.Error);

                _ = await ctx.Channel.SendMessageAsync(
                    embed:
                        Defaults.CreateEmbedBuilder(ctx)
                        .WithTitle($"Command \"{info.Name}\" failed!")
                        .WithColor(Color.Red)
                        .AddField(new EmbedFieldBuilder()
                        {
                            Name = $"Error: {output}",
                            IsInline = false,
                            Value = string.IsNullOrWhiteSpace(result.ErrorReason) ? "" : result.ErrorReason,
                        }).WithCurrentTimestamp().Build());
            }
            else
            {
                await _loggingService.LogAsync("CommandService", $"Command \"{info.Name}\" was " +
                    $"executed successfully by user \"{ctx.User.Username}\"in {ctx.Guild.Name}.");
            }
        }

        public ApplicationCommandService(IConfigurationRoot config, IServiceProvider services,
            InteractionService interactions, DiscordSocketClient client, LoggingService loggingService)
        {
            _config = config;
            _interactions = interactions;
            _services = services;
            _client = client;
            _loggingService = loggingService;

            _client.Ready += ClientReady;
            _client.SlashCommandExecuted += HandleSlashCommand;
            _interactions.SlashCommandExecuted += SlashCommandExecuted;
        }
    }
}
