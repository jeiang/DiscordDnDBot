using DiscordDnDBot.Types.Database;

using Discord;

using LiteDB;

using Microsoft.Extensions.Configuration;

namespace DiscordDnDBot.Services
{
    public class DatabaseService
    {
        private readonly IConfiguration _config;
        private readonly LoggingService _loggingService;
        private readonly BsonMapper _mapper;
        private readonly string _databaseName;

        private LiteDatabase GetDatabase()
        {
            if(!File.Exists(_databaseName))
            {
                _loggingService.LogAsync("DatabaseService", $"Creating empty database.", 
                    Discord.LogSeverity.Warning).Wait();
                File.Create(_databaseName).Dispose();
            }

            return new LiteDatabase(_databaseName);
        }

        public DatabaseService(IConfigurationRoot configurationRoot, LoggingService loggingService)
        {
            _config = configurationRoot;
            _loggingService = loggingService;
            
            try
            {
                if (string.IsNullOrWhiteSpace(_config["DatabasePath"]))
                {
                    _databaseName = Path.Combine(AppContext.BaseDirectory, "Database.db");
                }
                else
                {
                    _databaseName = new FileInfo(_config["DatabasePath"]).FullName;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogAsync("DatabaseService", $"Unable to create database at {_config["DatabasePath"]}." +
                    $"Using default path", Discord.LogSeverity.Warning, ex).Wait();
                _databaseName = Path.Combine(AppContext.BaseDirectory, "Database.db");
            }

            _mapper = new BsonMapper()
            {
                EmptyStringToNull = true,
                EnumAsInteger = true
            };
        }

        public async Task AddOrUpdate(Player player)
        {
            using LiteDatabase db = GetDatabase();
            ILiteCollection<Player> collection = db.GetCollection<Player>();
            await _loggingService.LogAsync("DatabaseService", $"Saving/Updating user: {player.Id}.", 
                Discord.LogSeverity.Verbose);
            _ = collection.Upsert(player);
        }

        public async Task DeletePlayer(Player playerToDelete)
        {
            using LiteDatabase db = GetDatabase();
            ILiteCollection<Player> collection = db.GetCollection<Player>();
            await _loggingService.LogAsync("DatabaseService", $"Deleting user: {playerToDelete.Id}", 
                LogSeverity.Verbose);
            if (!collection.Delete(playerToDelete.Id))
            {
                await _loggingService.LogAsync("DatabaseService", $"Failed to delete user: {playerToDelete.Id}",
                    LogSeverity.Warning);
            }
        }
    }
}
