using LiteDB;

namespace DiscordDnDBot.Types.Database
{
    public class Player
    {
        [BsonId]
        public int Id { get; }
        public string Timezone { get; }
        public long Offset { get; }

        [BsonCtor]
        public Player(int id, string timezone, long offset)
        {
            Id = id;
            Timezone = timezone;
            Offset = offset;
        }
    }
}
