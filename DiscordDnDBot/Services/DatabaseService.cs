using Discord;
using DiscordDnDBot.Types.Database;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace DiscordDnDBot.Services
{
    public class DatabaseService : IDisposable
    {
        // add new database for cached data
        private readonly IConfiguration _config;
        private readonly LoggingService _loggingService;
        private readonly LiteDatabase db;

        private static BsonMapper CreateBsonMapper()
        {
            BsonMapper mapper = new()
            {
                EmptyStringToNull = true,
                EnumAsInteger = true
            };

            mapper.RegisterType(
                serialize: (tzi) => tzi.ToSerializedString(),
                deserialize: (tzi) => TimeZoneInfo.FromSerializedString(tzi));

            return mapper;
        }

        public DatabaseService(IConfigurationRoot configurationRoot, LoggingService loggingService)
        {
            _config = configurationRoot;
            _loggingService = loggingService;

            string databaseName;
            try
            {
                databaseName =
                    string.IsNullOrWhiteSpace(_config["DatabasePath"])
                    ? Path.Combine(AppContext.BaseDirectory, "Database.db")
                    : new FileInfo(_config["DatabasePath"]).FullName;
            }
            catch (Exception ex)
            {
                _loggingService.LogAsync("DatabaseService", $"Unable to create database at {_config["DatabasePath"]}." +
                    $"Using default path", LogSeverity.Warning, ex).Wait();
                databaseName = Path.Combine(AppContext.BaseDirectory, "Database.db");
            }

            BsonMapper mapper = CreateBsonMapper();

            db = new LiteDatabase(new ConnectionString()
            {
                Filename = databaseName,
                Password = nameof(DiscordDnDBot),
                Upgrade = true,
                Connection = ConnectionType.Shared
            }, mapper);
        }

        /// <summary>
        /// Add 
        /// </summary>
        /// <param name="player">Player to add to the database.</param>
        /// <returns>True if added new, false if updated existing.</returns>
        public async Task<bool> AddOrUpdate(Player player)
        {
            ILiteCollection<Player> collection = db.GetCollection<Player>();
            await _loggingService.LogAsync("DatabaseService", $"Saving/Updating user: {player.Id}.",
                LogSeverity.Verbose);
            return collection.Upsert(player);
        }

        public async Task<bool> DeletePlayer(Player player)
        {
            ILiteCollection<Player> collection = db.GetCollection<Player>();
            await _loggingService.LogAsync("DatabaseService", $"Deleting user: {player.Id}",
                LogSeverity.Verbose);
            bool success = collection.Delete(player.Id);
            if (!success)
            {
                await _loggingService.LogAsync("DatabaseService", $"Failed to delete user: {player.Id}",
                    LogSeverity.Warning);
            }
            return success;
        }

        public async Task<Player?> GetPlayer(ulong id)
        {
            ILiteCollection<Player> collection = db.GetCollection<Player>();
            await _loggingService.LogAsync("DatabaseService", $"Retrieving user: {id}",
                LogSeverity.Verbose);
            return collection.FindOne(player => player.Id == id);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            db.Dispose();
        }
    }
}
