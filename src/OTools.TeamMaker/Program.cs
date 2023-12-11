using System.Collections.Concurrent;
using System.Text;

new TeamMaker(@"C:\Users\joe\Downloads\Book3.csv", 3).Run();

public class TeamMaker
{
    private byte _teamSize;

    private readonly Random _rnd = new();
    private readonly string _path;
    private readonly List<Runner> _runnerList = new();

    private readonly ConcurrentQueue<(IEnumerable<Team>, float)> _smallests = new();
    private readonly List<(IEnumerable<Team>, float)> _a = new();

    private int _sCount = 0;
    private int _jCount = 0;

    private int _iterations;
    private int _threadCount;

    public TeamMaker(string path, byte teamSize)
    {
        _path = path;
        _teamSize = teamSize;
    }

    private void ParseRunners()
    {
        string[] lines = File.ReadAllLines(_path);

        foreach (string line in lines)
        {
            string[] values = line.Split(',');

            string name = values[0];
            float weight = float.Parse(values[1]);

            _runnerList.Add(new(name, weight));
        }
    }

    public void Run()
    {
        ParseRunners();

        for (_jCount = 0; _jCount < 1000; _jCount++)
        {
            _smallests.Clear();

            //ThreadRunner(true);

            for (int i = 0; i < 10; i++)
                new Thread(() => ThreadRunner()).Start();
            
            ThreadRunner(true);
        }

        var teams = GetBestTeamsFinal().ToArray();

        Console.WriteLine("Best Teams \n_______________________");
        for (int i = 0; i < teams.Length; i++)
        {
            var t = teams[i];
            Console.WriteLine($"Team {i + 1} ({t.GetWeight()}):");
            for (int j = 0; j < t.Count; i++)
                Console.WriteLine($"\t{j}: {t[j].Name}");
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine(GetVariance(teams));
        Console.WriteLine(_sCount);
        Console.ReadLine();
    }

    public TeamResult Run2()
    {
        ParseRunners();

        throw new NotImplementedException();
    }

    private void ThreadRunner(bool x = false)
    {
        float variance = float.MaxValue;
        int count = 0;

        while (variance > 10 && count < 10000)
        {
            var teams = ChooseTeams();
            float v = GetVariance(teams);

            if (v < variance)
            {
                variance = v;
                _smallests.Enqueue((teams, variance));
            }

            count++;
        }

        if (x)
        {
            var t = GetBestTeamsInterim();

            _a.Add((t, GetVariance(t)));

            StringBuilder sb = new();

            sb.Append('<');
            for (int i = 0; i < 100; i++)
            {
                if (i <= _jCount / 10)
                    sb.Append('#');
                else
                    sb.Append('-');
            }
            sb.Append('>');

            Console.Clear();
            Console.WriteLine(sb);
            Console.WriteLine(_sCount);
        }
    }

    private IEnumerable<Team> ChooseTeams()
    {
        List<Team> teams = new();
        List<Runner> activeRunners = new(_runnerList);

        while (activeRunners.Count > 0)
        {
            List<Runner> teamRunners = new();

            for (int i = 0; i < _teamSize; i++)
            {
                int index = _rnd.Next(activeRunners.Count);
                teamRunners.Add(activeRunners[index]);
                activeRunners.RemoveAt(index);
            }

            _sCount++;
            teams.Add(new(teamRunners));
        }

        return teams;
    }

    private float GetVariance(IEnumerable<Team> teams)
    {
        List<float> weights = new();

        foreach (Team t in teams)
            weights.Add(t.GetWeight());

        return weights.Max() - weights.Min();
    }

    private IEnumerable<Team> GetBestTeamsInterim()
    {
        float nearest = float.MaxValue;
        IEnumerable<Team> current = Enumerable.Empty<Team>();

        foreach (var i in _smallests)
        {
            if (i.Item2 < nearest)
            {
                nearest = i.Item2;
                current = i.Item1;
            }
        }

        return current;
    }
    private IEnumerable<Team> GetBestTeamsFinal()
    {
        float nearest = float.MaxValue;
        IEnumerable<Team> current = Enumerable.Empty<Team>();

        foreach (var i in _a)
        {
            if (i.Item2 < nearest)
            {
                nearest = i.Item2;
                current = i.Item1;
            }
        }

        return current;

    }
}

public record struct Runner(string Name, float Weight);

public class Team : List<Runner>
{
    public Team(IEnumerable<Runner> runners) : base(runners) { }

    public float GetWeight() => this.Sum(x => x.Weight);
}

public struct TeamResult
{
    public List<Team> Teams { get; set; }
    public float Variance { get; set; }
    public float SCount { get; set; }
}