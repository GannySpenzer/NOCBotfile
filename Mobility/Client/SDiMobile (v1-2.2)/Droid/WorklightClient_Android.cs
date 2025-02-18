using System;

[assembly : Xamarin.Forms.Dependency (typeof (SDiMobile.Droid.WorklightClient_Android))]

namespace SDiMobile.Droid
{
	public class WorklightClient_Android : SDiMobile.IWorklightClientInstance
	{
		public WorklightClient_Android ()
		{
		}
		public Worklight.IWorklightClient GetClientInstance () {
			return (Worklight.Xamarin.Android.WorklightClient.CreateInstance (MainActivity.myActivity));
		}
	}
}

