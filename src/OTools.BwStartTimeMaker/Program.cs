using Newtonsoft.Json;
using OTools.StartTimeDistributor;
using Spectre.Console;
using Spectre.Console.Cli;
using Sunley.Mathematics;

var app = new CommandApp<StartTimeCommand>();
return app.Run(args);

internal sealed class StartTimeCommand : Command<StartTimeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[filePath]")]
        public string? FilePath { get; init; }

        [CommandOption("-i|--iofPath")]
        public string? IofCsvPath { get; init; }

        [CommandOption("-l|--online")]
        public bool UseOnlineIof { get; init; }

        [CommandOption("-o|--optionsPath")]
        public string? OptionsPath { get;init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!File.Exists(settings.FilePath))
            return 1;

        string[] lines = File.ReadAllLines(settings.FilePath);

        List<Entry> entries = new();

        foreach (string? line in lines.Skip(1))
        {
            string[] values = line.Split(',').Select(x => x.Replace("\"", "")).ToArray();

            int id = values[2].Parse<int>();
            string name = values[6];
            string club = values[18];
            string bofId = values[20];
            string iofId = values[21];
            string clas = values[30];
            byte course = clas switch
            {
                // Sat
                "MElite/WRE" => 1,
                "WElite/WRE" => 2,

                // Sun
                "MElite Sprint" => 1,
                "WElite Sprint" => 2,

                // Sat & Sun
                "M40+" => 3,
                "W40+" => 4,
                "M55+" => 4,
                "W55+" => 5,
                "M65+" => 5,
                "W65+" => 6,
                "M75+" => 6,
                "W75+" => 6,
                "M16-" => 7,
                "W16-" => 7,
                "M12-" => 8,
                "W12-" => 8,
                "Course 3 Open" => 3,
                "Course 4 Open" => 4,
                "Course 5 Open" => 5,
                "Course 6 Open" => 6,
                "Course 7 Open" => 7,
                "Course 8 Open" => 8,

                // Friday
                "Long" => 1,
                "Medium" => 2,
                "Short" => 3,
                "Junior/Novice" => 4,

                _ => throw new Exception()
            };

            StartTimeBlock pref = Enum.Parse<StartTimeBlock>(values[31]);

            entries.Add(new()
            {
                Id = id,

                Name = name,

                Class = clas,
                Course = course,
                Club = club,

                Preference = pref,

                RankingKey = iofId,
            });
        }

        var fridayStartTimes = new SimpleStartTimes(entries, )

        throw new NotImplementedException();
    }
}