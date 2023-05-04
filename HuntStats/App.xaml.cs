using HuntStats.State;

namespace HuntStats;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new MainPage();
	}

	protected override Window CreateWindow(IActivationState activationState)
	{
		var window = base.CreateWindow(activationState);
		if (window != null)
		{
			window.Title = "HüntStäts by Zaxiure";
			const int DefaultWidth = 1280;
			const int DefaultHeight = 800;

			// change window size.
			window.Width = DefaultWidth;
			window.Height = DefaultHeight;
		}
        return window;
    }
}
