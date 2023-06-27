using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;

public static class ImageEncode
{
	public static void Write(string fileName, string text)
	{
		using var image = Image.Load(fileName);
		
		if (image.Metadata.IptcProfile == null)
			image.Metadata.IptcProfile = new();
			
		// image.Metadata.IptcProfile.SetValue(IP)
	}
}