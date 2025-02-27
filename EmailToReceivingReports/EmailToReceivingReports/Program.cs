﻿using Microsoft.Exchange.WebServices.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net.Mail;
using System.Data.OleDb;
//using Spire.Xls;
//using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using System.Diagnostics;
using System.Net;
using Task = System.Threading.Tasks.Task;
using System.Net.Http;
using Newtonsoft.Json;
namespace EmailToReceivingReports
{
    class Program
    {
        static void Main(string[] args)
        {
            BackOrderReportProcess().Wait();

        }
        public partial class ResponseBO
        {
            public string access_token { get; set; }
        }
        //Per Ben-Commenting the StoreRoomOrderReport process only backorder is been used now
        //Madhu-INC0015039-OAUTH Change for Abbvie EmailToReceivingReports utility
        public async static Task BackOrderReportProcess()
        { 
            string OutlookURL = ConfigurationManager.AppSettings["OutlookURL"];
            string ProcessedFolderName = ConfigurationManager.AppSettings["ProcessedFolderName"];
            string DestinationPath = ConfigurationManager.AppSettings["DestinationPath"];
            string EmailIDAccount = ConfigurationManager.AppSettings["MailboxEmailID"];

            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            appPath = appPath.Substring(0, appPath.LastIndexOf("bin"));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            try
            {

                string logpath = string.Empty;

                string logFilePath = appPath + @"Logs\BackOrderReportProcessLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
                logFileInfo = new FileInfo(logFilePath);
                logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);

                if (!logDirInfo.Exists) logDirInfo.Create();
                if (!logFileInfo.Exists)
                {
                    fileStream = logFileInfo.Create();
                }
                else
                {
                    fileStream = new FileStream(logFilePath, FileMode.Append);
                }
                log = new StreamWriter(fileStream);
                log.WriteLine("*************************************Logs(" + String.Format(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")) + ")***********************************");
                var ewsScopes = new string[] { "https://outlook.office365.com/.default" };
                string tenentID = ConfigurationManager.AppSettings["tenantId"];
                var token_url = $"https://login.microsoftonline.com/" + tenentID + "/oauth2/v2.0/token";
                var data = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    {"scope", ewsScopes[0] },
                    {"client_id", ConfigurationManager.AppSettings["appId"] },
                    {"client_secret", ConfigurationManager.AppSettings["clientSecret"] }



                };



                HttpClient client = new HttpClient();
                var authResult = await client.PostAsync(token_url, new FormUrlEncodedContent(data));
                string accestoken = await authResult.Content.ReadAsStringAsync();
                ResponseBO objResponseBO = JsonConvert.DeserializeObject<ResponseBO>(accestoken);
                ExchangeService service = new ExchangeService();
                service.Url = new Uri(OutlookURL);



                log.WriteLine("Access token has been fetched. Token: " + objResponseBO.access_token);



                service.Credentials = new OAuthCredentials(objResponseBO.access_token);
                service.ImpersonatedUserId =
                    new ImpersonatedUserId(ConnectingIdType.SmtpAddress, EmailIDAccount);
                service.HttpHeaders.Add("X-AnchorMailbox", EmailIDAccount);



                FindFoldersResults moveToFolder = null;
                FolderView fv = new FolderView(999);
                Mailbox mailBoxToProcess = new Mailbox(EmailIDAccount);
                FolderId processedFolderID = null;

                FolderId fidProcessedParent = new FolderId(WellKnownFolderName.Inbox, mailBoxToProcess);
                ItemView view = new ItemView(int.MaxValue);

                //Get the filtered Email With Subject in Inbox
                FindItemsResults<Item> findResults = service.FindItems(fidProcessedParent, SetFilter(), view);
                log.WriteLine("Total Emails with filtered subject name is " + findResults.Items.Count() + ".");

                moveToFolder = service.FindFolders(fidProcessedParent, fv);
                //Get the Processed Folder Id
                foreach (Folder folder in moveToFolder.Folders)
                {
                    if (folder.DisplayName == ProcessedFolderName)
                    {
                        processedFolderID = folder.Id;
                        break;
                    }
                }

                int PreventDuplicationoffiles = 0;
                foreach (Item item in findResults.Items)
                {
                    EmailMessage message = EmailMessage.Bind(service, item.Id, new PropertySet(BasePropertySet.FirstClassProperties, EmailMessageSchema.From, ItemSchema.Attachments));
                    string body = message.Body.Text;
                    string from_name = message.From.ToString();
                    string from = message.From.Address.ToString();
                    string subject = message.Subject.ToString();
                    int attchcount = item.Attachments.Count();
                    int attachmentMoved = 0;
                    Boolean strrslt = false;
                    foreach (Microsoft.Exchange.WebServices.Data.Attachment attch in message.Attachments)
                    {
                        PreventDuplicationoffiles = PreventDuplicationoffiles + 1;
                        FileAttachment fileAttachment = attch as FileAttachment;
                        string fileName = "";
                        fileName = fileAttachment.Name;
                        int index = fileName.LastIndexOf('.');
                        string fileExtension = fileName.Substring(index + 1);
                        string csvfilename = fileAttachment.Name.Substring(0, fileAttachment.Name.LastIndexOf("."));

                        if (fileExtension == "xlsx" || fileExtension == "xls")
                        {
                            //stored the Attachment file in the AttachmentFile folder
                            try
                            {
                                csvfilename = "Receipts_" + String.Format("" + DateTime.Now.ToString("MMddyyyy_hhmmss")).ToString() + "" + PreventDuplicationoffiles + ".csv";
                                log.WriteLine("");
                                log.WriteLine("Begin Processing the Email from : " + from_name + " for File name " + fileAttachment.Name + "");
                                fileAttachment.Load(appPath + @"AttachmentFile\" + fileAttachment.Name);
                                if (System.IO.File.Exists(appPath + @"AttachmentFile\" + fileAttachment.Name))
                                {
                                    log.WriteLine("Downloaded the attachment file " + fileAttachment.Name);
                                }
                                else
                                {
                                    log.WriteLine("Error in Downloading the attachment file " + fileAttachment.Name + " in attachment folder.");
                                    log.WriteLine("-------------------------------------------------------------");
                                    break;
                                }

                                //Inserting into the Tables
                                DataTable dt = new DataTable();
                                dt = ReadExcel(appPath + @"AttachmentFile\" + fileAttachment.Name, log);
                                if (dt != null)
                                {
                                    if (dt.Rows.Count > 0)
                                    {
                                        //Inserting into DB
                                       
                                        strrslt = InsertBackOrderReport(dt);
                                        if (strrslt)
                                        {
                                            log.WriteLine("Back order report records inserted into the receipt log table.");
                                        }
                                        else
                                        {
                                            log.WriteLine("Error while inserting the records into the Back order report log table.");
                                        }
                                    }
                                    else
                                    {
                                        log.WriteLine("No records are availiable in the Attachment file. So we are moving this email to " + ProcessedFolderName + " folder.");
                                        attachmentMoved = attachmentMoved + 1;
                                        //Delete the Attachment file in AttachmentFile folder
                                        System.IO.DirectoryInfo Attchments_Folder = new DirectoryInfo(appPath + @"AttachmentFile");
                                        foreach (FileInfo file in Attchments_Folder.GetFiles())
                                        {
                                            file.Delete();
                                        }
                                        //System.IO.File.Delete(appPath + @"AttachmentFile\" + fileAttachment.Name);
                                        log.WriteLine("Deleted the Attachment file from the AttachmentFile folder even when the attachment has no datas.");
                                        break;
                                    }
                                }
                                else
                                {
                                    log.WriteLine("Error in reading the attachment file records.");
                                    break;
                                }

                                //Move to the destination VM 
                                //System.IO.File.Copy(appPath + @"ConvertedFile\" + csvfilename, DestinationPath + csvfilename);
                                //if (System.IO.File.Exists(DestinationPath + csvfilename))
                                //{
                                //    log.WriteLine("Moved the " + csvfilename + " file to destination VM");
                                //}
                                //else
                                //{
                                //    log.WriteLine("Error in the Moving the " + csvfilename + " file to destination VM");
                                //    log.WriteLine("-------------------------------------------------------------");
                                //    break;
                                //}

                                //Delete the Attachment file in AttachmentFile folder
                                System.IO.DirectoryInfo Attchment_Folder = new DirectoryInfo(appPath + @"AttachmentFile");
                                foreach (FileInfo file in Attchment_Folder.GetFiles())
                                {
                                    file.Delete();
                                }
                                //System.IO.File.Delete(appPath + @"AttachmentFile\" + fileAttachment.Name);
                                log.WriteLine("Deleted the Attachment file from the AttachmentFile folder");

                                //Delete the Attachment file in ConvertedFile folder
                                System.IO.DirectoryInfo Converted_Folder = new DirectoryInfo(appPath + @"ConvertedFile");
                                foreach (FileInfo files in Converted_Folder.GetFiles())
                                {
                                    files.Delete();
                                }
                                //System.IO.File.Delete(appPath + @"ConvertedFile\" + csvfilename);
                                log.WriteLine("Deleted the Converted file from the ConvertedFile folder");
                                log.WriteLine("Email and attachments processed succeffully");
                                log.WriteLine("");
                                attachmentMoved = attachmentMoved + 1;
                            }
                            catch (Exception ex)
                            {
                                SendErrorEmail(ex, ex.Message, Convert.ToString(ex.InnerException), "Main Method");
                                log.WriteLine("ERROR: " + ex.Message + ". Inner Exception : " + Convert.ToString(ex.InnerException));
                                log.WriteLine("-------------------------------------------------------------");
                                continue;
                            }
                        }
                    }

                    String DbUrl = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                    DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper();
                    //Email moved to Processed folder
                    if (attachmentMoved > 0)
                    {
                        if (strrslt)
                        {

                            if (DbUrl == "PROD" | DbUrl == "SPRD")
                            {
                                item.Move(processedFolderID);
                                log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                                log.WriteLine("-------------------------------------------------------------");
                            }
                            else
                            {
                                item.Move(processedFolderID);
                                log.WriteLine("Test DB Email  moved Processd folder.");
                                log.WriteLine("-------------------------------------------------------------");
                            }

                        }
                        else
                        {
                            log.WriteLine("Error in email process.");
                            log.WriteLine("-------------------------------------------------------------");
                        }
                    }
                    else
                    {
                        log.WriteLine("");
                        log.WriteLine("Email not moved to " + ProcessedFolderName + " folder since there is no attahments availible or attahcments not moved to destination.");
                        log.WriteLine("-------------------------------------------------------------");
                    }
                }
                log.WriteLine("-------------------------------------------------------------");
                log.Close();
            }
            catch (Exception ex)
            {
                SendErrorEmail(ex, ex.Message, Convert.ToString(ex.InnerException), "Main");
            }
        }




        private static SearchFilter SetFilter()
        {
            string FilterEmailWithSubject = ConfigurationManager.AppSettings["EmailSubject"];
            List<SearchFilter> searchFilterCollection = new List<SearchFilter>();
            searchFilterCollection.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.Subject, FilterEmailWithSubject));
            SearchFilter searchfiltr = new SearchFilter.SearchFilterCollection(LogicalOperator.Or, searchFilterCollection.ToArray());
            return searchfiltr;
        }

        private static DataTable ReadExcel(string UploadPath, StreamWriter log)
        {
            string conString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 8.0';", UploadPath);
            try
            {
                int rowsaffected;
                OleDbConnection oledbConn = new OleDbConnection(conString);
                try
                {
                    oledbConn.Open();
                    DataTable Sheets = oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    string worksheets = Sheets.Rows[0]["TABLE_NAME"].ToString();
                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + worksheets + "]", oledbConn);
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    oleda.Fill(ds);

                    oledbConn.Dispose();
                    oledbConn.Close();

                    log.WriteLine("Readed the excel datas successfully.");
                    return ds.Tables[0];
                }
                catch (Exception ex)
                {
                    oledbConn.Dispose();
                    oledbConn.Close();
                    log.WriteLine("Error While reading the Excel File : " + ex.Message);
                    SendErrorEmail(ex, ex.Message, Convert.ToString(ex.InnerException), "ReadExcel");
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("Error While reading the Excel File : " + ex.Message);
                return null;
            }
        }

        private static string DataTableToCSV(DataTable datatable, char seperator)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < datatable.Columns.Count; i++)
            {
                sb.Append(datatable.Columns[i]);
                if (i < datatable.Columns.Count - 1)
                    sb.Append(seperator);
            }
            sb.AppendLine();
            foreach (DataRow dr in datatable.Rows)
            {
                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    sb.Append(dr[i].ToString());

                    if (i < datatable.Columns.Count - 1)
                        sb.Append(seperator);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        //Madhu-INC0015039-OAUTH Change for Abbvie EmailToReceivingReports utility
        //Madhu-INC0039457	We would like to make updates and use a different report for ingestion on the Abbvie Item status
        private static Boolean InsertBackOrderReport(DataTable dt)
        {
            Boolean reslt = false;
            try
            {
                string StoreRoom = string.Empty;
                string backorder = string.Empty;
                string StockCatagory = string.Empty;
                string Item = string.Empty;
                string LastIssueDate = String.Empty;
                string ItemDescription = string.Empty;
                string IssueUnit = string.Empty;
                string CurrentBalance = string.Empty;
                string HardReservedQuantity = string.Empty;
                string strSQLstring = string.Empty;
                int rowsaffected;
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();

                  //deleting unwanted rows
                   dt.Rows[0].Delete();
                   dt.Rows[1].Delete();
                   //dt.Rows[2].Delete();
                   //dt.Rows[3].Delete();
                   //dt.Rows[4].Delete();
                  dt.AcceptChanges();

                   string ParentOrder = "";
                   Boolean isExit = false;

                foreach (DataRow rw in dt.Rows)
                {
                        string currentOrder = "";
                        if (!isExit)
                        {

                            if (rw["Stockroom Backorder Report"] != System.DBNull.Value)
                            {
                                currentOrder = rw["Stockroom Backorder Report"].ToString().Trim();
                                if (currentOrder == "Summary")
                                {
                                    isExit = true;
                                    currentOrder = "";
                                    StoreRoom = "";
                                }
                                else if (currentOrder == "")
                                {
                                    currentOrder = ParentOrder;
                                    StoreRoom = currentOrder;
                                }
                                else if (currentOrder == ParentOrder)
                                {
                                    StoreRoom = "";
                                }
                                else
                                {
                                    ParentOrder = currentOrder;
                                    StoreRoom = currentOrder;
                                }
                            }
                            else
                            {
                                StoreRoom = ParentOrder;
                            }
                            //Need to insert the values only if the backorder value is "Y"
                            try
                            {
                                if (rw["F13"] != System.DBNull.Value)
                                {
                                    backorder = rw["F13"].ToString().ToUpper();
                                }
                                else
                                {
                                    backorder = "N";
                                }
                            }
                            catch (Exception ex)
                            {
                                backorder = "N";
                        }
                        if (backorder == "Y")
                        {
                            if (StoreRoom != "")
                            {
                                //Validate the stock category
                                try
                                {
                                    if (rw["F2"] != System.DBNull.Value)
                                    {
                                        StockCatagory = rw["F2"].ToString();
                                    }
                                    else
                                    {
                                        StockCatagory = " ";
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                //Validate the Item
                                try
                                {
                                    if (rw["F3"] != System.DBNull.Value)
                                    {
                                        Item = rw["F3"].ToString();
                                    }
                                    else
                                    {
                                        Item = " ";
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                //Validate the Last Issuedate
                                try
                                {
                                    if (rw["F4"] != System.DBNull.Value)
                                    {
                                        LastIssueDate = rw["F4"].ToString();
                                    }
                                    else
                                    {
                                        LastIssueDate = "";
                                    }

                                }
                                catch (Exception ex)
                                {

                                }
                                //Validate the Item description
                                try
                                {
                                    if (rw["F6"] != System.DBNull.Value)
                                    {
                                        ItemDescription = rw["F6"].ToString();
                                        if (ItemDescription.Contains("'"))
                                        {
                                            ItemDescription = ItemDescription.Replace("'", "''");
                                        }
                                    }
                                    else
                                    {
                                        ItemDescription = " ";
                                    }


                                }
                                catch (Exception ex)
                                {

                                }
                                //Validate the Issue unit
                                try
                                {
                                    if (rw["F10"] != System.DBNull.Value)
                                    {
                                        IssueUnit = rw["F10"].ToString();
                                    }
                                    else
                                    {
                                        IssueUnit = " ";
                                    }

                                }
                                catch (Exception ex)
                                {

                                }
                                //Validate the Hardreserved quantity

                                try
                                {
                                    if (rw["F12"] != System.DBNull.Value)
                                    {
                                        HardReservedQuantity = rw["F12"].ToString();
                                    }
                                    else
                                    {
                                        HardReservedQuantity = " ";
                                    }

                                }
                                catch (Exception ex)
                                {

                                }
                                //Inserting into DB


                                strSQLstring = "INSERT INTO SDIX_BACKORDER_REPORT_LOG (STORE_ROOM,STOCK_TYPE,ITEM_ID,ITEM_DESC,ISSUE_UNIT,QUTY_RESERV,LASTUPDDTTM";
                                if (LastIssueDate != "")
                                {
                                    strSQLstring = strSQLstring + ",LASTISSUE_DATE ) " + System.Environment.NewLine;

                                }
                                else
                                {
                                    strSQLstring = strSQLstring + ") " + System.Environment.NewLine;

                                }
                                if (LastIssueDate != "")
                                {
                                    strSQLstring= strSQLstring +"VALUES('" + StoreRoom + "', '" + StockCatagory + "', '" + Item + "', '" + ItemDescription + "', '" + IssueUnit + "','" + HardReservedQuantity + "',SYSDATE,TO_DATE('" + LastIssueDate + "', 'MM/DD/YYYY HH:MI:SS AM'))";

                                }
                                else
                                {
                                    strSQLstring = strSQLstring + "VALUES('" + StoreRoom + "', '" + StockCatagory + "', '" + Item + "', '" + ItemDescription + "', '" + IssueUnit + "','" + HardReservedQuantity + "',SYSDATE)";

                                }
                                com = new OleDbCommand(strSQLstring, cn);
                                rowsaffected = com.ExecuteNonQuery();
                            }
                        }
                    }

                }
                cn.Close();
                cn.Dispose();
                reslt = true;

                return reslt;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Boolean SendErrorEmail(Exception exception, string strMessage, string InnerExcp, string MethodName)
        {
            string strbodyhead;
            string strbodydetl = string.Empty;
            Boolean isEmailSent = false;
            SDIEmailUtility.EmailServices SDIEmailService = new SDIEmailUtility.EmailServices();
            String DbUrl = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper();

            try
            {
                MailMessage Mailer = new MailMessage();
                string FromAddress = "SDIExchADMIN@SDI.com";
                string Mailcc = "";
                string MailBcc = "webdev@sdi.com;";

                StackTrace trace = new StackTrace(exception, true);
                string fileName = trace.GetFrame(0).GetFileName();
                int lineNo = trace.GetFrame(0).GetFileLineNumber();


                strbodydetl = strbodydetl + "<div>";
                strbodydetl = strbodydetl + "<p >" + strMessage + "... ";
                strbodydetl = strbodydetl + "&nbsp;<BR>";

                strbodydetl = strbodydetl + "Method Name :<span>    </span>" + MethodName + "<BR>";
                strbodydetl = strbodydetl + "Error Message :<span>    </span>" + strMessage + "<BR>";
                strbodydetl = strbodydetl + "Inner Exception :<span>    </span>" + InnerExcp + "<BR>";
                strbodydetl = strbodydetl + "Error Line No :<span>    </span>" + lineNo + "<BR>";
                strbodydetl = strbodydetl + "Date:<span>    </span>" + DateTime.Now + "<BR>";
                strbodydetl = strbodydetl + "&nbsp;<br>";
                strbodydetl = strbodydetl + "&nbsp;</p>";
                strbodydetl = strbodydetl + "</div>";

                Mailer.Body = strbodydetl;

                if (DbUrl == "PLGR" | DbUrl == "STAR" | DbUrl == "DEVL" | DbUrl == "RPTG")
                {
                    Mailer.To.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.To.Add(new MailAddress("avacorp@sdi.com"));
                    Mailer.Subject = "<<TEST SITE>> Receipts Process Error ";
                }
                else
                {
                    Mailer.To.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.Subject = "Receipts Process Error";
                }

                OleDbConnection connectionEmail = new OleDbConnection((Convert.ToString(ConfigurationManager.AppSettings["ConString"])));
                string[] MailAttachmentName = new string[0];
                List<byte[]> MailAttachmentbytes = new List<byte[]>();
                try
                {
                    SDIEmailService.EmailUtilityServices("MailandStore", "SDIExchange@SDI.com", Mailer.To.ToString(), Mailer.Subject, string.Empty, string.Empty, Mailer.Body, "SDIERRMAIL", MailAttachmentName, MailAttachmentbytes.ToArray());
                }
                catch (Exception ex)
                {
                    string strErr = ex.Message;
                }

                try
                {
                    try
                    {
                        connectionEmail.Close();
                        isEmailSent = true;
                    }
                    catch (Exception)
                    {

                        isEmailSent = false;
                    }
                }
                catch (Exception)
                {

                    connectionEmail.Close();
                    isEmailSent = false;
                }

            }
            catch (Exception)
            {

                isEmailSent = false;
            }
            return isEmailSent;
        }
    }
}
