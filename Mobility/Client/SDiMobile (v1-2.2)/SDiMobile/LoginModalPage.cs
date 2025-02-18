using System;

using Xamarin.Forms;


namespace SDiMobile
{
	public class LoginModalPage : CarouselPage
	{
		private IAppManager appMgr;

		public LoginModalPage (IAppManager appManager)
		{
			try {
				appMgr = appManager;
				Children.Add (new LoginPage (appMgr));
				//Children.Add (new UserAcctMaint ());
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "LoginModalPage-LoginModalPage");
			}
		}
	}
}

