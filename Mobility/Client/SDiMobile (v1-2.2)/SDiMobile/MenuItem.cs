using System;
using System.Collections.Generic;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class MenuItem
	{
		// constructor
		public MenuItem () {
		}

		// members
		public string Title { get; set; }

		public string IconSource { get; set; }

		public Type TargetType { get; set; }

		public string TargetId { get; set; }

		// handles whether this menu is the main/initial/top most page
		public bool IsMainPage { get; private set; }

		public void SetAsMainPage (MenuListData mItems) {
			try{
			if ((mItems != null) && (mItems.Count > 0)) {
				foreach (MenuItem itm in mItems) {
					itm.IsMainPage = false;
				}
			}
			this.IsMainPage = true;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "MenuItem-SetAsMainPage");
			}
		}

		// is this menu the "logout" menu item?
		public bool IsLogout { get; set; }

		// IAppManager instance
		public IAppManager ApplicationManager { get; set; }

	}
}

