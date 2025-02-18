using System;
using System.Collections.Generic;

using Foundation;
using UIKit;

namespace SDiMobile.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

//			// Code for starting up the Xamarin Test Cloud Agent
//			#if ENABLE_TEST_CLOUD
//			Xamarin.Calabash.Start();
//			#endif

			//var myApp = new App () {
			//	WorklightClientInstance = new SDiMobile.wlcInstance (Worklight.Xamarin.iOS.WorklightClient.CreateInstance ())
			//};
			var myApp = new App ();
			UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(236,240,241);

			LoadApplication (myApp);

			return base.FinishedLaunching (app, options);
		}
	}
}

