using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Worklight;
using Newtonsoft.Json;
using System.Json;

namespace SDiMobile
{
	public class ServerLogWriter
	{
		public ServerLogWriter ()
		{
		}
		public async void WriteLogsToServer()
		{
			SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
			var logslist = LocalDBObj.ReadAllLogsInLocalDB ();

			WorklightResponse res = null;

			var pString = JsonConvert.SerializeObject (logslist);

			WorklightProcedureInvocationData invocationData = new WorklightProcedureInvocationData (
				                                                  "SDIMobileSearchSOAP", 
				                                                  "WriteLogs", 
				                                                  new object [] { pString, "" });

			var wlc = worklightClientInstance.Instance;

			res = await wlc.wlcInstance.InvokeProcedure (invocationData);
			if (res.Success) {
				LocalDBObj.DeleteLocalLogs ();
			}


		}  //public async Task<List<p
	}
}

