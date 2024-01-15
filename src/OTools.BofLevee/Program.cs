using Spectre.Console;

string filePath;

if (args.Length == 0)
{
    Console.Write("Filepath: ");
    filePath = Console.ReadLine() ?? string.Empty;
}
else filePath = args[0];


string[] lines = File.ReadAllLines(filePath);

(int s, int j) members = (0, 0);
(int s, int j) nonMembers = (0, 0);

List<string> names = new();

//foreach (string line in lines)
//{
//    if (line == lines[0]) continue;

//    string[] values = line.Split(',');

//    DateOnly dob = DateOnly.Parse(values[6]);
//    string bofNum = values[1];

//    int age = DateTime.Now.Year - dob.Year;

//    if (age <= 20)
//    {
//        if (!string.IsNullOrEmpty(bofNum))
//            members.j++;
//        else
//            nonMembers.j++;
//    }
//    else
//    {
//        if (!string.IsNullOrEmpty(bofNum))
//            members.s++;
//        else
//            nonMembers.s++;
//    }
//}

foreach (string line in lines)
{
    if (line == lines[0]) continue;

    string[] values = line.Split(',');

    string member = values[2];
    string agecat = values[4];

    bool isSen = false;

    if (agecat.Length <= 1)
        isSen = true;

    if (!isSen && int.Parse(agecat[1..].ToString() ?? "99") >= 21)
        isSen = true;

    if (member != "")
    {
        if (isSen)
            members.s++;
        else
            members.j++;
    }
    else
    {
        names.Add(values[3]);

        if (isSen)
            nonMembers.s++;
        else
            nonMembers.j++;
    }
}

Table table = new();

table.AddColumns("", "Senior", "Junior");

table.AddRow("Member", members.s.ToString(), members.j.ToString());
table.AddRow("Non-Member", nonMembers.s.ToString(), nonMembers.j.ToString());

AnsiConsole.Write(table);
Console.WriteLine();

foreach (string n in names)
    Console.WriteLine(n);

Console.ReadLine();