using Newtonsoft.Json;
using OTools.StartTimeDistributor;
using OTools.StartTimeGenerator;
using Spectre.Console.Cli;
using System.Text.Json.Serialization;
using static StartTimeCommand;

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
        try
        {
            var startTimes = Create(
                settings.EntriesPath,
                settings.EntriesFilter,
                settings.RankingsPath,
                settings.RankingsFilter,
                settings.OptionsPath);

            Console.Write("Input: ");
            var str = Console.ReadLine();

            StartTimeEditor e = new(startTimes);

            var res = e.FuzzySearch(str);

            Console.ReadLine();

        } catch { return 1; }


        return 0;
    }

    public static Dictionary<Entry, DateTime> Create(string? entriesPath, string? entriesFilter, string? rankingsPath, string? rankingsFilter, string? optionsPath)
    {
        if (!File.Exists(entriesPath) ||
            !File.Exists(optionsPath) ||
            !File.Exists(rankingsPath))
            throw new ArgumentException();

        if (entriesFilter == null ||
            rankingsFilter == null)
            throw new ArgumentException();


        List<Entry> entries = Loader.LoadEntries(entriesPath, entriesFilter).ToList();
        Dictionary<string, float> rankings = Loader.LoadRankings(rankingsPath, rankingsFilter);

        var parameters = JsonConvert.DeserializeObject<StartTimeParameters>(File.ReadAllText(optionsPath))!;

        Dictionary<Entry, DateTime> startTimes = new();

        for (int i = 0; i < parameters.Parameters.Length; i++)
        {
            int[] groupings = parameters.CourseGroupings[i];
            GroupParameters param = parameters.Parameters[i];

            var groupStartTimes = param.AssociationType switch
            {
                "simple" => new SimpleStartTimes(entries, param, groupings).Create(),
                "ranked" => new RankedStartTimes(entries, rankings.Select(x => (x.Key, x.Value)), param, groupings).Create(),
                _ => throw new ArgumentException(),
            };

            foreach (var s in groupStartTimes)
                startTimes.Add(s.Key, s.Value);
        }

        return startTimes;
    }
}