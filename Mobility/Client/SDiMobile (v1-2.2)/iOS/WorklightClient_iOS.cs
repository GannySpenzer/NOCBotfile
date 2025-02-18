using System;

[assembly : Xamarin.Forms.Dependency (typeof (SDiMobile.iOS.WorklightClient_iOS))]

namespace SDiMobile.iOS
{
	[Foundation.Preserve (AllMembers = true)]
	public class WorklightClient_iOS : SDiMobile.IWorklightClientInstance
	{
		public WorklightClient_iOS ()
		{
		}
		public Worklight.IWorklightClient GetClientInstance () {
			return (Worklight.Xamarin.iOS.WorklightClient.CreateInstance ());
		}
	}
}

