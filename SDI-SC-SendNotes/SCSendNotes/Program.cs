using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Data.OleDb;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using SCSendNotes.BO;
using SCSendNotes.Enums;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace SCSendNotes
{
    class Program
    {
        static StreamWriter log;
        static HttpClient HttpClient = new HttpClient();
        static HttpClient LoginHttpClient = new HttpClient();
        static Boolean IsProd;
        static string LogPath;
        static int SendNotesAttemptCount;
        public static string DbUrl
        {
            get
            {

                if (ConfigurationSettings.AppSettings["CURRENTDB"] == ConfigurationSettings.AppSettings["PRDDB"])
                    return ConfigurationSettings.AppSettings["OLEDBFSPRDconString"];
                else
                    return ConfigurationSettings.AppSettings["OLEDBconString"];

            }
        }
        static void Main(string[] args)
        {
            try
            {
                IsProd = ConfigurationSettings.AppSettings["CURRENTDB"] == ConfigurationSettings.AppSettings["PRDDB"];
                StreamWriter objStreamWriter;
                string rootDir = "";
                rootDir = ConfigurationSettings.AppSettings["LogPath"];
                rootDir += "\\";
                //Write logs in Local Directory regarding orders send to SC
                string logpath = rootDir + "SCSendNotesUtil" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.GetHashCode() + ".txt";
                HttpClient = new HttpClient();
                LoginHttpClient = new HttpClient();
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

                //string querystring = "SELECT '#SDI_PARTS_UPDATE# ' || '''' || '[{\"Order\":\"' || D.ORDER_NO || '\",\"Line\":' || d.isa_intfc_ln || ',\"Description\":\"' || REPLACE(d.descr254,'\"','') ||" +
                //                     "\"'\",\"DeliveryDate\":\"' || to_char(a.ISA_EST_DELIVER_DT, 'YYYY-MM-DD') || '\",\"Quantity\":' || b.QTY_LN_ACCPT_VUOM ||',\"WorkOrder\":\"' || d.isa_work_order_no ||" +
                //                     "'\",\"Email\":\"' || e.isa_employee_email || '\",\"TrackningNo\":\"' || a.isa_asn_track_no ||'\",\"Url\":' || decode(a.isa_new_url, null, 'null', '\"' || a.isa_new_url || '\"') ||" +
                //                     "',\"LinePartStatus\":\"' || d.isa_line_status ||'\",\"DeliveryLocation\":\"' || d.shipto_id ||'\",\"PartImage\":' || 'null' ||',\"OrderDate\":\"' || to_char(trunc(d.add_dttm), 'YYYY-MM-DD') ||" +
                //                     "',\"ShortDescription\":\"' || substr(d.descr254, 1, 30) ||',\"ShipVia\":\"' || b.isa_asn_ship_via ||'\"}]' || '''' from sysadm8.ps_isa_fedex_stg a," +
                //                     "sysadm8.ps_isa_asn_shipped b,sysadm8.ps_po_line_distrib c,sysadm8.ps_isa_ord_intf_ln d,sdiexchange.sdix_users_tbl e where a.add_dttm > sysdate - 1 and a.business_unit = 'WAL00'" +
                //                     "and a.process_status = 'C' and b.QTY_LN_ACCPT_VUOM > 0 and a.business_unit = b.business_unit and a.po_id = b.po_id and a.isa_asn_track_no = b.tracking_nbr and b.business_unit = c.business_unit" +
                //                     "and b.po_id = c.po_id and b.line_nbr = c.line_nbr and c.business_unit = d.business_unit_po and c.req_id = d.order_no and c.req_line_nbr = d.isa_intfc_ln and d.business_unit_om = e.business_unit" +
                //                     "and d.isa_employee_id = e.isa_employee_id and exists(select 'x'  from sysadm8.ps_isa_fdx_statxrf e  where a.cust_id = e.cust_id and a.ship_status = e.ship_status  and e.isa_line_status = 'DLF')" +
                //                     "union select '#SDI_PARTS_UPDATE# ' || '''' || '[{\"Order\":\"' || D.ORDER_NO || '\",\"Line\":' || d.isa_intfc_ln || ',\"Description\":\"' || d.descr254 || '\",\"DeliveryDate\":\"' || to_char(a.ISA_EST_DELIVER_DT, 'YYYY-MM-DD') ||" +
                //                     "'\",\"Quantity\":' || b.QTY_LN_ACCPT_VUOM || ',\"WorkOrder\":\"' || d.isa_work_order_no || '\",\"Email\":\"' || e.isa_employee_email || '\",\"TrackningNo\":\"' || a.isa_asn_track_no || '\",\"Url\":' || decode(a.isa_new_url, null, 'null', '\"' || a.isa_new_url || '\"') ||" +
                //                     "',\"LinePartStatus\":\"' || d.isa_line_status || '\",\"DeliveryLocation\":\"' || d.shipto_id || '\",\"PartImage\":' || 'null' || ',\"OrderDate\":\"' || to_char(trunc(d.add_dttm), 'YYYY-MM-DD') || ',\"ShortDescription\":\"' || substr(d.descr254, 1, 30) ||" +
                //                     "',\"ShipVia\":\"' || b.isa_asn_ship_via || '\"}]' || '''' from sysadm8.ps_isa_fedex_stg a, sysadm8.ps_isa_asn_shipped b,sysadm8.ps_isa_ord_intf_ln d, sdiexchange.sdix_users_tbl e where a.add_dttm > sysdate - 1 and a.business_unit = 'I0W01'" +
                //                     " and a.process_status = 'C' and b.QTY_LN_ACCPT_VUOM > 0 and a.business_unit = b.business_unit and a.po_id = b.po_id and a.isa_asn_track_no = b.tracking_nbr and b.business_unit = d.business_unit_om and b.po_id = d.order_no and b.line_nbr = d.isa_intfc_ln" +
                //                     " and d.business_unit_om = e.business_unit and d.isa_employee_id = e.isa_employee_id and exists(select 'x' from sysadm8.ps_isa_fdx_statxrf e where a.cust_id = e.cust_id and a.ship_status = e.ship_status and e.isa_line_status = 'DLF')";

                //Query to fetch delivered orders to send to SC

                string querystring = "SELECT '#SDI_PARTS_UPDATE# ' || '''' || '[{\"Order\":\"' || D.ORDER_NO ||'\",\"ThirdpartyCompID\":\"' || case when e.thirdparty_comp_id is null then 0 else e.thirdparty_comp_id end || '\",\"Line\":' || d.isa_intfc_ln" +
                                        "|| ',\"PartNumber\":\"' || CASE WHEN d.vendor_id <>'W999999999' THEN REPLACE(d.itm_id_vndr,'\"','') ELSE REPLACE(d.inv_item_id,'\"','') END || '\",\"Description\":\"' " +
                                        "||  REPLACE(d.descr254,'\"','') || '\",\"DeliveryDate\":\"' || to_char(a.ISA_EST_DELIVER_DT,'YYYY-MM-DD') || '\",\"Quantity\":' " +
                                        "|| b.QTY_LN_ACCPT_VUOM || ',\"WorkOrder\":\"' || d.isa_work_order_no || '\",\"Email\":\"' || e.isa_employee_email || '\",\"TrackningNo\":\"' " +
                                        "|| a.isa_asn_track_no || '\",\"Url\":' ||decode(a.isa_new_url,null,'null','\"' || a.isa_new_url || '\"' )|| ',\"LinePartStatus\":\"' " +
                                        "|| d.isa_line_status || '\",\"DeliveryLocation\":\"' || case when d.user_char2='HAL' then d.user_char2 else d.shipto_id end || '\",\"PartImage\":' || 'null' || ',\"OrderDate\":\"' " +
                                        "|| to_char(trunc(d.add_dttm),'YYYY-MM-DD') || '\",\"ShortDescription\":\"' || REPLACE(substr(d.descr254,1,30),'\"','') ||'\",\"ShipVia\":\"' || b.isa_asn_ship_via || '\"}]' || '''' from sysadm8.ps_isa_fedex_stg a, sysadm8.ps_isa_asn_shipped b, " +
                                        "sysadm8.ps_po_line_distrib c, sysadm8.ps_isa_ord_intf_ln d, sdiexchange.sdix_users_tbl e where a.add_dttm > sysdate - 1/24 and " +
                                        "a.business_unit = 'WAL00' and a.process_status = 'C' and b.QTY_LN_ACCPT_VUOM > 0 and a.business_unit = b.business_unit and " +
                                        "a.po_id = b.po_id and a.isa_asn_track_no = b.tracking_nbr and b.business_unit = c.business_unit and b.po_id = c.po_id and " +
                                        "b.line_nbr = c.line_nbr and c.business_unit = d.business_unit_po and c.req_id = d.order_no and c.req_line_nbr = d.isa_intfc_ln " +
                                        "and d.business_unit_om = e.business_unit and d.isa_employee_id = e.isa_employee_id and exists " +
                                        "(select 'x' from sysadm8.ps_isa_fdx_statxrf e where a.cust_id = e.cust_id and a.ship_status = e.ship_status and e.isa_line_status = 'DLF') " +
                                        "union select '#SDI_PARTS_UPDATE# ' || '''' || '[{\"Order\":\"' || D.ORDER_NO ||'\",\"ThirdpartyCompID\":\"' || case when e.thirdparty_comp_id is null then 0 else e.thirdparty_comp_id end || '\",\"Line\":' || d.isa_intfc_ln ||" +
                                        "',\"PartNumber\":\"' || CASE WHEN d.vendor_id <>'W999999999' THEN REPLACE(d.itm_id_vndr,'\"','') ELSE REPLACE(d.inv_item_id,'\"','') END || '\",\"Description\":\"' " +
                                        "||  REPLACE(d.descr254,'\"','') || '\",\"DeliveryDate\":\"' || to_char(a.ISA_EST_DELIVER_DT,'YYYY-MM-DD') || '\",\"Quantity\":' || b.QTY_LN_ACCPT_VUOM " +
                                        "|| ',\"WorkOrder\":\"' || d.isa_work_order_no || '\",\"Email\":\"' || e.isa_employee_email || '\",\"TrackningNo\":\"' || a.isa_asn_track_no " +
                                        "|| '\",\"Url\":' ||decode(a.isa_new_url,null,'null','\"' || a.isa_new_url || '\"' )|| ',\"LinePartStatus\":\"' || d.isa_line_status " +
                                        "|| '\",\"DeliveryLocation\":\"' || case when d.user_char2='HAL' then d.user_char2 else d.shipto_id end || '\",\"PartImage\":' || 'null' || ',\"OrderDate\":\"' || to_char(trunc(d.add_dttm),'YYYY-MM-DD') " +
                                        "|| '\",\"ShortDescription\":\"' || substr(d.descr254,1,30) ||'\",\"ShipVia\":\"' || b.isa_asn_ship_via || '\"}]' || '''' from sysadm8.ps_isa_fedex_stg a, sysadm8.ps_isa_asn_shipped b, sysadm8.ps_isa_ord_intf_ln d, " +
                                        "sdiexchange.sdix_users_tbl e where a.add_dttm > sysdate - 1/24 and a.business_unit = 'I0W01' and a.process_status = 'C' " +
                                        "and b.QTY_LN_ACCPT_VUOM > 0 and a.business_unit = b.business_unit and a.po_id = b.po_id and a.isa_asn_track_no = b.tracking_nbr " +
                                        "and b.business_unit = d.business_unit_om and b.po_id = d.order_no and b.line_nbr = d.isa_intfc_ln and d.business_unit_om = e.business_unit " +
                                        "and d.isa_employee_id = e.isa_employee_id and exists (select 'x' from sysadm8.ps_isa_fdx_statxrf e where a.cust_id = e.cust_id " +
                                        "and a.ship_status = e.ship_status and e.isa_line_status = 'DLF') " +
                                        "UNION SELECT '#SDI_PARTS_UPDATE# ' || '''' || '[{\"Order\":\"' || d.order_no ||'\",\"ThirdpartyCompID\":\"' || case when e.thirdparty_comp_id is null then 0 else e.thirdparty_comp_id end || '\",\"Line\":' || d.isa_intfc_ln " +
                                        "|| ',\"PartNumber\":\"' || CASE WHEN d.vendor_id <>'W999999999' THEN REPLACE(d.itm_id_vndr,'\"','') ELSE REPLACE(d.inv_item_id,'\"','') END || '\",\"Description\":\"' " +
                                        "|| REPLACE(d.descr254,'\"','') || '\",\"DeliveryDate\":\"' || to_char(d.EXPECTED_DATE,'YYYY-MM-DD') || '\",\"Quantity\":' " +
                                        "|| d.qty_requested || ',\"WorkOrder\":\"' || d.isa_work_order_no || '\",\"Email\":\"' || e.isa_employee_email || '\",\"TrackningNo\":null' " +
                                        "|| ',\"Url\":null,\"LinePartStatus\":\"' || d.isa_line_status || '\",\"DeliveryLocation\":\"' || case when d.user_char2='HAL' then d.user_char2 else d.shipto_id end " +
                                        "|| '\",\"PartImage\":null,\"OrderDate\":\"' || to_char(trunc(d.add_dttm),'YYYY-MM-DD') || '\",\"ShortDescription\":\"' || substr(d.descr254,1,30) " +
                                        "|| '\",\"ShipVia\":\"NULL\"}]' || '''' FROM sysadm8.ps_isaordstatuslog a, sysadm8.ps_isa_ord_intf_ln d, sdiexchange.sdix_users_tbl e " +
                                        "WHERE a.isa_line_status = 'DLF' and a.DTTM_STAMP > sysdate - 1/24 and a.business_unit_om = 'I0W01' AND a.business_unit_om = d.business_unit_om " +
                                        "AND a.order_no = d.order_no AND a.isa_Intfc_ln = d.isa_intfc_ln AND d.business_unit_om = e.business_unit AND d.isa_employee_id = e.isa_employee_id " +
                                        "and not exists (select 'x' from ps_po_line_distrib p, ps_isa_asn_Shipped asn where d.business_unit_po = p.business_unit " +
                                        "and d.order_no = p.req_id and d.isa_intfc_ln = p.req_line_nbr and p.business_unit = asn.business_unit and p.po_id = asn.po_id and p.line_nbr = asn.line_Nbr)";

                DataSet result = new DataSet();
                result = GetOrdersFromDB(querystring);
                //If query return records,
                if (result != null && result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    
                    //Convert DT to List<string>
                    var res = (from DataColumn column in result.Tables[0].Columns
                               let columnName = column.ColumnName
                               from DataRow row in result.Tables[0].AsEnumerable()
                               where row[columnName].ToString().Contains("#SDI_PARTS_UPDATE#")
                               select row[columnName].ToString().Replace("#SDI_PARTS_UPDATE# '[", "").Replace("]'", "")).ToList();

                    //convert List<string> to List<BO>
                    List<DBResponse> lists = new List<DBResponse>();
                    DBResponse dbResponse = new DBResponse();
                    var deserializedstring = string.Empty;
                    foreach (var list in res)
                    {
                        try
                        {
                            deserializedstring = list.Replace("\\", "\\\\");
                            //var d1= JsonConvert.DeserializeObject<JObject>(list);
                            dbResponse = JsonConvert.DeserializeObject<DBResponse>(deserializedstring);
                            if (!string.IsNullOrEmpty(dbResponse.Description))
                            {
                                dbResponse.Description = Regex.Replace(dbResponse.Description, @"\t|\n|\r", "");
                            }
                            if (!string.IsNullOrEmpty(dbResponse.WorkOrder))
                            {
                                dbResponse.WorkOrder = Regex.Replace(dbResponse.WorkOrder, @"\t|\n|\r", "");

                            }
                            lists.Add(dbResponse);
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.Message);
                        }



                    }
                    //group list based on order no.
                    var groupedList = lists.GroupBy(u => u.Order).Select(grp => grp.ToList()).ToList();


                    foreach (var orderlist in groupedList)
                    {
                        DBResponse dBResponse = new DBResponse();
                        try
                        {
                            var lineArray = orderlist.Select(x => x.Line).ToArray();
                            var lines = string.Join(",", lineArray.Select(x => string.Format("'{0}'", x)).ToArray());

                            //serialize the list
                            //**DECLARE * *a variable named **'json' * *and initialize it with the JSON string representation of * *'orderlist' * * using ** JsonConvert.SerializeObject** method
                            var json = JsonConvert.SerializeObject(orderlist);

                            //**PARSE * *the JSON string**'json' * *into a JArray object and assign it to a variable named **'jArray' * *
                            JArray jArray = JArray.Parse(json);

                            //**START * *a foreach loop to iterate over each item in the JArray **'jArray' * *
                            foreach (var item in jArray)
                            {
                                //**CONVERT * *the current * *'item' * *to a JObject and remove the * *'ThirdPartyCompID' * *property from this JObject
                                ((JObject)item).Remove("ThirdPartyCompID");
                            }
                            
                            //** CONVERT**the modified JArray **'jArray' * *back to a JSON string representation and assign it to a variable named * *'modifiedJsonArrayString' * *
                             string modifiedJsonArrayString = jArray.ToString();
   
                            //BO to send as parameter for SC
                            SendNotesBO notesBO = new SendNotesBO();
                            notesBO.MailedTo = "";
                            notesBO.DoNotSendEmail = true;
                            //**ASSIGN * *the value for **notesBO.Note * * as **"#SDI_PARTS_UPDATE#'" + modifiedJsonArrayString + "'" * *
                            notesBO.Note = "#SDI_PARTS_UPDATE#'" + modifiedJsonArrayString + "'";
                            //**ASSIGN** the value for **notesBO.ActionRequired** as **false**
                            notesBO.ActionRequired = false;
                            notesBO.ScheduledDate = DateTime.Now;
                            notesBO.Actor = "";
                            notesBO.Visibility = 0;
                            notesBO.NotifyFollowers = false;
                            notesBO.DoNotSendEmail = true;
                            string serializedparameter = JsonConvert.SerializeObject(notesBO);
                            SendNotesAttemptCount = 0;

                            //check for orders already sent to SC or not.
                            string GetLogSql = $"select * from SDIX_Send_SCNotes_Log where order_no='" + orderlist.FirstOrDefault().Order + "' and line_no in (" + lines + ") and workorder='" + orderlist.FirstOrDefault().WorkOrder + "' and lower(status_Code) = lower('created')";
                            var dt = GetOrdersFromDB(GetLogSql);

                            if (dt.Tables == null || dt.Tables.Count == 0 || dt.Tables[0].Rows == null || dt.Tables[0].Rows.Count == 0)
                            {
                                //To make a call to SC, We should send data authorized with token.
                                var IsSuccess = GetSCToken(orderlist.FirstOrDefault().WorkOrder);

								if (!IsSuccess)
									return;
								ResponseEum notesresponse = SCSendNotes(serializedparameter, orderlist.FirstOrDefault().WorkOrder, orderlist);
                                if (notesresponse == ResponseEum.unauthorized)
                                    break;
                                //**IF** count of the **ThirdpartyCompID** in **orderlist** is greater than 0
                                if (orderlist != null && orderlist.Count(x => x.ThirdPartyCompID != null && x.ThirdPartyCompID >= 100) > 0)
                                {
                                    //**SET** **'notesBO.MailedTo'** to the **'Email'** property of the current **'item'**
                                    notesBO.MailedTo = orderlist[0].Email;
                                    //**SET** **'notesBO.DoNotSendEmail'** to **false**
                                    notesBO.DoNotSendEmail = false;
                                    //**SET** **'notesBO.Note'** to **"Parts Delivered"**
                                    notesBO.Note = "Parts Delivered";
                                    //**SET** **notesBO.ActionRequired** as **true**
                                    notesBO.ActionRequired = true;
                                    //**ASSIGN** **Thirdpartynotes** variable by Serializing **notesBO**
                                    string Thirdpartynotes = JsonConvert.SerializeObject(notesBO);
                                    //**INVOKE** **SCSendNotes** by passing the parameter **Thirdpartynotes, orderlist.FirstOrDefault().WorkOrder, orderlist** and Assign the response to **Thirdpartyresponse**
                                    ResponseEum Thirdpartyresponse = SCSendNotes(Thirdpartynotes, orderlist.FirstOrDefault().WorkOrder, orderlist);
                                    //**IF** **Thirdpartyresponse** IS EQUAL TO **Response.unauthorized**
                                    if (Thirdpartyresponse == ResponseEum.unauthorized)
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.WriteLine("Exception occurred in SUB MAIN " + ex.ToString());
                            log.WriteLine("******* *******");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("Exception occurred in MAIN " + ex.ToString());
                log.WriteLine("******* *******");
            }
            finally
            {
                log.WriteLine("******************END OF SCNOTES ************");
                log.Close();
            }
        }
        //Method to generate Token based on credentials and Client Key.
        static bool GetSCToken(string Workorder)
        {
            try
            {
				var SCCredintials = GetSCDetails(Workorder);
				string UserName = SCCredintials.UserName;
                string Password = SCCredintials.Password;
                string apiurl, ClientKey;
                apiurl = SCCredintials.Baseurl;
                ClientKey = SCCredintials.Clientkey;
                
                LoginHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", ClientKey);
                var formContent = new FormUrlEncodedContent(new[]
                {
                         new KeyValuePair<string, string>("username", UserName),
                         new KeyValuePair<string, string>("password", Password),
                         new KeyValuePair<string, string>("grant_type", "password")
                });
                //The string Content will be sent as JSON through the request when setting the Media type as application/json
                var response = LoginHttpClient.PostAsync(apiurl, formContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    var apiresponse = response.Content.ReadAsStringAsync().Result;
                    var responsebo = JsonConvert.DeserializeObject<LoginResponse>(apiresponse);
                    //set token in headers of httpclient.
                    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responsebo.access_token);
                    log.WriteLine(apiresponse);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                log.WriteLine("Exception occured in Token genration" + ex.ToString());
                return false;
            }
        }
        //Method which fetch records from table based on query and return it as Dataset
        static DataSet GetOrdersFromDB(string sql)
        {
            try
            {
                OleDbConnection connection = new OleDbConnection(DbUrl);
                connection.Open();
                OleDbCommand Command = new OleDbCommand(sql, connection);
                Command.CommandTimeout = 120;
                OleDbDataAdapter adapter = new OleDbDataAdapter(Command);
                DataSet result = new DataSet();
                adapter.Fill(result);
                return result;
            }
            catch (Exception ex)
            {
                log.WriteLine($"Exception n GetOrdersFromDB method ");
                log.WriteLine("******* *******");
                //**WRITE** **query** in the log if there is any exception occurs using **log.WriteLine()**
                log.WriteLine("Exception in query" + sql);
                return new DataSet();
            }
        }
		public static DataTable GetDataTable(string p_strQuery)
		{
			System.Data.DataTable UserdataSet = new System.Data.DataTable();
			OleDbConnection connection;
			try
			{
				connection = new OleDbConnection(DbUrl);
			}
			catch (Exception ex)
			{
				var d = ex.Message;
				return UserdataSet;
			}

			try
			{
				OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
				Command.CommandTimeout = 120;
				connection.Open();
				OleDbDataAdapter dataAdapter = new OleDbDataAdapter(Command);
				dataAdapter.Fill(UserdataSet);
				try
				{
					dataAdapter.Dispose();
				}
				catch (Exception ex)
				{
					var d = ex.Message;
				}
				try
				{
					Command.Dispose();
				}
				catch (Exception ex)
				{
					var d = ex.Message;
				}
				try
				{
					connection.Dispose();
					connection.Close();
				}
				catch (Exception ex)
				{
					var d = ex.Message;
				}

			}
			catch (Exception objException)
			{
				try
				{
					var r = objException.Message;
					connection.Dispose();
					connection.Close();
				}
				catch (Exception ex)
				{
                    log.WriteLine($"Exception n GetDataTable method ");
                    log.WriteLine("******* *******");
                    //**WRITE** **query** in the log if there is any exception occurs using **log.WriteLine()**
                    log.WriteLine("Exception in query" + p_strQuery);
                    var d = ex.Message;
				}
			}
			return UserdataSet;
		}
		static int InsertLogInDB(string sql)
        {
            try
            {
                OleDbConnection connection = new OleDbConnection(DbUrl);
                connection.Open();
                OleDbCommand Command = new OleDbCommand(sql, connection);
                Command.CommandTimeout = 120;
                return Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                log.WriteLine($"Exception n InsertLogInDB method :" + ex.ToString());
                log.WriteLine("******* *******");
                //**WRITE** **query** in the log if there is any exception occurs using **log.WriteLine()**
                log.WriteLine("Exception in query" + sql);
                return 0;
            }
        }

		public static  SCCredintialsBO GetSCDetails(string Work_Order,bool Islogin=true)
		{
			SCCredintialsBO SCCredintials = new SCCredintialsBO();
			try
			{
				string SamOrWal = "WALMART";
				string query = $"Select ISA_INSTALL_CUST from PS_ISA_WO_STATUS where ISA_WORK_ORDER_NO='{Work_Order}'";
				var DataTable =GetDataTable(query);
				if (DataTable.Rows.Count>0)
				{
					if(Convert.IsDBNull(DataTable.Rows[0]["ISA_INSTALL_CUST"]) || string.IsNullOrWhiteSpace(DataTable.Rows[0]["ISA_INSTALL_CUST"].ToString()))
					{
						SamOrWal="WALMART";
					}
					else if(DataTable.Rows[0]["ISA_INSTALL_CUST"].ToString().ToUpper()=="WALMART")
					{
						SamOrWal = "WALMART";
					}
					else
					{
						SamOrWal="SAMSCLUB";
					}

				}
					SCCredintials=GetSCCredintials(SamOrWal,Islogin);
            }
            catch (Exception ex)
			{
				log.WriteLine($"Exception n GetSCDetails method :" + Work_Order +ex.ToString());
				log.WriteLine("******* *******");
			}
			return SCCredintials;
		}

		public static SCCredintialsBO GetSCCredintials(string WOType, bool Islogin )
		{
			SCCredintialsBO SCCredintials = new SCCredintialsBO();
			try
			{
				string query = $"Select * from SDIX_USERSACCESSTOKEN_TBL where cred_type='{WOType}'";
				var DataTable = GetDataTable(query);
				if (DataTable.Rows.Count > 0)
				{
					if (!Convert.IsDBNull(DataTable.Rows[0]["CLIENT_ID"]) && !string.IsNullOrWhiteSpace(DataTable.Rows[0]["CLIENT_ID"].ToString()))
					{
						SCCredintials.UserName = DataTable.Rows[0]["CLIENT_ID"].ToString();
					}
					if (!Convert.IsDBNull(DataTable.Rows[0]["CLIENT_SECRET"]) && !string.IsNullOrWhiteSpace(DataTable.Rows[0]["CLIENT_SECRET"].ToString()))
					{
						SCCredintials.Password = DataTable.Rows[0]["CLIENT_SECRET"].ToString();
					}
					if(Islogin)
					{
						if (!Convert.IsDBNull(DataTable.Rows[0]["TOKENBASEURL"]) && !string.IsNullOrWhiteSpace(DataTable.Rows[0]["TOKENBASEURL"].ToString()))
						{
							SCCredintials.Baseurl = DataTable.Rows[0]["TOKENBASEURL"].ToString();
						}
					}
					else
					{
						if (!Convert.IsDBNull(DataTable.Rows[0]["BASEURL"]) && !string.IsNullOrWhiteSpace(DataTable.Rows[0]["BASEURL"].ToString()))
						{
							SCCredintials.Baseurl = DataTable.Rows[0]["BASEURL"].ToString();
						}
					}
					if (!Convert.IsDBNull(DataTable.Rows[0]["CLIENT_KEY"]) && !string.IsNullOrWhiteSpace(DataTable.Rows[0]["CLIENT_KEY"].ToString()))
					{
						SCCredintials.Clientkey = DataTable.Rows[0]["CLIENT_KEY"].ToString();
					}


				}
			}
			catch (Exception ex)
			{

				log.WriteLine($"Exception n GetSCCredintials method :" + WOType + ex.ToString());
				log.WriteLine("******* *******");
			}
			return SCCredintials;
		}

        //Method to call SC Post Method
        static ResponseEum SCSendNotes(string input, string workorder, List<DBResponse> dBResponse)
        {

            try
            {
                if (SendNotesAttemptCount < 3)
                {
                    SendNotesAttemptCount += 1;
                    string apiurl;
                    string InsertLogSql = string.Empty;
					var SCCredintials = GetSCDetails(workorder,false);
                    apiurl = SCCredintials.Baseurl;
                    apiurl += "/workorders/" + workorder + "/notes";
                    var response = HttpClient.PostAsync(apiurl, new StringContent(input, Encoding.UTF8, "application/json")).Result;

                    //Orders which send to SC gets inserted into table 
                    log.WriteLine("workorder :" + workorder + " " + "Order:" + dBResponse.FirstOrDefault().Order + " " + "Status Code: " + response.StatusCode + " | Response : " + response.Content.ReadAsStringAsync().Result);
                    InsertLogSql = "INSERT INTO SDIX_Send_SCNotes_Log (Order_No, Line_No, status_Code, response,add_dtm,WorkOrder)(";
                    int listCount = dBResponse.Count();
                    int currentCount = 1;
                    foreach (var obj in dBResponse)
                    {
                        InsertLogSql += "SELECT " + $"'{dBResponse.FirstOrDefault().Order}','{obj.Line}','{response.StatusCode}','{response.Content.ReadAsStringAsync().Result}',sysdate,'{workorder}'" + "FROM DUAL ";
                        if (listCount == currentCount)
                        {
                            InsertLogSql += ")";
                        }
                        else
                        {
                            InsertLogSql += "UNION ALL ";
                        }
                        currentCount++;
                    }

                    int count = InsertLogInDB(InsertLogSql);
                    if (count == 0)
                        log.WriteLine($"For Order NO : {dBResponse.FirstOrDefault().Order} not inserted in table");
                    if (response.IsSuccessStatusCode)
                    {
                        SendNotesAttemptCount = 0;
                        return ResponseEum.success;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var IsSuccess = GetSCToken(workorder);
                        if (!IsSuccess)
                            return ResponseEum.unauthorized;
                        else
                            return SCSendNotes(input, workorder, dBResponse);
                    }
                    else
                        return ResponseEum.failure;
                }
                else
                    return ResponseEum.atempt_exceed;
            }
            catch (Exception ex)
            {
                log.WriteLine("Exception occured at SCSendNotes method" + ex.ToString());
                return ResponseEum.unexcpectederror;
            }
        }
    }
}
