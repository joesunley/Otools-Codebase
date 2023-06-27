using Avalonia.Controls;

namespace OTools.AvaCommon;

public static class Extensions
{
	public static T ExtractTag<T>(this Control control) 
	{
		return (T)(control.Tag ?? throw new Exception("Tag is null"));
	}
}