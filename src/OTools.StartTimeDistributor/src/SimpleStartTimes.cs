using OTools.Events;
using OTools.Courses;

namespace OTools.StartTimeDistributor;

public static class SimpleStartTimes
{
	public static IEnumerable<Entry> Create(IEnumerable<Entry> input, StartTimeLimits limits)
	{
		Entry[] entries = input.ToArray();
		
		// Clear existing start times
		if (entries.Any(x => x.StartTime != null))
			foreach (Entry entry in entries) entry.StartTime = null;

		var seperated = SeperateIntoCourses(entries).ToList();

		double maxRunners = (limits.Last - limits.First) / limits.Delta;

		foreach (var course in seperated)
		{
			int count = course.Item2.Count();
			
			if (count > maxRunners)
				throw new OverflowException($"Course: {course.Item1.Name} exceeds the maximum runners");

			var shuffled = course.Item2.ToList().Shuffle();

			DateTime currTime = limits.Last - TimeSpan.FromMinutes(rng.Next(1, limits.Delta.Minutes));
			foreach (var entry in shuffled)
			{
				entry.StartTime = currTime;
				yield return entry;

				currTime -= limits.Delta;
			}
		}
	}

	internal static IEnumerable<(Course, IEnumerable<Entry>)> SeperateIntoCourses(IEnumerable<Entry> input)
	{
		IEnumerable<Course?> courses = input
										.Select(x => x.Course)
										.Distinct();

		return courses.Select(c => (c, input.Where(x => x.Course == c)));
	}

	private static readonly Random rng = new();
	internal static IList<T> Shuffle<T>(this IList<T> inp)
	{
		int n = inp.Count;

		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			(inp[k], inp[n]) = (inp[n], inp[k]);
		}

		return inp;
	}
}