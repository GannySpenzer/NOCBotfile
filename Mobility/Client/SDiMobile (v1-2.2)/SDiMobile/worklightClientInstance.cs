using System;
using System.Threading.Tasks;

using Worklight;

/* I do need a wrapper for the IWorklightClient (ie., Worklight.Xamarin.iOS.WorklightClient)
 * 		so I can check if "can connect" before actually doing an "adapter invoke" call
 */
using Newtonsoft.Json;

namespace SDiMobile
{
	public class worklightClientInstance
	{
		
		private worklightClientInstance ()
		{
		}

		private static worklightClientInstance m_myInst { get; set; }

		public static worklightClientInstance Instance {
			get {
				if (m_myInst == null) {
					m_myInst = new worklightClientInstance ();
				}
				return m_myInst;
			}
		}

		private IWorklightClient m_wlc { get; set; }

		public IWorklightClient wlcInstance {
			get {
				if (m_wlc == null) {
					m_wlc = Xamarin.Forms.DependencyService.Get<IWorklightClientInstance> ().GetClientInstance ();
				}
				return m_wlc;
			}
		}
		public static string deviceid;
		public async Task<bool> CanConnect () {
			bool isCanConnect = false;
			try {

				//m_wlc.RegisterChallengeHandler (this.AuthenticatorInstance);

				WorklightResponse res = await this.wlcInstance.Connect ();

				/*
				// lets log to the local client (not server)
				m_wlc.Logger("Xamarin").Trace ("connection");

				// write to the server the connection status
				m_wlc.Analytics.Log ("Connect response : " + res.Success.ToString());
				*/
				if (res.Success)
				{
					deviceid = JsonConvert.DeserializeObject<string> (res.ResponseJSON ["userInfo"] ["wl_deviceNoProvisioningRealm"] ["deviceId"].ToString ());
				}
				else {
					Console.WriteLine ("error::" + "Could not connect to IBM server : " + res.Message);
					SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
					LocalDBObj.InsertLogInLocalDB (new Exception ("Could not connect to IBM server : " + res.Message), "worklightClientInstance-CanConnect");
				}
					
				isCanConnect = res.Success;

			} catch (Exception ex) {
				Console.WriteLine ("error::" + ex.ToString ());
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "worklightClientInstance-CanConnect");
			}
			return (isCanConnect);
		}

	}
}

