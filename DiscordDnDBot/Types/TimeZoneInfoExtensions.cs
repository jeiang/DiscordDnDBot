using GeoTimeZone;

namespace DiscordDnDBot.Types
{
    internal class TimeZoneInfoExtensions
    {
        public static async Task<TimeZoneInfo?> GetTimeZoneInfoAsync(string location)
        {
            Coordinates coordinates = await Coordinates.GetCoordinatesAsync(location);

            if (double.IsNaN(coordinates.Latitude) || double.IsNaN(coordinates.Longitude))
            {
                return null;
            }
            
            TimeZoneResult? tz = TimeZoneLookup.GetTimeZone(coordinates.Latitude, coordinates.Longitude);
            return tz == null ? null : TimeZoneInfo.FindSystemTimeZoneById(tz.Result);
        }
    }
}
