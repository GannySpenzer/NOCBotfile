using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using POOverrideReload;
using System.Net;
using System.Configuration;
using System.IO;
using System.Data;
using OSVCService;
using POOverrideReload1;
using PODOverrideReload;

namespace POOverrideReload
{
    class POOverrideReload
    {
        static void Main(string[] args)
        {


            string testOrProd = " ";
            string authorization = " ";
            string password = " ";
            string serviceURL = " ";
            string serviceURL2 = " ";
            var strResponse = "Failure";
            Exception exErrorMsg;
            string resultSet = "";
            string processFlag = " ";

            DataTable dtResponse = new DataTable();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

            // InitializeLogger start here
            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "POOverrideReload");
            m_oLogger.LogMessage("Main", "Started utility POOverrideReload");

            using (var client = new WebClient())
            {
                //set default parameters
                testOrProd = ConfigurationManager.AppSettings["TestOrProd"];
                if (testOrProd == "TEST")
                {
                    serviceURL = ConfigurationManager.AppSettings["testGetQueryURL"];
                    serviceURL2 = ConfigurationManager.AppSettings["testDeletePostURL"];
                    authorization = ConfigurationManager.AppSettings["testAuthorization"];
                    password = ConfigurationManager.AppSettings["testPassword"];
                }
                else if (testOrProd == "PROD")
                {
                    serviceURL = ConfigurationManager.AppSettings["prodGetQueryURL"];
                    serviceURL2 = ConfigurationManager.AppSettings["prodDeletePostURL"];
                    authorization = ConfigurationManager.AppSettings["prodAuthorization"];
                    password = ConfigurationManager.AppSettings["prodPassword"];

                }

                {

                    string basicAuthBase641;
                    basicAuthBase641 = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", authorization, password)));
                    client.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641));
                    //client.Headers.Add("Content-Type:application/json");
                    client.Headers.Add("OSvC-CREST-Application-Context", "SDI Integration");

                    //STEP #1 - QUERY EXISTING DATA 

                    //var result = client.UploadString(serviceURL, resultSet.ToString());
                    m_oLogger.LogMessage("postPOOverrideReload", "QUERYING Oracle Data at " + serviceURL);
                    Stream myStream = client.OpenRead(serviceURL);
                    StreamReader sr = new StreamReader(myStream);
                    resultSet = sr.ReadToEnd();
                    string json = resultSet;
                    if (json.Trim() != "")
                    {
                        m_oLogger.LogMessage("POOverrideReload", "QUERYING Oracle Data successful");
                    }
                    else
                    {
                        m_oLogger.LogMessage("POOverrideReload", "QUERYING Oracle Data failed");
                        m_oLogger.LogMessageWeb("POOverrideReload", "QUERYING Oracle Data failed", "QUERYING Oracle Data failed");
                        return;
                    }

                    RootObject bo = JsonConvert.DeserializeObject<RootObject>(resultSet);

                    //var objects = JArray.Parse(resultSet);
                    m_oLogger.LogMessage("POOverrideReload", "DELETE Oracle Data started");
                    //try
                    //{
                    //    foreach (List<string> row in bo.items[0].rows)
                    //    {
                    //        //STEP #2 - DELETE DATA FOUND FROM QUERY #1
                    //        string rowToDel = row[0];

                    //        //var request = WebRequest.Create(serviceURL2);
                    //        //request.Method = "DELETE";
                    //        //var response = (HttpWebResponse)request.GetResponse();

                    //        client.UploadString(serviceURL2 + rowToDel, "DELETE", "");

                    //    }
                    //    m_oLogger.LogMessage("MatchExcepReload", "DELETE Oracle Data successful.");
                    //}
                    //catch (Exception ex)
                    //{
                    //    m_oLogger.LogMessage("MatchExcepReload", "DELETE Oracle Data failed.");
                    //    m_oLogger.LogMessageWeb("MatchExcepReload", "DELETE Oracle Data failed", "DELETE Oracle Data failed.  Resultset to delete: " + resultSet );
                    //    return;
                    //}
                    try
                    {
                        double delTimes = 0;
                        if (Convert.ToInt16(bo.items[0].rows.Count) > 0)
                            delTimes = Math.Ceiling(Convert.ToDouble(bo.items[0].rows.Count) / 1000);
                        string strDelQuery = "";
                        if (delTimes > 0)
                        {
                            for (int i = 0; i < delTimes; i++)
                            {
                                strDelQuery += "Delete From CO.POOverride LIMIT 1000;";
                            }
                            client.OpenRead(serviceURL2 + strDelQuery);
                            m_oLogger.LogMessage("POOverrideReload", "DELETE Oracle Data of " + bo.items[0].rows.Count.ToString() + " records successful.");
                        }
                    }
                    catch (Exception ex)
                    {
                        m_oLogger.LogMessage("POOverrideReload", "DELETE Oracle Data failed.");
                        m_oLogger.LogMessageWeb("POOverrideReload", "DELETE Oracle Data failed", "DELETE Oracle Data failed.  Resultset to delete: " + resultSet);
                        return;
                    }

                    try
                    {
                        POOverrideReloadDAL dal = new POOverrideReloadDAL();
                        dal.CreateTable(m_oLogger);

                        if (dal.dtResponseRowsCount > 0)
                        {
                            while (dal.gotAllData == "N")
                            {
                                PODData pod = dal.getData(m_oLogger);

                                //new batch SoapUI code
                                Batcher batcher = new Batcher(authorization, password);
                                batcher.CreateBuyExpBatch(pod, m_oLogger, out strResponse);

                                dal.UpdateTable(m_oLogger);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        strResponse = "FAILURE";
                        exErrorMsg = ex;
                    }

                }

                // Console.WriteLine(result);
                //var parsed = JObject.Parse(result);
                //strResponse = parsed.SelectToken("RequestStatus").Value<string>();

                m_oLogger.LogMessage("POOverrideReload", "POST POOverrideReload data to Oracle Helix server status " + strResponse);

                if (strResponse.ToUpper() != "SUCCESS")
                {
                    m_oLogger.LogMessage("POOverrideReload", "POST POOverrideReload data to Oracle Helix server status " + strResponse);
                    m_oLogger.LogMessageWeb("POOverrideReload", "POST POOverrideReload data to Oracle Helix server status " + strResponse, "POST POOverrideReload data to Oracle server status " + strResponse);

                }


            }

            //objWMReceiptsMappingDAL.UpdateWMReceiptMappingData(m_oLogger, processFlag);

            m_oLogger.LogMessage("Main", "POOverrideReload End");

        }

    }
}
