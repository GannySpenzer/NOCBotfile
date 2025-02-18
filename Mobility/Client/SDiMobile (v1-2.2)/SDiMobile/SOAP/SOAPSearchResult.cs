using System;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class SOAPSearchResult 
	{
		//		[JsonProperty("schema")]
		public SOAPschema schema { get; set; }
		[JsonProperty("diffgram")]
		public SOAPdiffgram diffgram { get; set; }

		public SOAPSearchResult () {
		}
	}
}

