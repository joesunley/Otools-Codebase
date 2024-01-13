using System.Diagnostics;

namespace OTools.StartTimeDistributor;

[DebuggerDisplay("Entry: {Name}")]
public struct Entry
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Club { get; set; }

    public string RankingKey { get; set; }

    //public Day[] Days { get; set; }

    //public struct Day
    //{
    //    public string Class { get; set; }
    //    public byte Course { get; set; }
    //    public StartTimeBlock Preference { get; set; }
    //}

    public string Class { get; set; }
    public byte Course { get; set; }
    public StartTimeBlock Preference { get; set; }
}

public enum StartTimeBlock { Early, Middle, Late, Open, None }


public class StartTimeParameters
{
    public int[][] CourseGroupings { get; set; }
    public GroupParameters[] Parameters { get; set; }
}

public class GroupParameters
{
    public string AssociationType { get; set; }
    public DateTime FirstStart { get; set; }
    public DateTime LastStart { get; set; }
    public int CourseSpacing { get; set; }
    public int ClassSpacing { get; set; }
    public int ClubSpacing { get; set; }
    public int StartInterval { get; set; }
    public int Grouping { get; set; }
}
