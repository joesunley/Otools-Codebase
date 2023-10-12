using Spectre.Console;

string filePath;

if (args.Length == 0)
{
    Console.WriteLine(Directory.GetFiles(Environment.CurrentDirectory));

    if (Directory.GetFiles(Environment.CurrentDirectory).Contains("Standard Export.csv"))
        filePath = Path.Combine(Environment.CurrentDirectory, "Standard Export.csv");
    else
    {
        Console.Write("Filepath: ");
        filePath = Console.ReadLine() ?? string.Empty;
    }
}
else filePath = args[0];


string[] lines = File.ReadAllLines(filePath);

(int s, int j) members = (0, 0);
(int s, int j) nonMembers = (0, 0);

foreach (string line in lines)
{
    if (line == lines[0]) continue;

    string[] values = line.Split(',');

    DateOnly dob = DateOnly.Parse(values[6]);
    string bofNum = values[1];

    int age = DateTime.Now.Year - dob.Year;

    if (age <= 20)
    {
        if (!string.IsNullOrEmpty(bofNum))
            members.j++;
        else
            nonMembers.j++;
    }
    else
    {
        if (!string.IsNullOrEmpty(bofNum))
            members.s++;
        else
            nonMembers.s++;
    }
}

Table table = new();

table.AddColumns("", "Senior", "Junior");

table.AddRow("Member", members.s.ToString(), members.j.ToString());
table.AddRow("Non-Member", nonMembers.s.ToString(), nonMembers.j.ToString());

AnsiConsole.Write(table);

Console.ReadLine();