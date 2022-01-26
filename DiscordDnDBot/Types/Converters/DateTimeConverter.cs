using Discord;
using Discord.Interactions;
using System.Globalization;

namespace DiscordDnDBot.Types.Converters
{
    internal class DateTimeConverter : TypeConverter
    {
        private readonly CultureInfo[] _cultureInfo = {
            new CultureInfo("en-GB"),
            new CultureInfo("en-US"),
            new CultureInfo("en-TT"),
            new CultureInfo("de-CH"),
            new CultureInfo("de-DE"),
            new CultureInfo("es-ES"),
            new CultureInfo("es-MX"),
            new CultureInfo("es-VE"),
            new CultureInfo("fr-CH"),
            new CultureInfo("fr-CA"),
            new CultureInfo("fr-FR"),
            new CultureInfo("ja-JP"),
            new CultureInfo("zh-CN")
        };

        private readonly DateTimeStyles _dateTimeStyles = DateTimeStyles.AllowWhiteSpaces;

        public override bool CanConvertTo(Type type) => typeof(DateTime).IsAssignableFrom(type);

        public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context,
            IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            foreach (var culture in _cultureInfo)
            {
                if (DateTime.TryParse((string)option.Value, culture, _dateTimeStyles, out DateTime dateTime))
                {
                    return Task.FromResult(TypeConverterResult.FromSuccess(dateTime));
                }
            }

            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed,
                $"Value {option.Value} cannot be converted to DateTime."));
        }
    }
}
