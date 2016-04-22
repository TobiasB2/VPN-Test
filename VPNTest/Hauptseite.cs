using System;
using Xamarin.Forms;

namespace VPNTest
{
	public class Hauptseite : ContentPage
	{
		private IVPNService VPNService;

		public Hauptseite (IVPNService vpn)
		{
			
			VPNService = vpn;

			var btnStart = new Button {
				Text = "Start",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			btnStart.Clicked += (sender, e) => {
				VPNService.StartVPNConnection();
			};

			var btnStop = new Button {
				Text = "Stop",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			btnStop.Clicked += (sender, e) => {
				VPNService.StopVPNConnection();
			};

			var btnAddConfig = new Button {
				Text = "Add Config",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			btnAddConfig.Clicked += (sender, e) => {
				VPNService.AddConfig();
			};

			var btnDelete = new Button {
				Text = "Löschen",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
			btnDelete.Clicked += (sender, e) => {
				VPNService.RemoveVPNConnection();
			};

			var layout = new StackLayout {
				Children = { btnStart, btnStop, btnAddConfig, btnDelete }
			};
			Content = layout;
		}
	}
}

