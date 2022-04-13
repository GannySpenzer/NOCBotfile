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
        public static StreamWriter UpdateWorkOrders(StreamWriter log)
        {
            try
            {
                string strqry = "Select * from ps_isa_wo_status where isa_wo_status <> 'COMPLETED'";
                DataSet WOdataset = GetAdapterSpc(strqry);
                if (WOdataset.Tables.Count > 0)
                {
                    if (WOdataset.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow workorder in WOdataset.Tables[0].AsEnumerable().Where(x=>x.Field<string>("ISA_WO_STATUS").ToLower() !="completed"))
                        {
                            string status = CheckWorkOrderStatus(workorder["ISA_WORK_ORDER_NO"].ToString(), "");
                            if (status.ToLower() == "completed")
                            {
                                strqry = "update ps_isa_wo_status set isa_wo_status = '" + status + "' where isa_work_order_no='" + workorder["ISA_WORK_ORDER_NO"].ToString() + "'";
                                    int rowaffect = ExecNonQuery(strqry);
                            }
                            else if(status.ToLower() == "invoiced")
                            {
                                log.WriteLine("Work order number with invoiced as status:" + workorder["ISA_WORK_ORDER_NO"].ToString());
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
            }
            return log;
        }        

        public static string CheckWorkOrderStatus(string workOrder, string THIRDPARTY_COMP_ID)
        {
            try
            {
                string APIresponse = string.Empty;
                if (!string.IsNullOrEmpty(workOrder) & !string.IsNullOrWhiteSpace(workOrder))
                {
                    /* Updating Work order for CBRE*/
                    string strquery = "SELECT DISTINCT LN.ISA_EMPLOYEE_ID AS ISA_EMPLOYEE_ID,LN.LASTUPDDTTM  from PS_ISA_ORD_INTF_LN LN,ps_isa_wo_status WO where LN.ISA_WORK_ORDER_NO = '" + workOrder + "' AND LN.ISA_WORK_ORDER_NO = WO.ISA_WORK_ORDER_NO AND WO.BUSINESS_UNIT_OM = LN.BUSINESS_UNIT_OM  order by ISA_EMPLOYEE_ID,LN.LASTUPDDTTM desc";
                    DataSet WOdataset = GetAdapterSpc(strquery);
                    string employeeId = Convert.ToString(WOdataset.Tables[0].Rows[0]["ISA_EMPLOYEE_ID"]);
                    string sqlquery = "select THIRDPARTY_COMP_ID from SDIX_USERS_TBL where ISA_EMPLOYEE_ID='" + employeeId + "'";
                    string THIRDPARTYID = GetScalar(sqlquery, false);

                    if (!string.IsNullOrEmpty(THIRDPARTYID))
                    {
                        if (THIRDPARTYID == ConfigurationSettings.AppSettings["CBRECompanyID"].ToString())
                            APIresponse = AuthenticateService("CBRE");
                        else
                            APIresponse = AuthenticateService("Walmart");
                    }
                    //APIresponse = AuthenticateService("Walmart");
                    if ((APIresponse != "Server Error" & APIresponse != "Internet Error" & APIresponse != "Error"))
                    {
                        if ((!APIresponse.Contains("error_description")))
                        {
                            ValidateUserResponseBO objValidateUserResponseBO = JsonConvert.DeserializeObject<ValidateUserResponseBO>(APIresponse);
                            string apiURL = "";
                            //if (ConfigurationSettings.AppSettings["OLEProdDB"] == ConfigurationSettings.AppSettings["OLECurrentDB"])
                                apiURL = "https://api.servicechannel.com/v3/odata/" + "workorders(" + workOrder + ")?$select=Status";
                            //else
                            //    apiURL = "https://sb2api.servicechannel.com/v3/odata/" + "workorders(" + workOrder + ")?$select=Status";
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
                return "Failed";
                //objWalmartSC.WriteLine("Method:CheckWorkOrderStatus - " + ex.Message);
            }
            return "Failed";
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


        public static string AuthenticateService(string credType)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string username = string.Empty;
                string password = string.Empty;
                string clientKey = string.Empty;
                if (credType == "Walmart")
                {
                    username = "SDIAPI";
                    password = "WalmartUser!123";
                    //if (ConfigurationSettings.AppSettings["OLEProdDB"] == ConfigurationSettings.AppSettings["OLECurrentDB"])
                        clientKey = "U0IuMjAwMDA1MTI1OS5DQkREOEY3Qy0xRjVBLTREMEItODFBNy0zRjlDNUVEODlBQjA6RkYyRUQ4MDItQ0Y4OS00RTNDLTgzRUYtNUU4ODZDOTBFNjQw";//Prod
                    //else
                    //clientKey = "U0IuMjAwMDA1MTI1OS5GNjg2RENCNi0yNDMzLTQ3QjgtOEVCNi0zMDg3QkZERkREM0U6NDkzMTlENDAtRUEzQS00NjY0LUE2MTctRjY2NkQ0QUVBNzA4";//Test
                }
                else
                {
                    username = ConfigurationSettings.AppSettings["CBREUName"];
                    password = ConfigurationSettings.AppSettings["CBREPassword"];
                    //if (ConfigurationSettings.AppSettings["OLEProdDB"] == ConfigurationSettings.AppSettings["OLECurrentDB"])
                        clientKey = "U0IuMjAxNDkxNzQzMC4wNjU5RjkwQS00RUJCLTQ5MjItOUY5MS02NUZGNjFFRDBCMEQ6NzhBOTFBNTEtMkJGMS00MzJFLUIwNEMtRjgzRjJEOTk5OTVB";
                    //else
                    //    clientKey = ConfigurationSettings.AppSettings["CBREClientKey"];
                }
                string apiurl = "";
                //if (ConfigurationSettings.AppSettings["OLEProdDB"] == ConfigurationSettings.AppSettings["OLECurrentDB"])
                    apiurl = "https://login.servicechannel.com/oauth/token";
                //else
                //    apiurl = "https://sb2login.servicechannel.com/oauth/token";
                var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("username", username), new KeyValuePair<string, string>("password", password), new KeyValuePair<string, string>("grant_type", "password") });
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
