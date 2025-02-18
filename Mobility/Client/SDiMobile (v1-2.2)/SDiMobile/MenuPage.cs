using System;

using Xamarin.Forms;


namespace SDiMobile
{
	public class MenuPage : ContentPage
	{
		private IAppManager appMgr { get; set; }

		public ListView Menu { get; set; }

		public MenuPage (IAppManager appManager)
		{
			try {
				appMgr = appManager;

				this.Title = "Menu";

				this.Icon = Device.OnPlatform (
					iOS: (FileImageSource)ImageSource.FromFile ("list.png"),
					Android: (FileImageSource)ImageSource.FromFile ("list.png"),
					WinPhone: (FileImageSource)ImageSource.FromFile ("list.png")
				);

				BackgroundColor = Color.FromHex ("ECF0F1");

				Menu = new MenuListView (appMgr);

				//var usrdetails = (userInfo)App.Current.Properties [App.LOGGED_IN_USER];
				SQLiteDataAccess sqliteobj = new SQLiteDataAccess ();
				var localuser = sqliteobj.ReadItemInLocalDB ();
				if (localuser == null) {
					localuser = new UserDetailBO ();
				}

				var usernamelayout = new StackLayout () { Orientation = StackOrientation.Vertical, Padding = new Thickness (10, 0, 0, 0) };
				usernamelayout.Children.Add (new Label () {
					//Text = usrdetails.Name,
					Text = localuser.Name,
					TextColor = Color.White,
					VerticalOptions = LayoutOptions.Center
				});
				usernamelayout.Children.Add (new Label () {
					FontSize = 12,
					TextColor = Color.White,
					//Text = usrdetails.BusinessUnitId + " - " + usrdetails.BusinessUnitName,
					Text = localuser.BusinessUnitId + " - " + localuser.BusinessUnitName,
					VerticalOptions = LayoutOptions.Center,
				});

				var menuLabel = new ContentView () {
					BackgroundColor = Color.FromHex ("7F8C8D"),
					Padding = new Thickness (10, 36, 0, 5),
					Content = usernamelayout
				};

				var layout = new StackLayout () { 
					Spacing = 0, 
					VerticalOptions = LayoutOptions.FillAndExpand
				};
				layout.Children.Add (menuLabel);
				layout.Children.Add (Menu);

				Content = layout;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "MenuPage-MenuPage");
			}
		}
	}
}

