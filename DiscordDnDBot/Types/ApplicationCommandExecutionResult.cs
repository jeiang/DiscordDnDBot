using Discord.Interactions;

namespace DiscordDnDBot.Types
{
    public class ApplicationCommandExecutionResult : RuntimeResult
    {
        public ApplicationCommandExecutionResult(InteractionCommandError? error, string reason) : base(error, reason)
        { }

        public static ApplicationCommandExecutionResult FromSuccess() => new(null, "");

        public static ApplicationCommandExecutionResult FromError(InteractionCommandError? error, string reason)
            => new(error, reason);
    }
}
