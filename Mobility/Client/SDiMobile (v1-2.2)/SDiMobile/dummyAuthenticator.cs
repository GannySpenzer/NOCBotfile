using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Worklight;
using Newtonsoft.Json;
using System.Json;


namespace SDiMobile
{
	public class dummyAuthenticator
	{
		private worklightClientInstance m_client;

		public dummyAuthenticator (worklightClientInstance client)
		{
			m_client = client;
		}

		public async Task<bool> authUser (string userId, string password) {

			var isValidUser = false;

			WorklightResponse res = null;

			WorklightProcedureInvocationData invocationData = new WorklightProcedureInvocationData(
				"SDIMobileREST", 
				"dummyAuth", 
				new object[] { userId, password });

			res = await m_client.wlcInstance.InvokeProcedure (invocationData);

			if ((res != null) && (res.Success)) {
				dummyAuthJSON authRes = null;

				JsonObject jsonObj = (JsonObject)res.ResponseJSON;

				if (jsonObj != null) {
					if (jsonObj.ContainsKey("resultSet")) {
						if (!string.IsNullOrEmpty(jsonObj["resultSet"].ToString())) {
							try {
								string jsonString = common.CheckCleanJSONString (jsonObj["resultSet"].ToString());
								authRes = (dummyAuthJSON)Newtonsoft.Json.JsonConvert.DeserializeObject(
									jsonString, 
									typeof(dummyAuthJSON), 
									new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore}
								);
							}
							catch (Exception ex) {
								Console.WriteLine ("error::" + ex.ToString ());
							}
						}
					}
				}

				if (authRes != null) {
					isValidUser = (bool)authRes.Authenticated;
				}
			}

			return isValidUser;

		}
	}

	[Foundation.Preserve (AllMembers = true)]
	public class dummyAuthJSON 
	{
		public dummyAuthJSON ()
		{
			InitMembers ();
		}

		[JsonProperty("UserId")]
		public string UserId;
		[JsonProperty("Name")]
		public string Name;
		[JsonProperty("BusinessUnitId")]
		public string BusinessUnitId;
		[JsonProperty("BusinessUnitName")]
		public string BusinessUnitName;
		[JsonProperty("Phone")]
		public string Phone;
		[JsonProperty("Email")]
		public string Email;
		[JsonProperty("Authenticated")]
		public bool Authenticated;
		[JsonProperty("Message")]
		public string Message;

		private void InitMembers () {
			UserId = "";
			Name = "";
			BusinessUnitId = "";
			BusinessUnitName = "";
			Phone = "";
			Email = "";
			Authenticated = false;
			Message = "";
		}
	}
}

