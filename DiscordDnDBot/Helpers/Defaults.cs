using Discord;

namespace DiscordDnDBot.Helpers
{
    internal static class Defaults
    {
        public static EmbedBuilder CreateEmbedBuilder(IInteractionContext? ctx = null)
        {
            EmbedBuilder embedBuilder =
                new()
                {
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Designed By Aidan Pinard",
                        IconUrl = "https://files.catbox.moe/jiyevg.png"
                    },
                    Url = "https://rroll.to/T0ouhS"
                };
            if (ctx != null)
            {
                IGuildUser user = ctx.Guild.GetUserAsync(ctx.User.Id).Result;
                embedBuilder = embedBuilder.WithAuthor(new EmbedAuthorBuilder()
                {
                    Name = $"@{user?.Nickname ?? ""} {(user != null ? '(' : "")}{ctx.User.Username}#{ctx.User.Discriminator}{(user != null ? ')' : "")}",
                    IconUrl = user?.GetGuildAvatarUrl() ?? user?.GetAvatarUrl() ?? ctx.User.GetAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl(),
                    Url = "https://rroll.to/T0ouhS"
                });
            }

            return embedBuilder;
        }
        public static Color GetRandomColor()
        {
            Color[] colors =
            {
                Color.Blue,
                Color.DarkBlue,
                Color.DarkerGrey,
                Color.DarkGreen,
                Color.DarkGrey,
                Color.DarkMagenta,
                Color.DarkOrange,
                Color.DarkPurple,
                Color.DarkRed,
                Color.DarkTeal,
                Color.Default,
                Color.Gold,
                Color.Green,
                Color.LighterGrey,
                Color.LightGrey,
                Color.LightOrange,
                Color.Magenta,
                Color.Orange,
                Color.Purple,
                Color.Red,
                Color.Teal
            };
            var random = Random.Shared;
            return colors[random.Next(colors.Length)];
        }
    }
}
