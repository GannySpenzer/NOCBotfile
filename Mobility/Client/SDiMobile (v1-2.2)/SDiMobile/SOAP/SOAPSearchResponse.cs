using System;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class SOAPSearchResponse 
	{
		[JsonProperty("SearchResult")]
		public SOAPSearchResult SearchResult { get; set; }
		[JsonProperty("xmlns")]
		public string xmlns { get; set; }

		public SOAPSearchResponse () {
		}
	}
}

