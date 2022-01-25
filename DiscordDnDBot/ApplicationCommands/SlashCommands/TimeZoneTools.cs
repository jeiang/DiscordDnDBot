using System.Text.RegularExpressions;

using DiscordDnDBot.Helpers;
using DiscordDnDBot.Services;
using DiscordDnDBot.Types;
using DiscordDnDBot.Types.Database;

using Discord;
using Discord.Interactions;

using TimeZoneNames;

namespace DiscordDnDBot.ApplicationCommands.SlashCommands
{
    [Group("timezone", "Tools to help with timezones.")]
    public class TimeZoneTools : InteractionModuleBase
    {
        [RequireRole("Administrator", Group = "AdminRole")]
        [RequireRole("Admin", Group = "AdminRole")]
        [RequireRole("Moderator", Group = "AdminRole")]
        [RequireOwner(Group = "AdminRole")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "AdminRole")]
        [Group("admin", "Timezone tools for administrators.")]
        public class TimezoneAdminTools: InteractionModuleBase
        {
            private readonly DatabaseService _databaseService;
            private readonly Regex _utcRegex;

            public TimezoneAdminTools(DatabaseService dbs)
            {
                _databaseService = dbs;
                _utcRegex = new Regex(@"[uU][tT][cC]\s*([+\-]?[0-9]{1,2})");
            }

            [SlashCommand("setuser", "Set the timezone for a user.")]
            public async Task<RuntimeResult> SetUserTimezone(
                [Summary("user", "The User who's timezone is to be set.")]
                IUser user,
                [Summary("location", "The location in which the user is.")]
                string location,
                [Summary("hidden", "Only show this for yourself. (Default: true)")]
                bool ephemeral = true)
            {
                await DeferAsync(ephemeral: ephemeral);

                TimeZoneInfo timeZone;
                Match utcMatch = _utcRegex.Match(location);
                if (utcMatch.Success)
                {
                    Capture capture = utcMatch.Captures[1];
                    if (int.TryParse(capture.Value, out int result))
                    {
                        TimeZoneInfo? tz =
                            TimeZoneInfo.GetSystemTimeZones()
                            .Where((tz) => tz.BaseUtcOffset.Hours == result)
                            .FirstOrDefault();
                        if (tz == null)
                        {
                            return ApplicationCommandExecutionResult.FromError(
                                InteractionCommandError.Unsuccessful, $"{location} could not be parsed as UTC timezone.");
                        }
                        else
                        {
                            timeZone = tz;
                        }
                    }
                    else
                    {
                        return ApplicationCommandExecutionResult.FromError(
                            InteractionCommandError.Unsuccessful, $"{location} could not be parsed as UTC timezone.");
                    }
                }
                else
                {
                    TimeZoneInfo? tz = await TimeZoneInfoExtensions.GetTimeZoneInfoAsync(location);
                    if (tz == null)
                    {
                        return ApplicationCommandExecutionResult.FromError(
                            InteractionCommandError.Unsuccessful, $"{location} could not be geolocated and the timezone " +
                            "could not be found.");
                    }
                    else
                    {
                        timeZone = tz;
                    }
                }

                Player player = new(user.Id, timeZone);
                bool addedNew = await _databaseService.AddOrUpdate(player);

                IGuildUser guildUser = await Context.Guild.GetUserAsync(user.Id);
                Embed embed =
                    Defaults.CreateEmbedBuilder(Context)
                    .WithTitle(nameof(TimeZoneTools))
                    .AddField($"{guildUser?.Nickname ?? ($"{user.Username}#{user.Discriminator}")}'s timezone " +
                        $"has been {(addedNew ? "saved as" : "updated to")} {timeZone}.", $"The date and time in " +
                        $"{guildUser?.Nickname ?? ($"{user.Username}#{user.Discriminator}")}'s region should be " +
                        $"{TimeZoneInfo.ConvertTime(DateTime.UtcNow, player.TimeZone):MMM dd, yyyy hh:mm tt}." +
                        "\n\nIf this is incorrect, try using a different town in the same timezone.")
                        .WithCurrentTimestamp()
                        .Build();

                _ = FollowupAsync(embed: embed, ephemeral: ephemeral);
                return ApplicationCommandExecutionResult.FromSuccess();
            }
        }

        private readonly DatabaseService _databaseService;
        private readonly Regex _utcRegex;
        private readonly LoggingService _loggingService;

        public TimeZoneTools(DatabaseService dbs, LoggingService loggingService)
        {
            _databaseService = dbs;
            _utcRegex = new Regex(@"[uU][tT][cC]\s*([+\-]?[0-9]{1,2})");
            _loggingService = loggingService;
        }

        [SlashCommand("set", "Set your current timezone using the name of your current city/country.")]
        public async Task<RuntimeResult> SetTimezone(
            [Summary("location", "Your current location. (e.g. New York, Singapore, England)")]
            string location,
            [Summary("hidden", "Only show this for yourself. (Default: true)")]
            bool ephemeral = true)
        {
            await DeferAsync();

            TimeZoneInfo timeZone;
            Match utcMatch = _utcRegex.Match(location);
            if (utcMatch.Success)
            {
                Capture capture = utcMatch.Captures[1];
                if (int.TryParse(capture.Value, out int result))
                {
                    TimeZoneInfo? tz = 
                        TimeZoneInfo.GetSystemTimeZones()
                        .Where((tz) => tz.BaseUtcOffset.Hours == result)
                        .FirstOrDefault();
                    if (tz == null)
                    {
                        return ApplicationCommandExecutionResult.FromError(
                            InteractionCommandError.Unsuccessful, $"{location} could not be parsed as UTC timezone.");
                    }
                    else
                    {
                        timeZone = tz;
                    }
                }
                else
                {
                    return ApplicationCommandExecutionResult.FromError(
                        InteractionCommandError.Unsuccessful, $"{location} could not be parsed as UTC timezone.");
                }
            }
            else
            {
                TimeZoneInfo? tz = await TimeZoneInfoExtensions.GetTimeZoneInfoAsync(location);
                if (tz == null)
                {
                    return ApplicationCommandExecutionResult.FromError(
                        InteractionCommandError.Unsuccessful, $"{location} could not be geolocated " +
                        "and the timezone could not be found. Try using a different town in the " +
                        "same timezone.");
                }
                else
                {
                    timeZone = tz;
                }
            }

            Player player = new(Context.User.Id, timeZone);
            bool addedNew = await _databaseService.AddOrUpdate(player);

            IGuildUser guildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            Embed embed =
                Defaults.CreateEmbedBuilder(Context)
                .WithTitle(nameof(TimeZoneTools))
            .AddField(
                (guildUser?.Nickname ?? ($"{Context.User.Username}#{Context.User.Discriminator}")) +
                $"'s timezone has been {(addedNew ? "saved as" : "updated to")} {timeZone}.", 
                $"The date and time in " +
                (guildUser?.Nickname ?? ($"{Context.User.Username}#{Context.User.Discriminator}")) +
                $"'s region should be " +
                $"{TimeZoneInfo.ConvertTime(DateTime.UtcNow, player.TimeZone):MMM dd, yyyy hh:mm tt}." +
                "\n\nIf this is incorrect, try using a different town in the same timezone.")
                .WithCurrentTimestamp()
                .Build();

            _ = FollowupAsync(embed: embed, ephemeral: ephemeral);
            return ApplicationCommandExecutionResult.FromSuccess();
        }

        [SlashCommand("get", "Show someone's current timezone.")]
        public async Task ShowCurrentTimezone(
            [Summary("user", "The person whose timezone should be shown. (Default: yourself)")]
            IUser? user = null,
            [Summary("hidden", "Only show this for yourself. (Default: true)")]
            bool ephemeral = true)
        {
            await DeferAsync(ephemeral: ephemeral);

            if (user == null)
            {
                user = Context.User;
            }

            Player? player = await _databaseService.GetPlayer(user.Id);

            if (player == null)
            {
                Embed embed =
                    Defaults.CreateEmbedBuilder(Context)
                    .WithTitle(nameof(TimeZoneTools))
                    .AddField("Timezone Not Found", user == Context.User
                        ? "Use the `/timezone set` command to set your timezone."
                        : $"Ask <@{user.Id}> to set their timezone using `/timezone set` " +
                        "(or an admin to set their timezone).")
                    .WithCurrentTimestamp()
                    .Build();
                _ = await FollowupAsync(ephemeral: ephemeral, embed: embed);
            }
            else
            {
                IGuildUser guildUser = await Context.Guild.GetUserAsync(user.Id);
                Embed embed =
                    Defaults.CreateEmbedBuilder(Context)
                    .WithTitle(nameof(TimeZoneTools))
                    .AddField(
                        (user == Context.User 
                        ? "Your " 
                        : $"{guildUser?.Nickname ?? ($"{user.Username}#{user.Discriminator}")}'s ") +
                        $"timezone is saved as " +
                        $"{player.TimeZone}.", $"The date and time in your region is " +
                        $"{TimeZoneInfo.ConvertTime(DateTime.UtcNow, player.TimeZone):MMM dd, yyyy hh:mm tt}." +
                        "\n\nIf this is incorrect, " +
                        (user == Context.User 
                        ? "use" 
                        : $"ask {guildUser?.Nickname ?? ($"{user.Username}#{user.Discriminator}")} or an admin to use") +
                        $" `/timezone set` to correct it.")
                    .WithCurrentTimestamp()
                    .Build();
                _ = await FollowupAsync(ephemeral: ephemeral, embed: embed);
            }
        }

        [SlashCommand("convert", "Show a date and time in each person's native timezone. Requires that the timezone is set.")]
        public async Task DisplayDateTime(
            [Summary("target-user", "Show the time in this person's timezone. (Default: Yourself)")]
            IUser? user = null,
            [Summary("date-time", "Date and time to convert. (Default: Current time)")]
            DateTime? date = null,
            [Summary("hidden", "Only show this for yourself. (Default: false)")]
            bool ephemeral = false)
        {
            await DeferAsync(ephemeral: ephemeral);

            if (user == null)
            {
                user = Context.User;
            }
            Embed embed;
            Player? player = await _databaseService.GetPlayer(user.Id);
            if (player == null)
            {
                embed =
                    Defaults.CreateEmbedBuilder(Context)
                    .WithTitle(nameof(TimeZoneTools))
                    .AddField("Timezone Not Found", user == Context.User
                        ? "Use the `/timezone set` command to set your timezone."
                        : $"Ask <@{user.Id}> to set their timezone using `/timezone set` " +
                        "(or an admin to set their timezone).")
                    .WithCurrentTimestamp()
                    .Build();
                _ = await FollowupAsync(ephemeral: ephemeral, embed: embed);
                return;
            }
            DateTimeOffset offset =
                date == null
                ? DateTime.UtcNow
                : TimeZoneInfo.ConvertTime(date.Value, player.TimeZone, TimeZoneInfo.Utc);
            DateTime dateTime =
                date == null 
                ? TimeZoneInfo.ConvertTime(DateTime.UtcNow, player.TimeZone) 
                : date.Value;
            TimeZoneValues tzName =
                TZNames.GetAbbreviationsForTimeZone(player.TimeZone.Id, "en-US");
            IGuildUser guildUser = await Context.Guild.GetUserAsync(user.Id);
            embed =
                Defaults.CreateEmbedBuilder(Context)
                .WithTitle(nameof(TimeZoneTools))
                .AddField(
                    $"{guildUser?.Nickname ?? ($"{user.Username}#{user.Discriminator}")}'s Timezone", 
                    $"Time: {dateTime:MMM dd, yyyy hh:mm tt}", true)
                .AddField("Your Timezone",$"Time: <t:{offset.ToUnixTimeSeconds()}>", true)
                .WithCurrentTimestamp()
                .Build();
            _ = await FollowupAsync(embed: embed, ephemeral: ephemeral);
        }
    }
}
