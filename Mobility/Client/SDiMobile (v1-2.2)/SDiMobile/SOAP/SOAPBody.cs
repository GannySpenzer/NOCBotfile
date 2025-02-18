using System;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class SOAPBody 
	{
		[JsonProperty("SearchResponse")]
		public SOAPSearchResponse SearchResponse { get; set; }

		public SOAPBody () {
		}
	}
}

