using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Microsoft.Exchange.WebServices.Data;
using System.Data;
using System.Data.OleDb;
using System.Drawing;

namespace NYCShipmentProcess
{
    public class Program
    {

        static void Main(string[] args)
        {
            try
            {
                getNYCShipmentMailReport();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void getNYCShipmentMailReport()
        {
            try
            {
                Console.WriteLine("NYC Shipment: Utility Started");
                string ProcessedFolderName = ConfigurationManager.AppSettings["ProcessedFolderName"];
                string UN = ConfigurationManager.AppSettings["UserName"];
                string Pwd = ConfigurationManager.AppSettings["Password"];
                string OutlookURL = ConfigurationManager.AppSettings["OutlookURL"];
                string strBackOrderSubject = ConfigurationManager.AppSettings["NYCMail"];
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                appPath = appPath.Substring(0, appPath.LastIndexOf("bin"));
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                StreamWriter log;
                FileStream fileStream = null;
                DirectoryInfo logDirInfo = null;
                FileInfo logFileInfo;
                try
                {
                    DirectoryInfo di = new DirectoryInfo(appPath + @"AttachmentFile\");
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
                    service.Credentials = new WebCredentials(UN, Pwd);
                    service.Url = new Uri(OutlookURL);
                    FindFoldersResults moveToFolder = null;
                    FolderView fv = new FolderView(999);
                    Mailbox mailBoxToProcess = new Mailbox(UN);
                    FolderId processedFolderID = null;

                    FolderId fidProcessedParent = new FolderId(WellKnownFolderName.Inbox, mailBoxToProcess);
                    ItemView view = new ItemView(int.MaxValue);

                    string logpath = string.Empty;
                    string logFilePath = appPath + @"Logs\NYCShipmentProcessLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
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
                    FindItemsResults<Item> findResults = service.FindItems(fidProcessedParent, SetFilter(strBackOrderSubject), view);
                    log.WriteLine("Total Emails with filtered subject name is " + findResults.Items.Count() + ".");
                    Console.WriteLine("Total Emails with filtered subject name is " + findResults.Items.Count() + ".");
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
                        DateTime mailDate = item.DateTimeReceived.Date;
                        DateTime currentDate = DateTime.UtcNow.Date;

                        if (mailDate == currentDate)
                        {
                            foreach (Attachment attch in message.Attachments)
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
                                    try
                                    {
                                        csvfilename = "Receipts_" + String.Format("" + DateTime.Now.ToString("MMddyyyy_hhmmss")).ToString() + "" + PreventDuplicationoffiles + ".csv";
                                        log.WriteLine("");
                                        log.WriteLine("Begin Processing the Email from : " + from_name + " for File name " + fileAttachment.Name + "");
                                        fileAttachment.Load(appPath + @"AttachmentFile\" + fileAttachment.Name);
                                        if (System.IO.File.Exists(appPath + @"AttachmentFile\" + fileAttachment.Name))
                                        {
                                            log.WriteLine("Downloaded the attachment file " + fileAttachment.Name);
                                            Console.WriteLine("Downloaded the attachment file " + fileAttachment.Name);
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
                                                Console.WriteLine("Started to insert the report");
                                                log.WriteLine("Started to insert the report");
                                                bool result = false;
                                                result = InsertNYCOrders(dt, log);//Inserting into DB
                                                if (result)
                                                {
                                                    Console.WriteLine("Insert Completed Successfully");
                                                    log.WriteLine("NYC Report Inserted Successfully.");
                                                    ProcessNYCOrders(log);
                                                    item.Move(processedFolderID);
                                                    log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                                                }
                                                else
                                                {
                                                    log.WriteLine("Error while inserting the records");
                                                }
                                            }

                                        }
                                        else
                                        {
                                            Console.WriteLine("No data Found in Excel");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.WriteLine("ERROR: " + ex.Message + ". Inner Exception : " + Convert.ToString(ex.InnerException));
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                            }

                        }
                        else
                        {
                            Console.WriteLine("Email Not Found");
                        }
                    }
                    log.WriteLine("-------------------------------------------------------------");
                    log.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
                Console.WriteLine("Utility Completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static DataTable ReadExcel(string UploadPath, StreamWriter log)
        {
            DataTable csvData = new DataTable();
            csvData.Columns.Add("OrderNO");
            csvData.Columns.Add("SchoolID");
            csvData.Columns.Add("ItemID");
            csvData.Columns.Add("ItemDesc");
            csvData.Columns.Add("UOM");
            csvData.Columns.Add("ORDQTY");
            csvData.Columns.Add("RCVQTY");

            string conString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 8.0';", UploadPath);
            try
            {
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
                    // 'deleting unwanted rows

                    ds.Tables[0].Rows[0].Delete();
                    ds.Tables[0].Rows[1].Delete();
                    ds.Tables[0].Rows[2].Delete();
                    ds.Tables[0].Rows[3].Delete();
                    // 'deleting unwanted columns
                    ds.Tables[0].Columns.Remove("F2");
                    ds.Tables[0].Columns.Remove("F4");
                    ds.Tables[0].Columns.Remove("F5");
                    ds.Tables[0].Columns.Remove("F6");
                    ds.Tables[0].Columns.Remove("F7");
                    ds.Tables[0].Columns.Remove("F8");
                    ds.Tables[0].Columns.Remove("F9");
                    ds.Tables[0].Columns.Remove("F15");
                    ds.Tables[0].Columns.Remove("F16");
                    ds.AcceptChanges();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                        csvData.Rows.Add(dr.ItemArray);
                    ds.Tables.Add(csvData);

                    int I = 0;
                    string strOrderNo = "";
                    string strSchoolID = "";
                    string NYCOrderNo = "";
                    string NYCSchoolID = "";
                    List<DataRow> rowsWantToDelete = new List<DataRow>();

                    if (csvData != null)
                    {
                        if (csvData.Rows.Count > 0)
                        {
                            for (I = 0; I <= csvData.Rows.Count - 1; I++)
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(csvData.Rows[I]["OrderNO"])))
                                {
                                    strOrderNo = Convert.ToString(csvData.Rows[I]["OrderNO"]);
                                    if (strOrderNo.Trim() == "")
                                    {
                                        if (NYCOrderNo != "")
                                        {
                                            strOrderNo = NYCOrderNo;
                                            strSchoolID = NYCSchoolID;
                                            csvData.Rows[I]["OrderNO"] = strOrderNo;
                                        }
                                    }
                                    else
                                    {
                                        strOrderNo = Convert.ToString(csvData.Rows[I]["OrderNO"]);

                                        if (!strOrderNo.Contains("Total"))
                                        {
                                            strSchoolID = Convert.ToString(csvData.Rows[I]["SchoolID"]);
                                            NYCOrderNo = strOrderNo;
                                            NYCSchoolID = strSchoolID;
                                        }
                                    }
                                }
                                else
                                {
                                    strOrderNo = NYCOrderNo;
                                    strSchoolID = NYCSchoolID;
                                    csvData.Rows[I]["OrderNO"] = strOrderNo;
                                    csvData.Rows[I]["SchoolID"] = strSchoolID;
                                }

                                if (strOrderNo.Contains("WO") | strOrderNo.Contains("Total"))
                                {
                                    DataRow rowD = csvData.Rows[I];
                                    rowsWantToDelete.Add(rowD);
                                }
                            }
                            csvData.AcceptChanges();

                            foreach (DataRow dr in rowsWantToDelete)
                                csvData.Rows.Remove(dr);

                            csvData.AcceptChanges();
                        }
                    }
                    oledbConn.Dispose();
                    oledbConn.Close();

                    log.WriteLine("Readed the excel datas successfully.");
                    return csvData;
                }
                catch (Exception ex)
                {
                    oledbConn.Dispose();
                    oledbConn.Close();
                    log.WriteLine("Error While reading the Excel File : " + ex.Message);
                    Console.WriteLine("Error While reading the Excel File : " + ex.Message);
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("Error While reading the Excel File : " + ex.Message);
                Console.WriteLine("Error While reading the Excel File : " + ex.Message);
                return null;
            }
        }

        private static Boolean InsertNYCOrders(DataTable csvData, StreamWriter log)
        {
            string OrderNO = string.Empty;
            string ItemID = string.Empty;
            string ItemDesc = string.Empty;
            string UOM = string.Empty;
            string ORDQTY = string.Empty;
            string RCVQTY = string.Empty;
            string SchoolID = string.Empty;
            int rowsaffected;
            int i = 0;
            Boolean result = false;
            try
            {
                log.WriteLine("Overall NYC Shipment Count: " + csvData.Rows.Count);
                Console.WriteLine("Overall NYC Shipment Count: " + csvData.Rows.Count);
                foreach (DataRow rows in csvData.Rows)
                {
                    i = i + 1;
                    OrderNO = Convert.ToString(rows[0]);
                    SchoolID = Convert.ToString(rows[1]);
                    ItemID = Convert.ToString(rows[2]);
                    ItemDesc = Convert.ToString(rows[3]);
                    UOM = Convert.ToString(rows[4]);
                    ORDQTY = Convert.ToString(rows[5]);
                    RCVQTY = Convert.ToString(rows[6]);

                    OrderNO = OrderNO.Substring(1);

                    if (ORDQTY.Trim() == "")
                        ORDQTY = "0";

                    if (RCVQTY.Trim() == "")
                        RCVQTY = "0";

                    if (ItemDesc.Contains("'"))
                        ItemDesc = ItemDesc.Replace("'", "''");

                    if (ItemID.Contains("NA") | ItemID.Contains(@"N\A") | ItemID.Trim() == "")
                        ItemID = " ";

                    if (!OrderNO.Contains("WO") & !OrderNO.Contains("W"))
                    {
                        string strSQLstring = "INSERT INTO SDIX_ISA_SHIP_VALIDATION (ORDER_NO,SCHOOL_ID,ITEM_ID,ITEM_DESC,UOM,QTY_PO,NYC_Received,PROCESS_FLAG,LASTUPDOPRID,LASTUPDDTTM	)" + System.Environment.NewLine +
                                                               " VALUES('" + OrderNO.Trim() + "', '" + SchoolID.Trim() + "', '" + ItemID + "', '" + ItemDesc.Trim() + "', '" + UOM.Trim() + "', '" + ORDQTY.Trim() + "','" + RCVQTY.Trim() + "','N','" + "Avacorp" + "',SYSDATE)";

                        rowsaffected = ExecNonQuery(strSQLstring);
                    }
                }
                result = true;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Inserting the data" + ex.Message);
                log.WriteLine("Error in Inserting the data" + ex.Message);
                return false;
            }
        }

        public static int ExecNonQuery(string p_strQuery, bool bGoToErrPage = true, bool bSendEmail = true)
        {
            int rowsAffected = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            OleDbConnection connection = new OleDbConnection(connectionString);

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
                    throw ex;
                }
                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception objException)
            {
                Console.WriteLine(objException.Message);
            }
            return rowsAffected;
        }

        public static DataSet GetAdapter(string p_strQuery)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            OleDbConnection connection = new OleDbConnection(connectionString);
            DataSet UserdataSet = new DataSet();
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
                    throw ex;
                }
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception objException)
            {
                Console.WriteLine(objException.Message);
            }
            return UserdataSet;
        }

        public static void updateShipmentValues(string strSchoolID, string strOrderNo, string strItemID)
        {
            string strSqlString;
            int rowAffected = 0;
            try
            {

                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection connection = new OleDbConnection(connectionString);
                strSqlString = "UPDATE SDIX_ISA_SHIP_VALIDATION SET PROCESS_FLAG = 'Y',LASTUPDOPRID='" + "avacorp" + "',LASTUPDDTTM = SYSDATE WHERE SCHOOL_ID = '" + strSchoolID + "' AND ORDER_NO='" + strOrderNo + "' AND ITEM_ID = '" + strItemID + "' AND PROCESS_FLAG = 'N'";

                OleDbCommand Command = new OleDbCommand(strSqlString, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                rowAffected = Command.ExecuteNonQuery();
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static DataSet getAccptRecvTotals(string poID, string poLine, string poBU)
        {
            DataSet dsRecvTotals = new DataSet();
            try
            {
                Int16 DateValidation = Convert.ToInt16(ConfigurationManager.AppSettings["DateValidation"]);
                string strSQLString;
                DataSet dsOrdLnItems = new DataSet();

                strSQLString = "SELECT TO_CHAR(RH.RECEIPT_DT,'MM/DD/YYYY') AS RECVDT, SUM(RCVSHIP.QTY_SH_NETRCV_VUOM) AS RECVDQTY ";
                strSQLString += "FROM sysadm8.PS_RECV_HDR RH, sysadm8.PS_RECV_LN_SHIP RCVSHIP  ";
                strSQLString += "WHERE '" + poBU + "' = RCVSHIP.BUSINESS_UNIT_PO AND '" + poID + "' = RCVSHIP.PO_ID AND RCVSHIP.LINE_NBR = '" + poLine + "'  ";
                strSQLString += "AND RCVSHIP.RECV_SHIP_STATUS <> 'X' AND RCVSHIP.BUSINESS_UNIT = RH.BUSINESS_UNIT  ";
                strSQLString += "AND RCVSHIP.RECEIVER_ID = RH.RECEIVER_ID  AND RH.RECEIPT_DT <= sysdate - " + DateValidation + " GROUP BY TO_CHAR(RH.RECEIPT_DT,'MM/DD/YYYY') ";
                dsRecvTotals = GetAdapter(strSQLString);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return dsRecvTotals;
        }

        private static DataSet getInvoiceAmount(string poID, string poLine, string poBU)
        {
            string strSQLString;
            DataSet dsInvoice = new DataSet();
            DataSet dsOrdLnItems = new DataSet();

            try
            {
                strSQLString = "SELECT D.GROSS_EXTENDED_AMT, D.INVOICE ";
                strSQLString += " FROM  SYSADM8.PS_ISA_INTG_BI A, SYSADM8.PS_INTFC_BI_CMP B, SYSADM8.PS_BI_LINE D";
                strSQLString += " WHERE A.BUSINESS_UNIT_PO = '" + poBU + "' AND A.PO_ID = '" + poID + "' AND A.LINE_NBR ='" + poLine + "' AND A.INTFC_ID  = B.INTFC_ID";
                strSQLString += " AND A.INTFC_LINE_NUM = B.INTFC_LINE_NUM AND   B.BUSINESS_UNIT = D.BUSINESS_UNIT AND B.INVOICE = D.INVOICE AND B.LINE_SEQ_NUM = D.LINE_SEQ_NUM";
                strSQLString += " AND D.ADD_DTTM > TO_TIMESTAMP('2017-07-01-00.06.00.000000','YYYY-MM-DD-HH24.MI.SS.FF')";

                dsInvoice = GetAdapter(strSQLString);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dsInvoice;
        }

        public static string checkREQOrderStatus(string strOrderNo, string strItemID)
        {
            string strSqlString;
            string orderStatus = "";

            try
            {
                strSqlString = "SELECT ISA_QUOTE_STATUS FROM PS_ISA_QUICK_REQ_L WHERE REQ_ID = '" + strOrderNo + "' AND VNDR_CATALOG_ID LIKE '%" + strItemID + "%'";
                orderStatus = GetScalar(strSqlString);

                if (orderStatus.Trim() != "R")
                    orderStatus = "N";
                else
                    orderStatus = "Y";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return orderStatus;
        }

        public static string GetScalar(string p_strQuery, bool bGoToErrPage = true)
        {

            string strReturn = "";
            string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            OleDbConnection connection = new OleDbConnection(connectionString);
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
                    throw ex32;
                }
                if (strReturn == null)
                    strReturn = "";
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex1)
                {
                    throw ex1;
                }
                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex2)
                {
                    throw ex2;
                }
            }
            catch (Exception objException)
            {
                strReturn = "";
                Console.WriteLine(objException.Message);
            }
            return strReturn;
        }

        public static void ProcessNYCOrders(StreamWriter log)
        {
            log.WriteLine("NYC Shipment Sending Mail Process Started");
            Console.WriteLine("NYC Shipment Sending Mail Process Started");
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            appPath = appPath.Substring(0, appPath.LastIndexOf("bin"));
            string strSqlString;
            DataSet unProSchds;
            DataSet unPrOrdds;
            DataSet OrdPODs;
            DataSet SDIRecDs;
            DataSet invoiceDS;

            string strbodyhead = "";
            string strbodydetl = "";
            string strSchoolID = "";
            string strNYCEmailID = "";
            string strOrderNo = "";
            string strItemID = "";
            string strNYCReceived = "";
            string strItemDesc = "";
            string strUOM = "";
            string strQTYORD = "";
            string MailFrom = "";
            string MailTo = "";
            string MailCc = " ";
            string MailBcc = " ";
            string MailSub = "";
            string MailBody = "";

            DataTable csvData;
            csvData = new DataTable();
            DataRow dr;
            csvData.Columns.Add("Order NO");
            csvData.Columns.Add("Item ID");
            csvData.Columns.Add("Item Description");
            csvData.Columns.Add("UOM");
            csvData.Columns.Add("Ordered QTY");
            csvData.Columns.Add("NYC Received");
            csvData.Columns.Add("SDI Received");

            // ' New datatable to show the list of item received is 0 in both SDi and NYC record

            DataTable ZeroItemDt;
            ZeroItemDt = new DataTable();

            ZeroItemDt.Columns.Add("School ID");
            ZeroItemDt.Columns.Add("Order NO");
            ZeroItemDt.Columns.Add("Line NO");
            ZeroItemDt.Columns.Add("Item ID");
            ZeroItemDt.Columns.Add("Item Description");
            ZeroItemDt.Columns.Add("UOM");
            ZeroItemDt.Columns.Add("Ordered QTY");
            ZeroItemDt.Columns.Add("NYC Received");
            ZeroItemDt.Columns.Add("SDI Received");
            ZeroItemDt.Columns.Add("Not Found Order");
            ZeroItemDt.Columns.Add("Cancel Order");

            // ' New datatable to show the summary list of NUC not certified items
            DataTable SummaryItemDt;
            SummaryItemDt = new DataTable();

            SummaryItemDt.Columns.Add("School ID");
            SummaryItemDt.Columns.Add("Order NO");
            SummaryItemDt.Columns.Add("Line NO");
            SummaryItemDt.Columns.Add("Item ID");
            SummaryItemDt.Columns.Add("Item Description");
            SummaryItemDt.Columns.Add("UOM");
            SummaryItemDt.Columns.Add("Ordered QTY");
            SummaryItemDt.Columns.Add("Invoice-No");
            SummaryItemDt.Columns.Add("Amount");
            SummaryItemDt.Columns.Add("NYC Received");
            SummaryItemDt.Columns.Add("SDI Received");
            SummaryItemDt.Columns.Add("SDI Received Date");

            try
            {
                strSqlString = "SELECT SCHOOL_ID FROM SDIX_ISA_SHIP_VALIDATION WHERE PROCESS_FLAG = 'N' GROUP BY SCHOOL_ID";
                unProSchds = GetAdapter(strSqlString);

                try
                {
                    if (unProSchds != null)
                    {
                        if (unProSchds.Tables.Count != 0)
                        {
                            if (unProSchds.Tables[0].Rows.Count != 0)
                            {
                                log.WriteLine("unProSchds Count: " + unProSchds.Tables[0].Rows.Count);
                                Console.WriteLine("unProSchds Count: " + unProSchds.Tables[0].Rows.Count);
                                foreach (DataRow dataRowMain in unProSchds.Tables[0].Rows)
                                {
                                    strSchoolID = System.Convert.ToString(dataRowMain["SCHOOL_ID"]).Trim();

                                    strSqlString = "SELECT ORDER_NO,ITEM_ID,NYC_RECEIVED,UOM,QTY_PO,ITEM_DESC FROM SDIX_ISA_SHIP_VALIDATION WHERE PROCESS_FLAG = 'N' AND SCHOOL_ID = '" + strSchoolID + "'";
                                    unPrOrdds = GetAdapter(strSqlString);

                                    // ' To select unprocessed order details based on school ID
                                    if (unPrOrdds != null)
                                    {
                                        if (unPrOrdds.Tables.Count != 0)
                                        {
                                            if (unPrOrdds.Tables[0].Rows.Count != 0)
                                            {
                                                foreach (DataRow DataRow in unPrOrdds.Tables[0].Rows)
                                                {
                                                    strOrderNo = System.Convert.ToString(DataRow["ORDER_NO"]).Trim();
                                                    strNYCReceived = System.Convert.ToString(DataRow["NYC_RECEIVED"]).Trim();
                                                    strItemID = System.Convert.ToString(DataRow["ITEM_ID"]);
                                                    strQTYORD = System.Convert.ToString(DataRow["QTY_PO"]).Trim();
                                                    strItemDesc = System.Convert.ToString(DataRow["ITEM_DESC"]).Trim();
                                                    strUOM = System.Convert.ToString(DataRow["UOM"]).Trim();

                                                    updateShipmentValues(strSchoolID, strOrderNo, strItemID);

                                                    string strPOID = "";
                                                    string strPOLine = "";
                                                    string strPOBU = "";
                                                    string strSDIQTY = "";
                                                    string strSDIRecDate = "";
                                                    decimal desSDIQTY;
                                                    List<DataRow> rowsWantToDelete = new List<DataRow>();
                                                    bool isOrderNotFound = false;
                                                    string strOrderLine = "";
                                                    string strInvoiceNo = "";
                                                    string strInvoiceAmount = "";

                                                    // ' To select valid order po id from SDI system when order numbers from validation table is match
                                                    strSqlString = "SELECT DL.PO_ID,DL.LINE_NBR,DL.BUSINESS_UNIT,QL.LINE_NBR AS ORDER_LINE ,LN.INV_ITEM_ID FROM PS_PO_LINE_DISTRIB DL, PS_ISA_QUICK_REQ_L QL , PS_ISA_ORD_INTF_LN LN WHERE QL.REQ_ID = DL.REQ_ID AND QL.LINE_NBR = DL.LINE_NBR AND LN.ORDER_NO = QL.REQ_ID AND LN.ISA_INTFC_LN = QL.LINE_NBR AND QL.REQ_ID = '" + strOrderNo + "' AND QL.VNDR_CATALOG_ID LIKE '%" + strItemID + "%' ";

                                                    OrdPODs = GetAdapter(strSqlString);

                                                    if (OrdPODs != null)
                                                    {
                                                        if (OrdPODs.Tables.Count != 0)
                                                        {
                                                            if (OrdPODs.Tables[0].Rows.Count == 1)
                                                            {
                                                                strPOID = Convert.ToString(OrdPODs.Tables[0].Rows[0][0]);
                                                                strPOLine = Convert.ToString(OrdPODs.Tables[0].Rows[0][1]);
                                                                strPOBU = Convert.ToString(OrdPODs.Tables[0].Rows[0][2]);
                                                                strOrderLine = Convert.ToString(OrdPODs.Tables[0].Rows[0][3]);

                                                                strItemID = Convert.ToString(OrdPODs.Tables[0].Rows[0][4]);

                                                                SDIRecDs = getAccptRecvTotals(strPOID, strPOLine, strPOBU);
                                                                if (SDIRecDs != null)
                                                                {
                                                                    if (SDIRecDs.Tables.Count != 0)
                                                                    {
                                                                        if (SDIRecDs.Tables[0].Rows.Count == 1)
                                                                        {
                                                                            // strSDIQTY = SDIRecDs.Tables(0).Rows(0).Item(1)

                                                                            if (!string.IsNullOrEmpty(Convert.ToString(SDIRecDs.Tables[0].Rows[0][0])))
                                                                                strSDIRecDate = Convert.ToString(SDIRecDs.Tables[0].Rows[0][0]);
                                                                            else
                                                                                strSDIRecDate = "";

                                                                            if (!string.IsNullOrEmpty(Convert.ToString(SDIRecDs.Tables[0].Rows[0][1])))
                                                                                strSDIQTY = Convert.ToString(SDIRecDs.Tables[0].Rows[0][1]);
                                                                            else
                                                                                strSDIQTY = "";
                                                                        }
                                                                    }
                                                                }

                                                                invoiceDS = getInvoiceAmount(strPOID, strPOLine, strPOBU);

                                                                if (invoiceDS != null)
                                                                {
                                                                    if (invoiceDS.Tables.Count != 0)
                                                                    {
                                                                        if (invoiceDS.Tables[0].Rows.Count == 1)
                                                                        {

                                                                            if (!string.IsNullOrEmpty(Convert.ToString(invoiceDS.Tables[0].Rows[0][1])))
                                                                                strInvoiceNo = Convert.ToString(invoiceDS.Tables[0].Rows[0][1]);
                                                                            else
                                                                                strInvoiceNo = "";

                                                                            if (!string.IsNullOrEmpty(Convert.ToString(invoiceDS.Tables[0].Rows[0][0])))
                                                                                strInvoiceAmount = Convert.ToString(invoiceDS.Tables[0].Rows[0][0]);
                                                                            else
                                                                                strInvoiceAmount = "";
                                                                        }
                                                                    }
                                                                }

                                                                if (strSDIQTY.Trim() != "")
                                                                {
                                                                    Decimal strSDIQTYs = Convert.ToDecimal(strSDIQTY);
                                                                    strSDIQTY = strSDIQTYs.ToString("0.00");

                                                                    Decimal strNYCReceiveds = Convert.ToDecimal(strNYCReceived);
                                                                    strNYCReceived = strNYCReceiveds.ToString("0.00");

                                                                    Decimal strQTYORDs = Convert.ToDecimal(strQTYORD);
                                                                    strQTYORD = strQTYORDs.ToString("0.00");

                                                                    strSDIQTYs = Convert.ToDecimal(strSDIQTY);
                                                                    string strNYC = strSDIQTYs.ToString("0");
                                                                    strNYCReceiveds = Convert.ToDecimal(strNYCReceived);
                                                                    string strSDI = strNYCReceiveds.ToString("0");

                                                                    int intNYCReceived = Convert.ToInt32(strNYC.Replace(".", ""));
                                                                    int intSDIReceived = Convert.ToInt32(strSDI.Replace(".", ""));

                                                                    // ' To check if the system QTY is not equal to validation table QTY
                                                                    // ' Adding miscalculated order to the seprate table for email body
                                                                    if (strNYCReceived != strSDIQTY)
                                                                    {
                                                                        dr = csvData.NewRow();
                                                                        dr["Order NO"] = strOrderNo;
                                                                        dr["Item ID"] = strItemID;
                                                                        dr["Item Description"] = strItemDesc;
                                                                        dr["UOM"] = strUOM;
                                                                        dr["Ordered QTY"] = strQTYORD;
                                                                        dr["NYC Received"] = strNYCReceived;
                                                                        dr["SDI Received"] = strSDIQTY;
                                                                        csvData.Rows.Add(dr);

                                                                        // To get the summary of non certified items 

                                                                        dr = SummaryItemDt.NewRow();
                                                                        dr["School ID"] = strSchoolID;
                                                                        dr["Order NO"] = strOrderNo;
                                                                        dr["Line NO"] = strOrderLine;
                                                                        dr["Item ID"] = strItemID;
                                                                        dr["Item Description"] = strItemDesc;
                                                                        dr["UOM"] = strUOM;
                                                                        dr["Ordered QTY"] = strQTYORD;
                                                                        dr["NYC Received"] = strNYCReceived;
                                                                        dr["SDI Received"] = strSDIQTY;
                                                                        dr["SDI Received Date"] = strSDIRecDate;
                                                                        dr["Invoice-No"] = strInvoiceNo;
                                                                        dr["Amount"] = strInvoiceAmount;
                                                                        SummaryItemDt.Rows.Add(dr);
                                                                    }

                                                                    // ' To send list of items that not recevied from both end SDI&NYC


                                                                    if (intNYCReceived == 0 & intSDIReceived == 0)
                                                                    {
                                                                        dr = ZeroItemDt.NewRow();
                                                                        dr["School ID"] = strSchoolID;
                                                                        dr["Order NO"] = strOrderNo;
                                                                        dr["Line NO"] = strOrderLine;
                                                                        dr["Item ID"] = strItemID;
                                                                        dr["Item Description"] = strItemDesc;
                                                                        dr["UOM"] = strUOM;
                                                                        dr["Ordered QTY"] = strQTYORD;
                                                                        dr["NYC Received"] = strNYCReceived;
                                                                        dr["SDI Received"] = strSDIQTY;
                                                                        dr["Not Found Order"] = "N";
                                                                        dr["Cancel Order"] = "N";
                                                                        ZeroItemDt.Rows.Add(dr);
                                                                    }
                                                                }
                                                                else
                                                                    isOrderNotFound = true;
                                                            }
                                                            else
                                                                isOrderNotFound = true;
                                                        }
                                                        else
                                                            isOrderNotFound = true;
                                                    }
                                                    else
                                                        isOrderNotFound = true;


                                                    if (isOrderNotFound)
                                                    {
                                                        string orderStatus = checkREQOrderStatus(strOrderNo, strItemID);
                                                        dr = ZeroItemDt.NewRow();
                                                        dr["School ID"] = strSchoolID;
                                                        dr["Order NO"] = strOrderNo;
                                                        dr["Line NO"] = strOrderLine;
                                                        dr["Item ID"] = strItemID;
                                                        dr["Item Description"] = strItemDesc;
                                                        dr["UOM"] = strUOM;
                                                        dr["Ordered QTY"] = strQTYORD;
                                                        dr["NYC Received"] = "";
                                                        dr["SDI Received"] = "";
                                                        dr["Not Found Order"] = "Y";
                                                        dr["Cancel Order"] = orderStatus;
                                                        ZeroItemDt.Rows.Add(dr);
                                                    }
                                                }
                                            }
                                        }
                                    }


                                    if (csvData != null)
                                    {
                                        if (csvData.Rows.Count != 0)
                                        {
                                            DataTable dtDistinct_Rows = new DataTable();

                                            dtDistinct_Rows = csvData.DefaultView.ToTable(true);

                                            string strSupervisors = "NZhang3@schools.nyc.gov";
                                            string strSchoolSupervisor = "";

                                            strSqlString = "SELECT EMAIL_ID FROM SDIX_SCHOOL_SUPERVISOR WHERE SCHOOL_ID='" + strSchoolID.Trim() + "'";

                                            strSchoolSupervisor = GetScalar(strSqlString);

                                            if (strSchoolSupervisor.Trim() != "")
                                                strSupervisors = strSchoolSupervisor + ";NZhang3@schools.nyc.gov";
                                            else
                                                strSupervisors = "NZhang3@schools.nyc.gov";

                                            // ' Compaining School ID for email id generation 
                                            strNYCEmailID = "C" + strSchoolID + "@schools.nyc.gov";
                                            DataTable dstcartSTK = new DataTable();

                                            dstcartSTK = dtDistinct_Rows;

                                            System.Web.UI.WebControls.DataGrid dtgcart;
                                            StringBuilder SBstk = new StringBuilder();
                                            StringWriter SWstk = new StringWriter(SBstk);
                                            System.Web.UI.HtmlTextWriter htmlTWstk = new System.Web.UI.HtmlTextWriter(SWstk);
                                            string dataGridHTML;
                                            dtgcart = new System.Web.UI.WebControls.DataGrid();
                                            // Code for line items
                                            dtgcart.DataSource = dstcartSTK;
                                            dtgcart.DataBind();
                                            dtgcart.CellPadding = 3;
                                            dtgcart.BorderColor = Color.Gray;
                                            dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray;
                                            dtgcart.HeaderStyle.Font.Bold = true;
                                            dtgcart.HeaderStyle.ForeColor = Color.Black;
                                            //dtgcart.Width.Percentage(90);

                                            dtgcart.RenderControl(htmlTWstk);

                                            dataGridHTML = SBstk.ToString();

                                            // 'MailFrom = "SDIExchange@SDI.com"
                                            MailFrom = "NYCcustodial@sdi.com";


                                            strbodyhead = "<table bgcolor='black' Width='100%'><tbody><tr><td style='width:1%;'><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td>" + "\r\n";
                                            strbodyhead = strbodyhead + "<td style='width:50% ;'><center><span style='font-weight:bold;color:white;font-size:24px;'>SDI Marketplace</span></center>" + "\r\n";
                                            strbodyhead = strbodyhead + "<center><span style='color:white;'>NYC Shipment Validation</span></center></td></tr></tbody></table>" + "\r\n";
                                            strbodyhead = strbodyhead + "<HR width='100%' SIZE='1'>";

                                            strbodydetl = "<div>";
                                            strbodydetl = "&nbsp;" + "\r\n";
                                            strbodydetl = strbodydetl + "<div>";
                                            strbodydetl = strbodydetl + "<p>Location Code:<span>" + strSchoolID + "</span><span></span><br>";
                                            strbodydetl = strbodydetl + "&nbsp;<br>";
                                            strbodydetl = strbodydetl + "<p>Please note that our records indicate that the items shown below on  the WR’s listed have been delivered, however these items have not yet been certified in FAMIS:</p>";

                                            strbodydetl = strbodydetl + "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" + "\r\n";
                                            strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" + dataGridHTML + "</TD></TR>";
                                            strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>";
                                            strbodydetl = strbodydetl + "</TABLE>" + "\r\n";
                                            strbodydetl = strbodydetl + "&nbsp;<br>";

                                            strbodydetl = strbodydetl + "<p>Please certify the items shipped or If you have any questions or require further assistance, please contact SDI @ NYCcustodial@sdi.com</p>";
                                            strbodydetl = strbodydetl + "&nbsp;<br>";
                                            strbodydetl = strbodydetl + "Sincerely,<br>";
                                            strbodydetl = strbodydetl + "&nbsp;<br>";
                                            strbodydetl = strbodydetl + "SDI Customer Care<br>";
                                            strbodydetl = strbodydetl + "&nbsp;<br>";
                                            strbodydetl = strbodydetl + "</p>";
                                            strbodydetl = strbodydetl + "</div>";
                                            strbodydetl = strbodydetl + "<HR width='100%' SIZE='1'>" + "\r\n";
                                            strbodydetl = strbodydetl + "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />";


                                            MailBody = strbodyhead + strbodydetl;

                                            if (!getDBName())
                                            {
                                                MailTo = "WebDev@sdi.com;avacorp@sdi.com;Ron.Fijalkowski@sdi.com";
                                                // MailCc = "Allen.Hiller@sdi.com"
                                                MailSub = "<<TEST SITE>>SDiZeus-School :" + strSchoolID + " -NYC Shipment Validation";
                                            }
                                            else
                                            {
                                                MailTo = strNYCEmailID;
                                                MailSub = "School :" + strSchoolID + " -NYC Shipment Validation";
                                                MailCc = strSupervisors;
                                                MailBcc = "WebDev@sdi.com;avacorp@sdi.com;Ron.Fijalkowski@sdi.com";
                                            }

                                            SDIEmailUtility.EmailServices SDIEmailService = new SDIEmailUtility.EmailServices();
                                            string[] MailAttachmentName = null;
                                            List<byte[]> MailAttachmentbytes = new List<byte[]>();

                                            try
                                           {
                                               SDIEmailService.EmailUtilityServices("MailandStore", MailFrom, MailTo, MailSub, MailCc, MailBcc, MailBody, "Shipment Validation", MailAttachmentName, MailAttachmentbytes.ToArray());
                                               csvData.Rows.Clear();
                                            }
                                            catch (Exception ex)
                                            {
                                                csvData.Rows.Clear();
                                                log.WriteLine("Email unProSchds: " + ex.Message);
                                                Console.WriteLine("Email unProSchds: " + ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.WriteLine("unProSchds: " + ex.Message);
                    Console.WriteLine("unProSchds: " + ex.Message);
                }
                if (ZeroItemDt != null)
                {
                    try
                    {
                        if (ZeroItemDt.Rows.Count != 0)
                        {
                            log.WriteLine("Zero Item Count: " + ZeroItemDt.Rows.Count);
                            Console.WriteLine("Zero Item Count: " + ZeroItemDt.Rows.Count);
                            DataTable dstcartSTK = new DataTable();

                            dstcartSTK = ZeroItemDt;
                            // 'Appending the Dataset values in the CSV
                            StringBuilder sb = new StringBuilder();
                            int intClmn = dstcartSTK.Columns.Count;
                            if (dstcartSTK.Rows.Count != 0)
                            {
                                int i = 0;
                                for (i = 0; i <= intClmn - 1; i += i + 1)
                                {
                                    sb.Append("\"" + dstcartSTK.Columns[i].ColumnName.ToString() + "\"");
                                    if (i == intClmn - 1)
                                        sb.Append(" ");
                                    else
                                        sb.Append(",");
                                }
                                sb.Append("\r\n");

                                // --------Data By  Columns--------------------------------------------------------------------------- 
                                foreach (DataRow row in dstcartSTK.Rows)
                                {
                                    int ir = 0;
                                    for (ir = 0; ir <= intClmn - 1; ir += ir + 1)
                                    {
                                        sb.Append("\"" + row[ir].ToString().Replace("\"", "\"\"") + "\"");
                                        if (ir == intClmn - 1)
                                            sb.Append(" ");
                                        else
                                            sb.Append(",");
                                    }
                                    sb.Append("\r\n");
                                }
                            }

                            byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());

                            string path = appPath + @"Contacts\" + "NYCSchoolReport.csv";
                            if (File.Exists(path))
                                File.Delete(path);

                            File.WriteAllText(path, sb.ToString());

                            strbodyhead = "<table bgcolor='black' Width='100%'><tbody><tr><td style='width:1%;'><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td>" + "\r\n";
                            strbodyhead = strbodyhead + "<td style='width:50% ;'><center><span style='font-weight:bold;color:white;font-size:24px;'>SDI Marketplace</span></center>" + "\r\n";
                            strbodyhead = strbodyhead + "<center><span style='color:white;'>NYC Shipment Zero Quantity Report</span></center></td></tr></tbody></table>" + "\r\n";
                            strbodyhead = strbodyhead + "<HR width='100%' SIZE='1'>";

                            strbodydetl = "<div>";
                            strbodydetl = "&nbsp;" + "\r\n";
                            strbodydetl = strbodydetl + "<div>";
                            strbodydetl = strbodydetl + "<p><span></span><span></span><br>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";
                            strbodydetl = strbodydetl + "<p>Please find the report for 0 NYC received and 0 SDI received</p>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";

                            strbodydetl = strbodydetl + "<p>Please certify the items shipped or If you have any questions or require further assistance, please contact SDI @ NYCcustodial@sdi.com</p>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";
                            strbodydetl = strbodydetl + "Sincerely,<br>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";
                            strbodydetl = strbodydetl + "SDI Customer Care<br>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";
                            strbodydetl = strbodydetl + "</p>";
                            strbodydetl = strbodydetl + "</div>";
                            strbodydetl = strbodydetl + "<HR width='100%' SIZE='1'>" + "\r\n";
                            strbodydetl = strbodydetl + "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />";


                            MailBody = strbodyhead + strbodydetl;

                            if (!getDBName())
                            {
                                MailTo = "WebDev@sdi.com;avacorp@sdi.com;Ron.Fijalkowski@sdi.com";
                                MailSub = "<<TEST SITE>>SDiZeus-School :" + strSchoolID + " - NYC Shipment Zero Quantity Report";
                                MailCc = "";
                                MailBcc = "";
                            }
                            else
                            {

                                MailTo = "Ron.Fijalkowski@sdi.com";
                                MailSub = "<<TEST SITE>>SDiZeus-School :" + strSchoolID + " - NYC Shipment Zero Quantity Report";
                                MailCc = "WebDev@sdi.com;avacorp@sdi.com";
                                MailBcc = "";
                            }

                            SDIEmailUtility.EmailServices SDIEmailService = new SDIEmailUtility.EmailServices();

                            MailFrom = "SDIExchange@SDI.com";
                            string fName = "";
                            fName = "NYCSchoolReport.csv";
                            string[] MailAttachmentName = new string[] { fName };
                            List<byte[]> MailAttachmentbytes = new List<byte[]>();

                            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                            BinaryReader br = new BinaryReader(fs);
                            byte[] fileContents = br.ReadBytes(System.Convert.ToInt32(fs.Length));
                            br.Close();
                            fs.Close();

                            MailAttachmentbytes.Add(fileContents);

                            try
                            {
                                SDIEmailService.EmailUtilityServices("MailandStore", MailFrom, MailTo, MailSub, MailCc, MailBcc, MailBody, "Shipment Validation", MailAttachmentName, MailAttachmentbytes.ToArray());
                                ZeroItemDt.Rows.Clear();
                            }
                            catch (Exception ex)
                            {
                                ZeroItemDt.Rows.Clear();
                                log.WriteLine("Email Zero Item: " + ex.Message);
                                Console.WriteLine("Email Zero Item: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine("Zero Item: " + ex.Message);
                        Console.WriteLine("Zero Item: " + ex.Message);
                    }
                }

                if (SummaryItemDt != null)
                {
                    try
                    {
                        if (SummaryItemDt.Rows.Count != 0)
                        {
                            log.WriteLine("Summary Item Count: " + SummaryItemDt.Rows.Count);
                            Console.WriteLine("Summary Item Count: " + SummaryItemDt.Rows.Count);
                            DataTable dstcartSTK = new DataTable();

                            dstcartSTK = SummaryItemDt;


                            // 'Appending the Dataset values in the CSV
                            StringBuilder sb = new StringBuilder();
                            int intClmn = dstcartSTK.Columns.Count;
                            if (dstcartSTK.Rows.Count != 0)
                            {
                                int i = 0;
                                for (i = 0; i <= intClmn - 1; i += i + 1)
                                {
                                    sb.Append("\"" + dstcartSTK.Columns[i].ColumnName.ToString() + "\"");
                                    if (i == intClmn - 1)
                                        sb.Append(" ");
                                    else
                                        sb.Append(",");
                                }
                                sb.Append("\r\n");

                                // --------Data By  Columns--------------------------------------------------------------------------- 


                                foreach (DataRow row in dstcartSTK.Rows)
                                {
                                    int ir = 0;
                                    for (ir = 0; ir <= intClmn - 1; ir += ir + 1)
                                    {
                                        sb.Append("\"" + row[ir].ToString().Replace("\"", "\"\"") + "\"");
                                        if (ir == intClmn - 1)
                                            sb.Append(" ");
                                        else
                                            sb.Append(",");
                                    }
                                    sb.Append("\r\n");
                                }
                            }

                            byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());
                            string path = appPath + @"Contacts\" + "NYCSchoolReport.csv";

                            if (File.Exists(path))
                                File.Delete(path);

                            File.WriteAllText(path, sb.ToString());

                            strbodyhead = "<table bgcolor='black' Width='100%'><tbody><tr><td style='width:1%;'><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td>" + "\r\n";
                            strbodyhead = strbodyhead + "<td style='width:50% ;'><center><span style='font-weight:bold;color:white;font-size:24px;'>SDI Marketplace</span></center>" + "\r\n";
                            strbodyhead = strbodyhead + "<center><span style='color:white;'>NYC Shipment Summary Report</span></center></td></tr></tbody></table>" + "\r\n";
                            strbodyhead = strbodyhead + "<HR width='100%' SIZE='1'>";

                            strbodydetl = "<div>";
                            strbodydetl = "&nbsp;" + "\r\n";
                            strbodydetl = strbodydetl + "<div>";
                            strbodydetl = strbodydetl + "<p><span></span><span></span><br>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";
                            strbodydetl = strbodydetl + "<p>Please find the summary report for the not certified items</p>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";

                            strbodydetl = strbodydetl + "<p>Please certify the items shipped or If you have any questions or require further assistance, please contact SDI @ NYCcustodial@sdi.com</p>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";
                            strbodydetl = strbodydetl + "Sincerely,<br>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";
                            strbodydetl = strbodydetl + "SDI Customer Care<br>";
                            strbodydetl = strbodydetl + "&nbsp;<br>";
                            strbodydetl = strbodydetl + "</p>";
                            strbodydetl = strbodydetl + "</div>";
                            strbodydetl = strbodydetl + "<HR width='100%' SIZE='1'>" + "\r\n";
                            strbodydetl = strbodydetl + "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />";


                            MailBody = strbodyhead + strbodydetl;

                            if (!getDBName())
                            {
                                MailTo = "WebDev@sdi.com;avacorp@sdi.com;Ron.Fijalkowski@sdi.com";
                                MailSub = "<<TEST SITE>>SDiZeus-School :" + strSchoolID + " - NYC Shipment Summary Report";
                                MailCc = "";
                                MailBcc = "";
                            }
                            else
                            {

                                MailTo = "MVoros@schools.nyc.gov;JZotos@schools.nyc.gov";
                                MailSub = "<<TEST SITE>>SDiZeus-School :" + strSchoolID + " - NYC Shipment Summary Report";
                                MailCc = "WebDev@sdi.com;avacorp@sdi.com;Ron.Fijalkowski@sdi.com";
                                MailBcc = "";
                            }
                            SDIEmailUtility.EmailServices SDIEmailService = new SDIEmailUtility.EmailServices();

                            MailFrom = "SDIExchange@SDI.com";
                            string fName = "";
                            fName = "NYCSchoolReport.csv";
                            string[] MailAttachmentName = new string[] { fName };
                            List<byte[]> MailAttachmentbytes = new List<byte[]>();

                            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                            BinaryReader br = new BinaryReader(fs);
                            byte[] fileContents = br.ReadBytes(System.Convert.ToInt32(fs.Length));
                            br.Close();
                            fs.Close();

                            MailAttachmentbytes.Add(fileContents);

                            try
                            {
                                SDIEmailService.EmailUtilityServices("MailandStore", MailFrom, MailTo, MailSub, MailCc, MailBcc, MailBody, "Shipment Validation", MailAttachmentName, MailAttachmentbytes.ToArray());
                                ZeroItemDt.Rows.Clear();
                            }
                            catch (Exception ex)
                            {
                                ZeroItemDt.Rows.Clear();
                                log.WriteLine("Email Summary Item: " + ex.Message);
                                Console.WriteLine("Email Summary Item: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine("Summary Item: " + ex.Message);
                        Console.WriteLine("Summary Item: " + ex.Message);
                    }
                }

                log.WriteLine("NYC Shipment Sending Mail Process Completed");
                Console.WriteLine("NYC Shipment Sending Mail Process Completed");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                log.WriteLine(ex.Message);
            }
        }
        public static bool getDBName()
        {
            bool isPRODDB = false;
            try
            {
                string PRODDbList = ConfigurationManager.AppSettings["OraPRODDbList"].ToString();
                string DbUrl = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper();
                isPRODDB = (PRODDbList.IndexOf(DbUrl.Trim().ToUpper()) > -1);
            }
            catch (Exception ex)
            {
                isPRODDB = false;
                Console.WriteLine(ex.Message);
            }
            return isPRODDB;
        }
        private static SearchFilter SetFilter(string strSubject1)
        {
            try
            {
                string FilterEmailWithSubject = strSubject1;
                List<SearchFilter> searchFilterCollection = new List<SearchFilter>();
                searchFilterCollection.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.Subject, strSubject1));
                SearchFilter searchfiltr = new SearchFilter.SearchFilterCollection(LogicalOperator.Or, searchFilterCollection.ToArray());
                return searchfiltr;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
