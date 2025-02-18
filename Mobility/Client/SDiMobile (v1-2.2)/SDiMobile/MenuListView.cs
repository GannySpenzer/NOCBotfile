using System;
using System.Collections.Generic;

using Xamarin.Forms;


namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class MenuListView : ListView
	{
		private IAppManager appMgr { get; set; }

		public MenuListView (IAppManager appManager)
		{
			try {
				appMgr = appManager;

				List<MenuItem> data = new MenuListData (appMgr);

				ItemsSource = data;
				VerticalOptions = LayoutOptions.FillAndExpand;
				BackgroundColor = Color.Transparent;
				SeparatorVisibility = SeparatorVisibility.None;

				var cell = new DataTemplate (typeof(ImageCell));
				cell.SetBinding (TextCell.TextProperty, "Title");
				cell.SetBinding (ImageCell.ImageSourceProperty, "IconSource");

				ItemTemplate = cell;
				SelectedItem = data [0];
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "MenuListView-MenuListView");
			}
		}
	}
}

