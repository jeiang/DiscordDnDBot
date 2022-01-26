using DiscordDnDBot.Services;
using LiteDB;

namespace DiscordDnDBot.Types.Database
{
    public static partial class DatabaseServiceExtensions
    {
        public static Task<bool> AddOrUpdate(this DatabaseService dbs, Player player)
        {
            LiteDatabase db = dbs.GetDatabase();
            ILiteCollection<Player> collection = db.GetCollection<Player>();
            return Task.FromResult(collection.Upsert(player));
        }

        public static Task<bool> DeletePlayer(this DatabaseService dbs, Player player)
        {
            LiteDatabase db = dbs.GetDatabase();
            ILiteCollection<Player> collection = db.GetCollection<Player>();
            return Task.FromResult(collection.Delete(player.Id));
        }

        public static Task<Player?> GetPlayer(this DatabaseService dbs, ulong id)
        {
            LiteDatabase db = dbs.GetDatabase();
            ILiteCollection<Player> collection = db.GetCollection<Player>();
            return Task.FromResult<Player?>(collection.FindOne(player => player.Id == id));
        }
    }
}
