using LiteDB;

namespace DiscordDnDBot.Types.Database
{
    public class Player
    {
        [BsonId]
        public ulong Id { get; set; }
        public TimeZoneInfo TimeZone { get; set; }

        public Player()
        {
            Id = 0;
            TimeZone = TimeZoneInfo.Local;
        }

        public Player(ulong _id, TimeZoneInfo timeZone)
        {
            Id = _id;
            TimeZone = timeZone;
        }
    }
}
