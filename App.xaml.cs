
using System;
using System.Windows;

namespace iRacingTVController
{
	public partial class App : Application
	{
		public App()
		{
			Startup += AppStartup;
		}

		async void AppStartup( object sender, StartupEventArgs e )
		{
			try
			{
				await StreamDeckPlugin.Program.StartStreamDeckConnection(null);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
				throw;
			}

			
			
			
			Program.Initialize();

			iRacingTVController.MainWindow.Instance.Initialize();
			iRacingTVController.MainWindow.Instance.Show();
		}
	}
}
