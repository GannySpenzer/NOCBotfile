using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Worklight;
using Newtonsoft.Json;
using System.Json;


namespace SDiMobile
{
    public class userAuthenticate
    {

        // MFP client instance
        public IWorklightClient wlcInstance { get; set; }

        private bool IsConnected { get; set; }

        public userAuthenticate(IWorklightClient wlc)
        {
            wlcInstance = wlc;
            IsConnected = false;
        }

        public async Task<user> AuthenticateUser(string userId, string password)
        {
            user usr = new user ();

            usr.UserId = userId;
            usr.Password = password;
            usr.IsAuthenticated = false;

            try
            {

                WorklightResponse res = null;

                // let's try connecting to server
                if (!this.IsConnected)
                {
                    res = await wlcInstance.Connect ();
                    if (res.Success)
                        this.IsConnected = true;
                    else
                        throw new Exception("unable to connect to server");
                }

                // let's invoke user authenticate procedure
                WorklightProcedureInvocationData invocationData = new WorklightProcedureInvocationData ("SDiUserLoginSQL", "authUser", new object [] { usr.UserId, usr.Password });

                res = await wlcInstance.InvokeProcedure (invocationData);

                if (res.Success)
                {
                    JsonObject jsonObj = (JsonObject)res.ResponseJSON;

                    userJSON users = null;

                    if (jsonObj != null) {
                        if (jsonObj.ContainsKey("resultSet")) {
                            if (!string.IsNullOrEmpty(jsonObj["resultSet"].ToString())) {
                                users = Newtonsoft.Json.JsonConvert.DeserializeObject<userJSON>(jsonObj.ToString());
                            }
                        }
                    }

                    if (users != null)
                    {
                        if (users.userList.Count > 0)
                        {
                            usr = users.userList[0];
                            usr.IsAuthenticated = true;
                        }
                    }
                }
                else
                {
                    throw new Exception("invocation of adapter procecure failed");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return usr;
        }

    }
}
