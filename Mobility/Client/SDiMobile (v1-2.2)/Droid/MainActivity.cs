using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SDiMobile.Droid
{
	[Activity (Label = "Ordering", Theme = "@style/MyTheme", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		public static Xamarin.Forms.Platform.Android.FormsApplicationActivity myActivity;

		protected override void OnCreate (Bundle bundle)
		{
			try {
				base.OnCreate (bundle);

				global::Xamarin.Forms.Forms.Init (this, bundle);

				myActivity = this;

				//var myApp = new App () {
				//	WorklightClientInstance = new SDiMobile.wlcInstance (Worklight.Xamarin.Android.WorklightClient.CreateInstance (this))
				//};
				var myApp = new App ();

				LoadApplication (myApp);
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "MainActivity-OnCreate");
			}
		}
	}
}

