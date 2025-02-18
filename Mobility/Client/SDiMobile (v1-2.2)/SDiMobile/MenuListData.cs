using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class MenuListData : List<MenuItem>
	{
		private IAppManager appMgr { get; set; }

		public MenuListData (IAppManager appManager)
		{
			try {
				appMgr = appManager;

				MenuItem mItem = null;

				mItem = new MenuItem () { 
					Title = "Search Items/Parts", 

					IconSource = Device.OnPlatform (
						iOS: "search.png",
						Android: "search1.png",
						WinPhone: "search.png"
					),
			
					TargetType = typeof(PartSearchPage),
					TargetId = PartSearchPage.PAGE_ID,
					ApplicationManager = appMgr
				
				};
				this.Add (mItem);
				mItem.SetAsMainPage (this);

				mItem = new MenuItem () { 
					Title = "Shopping Cart", 
					IconSource = Device.OnPlatform (
						iOS: "shopcart.png",
						Android: "shopcart1.png",
						WinPhone: "shopcart.png"
					),
					TargetType = typeof(ShoppingCartPage),
					TargetId = ShoppingCartPage.PAGE_ID,
					ApplicationManager = appMgr
				};
				this.Add (mItem);

				mItem = new MenuItem () { 
					Title = "My Account", 
					IconSource = Device.OnPlatform (
						iOS: "user-4.png",
						Android: "user_4_1.png",
						WinPhone: "user-4.png"
					),
					TargetType = typeof(UserProfilePage),
					TargetId = UserProfilePage.PAGE_ID,
					ApplicationManager = appMgr
				};
				this.Add (mItem);

				mItem = new MenuItem () {
					Title = "Sign Out",
					IconSource = Device.OnPlatform (
						iOS: "locked.png",
						Android: "locked1.png",
						WinPhone: "locked.png"
					),
					TargetType = typeof(LoginModalPage),
					IsLogout = true,
					ApplicationManager = appMgr
				};
				this.Add (mItem);

				mItem = null;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "MenuListData-MenuListData");
			}
		}
	}
}

