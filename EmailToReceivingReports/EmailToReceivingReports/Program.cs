using Microsoft.Exchange.WebServices.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Http;
using System.Web;
using System.Net.Mail;
using System.Data.OleDb;
//using Spire.Xls;
//using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using System.Diagnostics;
using System.Net;

namespace EmailToReceivingReports
{
    class Program
    {
        static void Main(string[] args)
        {
            BackOrderReportProcess();
           StoreRoomOrderReportProcess();

        }

        private static void StoreRoomOrderReportProcess()
        {
            string UN = ConfigurationManager.AppSettings["UserName"];
            string Pwd = ConfigurationManager.AppSettings["Password"];
            string OutlookURL = ConfigurationManager.AppSettings["OutlookURL"];
            string ProcessedFolderName = ConfigurationManager.AppSettings["ProcessedFolderName"];
            string DestinationPath = ConfigurationManager.AppSettings["DestinationPath"];
            string EmailIDAccount = ConfigurationManager.AppSettings["MailboxEmailID"];
            string strBackOrderSubject1 = ConfigurationManager.AppSettings["FilterStoreRoomReport"];
            string strBackOrderSubject2 = ConfigurationManager.AppSettings["FilterStoreRoomReport2"];

            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            appPath = appPath.Substring(0, appPath.LastIndexOf("bin"));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            try
            {
                ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
                service.Credentials = new WebCredentials(UN, Pwd);
                service.Url = new Uri(OutlookURL);

                FindFoldersResults moveToFolder = null;
                FolderView fv = new FolderView(999);
                Mailbox mailBoxToProcess = new Mailbox(EmailIDAccount);
                FolderId processedFolderID = null;

                FolderId fidProcessedParent = new FolderId(WellKnownFolderName.Inbox, mailBoxToProcess);
                ItemView view = new ItemView(int.MaxValue);

                string logpath = string.Empty;

                string logFilePath = appPath + @"Logs\StoreRoomOrderReportProcessLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
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

                //Get the filtered Email With Subject in Inbox
                FindItemsResults<Item> findResults = service.FindItems(fidProcessedParent, SetFilter(strBackOrderSubject1, strBackOrderSubject2), view);
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
                                        //Converting into CSV file
                                        //StreamWriter CSV_log;
                                        //FileStream CSV_fileStream = null;
                                        //DirectoryInfo CSV_logDirInfo = null;
                                        //FileInfo CSV_logFileInfo;

                                        //string CSV_FilePath = appPath + @"ConvertedFile\" + csvfilename;
                                        //CSV_logFileInfo = new FileInfo(CSV_FilePath);
                                        //CSV_logDirInfo = new DirectoryInfo(CSV_logFileInfo.DirectoryName);

                                        //if (!CSV_logDirInfo.Exists) CSV_logDirInfo.Create();
                                        //if (!CSV_logDirInfo.Exists)
                                        //{
                                        //    CSV_fileStream = CSV_logFileInfo.Create();
                                        //}
                                        //else
                                        //{
                                        //    CSV_fileStream = new FileStream(CSV_FilePath, FileMode.Append);
                                        //}
                                        //CSV_log = new StreamWriter(CSV_fileStream);

                                        //string strContents = string.Empty;
                                        //strContents = DataTableToCSV(dt, '^');

                                        //CSV_log.WriteLine(strContents);
                                        //CSV_log.Close();

                                        //if (System.IO.File.Exists(appPath + @"ConvertedFile\" + csvfilename))
                                        //{
                                        //    log.WriteLine("Converted the attachment file into " + csvfilename);
                                        //}
                                        //else
                                        //{
                                        //    log.WriteLine("Error in Converting the attachment file into " + csvfilename);
                                        //    log.WriteLine("-------------------------------------------------------------");
                                        //    break;
                                        //}

                                        //Inserting into DB
                                        Boolean strrslt = false;
                                        strrslt = InsertStoreRoomOrderReport(dt);
                                        if (strrslt)
                                        {
                                            log.WriteLine("Store room order report records inserted into the receipt log table.");
                                        }
                                        else
                                        {
                                            log.WriteLine("Error while inserting the records into the store room order report log table.");
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
                SendErrorEmail(ex, ex.Message, Convert.ToString(ex.InnerException), "Main");
            }
        }

        private static void BackOrderReportProcess() {
            string UN = ConfigurationManager.AppSettings["UserName"];
            string Pwd = ConfigurationManager.AppSettings["Password"];
            string OutlookURL = ConfigurationManager.AppSettings["OutlookURL"];
            string ProcessedFolderName = ConfigurationManager.AppSettings["ProcessedFolderName"];
            string DestinationPath = ConfigurationManager.AppSettings["DestinationPath"];
            string EmailIDAccount = ConfigurationManager.AppSettings["MailboxEmailID"];
            string strBackOrderSubject = ConfigurationManager.AppSettings["FilterBackOrderReport"];
            string strBackOrderSubject2 = ConfigurationManager.AppSettings["FilterBackOrderReport2"];

            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            appPath = appPath.Substring(0, appPath.LastIndexOf("bin"));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            try
            {
                ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
                service.Credentials = new WebCredentials(UN, Pwd);
                service.Url = new Uri(OutlookURL);

                FindFoldersResults moveToFolder = null;
                FolderView fv = new FolderView(999);
                Mailbox mailBoxToProcess = new Mailbox(EmailIDAccount);
                FolderId processedFolderID = null;

                FolderId fidProcessedParent = new FolderId(WellKnownFolderName.Inbox, mailBoxToProcess);
                ItemView view = new ItemView(int.MaxValue);

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

                //Get the filtered Email With Subject in Inbox
                FindItemsResults<Item> findResults = service.FindItems(fidProcessedParent, SetFilter(strBackOrderSubject, strBackOrderSubject2), view);
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
                                        //Converting into CSV file
                                        //StreamWriter CSV_log;
                                        //FileStream CSV_fileStream = null;
                                        //DirectoryInfo CSV_logDirInfo = null;
                                        //FileInfo CSV_logFileInfo;

                                        //string CSV_FilePath = appPath + @"ConvertedFile\" + csvfilename;
                                        //CSV_logFileInfo = new FileInfo(CSV_FilePath);
                                        //CSV_logDirInfo = new DirectoryInfo(CSV_logFileInfo.DirectoryName);

                                        //if (!CSV_logDirInfo.Exists) CSV_logDirInfo.Create();
                                        //if (!CSV_logDirInfo.Exists)
                                        //{
                                        //    CSV_fileStream = CSV_logFileInfo.Create();
                                        //}
                                        //else
                                        //{
                                        //    CSV_fileStream = new FileStream(CSV_FilePath, FileMode.Append);
                                        //}
                                        //CSV_log = new StreamWriter(CSV_fileStream);

                                        //string strContents = string.Empty;
                                        //strContents = DataTableToCSV(dt, '^');

                                        //CSV_log.WriteLine(strContents);
                                        //CSV_log.Close();

                                        //if (System.IO.File.Exists(appPath + @"ConvertedFile\" + csvfilename))
                                        //{
                                        //    log.WriteLine("Converted the attachment file into " + csvfilename);
                                        //}
                                        //else
                                        //{
                                        //    log.WriteLine("Error in Converting the attachment file into " + csvfilename);
                                        //    log.WriteLine("-------------------------------------------------------------");
                                        //    break;
                                        //}

                                        //Inserting into DB
                                        Boolean strrslt = false;
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
                SendErrorEmail(ex, ex.Message, Convert.ToString(ex.InnerException), "Main");
            }
        }

        private static SearchFilter SetFilter(string strSubject1,string strSubject2)
        {
            string FilterEmailWithSubject = strSubject1;
            List<SearchFilter> searchFilterCollection = new List<SearchFilter>();
            searchFilterCollection.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.Subject, strSubject1));
            searchFilterCollection.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.Subject, strSubject2));
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

        private static Boolean InsertStoreRoomOrderReport(DataTable dt)
        {
            Boolean reslt = false;
            try
            {
                string PO = string.Empty;
                string POLine = string.Empty;
                string RequestedBy = string.Empty;
                string Item = string.Empty;
                string Description = string.Empty;
                string Manufacturer = string.Empty;
                string Model = string.Empty;
                string UnitCost = string.Empty;
                string PODescription = string.Empty;
                string POType = string.Empty;
                string Status = string.Empty;
                string StatusDate = string.Empty;
                string company = string.Empty;
              
              

                int rowsaffected;
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();

                //deleting unwanted rows
                dt.Rows[0].Delete();
                dt.Rows[1].Delete();
                dt.Rows[2].Delete();
               // dt.Rows[3].Delete();
                dt.AcceptChanges();

                string ParentOrder = "";

                Boolean isExit = false;

                foreach (DataRow rw in dt.Rows)
                {
                    string currentOrder = "";
                    string ReqBy = "";
                    if (!isExit)
                    {

                        if (rw["STOREROOM ORDER LINE ITEM QUEUE"] != System.DBNull.Value)
                        {
                            currentOrder = rw["STOREROOM ORDER LINE ITEM QUEUE"].ToString().Trim();
                            ReqBy = rw["F2"].ToString().Trim();
                            if (ReqBy == "Summary")
                            {
                                isExit = true;
                                currentOrder = "";
                                PO = "";
                            }
                            else if (currentOrder == "")
                            {
                                currentOrder = ParentOrder;
                                PO = currentOrder;
                            }
                            else if (currentOrder == ParentOrder)
                            {
                                PO = "";
                            }
                            else
                            {
                                ParentOrder = currentOrder;
                                PO = currentOrder;
                            }
                        }
                        else
                        {
                            PO = ParentOrder;
                        }


                        if (PO != "")
                        {
                            // StoreRoom = rw["Stockroom Backorder Report"].ToString();
                            POLine = rw["F3"].ToString();
                            Item = rw["F4"].ToString();
                            Description = rw["F8"].ToString();

                            if (Description.Contains("'"))
                            {
                                Description = Description.Replace("'", "''");
                            }
                            RequestedBy = rw["F2"].ToString();                        
                            Manufacturer = rw["F5"].ToString();
                            Model = rw["F6"].ToString();
                            UnitCost = rw["F7"].ToString();
                            PODescription = rw["F9"].ToString();

                            if (PODescription.Contains("'"))
                            {
                                PODescription = PODescription.Replace("'", "''");
                            }

                            POType = rw["F10"].ToString();
                            Status = rw["F11"].ToString(); // Convert.ToDateTime(rw["Ordered Date"]).ToString("MM/dd/yyyy");
                            StatusDate = Convert.ToDateTime(rw["F12"]).ToString("MM/dd/yyyy"); ;
                            company = rw["F13"].ToString();


                            string strSQLstring = "INSERT INTO SDIX_STORROOM_ORD_REPORT_LOG (PO_ID,PO_LINE, REQUESTED_BY , ITEM_ID,ITEM_DESC ,MFG_ID ,ITEM_MODEL ,UNIT_COST,PO_DESC ,PO_TYPE,STATUS,STATUS_DATE , COMPANY_ID,LASTUPDDTTM ) " + System.Environment.NewLine +
                                              "VALUES('" + PO + "', " + POLine + ", '" + RequestedBy + "', '" + Item + "', '" + Description + "','" + Manufacturer + "', '" + Model + "', '" + UnitCost + "', '" + PODescription + "', '" + POType + "','" + Status + "',TO_DATE('" + StatusDate + "', 'mm/dd/yyyy'),'" + company + "',SYSDATE)";

                            com = new OleDbCommand(strSQLstring, cn);
                            rowsaffected = com.ExecuteNonQuery();
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

        private static Boolean InsertBackOrderReport(DataTable dt)
        {
            Boolean reslt = false;
            try
            {
                string StoreRoom = string.Empty;
                string StockCatagory = string.Empty;
                string Item = string.Empty;
                string ItemDescription = string.Empty;
                string IssueUnit = string.Empty;
                string CurrentBalance = string.Empty;
                string HardReservedQuantity = string.Empty;
                string Model = string.Empty;
                string PackSize = string.Empty;
                string PrimaryVendor = string.Empty;
                string Buyer = string.Empty;
                //
               // string Transac_Type = string.Empty;


                int rowsaffected;
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();

                  //deleting unwanted rows
                   dt.Rows[0].Delete();
                   dt.Rows[1].Delete();
                   dt.Rows[2].Delete();
                   dt.Rows[3].Delete();
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
                            if (currentOrder == "Summary") {
                                isExit = true;
                                currentOrder = "";
                                StoreRoom = "";
                            }
                            else if (currentOrder == "")
                            {
                                currentOrder = ParentOrder;
                                StoreRoom = currentOrder;
                            }
                            else if (currentOrder == ParentOrder) {
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


                        if (StoreRoom != "")
                        {
                           // StoreRoom = rw["Stockroom Backorder Report"].ToString();
                            StockCatagory = rw["F2"].ToString();
                            Item = rw["F3"].ToString();
                            ItemDescription = rw["F4"].ToString();

                            if (ItemDescription.Contains("'")) {
                                ItemDescription = ItemDescription.Replace("'", "''");
                            }
                            IssueUnit = rw["F5"].ToString();
                            CurrentBalance = rw["F6"].ToString();
                            HardReservedQuantity = rw["F7"].ToString();
                            Model = rw["F8"].ToString();
                            PackSize = rw["F9"].ToString();
                            PrimaryVendor = rw["F10"].ToString();
                            Buyer = rw["F11"].ToString();

                            string strSQLstring = "INSERT INTO SDIX_BACKORDER_REPORT_LOG (STORE_ROOM,STOCK_TYPE,ITEM_ID,ITEM_DESC,ISSUE_UNIT,QUTY_BAL,QUTY_RESERV,ITEM_MODEL,PACK_SIZE,PRIMARY_VENDR,BUYER,LASTUPDDTTM ) " + System.Environment.NewLine +
                                              "VALUES('" + StoreRoom + "', '" + StockCatagory + "', " + Item + ", '" + ItemDescription + "', '" + IssueUnit + "','" + CurrentBalance + "', " + HardReservedQuantity + ", '" + Model + "', '" + PackSize + "', '" + PrimaryVendor + "','" + Buyer + "',SYSDATE)";

                            com = new OleDbCommand(strSQLstring, cn);
                            rowsaffected = com.ExecuteNonQuery();
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
