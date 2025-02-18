using System;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class userRequestingAuth
	{

		public string userId { get; set; }
		public string password { get; set; }

		public userRequestingAuth () {
		}

		public userRequestingAuth (string uid, string pw) {
			userId = uid;
			password = pw;
		}

	}
}

