using System;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class SOAPdiffgram 
	{
		[JsonProperty("msdata")]
		public string msdata { get; set; }

		[JsonProperty("Results")]
		public SOAPResults Results { get; set; }

		[JsonProperty("diffgr")]
		public string diffgr { get; set; }

		// constructor
		public SOAPdiffgram () {
		}
	}
}

