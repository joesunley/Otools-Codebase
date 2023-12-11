using System.Diagnostics;

namespace OTools.StartTimeDistributor;

[DebuggerDisplay("Entry: {Name}")]
public struct Entry
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Club { get; set; }
    public byte Course { get; set; }
    public string Class { get; set; }

    public string RankingKey { get; set; }

    public StartTimeBlock Preference { get; set; }
}

public enum StartTimeBlock { Early, Middle, Late, Open }

public struct StartTimeParameters
{
    public DateTime FirstStart { get; set; }
    public DateTime LastStart { get; set; }

    public int[] ExcludedCourses { get; set; }
    public string[] ExcludedClasses { get; set; }
    public string[] ExcludedClubs { get; set; }

    public TimeSpan CourseSpacing { get; set; }
    public TimeSpan ClassSpacing { get; set; }
    public TimeSpan ClubSpacing { get; set; }

    public TimeSpan StartInterval { get; set; }

    public StartTimeParameters()
    {
        FirstStart = default;
        LastStart = default;

        ExcludedCourses = Array.Empty<int>();
        ExcludedClasses = Array.Empty<string>();
        ExcludedClubs = Array.Empty<string>();

        CourseSpacing = TimeSpan.FromMinutes(1);
        ClassSpacing = TimeSpan.FromMinutes(1);
        ClubSpacing = TimeSpan.FromMinutes(1);

        StartInterval = TimeSpan.FromMinutes(1);
    }
}

public class StartTimeParametersNew
{
    public DayStartTimeParameters[] Days { get; set; }
}

public class DayStartTimeParameters
{
    public int[][] CourseGroupings { get; set; }
    public GroupParameters[] Parameters { get; set; }
}

public class GroupParameters
{
    public string AssociationType { get; set; }
    public string FirstStart { get; set; }
    public string LastStart { get; set; }
    public int CourseSpacing { get; set; }
    public int ClassSpacing { get; set; }
    public int ClubSpacing { get; set; }
    public int StartInterval { get; set; }
    public int Grouping { get; set; }
}
