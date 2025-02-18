using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class SOAPResults 
	{
		[JsonProperty ("RelevantResults")]
		[JsonConverter (typeof(singleOrArrayConverter<SOAPRelevantResults>))]
		public List<SOAPRelevantResults> RelevantResults { get; set; }

		[JsonProperty("xmlns")]
		public string xmlns { get; set; }

		public SOAPResults () {
		}
	}
}

