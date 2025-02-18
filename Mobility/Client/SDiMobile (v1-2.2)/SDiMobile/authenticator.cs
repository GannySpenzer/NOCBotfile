using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Json;
using Worklight;


namespace SDiMobile
{
	
	public class Authenticator : ChallengeHandler
	{
		private string realmName;
		private bool isAuthRequired;
		private bool isAuthSuccessful;
		private AdapterAuthenticationInfo authInfo;

		public userRequestingAuth User { get; set; }

		public IWorklightClient Client { get; set; }
		
		public Authenticator (string appRealm)
		{
			InitMembers (appRealm);
		}

		public Authenticator (string appRealm, IWorklightClient wlcClient) {
			InitMembers (appRealm, wlcClient);
		}

		public Authenticator (string appRealm, IWorklightClient wlcClient, userRequestingAuth appUser)
		{
			InitMembers (appRealm, wlcClient, appUser);
		}

		private void InitMembers (string appRealm, IWorklightClient wlcClient = null, userRequestingAuth appUser = null)
		{
			isAuthRequired = false;
			isAuthSuccessful = false;

			if ((appRealm != null) && (appRealm.Trim ().Length > 0))
				realmName = appRealm;

			if (wlcClient != null)
				Client = wlcClient;

			if ((appUser != null) && (appUser.userId != null) && (appUser.userId.Trim().Length > 0))
				User = appUser;

			authInfo = new AdapterAuthenticationInfo();
		}

		public override string GetRealm () {
			Console.WriteLine ("GetRealm is called ... returning " + realmName);
			return realmName;
		}

		public override void HandleChallenge (WorklightResponse resp) {
			Console.WriteLine ("HandleChallenge is called");
			Console.WriteLine ("Private var isAuthRequired = " + isAuthRequired.ToString());

			JsonObject x1 = (JsonObject)resp.ResponseJSON;
			Console.WriteLine ("HandleChallenge resp from server : " + x1.ToString ());

			bool isChallengeFromServer = false;

			if ((resp != null) 
				&& (resp.ResponseJSON != null) 
				&& (resp.ResponseJSON.ContainsKey("authRequired")) 
				&& (resp.ResponseJSON.ContainsKey("isSuccessful"))) 
			{
				try {
					serverAuthChallengeResponseJSON r = null;
					JsonObject jsonObj = (JsonObject)resp.ResponseJSON;
					r = Newtonsoft.Json.JsonConvert.DeserializeObject<serverAuthChallengeResponseJSON>(jsonObj.ToString());
					isChallengeFromServer = (r.isSuccessful && r.authRequired);
				}
				catch (Exception) { }
			} 

			if (isChallengeFromServer) {
				WorklightProcedureInvocationData invocationData = new WorklightProcedureInvocationData (
					"SDIMobileREST", 
					"submitAuthentication", 
					new object[] { "bautise1", "sd1exchang" });
				authInfo.InvocationData = invocationData;
				authInfo.RequestOptions = null;
				isAuthRequired = true;
			} else {
				isAuthRequired = false;
				isAuthSuccessful = true;
			}
		}

		public override bool IsCustomResponse (WorklightResponse resp) {
			// should check if "response" from server is a type of challenge for authentication
			//		TRUE	- it is a challenge from server
			//		FALSE	- no other type of response from server
			Console.WriteLine ("isCustomResponse called");
			var respString = "";
			if ((resp != null) && (resp.ResponseJSON != null) && (resp.ResponseJSON.ContainsKey("authRequired")) && (resp.ResponseJSON.ContainsKey("isSuccessful"))) { // && (resp.ResponseJSON ["authRequired"] != null)) {
				try {
					serverAuthChallengeResponseJSON r = null;
					JsonObject jsonObj = (JsonObject)resp.ResponseJSON;
					r = Newtonsoft.Json.JsonConvert.DeserializeObject<serverAuthChallengeResponseJSON>(jsonObj.ToString());
					isAuthRequired = (r.isSuccessful && r.authRequired);
					respString = jsonObj.ToString();
				}
				catch (Exception) { }
			} 
			Console.WriteLine ("challenge response/message from server? " + isAuthRequired.ToString());
			if (isAuthRequired) 
				Console.WriteLine ("resp : " + respString);
			return isAuthRequired;
		}

		public override void OnFailure (WorklightResponse resp) {
			JsonObject x = (JsonObject)resp.ResponseJSON;
			Console.WriteLine ("OnFailure called : " + x.ToString());
		}

		public override void OnSuccess (WorklightResponse resp) {
			JsonObject x = (JsonObject)resp.ResponseJSON;
			Console.WriteLine ("OnSuccess called : " + x.ToString());
		}

		public override bool ShouldSubmitSuccess() {
		Console.WriteLine ("ShouldSubmitSuccess is called ... returning " + isAuthSuccessful.ToString());
		return isAuthSuccessful;
		}

		public override bool ShouldSubmitAdapterAuthentication() {
			Console.WriteLine ("ShouldSubmitAdapterAuthentication is called ... returning " + isAuthRequired.ToString());
			return isAuthRequired;

		}

		public override AdapterAuthenticationInfo GetAdapterAuthenticationParameters () {
			Console.WriteLine ("GetAdapterAuthenticationParameters is called ... returning " + authInfo.ToString());
			return authInfo;
		}
	} //Authenticator class

	public class serverAuthChallengeResponseJSON 
	{
		[JsonProperty("errorMessage")]
		public string errorMessage { get; set; }
		[JsonProperty("isSuccessful")]
		public bool isSuccessful { get; set; }
		[JsonProperty("authRequired")]
		public bool authRequired { get; set; }
	} //serverAuthChallengeResponseJSON

} //namespace

