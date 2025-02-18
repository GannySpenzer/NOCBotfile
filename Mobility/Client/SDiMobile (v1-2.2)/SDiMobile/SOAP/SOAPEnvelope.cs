using System;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class SOAPEnvelope 
	{
		[JsonProperty("Body")]
		public SOAPBody Body { get; set; }
		[JsonProperty("xsd")]
		public string xsd { get; set; }
		[JsonProperty("soap")]
		public string soap { get; set; }
		[JsonProperty("xsi")]
		public string xsi { get; set; }

		public SOAPEnvelope () {
		}
	}
}

