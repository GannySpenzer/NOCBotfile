using System;

using Xamarin.Forms;


namespace SDiMobile
{
	public class AppMainPage : MasterDetailPage
	{
		
		private IAppManager appMgr { get; set; }
		private NavigationPage Navigator { get; set; }
		private MenuPage menuPage { get; set; }

		public AppMainPage (IAppManager appManager)
		{
			try {
				appMgr = appManager;

				// menu page
				menuPage = new MenuPage (appMgr);
				menuPage.Menu.ItemSelected += (sender, e) => NavigateTo (e.SelectedItem as MenuItem);

				// setup master/detail pages
				Master = menuPage;

				var root = new PartSearchPage ();

				Navigator = new NavigationPage (root);
				Navigator.PoppedToRoot += (object sender, NavigationEventArgs e) => NavigatorPoppedToRoot (e.Page as Page);
				Navigator.Popped += (object sender, NavigationEventArgs e) => NavigatorPopped (e.Page as Page);
				Navigator.Pushed += (object sender, NavigationEventArgs e) => NavigatorPushed (e.Page as Page);

				root.Navigator = Navigator.Navigation;

				Detail = Navigator;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "AppMainPage-AppMainPage");
			}
		}

		private async void NavigateTo (MenuItem menu)
		{
			try {
				if (menu != null) {
					if (!menu.IsLogout) {
						// check if the "target" page is already the "current" displayed page
						//		then do nothing
						string pgCurrentId = "";
						if (Navigator.CurrentPage != null) {
							pgCurrentId = ((INavPageChild)Navigator.CurrentPage).PageId;
						}
						if (menu.TargetId != pgCurrentId) {
							// check if menu item is the "main" page
							//		pop to the root if it is, create an instance and push if it's not
							if (!menu.IsMainPage) {
								Page displayPage = (Page)Activator.CreateInstance (menu.TargetType);
								if (displayPage is INavPageChild)
									((INavPageChild)displayPage).Navigator = Navigator.Navigation;
								await Navigator.PushAsync (displayPage);
							} else {
								await Navigator.PopToRootAsync ();
							}
							IsPresented = false;
						}
					} else {
						App.myInstance.Logout ();
					}
				}
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "AppMainPage-NavigateTo");
			}
		}

		private void NavigatorPoppedToRoot (Page pg) {
			try {
			
				/* Page pg - sends the "target" page (going to)
			 */

				NavigatorSyncMenuItemWithDisplayedPage ();
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "AppMainPage-NavigatorPoppedToRoot");
			}

		}

		private void NavigatorPopped (Page pg) {
			try {
				/* Page pg - sends the "popped" page
			 */

				NavigatorSyncMenuItemWithDisplayedPage ();
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "AppMainPage-NavigatorPopped");
			}

		}

		private void NavigatorPushed (Page pg) {
			try {
			NavigatorSyncMenuItemWithDisplayedPage ();
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "AppMainPage-NavigatorPushed");
			}
		}

		private void NavigatorSyncMenuItemWithDisplayedPage () {
			// select the "menu item" that corresponds to our currently displayed page
			try {
				if (((MenuItem)menuPage.Menu.SelectedItem).TargetId != ((INavPageChild)Navigator.CurrentPage).PageId) {
					foreach (MenuItem mItem in menuPage.Menu.ItemsSource) {
						if (mItem.TargetId == ((INavPageChild)Navigator.CurrentPage).PageId) {
							menuPage.Menu.SelectedItem = mItem;
							break;
						}
					}
				}
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "AppMainPage-NavigatorSyncMenuItemWithDisplayedPage");
			}
		}

	}
}

