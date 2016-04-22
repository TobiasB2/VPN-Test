using System;

using Xamarin.Forms;

namespace VPNTest
{
	public class App : Application
	{
		IVPNService VPNService;
		public App ()
		{
			// The root page of your application
			VPNService = DependencyService.Get<IVPNService> ();	
			MainPage = MainPage = new Hauptseite (VPNService);
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}


	}
}

