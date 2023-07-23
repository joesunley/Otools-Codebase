using System.Collections;
using OTools.Courses;

namespace OTools.Events;

public class ResultsList
{
	protected readonly Dictionary<Course, IList<Result>> _results;

	protected ResultsList(Dictionary<Course, IList<Result>> inp)
	{
		_results = inp;
	}

	public IList<Result> this[Course c] => _results[c];

	public static ResultsList Create(IEnumerable<Result> results)
	{
		IEnumerable<IGrouping<Course?, Result>> groups = results.GroupBy(x => x.Entry.Course);

		Dictionary<Course, IList<Result>> res = new();
		
		foreach (IGrouping<Course?, Result> g in groups)
		{
			if (g.Key is null) continue;
			
			List<(Result r, TimeSpan)> ordered = g.Select(x => (x, x.GetResultTime()))
												  .OrderBy(tuple => tuple.Item2).ToList();

			res.Add(g.Key, ordered.Select(x => x.r).ToList());
		}

		return new(res);
	}
}

