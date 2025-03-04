﻿using Microsoft.Exchange.WebServices.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net.Mail;
using System.Data.OleDb;
using System.Data;
using System.Diagnostics;
using Task = System.Threading.Tasks.Task;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmailToReceipt
{
    class ProcessMail
    {
       
        public static void Main()
        {
            EmailToReceiptProcess().Wait(); //Mythili -- INC0012719 change from basic authentication to OAuth
        }
        
        public partial class ResponseBO
        {
            public string access_token { get; set; }
        }
        
        public async static Task EmailToReceiptProcess()
        {
            try
            {
                //string UN = ConfigurationManager.AppSettings["UserName"];
                //string Pwd = ConfigurationManager.AppSettings["Password"];
                string OutlookURL = ConfigurationManager.AppSettings["OutlookURL"];
                string ProcessedFolderName = ConfigurationManager.AppSettings["ProcessedFolderName"];
                string DestinationPath = ConfigurationManager.AppSettings["DestinationPath"];
                string EmailIDAccount = ConfigurationManager.AppSettings["MailboxEmailID"];
                
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                appPath = appPath.Substring(0, appPath.LastIndexOf("bin"));

                StreamWriter log;
                FileStream fileStream = null;
                DirectoryInfo logDirInfo = null;
                FileInfo logFileInfo;

                string logpath = string.Empty;

                string logFilePath = appPath + @"Logs\ReceiptsProcessLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
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

                try
                {
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

                    log.WriteLine("Access token has been fetched.");

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
                        int attchcount = message.Attachments.Count();
                        int attachmentMoved = 0;
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
                                            log.WriteLine("Number of line to be processed: " + dt.Rows.Count);
                                            //Converting into CSV file
                                            StreamWriter CSV_log;
                                            FileStream CSV_fileStream = null;
                                            DirectoryInfo CSV_logDirInfo = null;
                                            FileInfo CSV_logFileInfo;

                                            string CSV_FilePath = appPath + @"ConvertedFile\" + csvfilename;
                                            CSV_logFileInfo = new FileInfo(CSV_FilePath);
                                            CSV_logDirInfo = new DirectoryInfo(CSV_logFileInfo.DirectoryName);

                                            if (!CSV_logDirInfo.Exists) CSV_logDirInfo.Create();
                                            if (!CSV_logDirInfo.Exists)
                                            {
                                                CSV_fileStream = CSV_logFileInfo.Create();
                                            }
                                            else
                                            {
                                                CSV_fileStream = new FileStream(CSV_FilePath, FileMode.Append);
                                            }
                                            CSV_log = new StreamWriter(CSV_fileStream);

                                            string strContents = string.Empty;
                                            strContents = DataTableToCSV(dt, '^');

                                            CSV_log.WriteLine(strContents);
                                            CSV_log.Close();

                                            if (System.IO.File.Exists(appPath + @"ConvertedFile\" + csvfilename))
                                            {
                                                log.WriteLine("Converted the attachment file into " + csvfilename);
                                            }
                                            else
                                            {
                                                log.WriteLine("Error in Converting the attachment file into " + csvfilename);
                                                log.WriteLine("-------------------------------------------------------------");
                                                break;
                                            }

                                            //Inserting into DB
                                            Boolean strrslt = false;
                                            strrslt = InsertTbl(dt);
                                            if (strrslt)
                                            {
                                                log.WriteLine("Receipts records inserted into the receipt log table.");
                                            }
                                            else
                                            {
                                                log.WriteLine("Error while inserting the records into the receipt log table.");
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
                                    System.IO.File.Copy(appPath + @"ConvertedFile\" + csvfilename, DestinationPath + csvfilename);
                                    if (System.IO.File.Exists(DestinationPath + csvfilename))
                                    {
                                        log.WriteLine("Moved the " + csvfilename + " file to destination VM");
                                    }
                                    else
                                    {
                                        log.WriteLine("Error in the Moving the " + csvfilename + " file to destination VM");
                                        log.WriteLine("-------------------------------------------------------------");
                                        break;
                                    }

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
                        //Email moved to Processed folder
                        if (attachmentMoved > 0)
                        {
                            item.Move(processedFolderID);
                            log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                            log.WriteLine("-------------------------------------------------------------");
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
                    string exception = ex.Message;
                    log.WriteLine("Error:" + exception);
                    log.Close();
                    SendErrorEmail(ex, exception, Convert.ToString(ex.InnerException), "Main");
                }
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
                SendErrorEmail(ex, exception, Convert.ToString(ex.InnerException), "Main");
            }
        }

        private static SearchFilter SetFilter()
        {
            string FilterEmailWithSubject = ConfigurationManager.AppSettings["FilterEmailSubject"];
            List<SearchFilter> searchFilterCollection = new List<SearchFilter>();
            searchFilterCollection.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.Subject, FilterEmailWithSubject));
            //searchFilterCollection.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.Subject, FilterEmailWithSubject.ToLower()));         
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

        private static Boolean InsertTbl(DataTable dt)
        {
            Boolean reslt = false;
            try
            {
                string PO_ID = string.Empty;
                string PO_Desc = string.Empty;
                string PO_LINE_NO = string.Empty;
                string Receipt_ID = string.Empty;
                string Qty_Requested = string.Empty;
                string Qty_Received = string.Empty;
                string Item_ID = string.Empty;
                string StoreRoom_LOC = string.Empty;
                string BIN_LOC = string.Empty;
                string Ord_date = string.Empty;
                string Transac_date = string.Empty;
                string Transac_Type = string.Empty;

                int rowsaffected;
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();

                foreach (DataRow rw in dt.Rows)
                {
                    try
                    {
                        PO_ID = rw["PO"].ToString();
                        PO_Desc = rw["PO Description"].ToString();
                        PO_LINE_NO = rw["PO Line"].ToString();
                        Receipt_ID = rw["Receipt ID"].ToString(); ;
                        Qty_Requested = rw["Quantity Requested"].ToString(); ;
                        Qty_Received = rw["Quantity Received (Converted)"].ToString();
                        Item_ID = rw["Item"].ToString(); ;
                        if (Item_ID.Trim() == "")
                        {
                            Item_ID = " ";
                        }
                        StoreRoom_LOC = rw["To Location (Storeroom)"].ToString(); ;
                        if (StoreRoom_LOC.Trim() == "")
                        {
                            StoreRoom_LOC = " ";
                        }
                        BIN_LOC = rw["To Bin"].ToString();
                        if (BIN_LOC.Trim() == "")
                        {
                            BIN_LOC = " ";
                        }
                        Ord_date = Convert.ToDateTime(rw["Ordered Date"]).ToString("MM/dd/yyyy");
                        Transac_date = Convert.ToDateTime(rw["Transaction Date"]).ToString("MM/dd/yyyy");
                        Transac_Type = rw["Transaction Type"].ToString();

                        string strSQLstring = "INSERT INTO SDIX_RECEIPTS_LOG_TBL (PO_ID, PO_DESC, PO_LINE_NO, RECEIPT_ID, INV_ITEM_ID, QTY_REQUESTED, QTY_RECEIVED, STOREROOM_LOC, BIN_LOC, ORD_DTTM, TRANSAC_DTTM, TRANSAC_TYPE, LASTUPDDTTM) " + System.Environment.NewLine +
                                                "VALUES('" + PO_ID + "', '" + PO_Desc + "', " + PO_LINE_NO + ", '" + Receipt_ID + "', '" + Item_ID + "', " + Qty_Requested + ", " + Qty_Received + ", '" + StoreRoom_LOC + "', '" + BIN_LOC + "', TO_DATE('" + Ord_date + "', 'mm/dd/yyyy'), TO_DATE('" + Transac_date + "','mm/dd/yyyy'), '" + Transac_Type + "', SYSDATE)";

                        com = new OleDbCommand(strSQLstring, cn);
                        rowsaffected = com.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string exception = ex.Message;
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
                string MailBcc = "webdev@sdi.com;yury.arkadin@sdi.com";

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
                    SDIEmailService.EmailUtilityServices("MailandStore", "SDIExchange@SDI.com", Mailer.To.ToString(), Mailer.Subject, string.Empty, MailBcc, Mailer.Body, "SDIERRMAIL", MailAttachmentName, MailAttachmentbytes.ToArray());
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
