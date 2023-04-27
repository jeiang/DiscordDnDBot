using Discord;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace DiscordDnDBot.Services
{
    public class DatabaseService : IDisposable
    {
        // add new database for cached data
        private readonly IConfigurationRoot _config;

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

        public LiteDatabase GetDatabase() => db;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            db.Dispose();
        }
    }
}