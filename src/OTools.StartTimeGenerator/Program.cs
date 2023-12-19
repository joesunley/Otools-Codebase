using Spectre.Console.Cli;

var app = new CommandApp<StartTimeCommand>();
return app.Run(args);

internal sealed class StartTimeCommand : Command<StartTimeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[entriespath]")]
        public string? EntriesPath { get; init; }

        [CommandOption("-e|--entryfilter")]
        public string? EntriesFilter { get; init; }

        [CommandOption("-r|--rankingspath")]
        public string? RankingsPath { get; init; }

        [CommandOption("-f|--rankingfilter")]
        public string? RankingsFilter { get; init; }

        [CommandOption("-o|--optionspath")]
        public string? OptionsPath { get; init; } 
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        throw new NotImplementedException();
    }
}