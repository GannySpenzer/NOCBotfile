using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class userJSON
	{
		
		[JsonProperty("isSuccessful")]
		public bool IsSuccessful { get; set; }

		[JsonProperty("resultSet")]
		public List<user> userList { get; set; }

		public userJSON ()
		{
		}

	}
}

