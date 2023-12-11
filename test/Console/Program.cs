using Newtonsoft.Json;
using OTools.StartTimeDistributor;
using Sunley.Mathematics;
using System.Text.Json.Serialization;

StartTimeParameters sunParams = new()
{
    FirstStart = new(2024, 01, 28, 9, 00, 00),
    LastStart = new(2024, 01, 28, 12, 00, 00),

    //ExcludedCourses = new[] { 1, 2 },
    
    CourseSpacing = TimeSpan.FromMinutes(1),
    ClassSpacing = TimeSpan.FromMinutes(2),
    ClubSpacing = TimeSpan.FromMinutes(4),

    StartInterval = TimeSpan.FromMinutes(1),
};

var json = JsonConvert.DeserializeObject<StartTimeParametersNew>(File.ReadAllText(@"C:\Users\joe\Downloads\starttimeoptions.json"));

var lines = File.ReadAllLines(@"C:\Users\joe\Downloads\participant_list_20231110_121300.csv");

List<Entry> entries = new();

foreach (var line in lines.Skip(1))
{
    var values = line.Split(',').Select(x => x.Replace("\"", "")).ToArray();

    int id = values[2].Parse<int>();
    string name = values[6];
    string club = values[18];
    string iofId = values[21];
    string clas = values[30];
    byte course = clas switch
    {
        "MElite Sprint" => 1,
        "WElite Sprint" => 2,

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
        _ => throw new Exception(),
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
    });
}

var results = new SimpleStartTimes(entries, sunParams).Create();

Console.WriteLine(results.Count);