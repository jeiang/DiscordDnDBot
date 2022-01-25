# DiscordDnDBot


## Config
Rename config.defaults.json to config.json and change fields as appropriate.

Explanation of the fields in config.defaults.json.
- `Token`: The discord token to log in with. **Required**
- `LogLevel`: How much to log. Default is Info. Supported Levels:
    - Debug
    - Verbose
    - Info
    - Warning
    - Error
    - Critical
- `DatabasePath`: Filepath to where to store the bot's database. Default is "./Database.db".
- `InteractionServiceConfig`: Configuration for the interaction service. See [Discord.Net API](https://discordnet.dev/api/Discord.Interactions.InteractionServiceConfig.html) for details on each field.
- `DiscordSocketConfig`: Configuration for the interaction service. See [Discord.Net API](https://discordnet.dev/api/Discord.WebSocket.DiscordSocketConfig.html) for details on each field.
- `ApplicationCommands`: Whether to load or delete application commands saved on Discord for this bot.
    - `Global`: Applies to Global commands.
    - `Other`: Should be a Discord Guild Id as a string. Applies to commands for that Guild

### To be Implemented
- `CommandName`: Load specific commands for guilds or globally.
- `DiscordConfig`: Configuration for Discord.Net in Discord.Net. See [Discord.Net docs](https://discordnet.dev/api/Discord.DiscordConfig.html) for details.

## Notes
This is still a work in progress, and the code may look very wonky.