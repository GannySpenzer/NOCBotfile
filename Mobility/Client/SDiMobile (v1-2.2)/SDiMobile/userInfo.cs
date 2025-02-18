using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Json;

using Worklight;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class userInfo
	{
		
		[JsonProperty("UserId")]
		public string UserId { get; set; }
		[JsonProperty("Name")]
		public string Name { get; set; }
		[JsonProperty("BusinessUnitId")]
		public string BusinessUnitId { get; set; }
		[JsonProperty("BusinessUnitName")]
		public string BusinessUnitName { get; set; }
		[JsonProperty("Phone")]
		public string Phone { get; set; }
		[JsonProperty("Email")]
		public string Email { get; set; }
		[JsonProperty("ProductViewId")]
		public int ProductViewId { get; set; }
		[JsonProperty("UniqueId")]
		public int UniqueUserId { get; set; }
		[JsonProperty("CustomerId")]
		public string CustomerId { get; set; }
		[JsonProperty("Privs")]
		public List<userPriv> Privs { get; set; }
		[JsonProperty("Message")]
		public string Message { get; set; }

		[JsonIgnore()]
		public string Password { get; set; }

		[JsonIgnore()]
		public bool IsLoggedIn { get; set; }

		[JsonIgnore()]
		public DateTime LastInfoSyncDTTM { get; set; }

		public userInfo () {
			InitMembers ();
		}

		private void InitMembers () {
			this.IsLoggedIn = false;
		}

		public async Task<bool> checkUpdateInfo () {
			const double TimeToReqry = 2;
			bool isUpdated = false;

			// default to about 20 minutes ago
			TimeSpan dtDiff = DateTime.Now - DateTime.Now.AddMinutes ((TimeToReqry + 1) * -1);
			// get duration from last sync
			try {
				dtDiff = DateTime.Now - this.LastInfoSyncDTTM;
			}
			catch (Exception) {
			}
			// check if more than 20 minutes ago
			//		then its time to re-query user info because it might have changed ... specially B/U
			if (Math.Abs (dtDiff.TotalMinutes) > TimeToReqry) {
				isUpdated = true;
				WorklightResponse res = null;

				WorklightProcedureInvocationData invocationData = new WorklightProcedureInvocationData (
					"SDIMobileREST", 
					"getUserInfo", 
					new object [] { this.UserId });

				var wlc = worklightClientInstance.Instance;

				res = await wlc.wlcInstance.InvokeProcedure (invocationData);

				if ((res != null) && (res.Success)) {
					JsonObject jsonObj = (JsonObject)res.ResponseJSON;

					if (jsonObj != null) {
						if (jsonObj.ContainsKey("resultSet")) {
							if (!string.IsNullOrEmpty(jsonObj["resultSet"].ToString())) {
								try {
									userInfo usr = null;
									string jsonString = common.CheckCleanJSONString (jsonObj["resultSet"].ToString());
									usr = (userInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(
										jsonString, 
										typeof(userInfo), 
										new JsonSerializerSettings () {NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore}
									);
									this.UserId = usr.UserId;
									this.Name = usr.Name;
									this.BusinessUnitId = usr.BusinessUnitId;
									this.BusinessUnitName = usr.BusinessUnitName;
									this.Phone = usr.Phone;
									this.Email = usr.Email;
									this.ProductViewId = usr.ProductViewId;
									this.UniqueUserId = usr.UniqueUserId;
									this.CustomerId = usr.CustomerId;
									this.Privs = usr.Privs;
									this.Message = usr.Message;
									this.LastInfoSyncDTTM = DateTime.Now;

									UserDetailBO LocaldUser = new UserDetailBO();
									LocaldUser.BusinessUnitId = usr.BusinessUnitId;
									LocaldUser.BusinessUnitName = usr.BusinessUnitName;
									LocaldUser.CustomerId = usr.CustomerId;
									LocaldUser.Email = usr.Email;
									LocaldUser.IsLoggedIn = true;
									LocaldUser.LastInfoSyncDTTM = usr.LastInfoSyncDTTM;
									LocaldUser.Message = usr.Message;
									LocaldUser.Name = usr.Name;
									LocaldUser.Phone = usr.Phone;
									//LocaldUser.Privs = usr.Privs;
									LocaldUser.ProductViewId = usr.ProductViewId;
									LocaldUser.UniqueUserId = usr.UniqueUserId;
									LocaldUser.UserId = usr.UserId;
									LocaldUser.DeviceID = worklightClientInstance.deviceid;
									SQLiteDataAccess sqliteobj = new SQLiteDataAccess();
									var currentuser = sqliteobj.ReadItemInLocalDB();
									LocaldUser.Password = currentuser.Password;
									sqliteobj.CreateItemInLocalDB(LocaldUser,usr.Privs);

									usr = null;
								}
								catch (Exception ex) {
									Console.WriteLine ("error::" + ex.ToString ());
								}
							}
						}
					}
				}
			}	//if (Math.Abs (dtDiff.TotalMinutes) > TimeToReqry) {
			return isUpdated;
		}

		public static async Task<userInfo> getUserInfo (string userId) {
			userInfo usr = null;

			WorklightResponse res = null;

			WorklightProcedureInvocationData invocationData = new WorklightProcedureInvocationData (
				"SDIMobileREST", 
				"getUserInfo", 
				new object [] { userId });

			var wlc = worklightClientInstance.Instance;

			res = await wlc.wlcInstance.InvokeProcedure (invocationData);

			if ((res != null) && (res.Success)) {
				JsonObject jsonObj = (JsonObject)res.ResponseJSON;

				if (jsonObj != null) {
					if (jsonObj.ContainsKey("resultSet")) {
						if (!string.IsNullOrEmpty(jsonObj["resultSet"].ToString())) {
							try {
								string jsonString = common.CheckCleanJSONString (jsonObj["resultSet"].ToString());
								usr = (userInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(
									jsonString, 
									typeof(userInfo), 
									new JsonSerializerSettings () {NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore}
								);
								usr.LastInfoSyncDTTM = DateTime.Now;
							}
							catch (Exception ex) {
								Console.WriteLine ("error::" + ex.ToString ());
							}
						}
					}
				}
			}

			return usr;
		}

	}

}

