//using Newtonsoft.Json;
//using OneOf;
//using OTools.CoursePlanner;
//using OTools.Courses;
//using OTools.StartTimeDistributor;
//using Spectre.Console;
//using Spectre.Console.Cli;
//using Sunley.Mathematics;
//using System.Data;
//using System.Globalization;

//var app = new CommandApp<StartTimeCommand>();
//return app.Run(args);

//internal sealed class StartTimeCommand : Command<StartTimeCommand.Settings>
//{
//    public sealed class Settings : CommandSettings
//    {
//        [CommandArgument(0, "[filePath]")]
//        public string? FilePath { get; init; }

//        [CommandOption("-m|--iofMalePath")]
//        public string? IofMalePath { get; init; }

//        [CommandOption("-w|--iofFemalePath")]
//        public string? IofFemalePath { get; init; }

//        [CommandOption("-o|--optionsPath")]
//        public string? OptionsPath { get;init; }

//    }

//    public override int Execute(CommandContext context, Settings settings)
//    {

//        if (!File.Exists(settings.FilePath))
//            return 1;

//        string[] lines = File.ReadAllLines(settings.FilePath);

//        List<Entry> entries = new();

//        foreach (string? line in lines.Skip(1))
//        {
//            string[] values = line.Split(',').Select(x => x.Replace("\"", "")).ToArray();

//            int id = values[0].Parse<int>();
//            string name = values[1];
//            string club = values[2];
//            string bofId = values[3];
//            string iofId = values[4];

//            entries.Add(new()
//            {
//                Id = id,

//                Name =  name,
//                Club = club,
//                RankingKey = iofId,

//                Days = GetDays(values),
//            });
//        }

//        if (!File.Exists(settings.OptionsPath) || !File.Exists(settings.IofMalePath) || !File.Exists(settings.IofFemalePath))
//            return 1;

//        var parameters = JsonConvert.DeserializeObject<StartTimeParameters>(File.ReadAllText(settings.OptionsPath));

//        var friStartTimes = new SimpleStartTimes(entries, parameters!.Days[0], 0).Create();

//        string[] rankings = File.ReadAllLines(settings.IofMalePath).Concat(File.ReadAllLines(settings.IofFemalePath)).ToArray();

//        var satEliteStartTimes = new RankedStartTimes(entries, WorldRanking.FromCSV(new[] { settings.IofMalePath, settings.IofFemalePath }).Select(x => (x.Key, x.Value)), parameters!.Days[1], 1).Create();
//        var satNormalStartTimes = new SimpleStartTimes(entries, parameters!.Days[1], 1).Create();
//        var satStartTimes = satEliteStartTimes.Concat(satNormalStartTimes).ToDictionary(x => x.Key, x => x.Value);

//        var sunStartTimes = new SimpleStartTimes(entries, parameters!.Days[2], 2).Create();

//        while (true) {
//            // Show Menu

//            AnsiConsole.Clear();
//            AnsiConsole.Write(
//@"1. Show Friday Start Times
//2. Show Saturday Start Times
//3. Show Sunday Start Times
//4. Export Start Times
//X. Exit

//Choice: ");          

//            char choice = Console.ReadKey().KeyChar;

//            switch (choice.ToString().ToLower()[0])
//            {
//                case 'x':
//                    return 0;

//                case '1':
//                    var filter = GetFilter();
//                    AnsiConsole.Clear();
//                    if (filter.IsT2)
//                    {
//                        AnsiConsole.Write(CreateTable(friStartTimes, parameters.Days[0], 0));
//                        break;
//                    }
//                    AnsiConsole.Write(CreateStartListGrid(friStartTimes, 0, filter));
//                    break;

//                case '2':
//                    filter = GetFilter();
//                    AnsiConsole.Clear();
//                    if (filter.IsT2)
//                    {
//                        AnsiConsole.Write(CreateTable(satStartTimes, parameters.Days[1], 1));
//                        break;
//                    }
//                    AnsiConsole.Write(CreateStartListGrid(satStartTimes, 1, filter));
//                    break;
//                case '3':
//                    filter = GetFilter();
//                    AnsiConsole.Clear();
//                    if (filter.IsT2)
//                    {
//                        AnsiConsole.Write(CreateTable(sunStartTimes, parameters.Days[2], 2));
//                        break;
//                    }
//                    AnsiConsole.Write(CreateStartListGrid(sunStartTimes, 2, filter));
//                    break;
//                case '4':
//                    AnsiConsole.WriteLine();
//                    AnsiConsole.Write("Filepath: ");

//                    string filePath = Console.ReadLine() ?? throw new InvalidOperationException();

//                    {
//                        string[] header = new string[] { "Participant - SiEntries ID", "Admin Only - Start Time Fri", "Admin Only - Start Time Sat", "Admin Only - Start Time Sun" };

//                        var allEntrys = friStartTimes.Keys.Concat(satStartTimes.Keys).Concat(sunStartTimes.Keys).Distinct().OrderBy(x => x.Id).ToArray();


//                        List<string[]> outLines = new();
//                        outLines.Add(header);

//                        foreach (var entry in allEntrys)
//                        {
//                            string[] outEntry = new string[4];

//                            outEntry[0] = entry.Id.ToString();

//                            outEntry[1] = friStartTimes.TryGetValue(entry, out DateTime friTime) ? friTime.ToString("G", CultureInfo.CurrentCulture) : "";
//                            outEntry[2] = satStartTimes.TryGetValue(entry, out DateTime satTime) ? satTime.ToString("G", CultureInfo.CurrentCulture) : "";
//                            outEntry[3] = sunStartTimes.TryGetValue(entry, out DateTime sunTime) ? sunTime.ToString("G", CultureInfo.CurrentCulture) : "";

//                            outLines.Add(outEntry);
//                        }

//                        File.WriteAllLines(filePath, outLines.Select(x => string.Join(',', x)));
//                    }

//                    AnsiConsole.WriteLine("StartTimes saved\nPress any key to continue...");
//                    Console.ReadKey();
//                    break;
//            }

//            Console.ReadKey();
//        }

//    }

//    private static byte CourseMap(string clas)
//    {
//        return clas switch
//        {
//            // Sat
//            "MElite/WRE" => 1,
//            "WElite/WRE" => 2,

//            // Sun
//            "MElite Sprint" => 1,
//            "WElite Sprint" => 2,

//            // Sat & Sun
//            "M40+" => 3,
//            "W40+" => 4,
//            "M55+" => 4,
//            "W55+" => 5,
//            "M65+" => 5,
//            "W65+" => 6,
//            "M75+" => 6,
//            "W75+" => 6,
//            "M16-" => 7,
//            "W16-" => 7,
//            "M12-" => 8,
//            "W12-" => 8,
//            "Course 3 Open" => 3,
//            "Course 4 Open" => 4,
//            "Course 5 Open" => 5,
//            "Course 6 Open" => 6,
//            "Course 7 Open" => 7,
//            "Course 8 Open" => 8,
//            "M Open Urban" => 3,
//            "W Open Urban" => 4,

//            // Friday
//            "Long" => 1,
//            "Medium" => 2,
//            "Short" => 3,
//            "Junior/Novice" => 4,

//            _ => throw new Exception()
//        };
//    }
//    private static Entry.Day[] GetDays(string[] values)
//    {
//        Entry.Day[] days = new Entry.Day[3];

//        if (values[5] != "Not Entering Friday")
//            days[0] = new()
//            {
//                Class = values[5],
//                Course = CourseMap(values[5]),
//                Preference = Enum.Parse<StartTimeBlock>(values[6]),
//            };

//        if (values[7] != "Not Entering Saturday")
//            days[1] = new()
//            {
//                Class = values[7],
//                Course = CourseMap(values[7]),
//                Preference = Enum.Parse<StartTimeBlock>(values[8]),
//            };

//        if (values[9] != "Not Entering Sunday")
//            days[2] = new()
//            {
//                Class = values[9],
//                Course = CourseMap(values[9]),
//                Preference = Enum.Parse<StartTimeBlock>(values[10]),
//            };

//        return days;

//    }

//    private static Table CreateStartListGrid(Dictionary<Entry, DateTime> startTimes, int day, OneOf<byte, string, char> filter)
//    {
//        if (filter.IsT2)
//        {
             
//            //return CreateStartListGrid(startTimes, day, filter.AsT2 == '1');
//        }

//        var filtered = startTimes.Where(x =>
//        {
//            if (filter.IsT0)
//                return x.Key.Days[day].Course == filter.AsT0;
//            else
//                return x.Key.Days[day].Class == filter.AsT1;
//        }).Select(x => (x.Value, x.Key.Name));

//        Table table = new();
//        table.Centered();

//        table.AddColumn("Start Time");
//        table.AddColumn("Name");

//        foreach (var (time, name) in filtered.OrderBy(x => x.Item1))
//        {
//            table.AddRow(new string[] { time.ToString("HH:mm:ss"), name });
//        }

//        return table;
//    }
//    public static Table CreateStartListGrid(Dictionary<Entry, DateTime> startTimes, int day, bool byCourse)
//    {
//        var courses = startTimes.Keys.Select(x => byCourse ? x.Days[day].Course.ToString() : x.Days[day].Class).Distinct().OrderBy(x => x).ToArray();

//        Table table = new();
//        table.Centered();

//        table.AddColumn("Start Time");
//        foreach (var course in courses)
//            table.AddColumn(course);

        

//        foreach (var time in startTimes.Values.Distinct().OrderBy(x => x))
//        {
//            List<string> row = new() { time.ToString("HH:mm:ss") };

//            var filtered = startTimes.Where(x => x.Value == time).ToDictionary(x => x.Key, x => x.Value);

//            foreach (var course in courses)
//            {
//                Entry? entry;
                
//                try
//                {
//                    entry = filtered.Keys.First(x => byCourse ? x.Days[day].Course.ToString() == course : x.Days[day].Class == course);
//                } catch { entry = null; }

//                if (entry is null)
//                    row.Add("");
//                else
//                    row.Add(entry.Value.Name);
//            }


//            table.AddRow(row.ToArray());
//        }

//        return table;
//    }

//    private static Table CreateTable(Dictionary<Entry, DateTime> entries, DayStartTimeParameters parameters, int day)
//    {
//        Table table = new();
//        table.Centered();

//        var courses = parameters.CourseGroupings.Aggregate((x, y) => x.Concat(y).ToArray());

//        table.AddColumn("Start Time");
//        foreach (var course in courses)
//            table.AddColumn(course.ToString());

//        var reversedEntries = entries.Select(x => (x.Value, x.Key)).ToArray();
//        TimeSpan startInterval = TimeSpan.FromMinutes(parameters.Parameters.Select(x => x.StartInterval).Min());

//        DateTime currentTime = parameters.Parameters.Select(x => x.FirstStart).Min();
//        DateTime lastStart = parameters.Parameters.Select(x => x.LastStart).Max();
        
//        while (currentTime <= lastStart)
//        {
//            var thisTime = reversedEntries.Where(x => x.Value == currentTime).Select(x => x.Key);

//            List<string> cols = new() { currentTime.ToString("T") };
//            foreach (var course in courses)
//            {
//                string e = thisTime.FirstOrDefault(x => x.Days[day].Course == course).Name ?? "";
//                cols.Add(e);
//            }

//            table.AddRow(cols.ToArray());

//            currentTime += startInterval;
//        }

//        return table;
//    }

//    private static OneOf<byte, string, char> GetFilter()
//    {
//        while (true)
//        {
//            AnsiConsole.Clear();
//            AnsiConsole.Write(
//@"1. Filter by Course
//2. Filter by Class
//3. Show All
                              
//Choice: ");

//            char choice = Console.ReadKey().KeyChar;
//            AnsiConsole.WriteLine();
//            switch (choice.ToString().ToLower()[0])
//            {
//                case '1':
//                    AnsiConsole.Write("Course: ");
//                    return Console.ReadKey().KeyChar.ToString().Parse<byte>();

//                case '2':
//                    AnsiConsole.Write("Class: ");
//                    return Console.ReadLine()!;

//                case '3':
//                    AnsiConsole.WriteLine();
//                    AnsiConsole.Write(
//@"1. Group by Course
//2. Group by Class

//Choice: ");
//                    return Console.ReadKey().KeyChar;
//            }

//        }
//    }   
//}