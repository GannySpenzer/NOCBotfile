using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using ExpediterReload1;
using System.Net;
using System.Configuration;
using System.IO;
using System.Data;
using System.Web.Http;
using OSVCService;

namespace ExpediterReload
{
    class ExpediterReload
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

            //string ACTION_ITEMS = " ";
            //string BUSINESS_UNIT = " ";
            //string BUYER_ID = " ";
            //string BUYER_TEAM = " ";
            //string CLIENT = " ";
            //string DESCRIPTION = " ";
            //string EXPEDITING_COMMENTS = " ";
            //string INVENTORY_BUSINESS_UNIT = " ";
            //string ITEM = " ";
            //string LAST_COMMENT_DATE = " ";
            //string LAST_OPERATOR = " ";
            //string LINE_NUMBER = " ";
            //string PO_DATE = " ";
            //string PO_ID = " ";
            //string PS_URL = " ";
            //string PRIORITY_FLAG = " ";
            //string PROBLEM_CODE = " ";
            //string SITE_NAME = " ";
            //string STATUS_AGE = " ";
            //string VENDOR_ID = " ";
            //string VENDOR_NAME = " ";
            //DateTime dateparse;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

            // InitializeLogger start here
            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "ExpediterReload");
            m_oLogger.LogMessage("Main", "Started utility ExpediterReload");

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //m_oLogger.LogMessage("postExpediterReload", "GET Oracle Data starts here");
            //m_oLogger.LogMessage("postWMReceiptMappingData", "POST WMReceiptMapping Data" + resultSet.ToString());
            //m_oLogger.LogMessage("postWMReceiptMappingData", "POST WMMapping Data URL : https://10.118.13.27:8243/SDIOutboundWMReceiptAPI/v1_0");
            //m_oLogger.LogMessage("postWMReceiptMappingData", "POST WMReceiptMapping Data URL : " + serviceURL);


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
                    m_oLogger.LogMessage("postExpediterReload", "QUERYING Oracle Data at " + serviceURL );
                    Stream myStream = client.OpenRead(serviceURL);
                    StreamReader sr = new StreamReader(myStream);
                    resultSet = sr.ReadToEnd();
                    string json = resultSet;
                    if (json.Trim() != "")
                    {
                        m_oLogger.LogMessage("ExpediterReload", "QUERYING Oracle Data successful");
                    }
                    else
                    {
                        m_oLogger.LogMessage("ExpediterReload", "QUERYING Oracle Data failed");
                        m_oLogger.LogMessageWeb("ExpediterReload", "QUERYING Oracle Data failed", "QUERYING Oracle Data failed");
                        return;
                    }

                    RootObject  bo = JsonConvert.DeserializeObject<RootObject>(resultSet);

                    //var objects = JArray.Parse(resultSet);
                    m_oLogger.LogMessage("ExpediterReload", "DELETE Oracle Data started");
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
                    //    m_oLogger.LogMessage("ExpediterReload", "DELETE Oracle Data successful.");
                    //}
                    //catch (Exception ex)
                    //{
                    //    m_oLogger.LogMessage("ExpediterReload", "DELETE Oracle Data failed.");
                    //    m_oLogger.LogMessageWeb("ExpediterReload", "DELETE Oracle Data failed", "DELETE Oracle Data failed.  Resultset to delete: " + resultSet );
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
                                strDelQuery += "Delete From CO.BuyExp LIMIT 1000;";
                            }
                            client.OpenRead(serviceURL2 + strDelQuery);
                            m_oLogger.LogMessage("ExpediterReload", "DELETE Oracle Data of " + bo.items[0].rows.Count.ToString() + " records successful.");
                        }
                    }
                    catch (Exception ex)
                    {
                        m_oLogger.LogMessage("ExpediterReload", "DELETE Oracle Data failed.");
                        m_oLogger.LogMessageWeb("ExpediterReload", "DELETE Oracle Data failed", "DELETE Oracle Data failed.  Resultset to delete: " + resultSet);
                        return;
                    }


                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //STEP #3 - QUERY TABLE AND POST NEW DATA 
                    //m_oLogger.LogMessage("ExpediterReload", "Query table started");
                    //ExpediterReloadDAL objGetExpediterReloadDAL = new ExpediterReloadDAL();
                    //dtResponse = objGetExpediterReloadDAL .getExpediterData (m_oLogger);
                    //if (dtResponse.Rows.Count == 0)
                    //{
                    //    m_oLogger.LogMessage("ExpediterReload", "Query returned no records.");
                    //    return;
                    //}
                    //else
                    //    m_oLogger.LogMessage("ExpediterReload", "POST ExpediterReload data started.");
                    //for (int i = 0; i < dtResponse.Rows.Count; i++)
                    //{
                    //    DataRow rowInit;
                    //    rowInit = dtResponse.Rows[i];

                    //    try
                    //    {
                    //ACTION_ITEMS = rowInit["ACTION_ITEMS"].ToString();
                    //BUSINESS_UNIT = rowInit["BUSINESS_UNIT"].ToString();
                    //BUYER_ID = rowInit["BUYER_ID"].ToString();
                    //BUYER_TEAM = "Test";                                     //?????
                    //CLIENT = rowInit["CLIENT"].ToString();
                    //DESCRIPTION = rowInit["DESCRIPTION"].ToString();
                    //EXPEDITING_COMMENTS = rowInit["EXPEDITING_COMMENTS"].ToString();
                    //INVENTORY_BUSINESS_UNIT = rowInit["BUSINESS_UNIT_IN"].ToString();
                    //ITEM = rowInit["ITEM"].ToString();

                    //LAST_COMMENT_DATE = rowInit["LAST_COMMENT_DATE"].ToString();
                    //dateparse = DateTime.Parse(LAST_COMMENT_DATE );
                    //LAST_COMMENT_DATE  = dateparse.ToString("yyyy-MM-ddTHH:mm:ss.000Z");

                    //LAST_OPERATOR = rowInit["LAST_OPERATOR"].ToString();
                    //LINE_NUMBER = rowInit["LINE_NBR"].ToString();

                    //PO_DATE = rowInit["PO_DATE"].ToString();
                    //dateparse = DateTime.Parse(PO_DATE);
                    //PO_DATE = dateparse.ToString("yyyy-MM-ddTHH:mm:ss.000Z");

                    //PO_ID = rowInit["PO_ID"].ToString();
                    //PS_URL = "Test";                                        //?????
                    //PRIORITY_FLAG = "Test";                                 //?????
                    //PROBLEM_CODE = rowInit["PROBLEM_CODE"].ToString();
                    //SITE_NAME = "Test";                                     //?????
                    //STATUS_AGE = "10";
                    //VENDOR_ID = rowInit["VENDOR_ID"].ToString();
                    //VENDOR_NAME = rowInit["VENDOR"].ToString();

                    //ACTION_ITEMS.Add(rowInit["ACTION_ITEMS"].ToString());
                    //BUSINESS_UNIT.Add(rowInit["BUSINESS_UNIT"].ToString());
                    //BUYER_ID.Add(rowInit["BUYER_ID"].ToString());
                    //BUYER_TEAM.Add("Test");                                     //?????
                    //CLIENT.Add(rowInit["CLIENT"].ToString());
                    //DESCRIPTION.Add(rowInit["DESCRIPTION"].ToString());
                    //EXPEDITING_COMMENTS.Add(rowInit["EXPEDITING_COMMENTS"].ToString());
                    //INVENTORY_BUSINESS_UNIT.Add(rowInit["BUSINESS_UNIT_IN"].ToString());
                    //ITEM.Add(rowInit["ITEM"].ToString());

                    //string LAST_COMMENT_DATEtest = rowInit["LAST_COMMENT_DATE"].ToString();
                    //dateparse = DateTime.Parse(LAST_COMMENT_DATEtest);
                    //LAST_COMMENT_DATE.Add(dateparse.ToString("yyyy-MM-ddTHH:mm:ss.000Z"));

                    //LAST_OPERATOR.Add( rowInit["LAST_OPERATOR"].ToString());
                    //LINE_NUMBER.Add(rowInit["LINE_NBR"].ToString());

                    //string PO_DATEtest = rowInit["PO_DATE"].ToString();
                    //dateparse = DateTime.Parse(PO_DATEtest);
                    //PO_DATE.Add(dateparse.ToString("yyyy-MM-ddTHH:mm:ss.000Z"));

                    //PO_ID.Add( rowInit["PO_ID"].ToString());
                    //PS_URL.Add("Test");                                        //?????
                    //PRIORITY_FLAG.Add("Test");                                 //?????
                    //PROBLEM_CODE.Add(rowInit["PROBLEM_CODE"].ToString());
                    //SITE_NAME.Add("Test");                                     //?????
                    //STATUS_AGE.Add( "10");
                    //VENDOR_ID.Add(rowInit["VENDOR_ID"].ToString());
                    //VENDOR_NAME.Add(rowInit["VENDOR"].ToString());
                    //}
                    //catch (Exception ex)
                    //{
                    //    m_oLogger.LogMessage("ExpeditorReload", "Error trying to parse data at line " + i.ToString(), ex);

                    //}

                    //DOC_NUM = DOC_NUM.PadLeft(14, '0');//i.e. "0000000000000004"
                    //string CLIENT = System.DateTime.Now.ToString("yyyyMMdd");
                    //string SITEID = rowInit["PLANT"].ToString();
                    //DateTime PSTNG_DATEcnv = Convert.ToDateTime(rowInit["ADD_DTTM"]);
                    //string PSTNG_DATE = PSTNG_DATEcnv.ToString("yyyyMMdd");
                    //string DOC_DATE = PSTNG_DATEcnv.ToString("yyyyMMdd");
                    //string REF_DOC_NO = DOC_NUM;                              //?
                    //string PONUM = rowInit["ISA_CUST_PO_ID"].ToString();
                    //string LINENUM = rowInit["ISA_CUST_PO_LINE"].ToString();
                    //string VENDELIVERYDATE = rowInit["DUE_DT"].ToString();
                    //}

                    try
                    {
                        ExpediterReloadDAL dal = new ExpediterReloadDAL();
                        dal.CreateTable(m_oLogger);

                        if (dal.dtResponseRowsCount > 0)
                        {
                            while (dal.gotAllData == "N")
                            {
                                BEData bed = dal.getData(m_oLogger);

                                //new batch SoapUI code
                                Batcher batcher = new Batcher(authorization, password);
                                batcher.CreateBuyExpBatch(bed, m_oLogger, out strResponse);

                                dal.UpdateTable(m_oLogger);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        strResponse = "FAILURE";
                        exErrorMsg = ex;
                    }

                    ////////////////////////////////////////////////////test//////////////////////////////////////
                    //DataRow[] rows = dtResponse.Select();


                    ////List<ExpediterReloadBO> target = rows.Where(row => row.Field<string>("ITEM") == ITEM  )
                    //List<ExpediterReloadBO> target = dtResponse.AsEnumerable()
                    //    .Select (row => new ExpediterReloadBO
                    //    {
                    //        Action_Items = ACTION_ITEMS ,
                    //        Business_Unit = BUSINESS_UNIT,  
                    //        Buyer_ID = BUYER_ID ,
                    //        Buyer_Team = BUYER_TEAM,
                    //        Client = CLIENT ,
                    //        Description = DESCRIPTION ,
                    //        Expediting_Comments = EXPEDITING_COMMENTS ,
                    //        Inventory_Business_Unit = INVENTORY_BUSINESS_UNIT ,
                    //        Item = ITEM ,
                    //        Last_Comment_Date = LAST_COMMENT_DATE ,
                    //        Last_Operator = LAST_OPERATOR ,
                    //        Line_Number = LINE_NUMBER ,
                    //        PO_Date = PO_DATE ,
                    //        PO_ID = PO_ID ,
                    //        PS_URL = PS_URL,
                    //        Priority_Flag = PRIORITY_FLAG,
                    //        Problem_Code = PROBLEM_CODE ,
                    //        Site_Name = SITE_NAME ,
                    //        Status_Age = STATUS_AGE ,
                    //        Vendor_ID = VENDOR_ID ,
                    //        Vendor_Name = VENDOR_NAME 
                    //    }).ToList();
                    //string json1 = JsonConvert.SerializeObject(target, Formatting.None);
                    //json1 = json1.Remove(0, 1);
                    //json1 = json1.Remove(json1.Length - 1);
                    ////////////////////////////////////////////////////test//////////////////////////////////////

                    //single post REST method
                    //StringBuilder sbInit = new StringBuilder();
                    //string xmlStr = string.Empty;
                    //string xmlStringInit = string.Empty;
                    //string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    //using (StreamReader sr2 = new StreamReader(dir + "/BuyExpPost.txt"))
                    //{

                    //    xmlStr = sr2.ReadToEnd();
                    //    sbInit.AppendFormat(xmlStr, ACTION_ITEMS, BUSINESS_UNIT, BUYER_ID, BUYER_TEAM, CLIENT, DESCRIPTION, EXPEDITING_COMMENTS, INVENTORY_BUSINESS_UNIT, ITEM, LAST_COMMENT_DATE, LAST_OPERATOR, LINE_NUMBER, PO_DATE, PO_ID, PS_URL, PRIORITY_FLAG, PROBLEM_CODE, SITE_NAME, STATUS_AGE, VENDOR_ID, VENDOR_NAME);
                    //    xmlStringInit = sbInit.ToString();
                    //}
                    //client.UploadString(serviceURL2, xmlStringInit);
                    ///////////////////////////////////////////////////////


                    m_oLogger.LogMessage("ExpediterReload", "POST ExpediterReload data to Solvay server status " + strResponse);

                    if (strResponse.ToUpper() != "SUCCESS")
                    {
                        m_oLogger.LogMessage("ExpediterReload", "POST ExpediterReload data to Solvay server status " + strResponse);
                        m_oLogger.LogMessageWeb("ExpediterReload", "POST ExpediterReload data to Solvay server status " + strResponse, "POST ExpediterReload data to Solvay server status " + strResponse);

                    }


                }


                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //if (strResponse.ToUpper() == "SUCCESS")
                //{
                //    processFlag = "I";
                //}
                //else
                //{
                //    processFlag = "E"; //error
                //}
                //objWMReceiptsMappingDAL.UpdateWMReceiptMappingData(m_oLogger, processFlag);

                m_oLogger.LogMessage("Main", "ExpediterReload End");

            }

        }
    }
}
