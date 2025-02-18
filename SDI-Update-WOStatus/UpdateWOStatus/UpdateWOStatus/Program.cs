using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace UpdateWOStatus
{
    class Program
    {        
        static void Main(string[] args)
        {
            StreamWriter objStreamWriter;
            string rootDir = "";
            rootDir = ConfigurationSettings.AppSettings["LogPath"];
            string logpath = rootDir + "UpdateWOStatus" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.GetHashCode() + ".txt";

            StreamWriter log;
            FileStream fileStream;
            DirectoryInfo logDirInfo;
            FileInfo logFileInfo;
            logFileInfo = new FileInfo(logpath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);

            if (!logDirInfo.Exists)
            {
                logDirInfo.Create();
            }

            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logpath, FileMode.Append);
            }

            log = new StreamWriter(fileStream);

            log.WriteLine("*********************Logs(" + String.Format(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")) + ")***********************************");

            log = UpdateWorkOrders(log);

            log.WriteLine("********************End of Update********************");

            log.Close();
            //UpdateWorkOrders();
            //string apiresponse = AuthenticateService1("Walmart");
        }
        //Shanmugapriya-INC0032107 CLONE - As a Sam's Club User, I need my Work Order status updated in the PS_ISA_WO_STATUS table For UpdateWOStatus utility
        public static bool getDBName()
        {
            bool isPRODDB = false;
            string PRODDbList = ConfigurationSettings.AppSettings["OLEProdDB"].ToString();
            string DbUrl = ConfigurationSettings.AppSettings["OLEDBconString"].ToString();
           
            try
            {
                DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper();
                isPRODDB = (PRODDbList.IndexOf(DbUrl.Trim().ToUpper()) > -1);
            }
            catch (Exception ex)
            {
                isPRODDB = false;
            }

            return isPRODDB;
        }
        //Shanmugapriya-INC0032107 CLONE - As a Sam's Club User, I need my Work Order status updated in the PS_ISA_WO_STATUS table For UpdateWOStatus utility
        private static void SendEmail(StreamWriter log, string Message = "")
        {
            SDiEmailUtilityService.EmailServices SDIEmailService = new SDiEmailUtilityService.EmailServices();
            List<byte[]> MailAttachmentbytes = new List<byte[]>();
            MailMessage email = new MailMessage();
            string MailToSpecial = ConfigurationSettings.AppSettings["MailToSpecial"];

            email.From = new MailAddress("WalmartPurchasing@sdi.com");
            if (!getDBName())
            {
                email.To.Add("webdev@sdi.com");
            }
            else
            {
                try
                {
                    foreach (var address in MailToSpecial.Split(';'))
                    {
                        email.To.Add(new MailAddress(address, ""));
                    }
                }
                catch
                {
                    log.WriteLine("Error in emailsent");
                }

            }
            email.Subject = "Error in the Update workorder status.";

            email.Priority = MailPriority.High;

            email.Body = "<html><body><table><tr><td> Update workorder status has completed with errors. Please check the logs. </td></tr>" + Message;
            try
            {
                SDIEmailService.EmailUtilityServices("MailandStore", email.From.Address, email.To.ToString(), email.Subject, String.Empty, String.Empty, email.Body, "StatusChangeEmail0", null, MailAttachmentbytes.ToArray());
            }
            catch
            {
                log.WriteLine("     Error - the email was not sent");
            }
        }

        public static StreamWriter UpdateWorkOrders(StreamWriter log)
        {
            try
            {
                string strqry = "Select * from ps_isa_wo_status where isa_wo_status <> 'COMPLETED' and business_unit_om ='I0W01'";
                DataSet WOdataset = GetAdapterSpc(strqry);

                if (WOdataset.Tables.Count > 0)
                {
                    if (WOdataset.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow workorder in WOdataset.Tables[0].AsEnumerable().Where(x=>x.Field<string>("ISA_WO_STATUS").ToLower() !="completed"))
                        {
                            //Shanmugapriya-INC0032107 CLONE - As a Sam's Club User, I need my Work Order status updated in the PS_ISA_WO_STATUS table For UpdateWOStatus utility
                            bool IsSamsclubWO = false;
                            if (!string.IsNullOrEmpty(workorder["ISA_Install_Cust"].ToString()))
                            {
                                string WO_Type = workorder["ISA_Install_Cust"].ToString();
                                log.WriteLine("Work order type:" + WO_Type);
                                string samsclub = ConfigurationManager.AppSettings["SamsClub"];

                                if (WO_Type.ToUpper() == samsclub)
                                {
                                    IsSamsclubWO = true;
                                }
                            }
                            else
                            {
                                log.WriteLine("Install Cust is null or empty for work order: " + workorder["ISA_WORK_ORDER_NO"].ToString());
                            }

                            string status = CheckWorkOrderStatus(workorder["ISA_WORK_ORDER_NO"].ToString(), "", IsSamsclubWO, log);
                            if (status.ToLower() == "completed")
                            {
                                strqry = "update ps_isa_wo_status set isa_wo_status = '" + status + "', LAST_UPDATE_DTTM = SYSTIMESTAMP where isa_work_order_no='" + workorder["ISA_WORK_ORDER_NO"].ToString() + "'";
                                int rowaffect = ExecNonQuery(strqry);
                                log.WriteLine("Work order status updated to 'Completed' status:" + workorder["ISA_WORK_ORDER_NO"].ToString());
                            }
                            else if(status.ToLower() == "invoiced")
                            {
                                status = "COMPLETED";
                                strqry = "update ps_isa_wo_status set isa_wo_status = '" + status + "', LAST_UPDATE_DTTM = SYSTIMESTAMP where isa_work_order_no='" + workorder["ISA_WORK_ORDER_NO"].ToString() + "'";
                                int rowaffect = ExecNonQuery(strqry);
                                log.WriteLine("Work order status updated to 'Completed' due to 'Invoiced' status:" + workorder["ISA_WORK_ORDER_NO"].ToString());
                            }
                            else if (status.ToLower() == "failed")
                            {
                                log.WriteLine("Work order number with failed response:" + workorder["ISA_WORK_ORDER_NO"].ToString());
                            }
                            else
                            {
                                log.WriteLine("Work order numbers with status other than completed and invoiced:" + workorder["ISA_WORK_ORDER_NO"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLine(ex.Message);
               SendEmail(log, ex.Message);
            }
            return log;
        }        

        public static string CheckWorkOrderStatus(string workOrder, string THIRDPARTY_COMP_ID, bool IsSamsclubWO, StreamWriter log)
        {
            try
            {
                string username = string.Empty;
                string password = string.Empty;
                string clientKey = string.Empty;
                string baseurl = string.Empty;
                string credType = string.Empty;
                string apiurl = string.Empty;
                string grant_type = string.Empty;
                string APIresponse = string.Empty;
                string samsclub = ConfigurationManager.AppSettings["SamsClub"];
                if (!string.IsNullOrEmpty(workOrder) & !string.IsNullOrWhiteSpace(workOrder))
                {
                    /* SDI-45198 Updating Work order for CBRE [change by vishalini] */
                    workOrder = workOrder.Trim();
                        string strquery = "SELECT DISTINCT LN.ISA_EMPLOYEE_ID AS ISA_EMPLOYEE_ID,LN.LASTUPDDTTM  from PS_ISA_ORD_INTF_LN LN,ps_isa_wo_status WO where LN.ISA_WORK_ORDER_NO = '" + workOrder + "' AND LN.ISA_WORK_ORDER_NO = WO.ISA_WORK_ORDER_NO AND WO.BUSINESS_UNIT_OM = LN.BUSINESS_UNIT_OM  AND LN.ISA_EMPLOYEE_ID <> ' ' order by ISA_EMPLOYEE_ID,LN.LASTUPDDTTM desc";
                        DataSet WOdataset = GetAdapterSpc(strquery);
                    string employeeId= ""; 
                         employeeId = Convert.ToString(WOdataset.Tables[0].Rows[0]["ISA_EMPLOYEE_ID"]);
                        

                        string sqlquery = "select THIRDPARTY_COMP_ID from SDIX_USERS_TBL where ISA_EMPLOYEE_ID='" + employeeId + "'";
                        string THIRDPARTYID = GetScalar(sqlquery, false);
                   //Shanmugapriya-INC0032107 CLONE - As a Sam's Club User, I need my Work Order status updated in the PS_ISA_WO_STATUS table For UpdateWOStatus utility
                    DataSet ds = new DataSet();
                    if (THIRDPARTYID == ConfigurationSettings.AppSettings["CBRECompanyID"].ToString())
                    {
                        //APIresponse = AuthenticateService("CBRE");
                        ds = GetCredentials("CBRE");
                    }
                    else if (IsSamsclubWO == true)
                    {
                        ds = GetCredentials(samsclub);
                    }
                    else
                    {
                        ds = GetCredentials("WALMART");
                    }

                    try
                    {
                        if (ds != null)
                        {
                            if (ds.Tables.Count > 0)
                            {
                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    try
                                    {
                                        username = ds.Tables[0].Rows[0]["CLIENT_ID"].ToString();
                                        password = ds.Tables[0].Rows[0]["CLIENT_SECRET"].ToString();
                                        clientKey = ds.Tables[0].Rows[0]["CLIENT_KEY"].ToString();
                                        baseurl = ds.Tables[0].Rows[0]["BASEURL"].ToString();
                                        apiurl = ds.Tables[0].Rows[0]["TOKENBASEURL"].ToString();
                                        credType = ds.Tables[0].Rows[0]["CRED_TYPE"].ToString();
                                        grant_type = ds.Tables[0].Rows[0]["GRANT_TYPE"].ToString();
                                    }
                                    catch (Exception innerEx)
                                    {
                                        log.WriteLine("Error while retrieving values from DataSet: " + innerEx.Message);
                                       SendEmail(log, innerEx.Message);                                        
                                    }
                                }
                                else
                                {
                                    log.WriteLine("Error: DataSet 'ds' is empty.");
                                }
                            }
                            else
                            {
                                log.WriteLine("Error: DataSet 'ds' has no data.");
                            }
                        }
                        else
                        {
                            log.WriteLine("Error: DataSet 'ds' is null.");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine("Error in outer try-catch block: " + ex.Message);
                        //SendEmail(log, ex.Message);                        
                    }
                    try
                    {
                        APIresponse = AuthenticateService(credType, username, password, clientKey, apiurl, grant_type, log);
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine("Method:CheckWorkOrderStatus - ", ex.Message);
                       // SendEmail(log, ex.Message);                        
                    }

                    //APIresponse = AuthenticateService("Walmart");
                    if ((APIresponse != "Server Error" & APIresponse != "Internet Error" & APIresponse != "Error"))
                    {
                        if ((!APIresponse.Contains("error_description")))
                        {
                            ValidateUserResponseBO objValidateUserResponseBO = JsonConvert.DeserializeObject<ValidateUserResponseBO>(APIresponse);
                            string apiURL = "";
                            //Shanmugapriya-INC0032107 CLONE - As a Sam's Club User, I need my Work Order status updated in the PS_ISA_WO_STATUS table For UpdateWOStatus utility

                            //if (ConfigurationSettings.AppSettings["OLEProdDB"] == ConfigurationSettings.AppSettings["OLECurrentDB"])
                            //    apiURL = "https://api.servicechannel.com/v3/odata/" + "workorders(" + workOrder + ")?$select=Status";                                
                            //else
                            //    apiURL = "https://sb2api.servicechannel.com/v3/odata/" + "workorders(" + workOrder + ")?$select=Status";
                            apiURL = baseurl + "/odata/" + "workorders(" + workOrder + ")?$select=Status";
                            HttpClient httpClient = new HttpClient();
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token);
                            var response = httpClient.GetAsync(apiURL).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                string workorderAPIResponse = response.Content.ReadAsStringAsync().Result;
                                StatusBO objCheckWo = JsonConvert.DeserializeObject<StatusBO>(workorderAPIResponse);
                                return objCheckWo.Status.Primary;
                                //objWalmartSC.WriteLine("Method: CheckWorkOrderStatus() Result-" + Convert.ToString(objCheckWo.Status.Extended));
                            }
                            else
                            {
                                //objWalmartSC.WriteLine("Method: CheckWorkOrderStatus() Result- Failed in API response");
                                return "Failed";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("Method:CheckWorkOrderStatus - ", ex.Message);
              // SendEmail(log, ex.Message);                 
                //objWalmartSC.WriteLine("Method:CheckWorkOrderStatus - " + ex.Message);
            }
            return "Failed";
        }
        //Shanmugapriya-INC0032107 CLONE - As a Sam's Club User, I need my Work Order status updated in the PS_ISA_WO_STATUS table For UpdateWOStatus utility
        public static DataSet GetCredentials(string credType)
        {
            DataSet ds = new DataSet();
            try
            {
                string strQuery = "SELECT CLIENT_ID, CLIENT_SECRET, CLIENT_KEY, BASEURL, TOKENBASEURL, CRED_TYPE, GRANT_TYPE FROM SDIX_USERSACCESSTOKEN_TBL WHERE CRED_TYPE = '" + credType + "' AND BUSINESS_UNIT='I0W01' AND ISACTIVE='Y' ";
                ds = GetAdapterSpc(strQuery);
            }
            catch (Exception ex)
            {
                SendEmail(null, "Error in GetCredentials method: " + ex.Message);
                // Set a default or empty DataSet here if needed
            }

            return ds;
        }

        public static DataSet GetAdapterSpc(string p_strQuery)
        {

            // Gives us a reference to the current asp.net 
            // application executing the method.
            // -----------------Write DB Query Method starts ---------------------
            // Dim objORDBData As ORDBData = New ORDBData()
            // objORDBData.writeQueries(p_strQuery)
            // -----------------Write DB Query Method ends ---------------------
            DataSet WorkOrderDataset = new DataSet();
            // Dim testSQLstr = "Provider=OraOLEDB.Oracle.1;Password=Sd1UniProd;User Id=sdiprod;Data Source=unip1.sdi.com"
            OleDbConnection connection = new OleDbConnection(DbUrl); // DbUrl
            try
            {
                OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(Command);



                dataAdapter.Fill(WorkOrderDataset);
                try
                {
                    dataAdapter.Dispose();
                }
                catch (Exception ex)
                {
                }
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex)
                {
                }
                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
                // connection.close()
                return WorkOrderDataset;
            }
            catch (Exception objException)
            {

                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
                // connection.close()
                return new DataSet();
            }
        }
        public static int ExecNonQuery(string p_strQuery, bool bGoToErrPage = true)
        {
            int rowsAffected = 0;
            OleDbConnection connection = new OleDbConnection(DbUrl);
            try
            {
                OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                rowsAffected = Command.ExecuteNonQuery();
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex)
                {
                }
                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception objException)
            {
                rowsAffected = 0;
                try
                {
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
            }
            return rowsAffected;
        }
        public static string GetScalar(string p_strQuery, bool bGoToErrPage = true)
        {
            // Gives us a reference to the current asp.net 
            // application executing the method.
            //HttpApplication currentApp = HttpContext.Current.ApplicationInstance;
            string strReturn = "";
            OleDbConnection connection = new OleDbConnection(DbUrl);
            try
            {
                OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                try
                {
                    strReturn = System.Convert.ToString(Command.ExecuteScalar());
                }
                catch (Exception ex32)
                {
                    strReturn = "";
                }
                if (strReturn == null)
                    strReturn = "";
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex1)
                {
                }
                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex2)
                {
                }
            }
            // connection.close()
            catch (Exception objException)
            {
                strReturn = "";

                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex)
                {

                    // connection.close()
                   
                }
            }

            return strReturn;
        }

        //Shanmugapriya-INC0032107 CLONE - As a Sam's Club User, I need my Work Order status updated in the PS_ISA_WO_STATUS table For UpdateWOStatus utility
        public static string AuthenticateService(string credType, string username, string password, string clientKey, string apiurl, string grant_type, StreamWriter log)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //string username = string.Empty;
                //string password = string.Empty;
                //string clientKey = string.Empty;
                //if (credType == "Walmart")
                //{
                //    username = "SDIAPI";
                //    password = "WalmartUser!123";
                //    if (ConfigurationSettings.AppSettings["OLEProdDB"] == ConfigurationSettings.AppSettings["OLECurrentDB"])
                //        clientKey = "U0IuMjAwMDA1MTI1OS5DQkREOEY3Qy0xRjVBLTREMEItODFBNy0zRjlDNUVEODlBQjA6RkYyRUQ4MDItQ0Y4OS00RTNDLTgzRUYtNUU4ODZDOTBFNjQw";//Prod
                //    else
                //        clientKey = "U0IuMjAwMDA1MTI1OS5GNjg2RENCNi0yNDMzLTQ3QjgtOEVCNi0zMDg3QkZERkREM0U6NDkzMTlENDAtRUEzQS00NjY0LUE2MTctRjY2NkQ0QUVBNzA4";//Test
                //}
                //else
                //{
                //    username = ConfigurationSettings.AppSettings["CBREUName"];
                //    password = ConfigurationSettings.AppSettings["CBREPassword"];
                //    if (ConfigurationSettings.AppSettings["OLEProdDB"] == ConfigurationSettings.AppSettings["OLECurrentDB"])
                //        clientKey = "U0IuMjAxNDkxNzQzMC4wNjU5RjkwQS00RUJCLTQ5MjItOUY5MS02NUZGNjFFRDBCMEQ6NzhBOTFBNTEtMkJGMS00MzJFLUIwNEMtRjgzRjJEOTk5OTVB";
                //    else
                //        clientKey = ConfigurationSettings.AppSettings["CBREClientKey"];
                //}
                //string apiurl = "";
                //if (ConfigurationSettings.AppSettings["OLEProdDB"] == ConfigurationSettings.AppSettings["OLECurrentDB"])
                //    apiurl = "https://login.servicechannel.com/oauth/token";
                //else
                //    apiurl = "https://sb2login.servicechannel.com/oauth/token";

                var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("username", username), new KeyValuePair<string, string>("password", password), new KeyValuePair<string, string>("grant_type", grant_type) });
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", clientKey); // Add("Authorization", "Basic " + clientKey)
                var response = httpClient.PostAsync(apiurl, formContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    var APIResponse = response.Content.ReadAsStringAsync().Result;
                    return APIResponse;                    
                }
                else
                {
                    var APIResponse = response.Content.ReadAsStringAsync().Result;
                    // Dim eobj As ExceptionHelper = New ExceptionHelper()
                    // eobj.writeExceptionMessage(APIResponse, "AuthenticateService")
                    if (APIResponse.Contains("error_description"))
                        return APIResponse;
                    return "Server Error";
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("Method:AuthenticateService - " + ex.Message);
               // SendEmail(log, ex.Message);    
                //bjWalmartSC.WriteLine("Method:AuthenticateService - " + ex.Message);
            }
            return "Server Error";
        }

        
        public static string DbUrl
        {
            get
            {
                return ConfigurationSettings.AppSettings["OLEDBconString"];
            }
        }

    }
    public class StatusBO
    {
        public Status Status { get; set; }
    }
    public class Status
    {
        public string Primary { get; set; }
    }
    public class ValidateUserResponseBO
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }   
}
