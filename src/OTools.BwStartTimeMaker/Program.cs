using Newtonsoft.Json;
using OneOf;
using OTools.CoursePlanner;
using OTools.StartTimeDistributor;
using Spectre.Console;
using Spectre.Console.Cli;
using Sunley.Mathematics;
using System.Data;
using System.Globalization;

var app = new CommandApp<StartTimeCommand>();
int result =  app.Run(args);
Console.WriteLine(result);
return result;

internal sealed class StartTimeCommand : Command<StartTimeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[filePath]")]
        public string? FilePath { get; init; }

        [CommandOption("-m|--iofMalePath")]
        public string? IofMalePath { get; init; }

        [CommandOption("-w|--iofFemalePath")]
        public string? IofFemalePath { get; init; }

        [CommandOption("-o|--optionsPath")]
        public string? OptionsPath { get;init; }

        [CommandOption("-l|--load")]
        public bool IsLoad { get; init; }

    }

    public override int Execute(CommandContext context, Settings settings)
    {

        if (!File.Exists(settings.FilePath))
            return 1;

        List<Entry> entries = new();
        Dictionary<Entry, DateTime> friday = new();
        Dictionary<Entry, DateTime> saturday = new();
        Dictionary<Entry, DateTime> sunday = new();


        if (settings.IsLoad) {

            List<TempEntry> tEntr = new();

            string[] lines = File.ReadAllLines(settings.FilePath);
            string[] header = lines[0].Split(',');

            foreach (string l in lines.Skip(1))
            {
                string[] line = l.Split(',');

                DateTime time = DateTime.Parse(line[0]);

                for (int i = 1; i < line.Length; i++)
                {
                    if (line[i] != "")
                    {
                        int id = line[i].Split(';')[1].Parse<int>();

                        if (!tEntr.Select(x => x.id).Contains(id))
                        {
                            if (header[i].Contains("Friday"))
                                tEntr.Add(new() { id = id, fri = time });
                            else if (header[i].Contains("Saturday"))
                                tEntr.Add(new() { id = id, sat = time });
                            else if (header[i].Contains("Sunday"))
                                tEntr.Add(new() { id = id, sat = time });
                            else throw new Exception();
                        }
                        else
                        {
                            int index = tEntr.IndexOf(tEntr.First(x => x.id == id));

                            if (header[i].Contains("Friday"))
                                tEntr[index] = new() { id = id, fri = time, sat = tEntr[index].sat, sun = tEntr[index].sun };
                            else if (header[i].Contains("Saturday"))
                                tEntr[index] = new() { id = id, fri = tEntr[index].fri, sat = time, sun = tEntr[index].sun };
                            else if (header[i].Contains("Sunday"))
                                tEntr[index] = new() { id = id, fri = tEntr[index].fri, sat = tEntr[index].sat, sun = time };
                            else throw new Exception();

                        }
                    }
                }
            }

            AnsiConsole.Write("Filepath: ");
            string path = Console.ReadLine() ?? throw new InvalidOperationException();

            string[] outHeader = new string[] { "Participant - SiEntries ID", "Admin Only - Start Time Fri", "Admin Only - Start Time Sat", "Admin Only - Start Time Sun" };

            List<string[]> outLines = new() { outHeader };

            foreach (var e in tEntr)
            {
                string[] outEntry = new string[4];

                outEntry[0] = e.id.ToString();

                outEntry[1] = e.fri.TimeOfDay.ToString() ?? "";
                outEntry[2] = e.sat.TimeOfDay.ToString() ?? "";
                outEntry[3] = e.sun.TimeOfDay.ToString() ?? "";

                if (outEntry[1] == "00:00:00") outEntry[1] = "";
                if (outEntry[2] == "00:00:00") outEntry[2] = "";
                if (outEntry[3] == "00:00:00") outEntry[3] = "";

                outLines.Add(outEntry);
            }

            File.WriteAllLines(path, outLines.Select(x => string.Join(',', x)));

            Console.WriteLine("Done");
            Console.ReadLine();

            return 0;
        }
        else
        {
            string[] lines = File.ReadAllLines(settings.FilePath);


            foreach (string? line in lines.Skip(1))
            {
                string[] values = line.Split(',').Select(x => x.Replace("\"", "")).ToArray();

                int id = values[0].Parse<int>();
                string name = values[1];
                string club = values[2];
                string bofId = values[3];
                string iofId = values[4];

                entries.Add(new()
                {
                    Id = id,

                    Name = name,
                    Club = club,
                    RankingKey = iofId,

                    Days = GetDays(values),
                });
            }

            if (!File.Exists(settings.OptionsPath) || !File.Exists(settings.IofMalePath) || !File.Exists(settings.IofFemalePath))
                return 1;

            var parameters = JsonConvert.DeserializeObject<StartTimeParameters>(File.ReadAllText(settings.OptionsPath));

            friday = new SimpleStartTimes(entries, parameters!.Days[0], 0).Create();

            string[] rankings = File.ReadAllLines(settings.IofMalePath).Concat(File.ReadAllLines(settings.IofFemalePath)).ToArray();

            var satEliteStartTimes = new RankedStartTimes(entries, WorldRanking.FromCSV(new[] { settings.IofMalePath, settings.IofFemalePath }).Select(x => (x.Key, x.Value)), parameters!.Days[1], 1).Create();
            var satNormalStartTimes = new SimpleStartTimes(entries, parameters!.Days[1], 1).Create();
            saturday = satEliteStartTimes.Concat(satNormalStartTimes).ToDictionary(x => x.Key, x => x.Value);

            sunday = new SimpleStartTimes(entries, parameters!.Days[2], 2).Create();
        }


        while (true) {
            // Show Menu

            AnsiConsole.Clear();
            AnsiConsole.Write(
@"1. Show Friday Start Times
2. Show Saturday Start Times
3. Show Sunday Start Times
4. Export Spreadsheet
S. Save Start Times
E. Export Start Times
X. Exit

Choice: ");

            

            char choice = Console.ReadKey().KeyChar;

            switch (choice.ToString().ToLower()[0])
            {
                case 'x':
                    return 0;

                case '1':
                    var filter = GetFilter();
                    AnsiConsole.Clear();
                    AnsiConsole.Write(CreateStartListGrid(friday, 0, filter));
                    break;

                case '2':
                    filter = GetFilter();
                    AnsiConsole.Clear();
                    AnsiConsole.Write(CreateStartListGrid(saturday, 1, filter));
                    break;
                case '3':
                    filter = GetFilter();
                    AnsiConsole.Clear();
                    AnsiConsole.Write(CreateStartListGrid(sunday, 2, filter));
                    break;
                case '4':
                {

                    AnsiConsole.WriteLine();
                    AnsiConsole.Write("Filepath: ");

                    string fPath = Console.ReadLine() ?? throw new InvalidOperationException();


                    var allTimes = friday.Select(x => x.Value).Concat(saturday.Select(x => x.Value)).Concat(sunday.Select(x => x.Value));
                    TimeSpan firstStart = allTimes.Select(x => x.TimeOfDay).Min();
                    TimeSpan lastStart = allTimes.Select(x => x.TimeOfDay).Max();

                    TimeSpan curr = firstStart;

                    var friCourses = friday.Keys.Select(x => x.Days[0].Course).Distinct();
                    var satCourses = saturday.Keys.Select(x => x.Days[1].Course).Distinct();
                    var sunCourses = sunday.Keys.Select(x => x.Days[2].Course).Distinct();


                    List<string> header = new() { "TimeOfDay " };

                    foreach (var c in friCourses)
                        header.Add($"Friday {c}");
                    foreach (var c in satCourses)
                        header.Add($"Saturday {c}");
                    foreach (var c in sunCourses)
                        header.Add($"Sunday {c}");

                    List<string[]> aLines = new() { header.ToArray() };

                    while (curr < lastStart)
                    {
                        List<string> tLine = new() { curr.ToString() };

                        foreach (var c in friCourses)
                        {
                            var r = friday.Where(x => x.Value.TimeOfDay == curr && x.Key.Days[0].Course == c);

                            if (r.Count() == 0)
                                tLine.Add("");
                            else
                            {
                                var t = r.First().Key;
                                tLine.Add($"{t.Name};{t.Id}");
                            }
                        }
                        foreach (var c in satCourses)
                        {
                            var r = saturday.Where(x => x.Value.TimeOfDay == curr && x.Key.Days[1].Course == c);

                            if (r.Count() == 0)
                                tLine.Add("");
                            else
                            {
                                var t = r.First().Key;
                                tLine.Add($"{t.Name};{t.Id}");
                            }
                        }
                        foreach (var c in sunCourses)
                        {
                            var r = sunday.Where(x => x.Value.TimeOfDay == curr && x.Key.Days[2].Course == c);

                            if (r.Count() == 0)
                                tLine.Add("");
                            else
                            {
                                var t = r.First().Key;
                                tLine.Add($"{t.Name};{t.Id}");
                            }
                        }

                        aLines.Add(tLine.ToArray());
                        curr += TimeSpan.FromMinutes(1);
                    }

                    File.WriteAllLines(fPath, aLines.Select(x => string.Join(',', x)));

                    AnsiConsole.WriteLine("Saved");

                }
                break;
                case 's':
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write("Filepath: ");

                    string filePath = Console.ReadLine() ?? throw new InvalidOperationException();

                    Save(filePath, friday, saturday, sunday);

                    AnsiConsole.WriteLine("StartTimes saved\nPress any key to continue...");
                    Console.ReadKey();
                    break;
                }
                case 'e':
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write("Filepath: ");

                    string filePath = Console.ReadLine() ?? throw new InvalidOperationException();

                    Export(filePath, friday, saturday, sunday);

                    AnsiConsole.WriteLine("StartTimes saved\nPress any key to continue...");
                    Console.ReadKey();
                    break;

                }
            }

            Console.ReadKey();
        }

    }

    private static byte CourseMap(string clas)
    {
        return clas switch
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
            "M Open Urban" => 3,
            "W Open Urban" => 4,

            // Friday
            "Long" => 1,
            "Medium" => 2,
            "Short" => 3,
            "Junior/Novice" => 4,

            _ => throw new Exception()
        };
    }
    private static Entry.Day[] GetDays(string[] values)
    {
        Entry.Day[] days = new Entry.Day[3];

        if (values[5] != "Not Entering Friday")
            days[0] = new()
            {
                Class = values[5],
                Course = CourseMap(values[5]),
                Preference = Enum.Parse<StartTimeBlock>(values[6]),
            };

        if (values[7] != "Not Entering Saturday")
            days[1] = new()
            {
                Class = values[7],
                Course = CourseMap(values[7]),
                Preference = Enum.Parse<StartTimeBlock>(values[8]),
            };

        if (values[9] != "Not Entering Sunday")
            days[2] = new()
            {
                Class = values[9],
                Course = CourseMap(values[9]),
                Preference = Enum.Parse<StartTimeBlock>(values[10]),
            };

        return days;

    }

    private static Table CreateStartListGrid(Dictionary<Entry, DateTime> startTimes, int day, OneOf<byte, string, char> filter)
    {
        if (filter.IsT2)
        {
            return CreateStartListGrid(startTimes, day, filter.AsT2 == '1');
        }

        var filtered = startTimes.Where(x =>
        {
            if (filter.IsT0)
                return x.Key.Days[day].Course == filter.AsT0;
            else
                return x.Key.Days[day].Class == filter.AsT1;
        }).Select(x => (x.Value, x.Key.Name));

        Table table = new();
        table.Centered();

        table.AddColumn("Start Time");
        table.AddColumn("Name");

        foreach (var (time, name) in filtered.OrderBy(x => x.Item1))
        {
            table.AddRow(new string[] { time.ToString("HH:mm:ss"), name });
        }

        return table;
    }
    public static Table CreateStartListGrid(Dictionary<Entry, DateTime> startTimes, int day, bool byCourse)
    {
        var courses = startTimes.Keys.Select(x => byCourse ? x.Days[day].Course.ToString() : x.Days[day].Class).Distinct().OrderBy(x => x).ToArray();

        Table table = new();
        table.Centered();

        table.AddColumn("Start Time");
        foreach (var course in courses)
            table.AddColumn(course);

        //grid.AddRow(new string[] { "Start Time" }.Concat(courses).ToArray());

        //foreach (var time in startTimes.Values.Distinct().OrderBy(x => x))
        var distinctStartTimes = startTimes.Values.Distinct();
        for (DateTime time = distinctStartTimes.Min(); time <= distinctStartTimes.Max(); time += TimeSpan.FromMinutes(1))
        {
            List<string> row = new() { time.ToString("HH:mm:ss") };

            var filtered = startTimes.Where(x => x.Value == time).ToDictionary(x => x.Key, x => x.Value);

            foreach (var course in courses)
            {
                Entry? entry;
                
                try
                {
                    entry = filtered.Keys.First(x => byCourse ? x.Days[day].Course.ToString() == course : x.Days[day].Class == course);
                } catch { entry = null; }

                if (entry is null)
                    row.Add("");
                else
                    row.Add(entry.Value.Name);
            }


            table.AddRow(row.ToArray());
        }

        return table;
    }

    private static OneOf<byte, string, char> GetFilter()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
@"1. Filter by Course
2. Filter by Class
3. Show All
                              
Choice: ");

            char choice = Console.ReadKey().KeyChar;
            AnsiConsole.WriteLine();
            switch (choice.ToString().ToLower()[0])
            {
                case '1':
                    AnsiConsole.Write("Course: ");
                    return Console.ReadKey().KeyChar.ToString().Parse<byte>();

                case '2':
                    AnsiConsole.Write("Class: ");
                    return Console.ReadLine()!;

                case '3':
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(
@"1. Group by Course
2. Group by Class

Choice: ");
                    return Console.ReadKey().KeyChar;
            }

        }
    }   

    private static void Save(string filePath, Dictionary<Entry, DateTime> friday, Dictionary<Entry, DateTime> saturday, Dictionary<Entry, DateTime> sunday)
    {
        string[] header = new string[] { "Participant - SiEntries ID", "Participant - Full Name", "Admin Only - Start Time Fri", "Admin Only - Start Time Sat", "Admin Only - Start Time Sun" };

        var allEntrys = friday.Keys.Concat(saturday.Keys).Concat(sunday.Keys).Distinct().OrderBy(x => x.Id).ToArray();

        List<string[]> outLines = new() { header };

        foreach (var entry in allEntrys)
        {
            string[] outEntry = new string[5];

            outEntry[0] = entry.Id.ToString();
            outEntry[1] = entry.Name.ToString();

            outEntry[2] = friday.TryGetValue(entry, out DateTime friTime) ? friTime.ToString("G", CultureInfo.CurrentCulture) : "";
            outEntry[3] = saturday.TryGetValue(entry, out DateTime satTime) ? satTime.ToString("G", CultureInfo.CurrentCulture) : "";
            outEntry[4] = sunday.TryGetValue(entry, out DateTime sunTime) ? sunTime.ToString("G", CultureInfo.CurrentCulture) : "";

            outLines.Add(outEntry);
        }

        File.WriteAllLines(filePath, outLines.Select(x => string.Join(',', x)));
    }

    private static void Export(string filePath, Dictionary<Entry, DateTime> friday, Dictionary<Entry, DateTime> saturday, Dictionary<Entry, DateTime> sunday)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        string[] header = new string[] { "Participant - SiEntries ID", "Admin Only - Start Time Fri", "Admin Only - Start Time Sat", "Admin Only - Start Time Sun" };

        var allEntrys = friday.Keys.Concat(saturday.Keys).Concat(sunday.Keys).Distinct().OrderBy(x => x.Id).ToArray();


        List<string[]> outLines = new() { header };

        foreach (var entry in allEntrys)
        {
            string[] outEntry = new string[4];

            outEntry[0] = entry.Id.ToString();

            outEntry[1] = friday.TryGetValue(entry, out DateTime friTime) ? friTime.ToString("G", CultureInfo.CurrentCulture) : "";
            outEntry[2] = saturday.TryGetValue(entry, out DateTime satTime) ? satTime.ToString("G", CultureInfo.CurrentCulture) : "";
            outEntry[3] = sunday.TryGetValue(entry, out DateTime sunTime) ? sunTime.ToString("G", CultureInfo.CurrentCulture) : "";

            outLines.Add(outEntry);
        }

        File.WriteAllLines(filePath, outLines.Select(x => string.Join(',', x)));
    }

    private static (List<Entry> entry, Dictionary<Entry, DateTime> fri, Dictionary<Entry, DateTime> sat, Dictionary<Entry, DateTime> sun) Load(string filePath)
    {
        Dictionary<Entry, DateTime> friday = new();
        Dictionary<Entry, DateTime> saturday = new();
        Dictionary<Entry, DateTime> sunday = new();
        List<Entry> entries = new();

        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        string[] lines = File.ReadAllLines(filePath);

        foreach (var line in lines.Skip(1))
        {
            string[] split = line.Split(',');

            Entry e = new()
            {
                Id = split[0].Parse<int>(),
                Name = split[1],
            };

            if (split[2] != "")
                friday.Add(e, split[2].Parse<DateTime>());

            if (split[3] != "")
                saturday.Add(e, split[3].Parse<DateTime>());

            if (split[4] != "")
                sunday.Add(e, split[4].Parse<DateTime>());
        }

        return (entries, friday, saturday, sunday);
    }
}

struct TempEntry {
    public int id;
    public DateTime fri;
    public DateTime sat;
    public DateTime sun;
}