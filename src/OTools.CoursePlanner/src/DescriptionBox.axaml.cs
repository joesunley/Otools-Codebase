using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OTools.CoursePlanner;

public partial class DescriptionBox : UserControl
{
	public DescriptionBox()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public void Update()
	{
		
	}
}