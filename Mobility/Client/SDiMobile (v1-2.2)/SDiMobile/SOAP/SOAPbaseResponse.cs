using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class SOAPbaseResponse
	{
		[JsonProperty("statusCode")]
		public int statusCode { get; set; }
		[JsonProperty("errors")]
		public List<string> errors { get; set; }
		[JsonProperty("isSuccessful")]
		public bool isSuccessful { get; set; }
		[JsonProperty("Envelope")]
		public SOAPEnvelope Envelope { get; set; }
		[JsonProperty("statusReason")]
		public string statusReason { get; set; }
		//		[JsonProperty("responseHeaders")]
		public SOAPResponseHeaders responseHeaders { get; set; }
		[JsonProperty("warnings")]
		public List<string> warnings { get; set; }
		[JsonProperty("responseTime")]
		public double responseTime { get; set; }
		[JsonProperty("totalTime")]
		public double totalTime { get; set; }
		[JsonProperty("info")]
		public List<string> info { get; set; }

		public SOAPbaseResponse () {
		}
	}

}

