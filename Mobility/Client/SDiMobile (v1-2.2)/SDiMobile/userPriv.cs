using System;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class userPriv
	{

		[JsonProperty("PrivType")]
		public string PrivType { get; set; }
		[JsonProperty("PrivName")]
		public string PrivName { get; set; }

		public userPriv ()
		{
		}

	}
}

