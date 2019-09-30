using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Net.Mail;
using Microsoft.Exchange.WebServices.Data;

namespace PDF_Extractor_Sample
{
    class Program
    {

        static void Main(string[] args)
        {
            string UN = ConfigurationManager.AppSettings["UserName"];
            string Pwd = ConfigurationManager.AppSettings["Password"];
            string OutlookURL = ConfigurationManager.AppSettings["OutlookURL"];
            string ProcessedFolderName = ConfigurationManager.AppSettings["ProcessedFolderName"];
            string DestinationPath = ConfigurationManager.AppSettings["DestinationPath"];
            string EmailIDAccount = ConfigurationManager.AppSettings["MailboxEmailID"];

            DataTable ShipTo = new DataTable();
            DataTable dt_HDR = new DataTable();

            dt_HDR.Columns.Add("PO_ID", typeof(string));
            dt_HDR.Columns.Add("PO_DT", typeof(string));
            dt_HDR.Columns.Add("CONTACT_NAME", typeof(string));
            dt_HDR.Columns.Add("CONTACT_PHONE", typeof(string));
            dt_HDR.Columns.Add("CONTACT_FAX", typeof(string));
            dt_HDR.Columns.Add("CONTACT_EMAIL", typeof(string));
            dt_HDR.Columns.Add("TAXABLE_CD", typeof(string));
            dt_HDR.Columns.Add("DUE_DT", typeof(string));
            dt_HDR.Columns.Add("ADD_DTTM", typeof(string));
            dt_HDR.Columns.Add("PROCESS_FLAG", typeof(string));
            dt_HDR.Columns.Add("PROCESS_INSTANCE", typeof(string));

            DataTable dt_LINE = new DataTable();

            dt_LINE.Columns.Add("PO_ID", typeof(string));
            dt_LINE.Columns.Add("LINE_NBR", typeof(string));
            dt_LINE.Columns.Add("ISA_ITEM", typeof(string));
            dt_LINE.Columns.Add("PREV_INV_ITEM_ID", typeof(string));
            dt_LINE.Columns.Add("QTY_REQUESTED", typeof(string));
            dt_LINE.Columns.Add("PRICE", typeof(string));
            dt_LINE.Columns.Add("ISA_CUSTOMER_UOM", typeof(string));
            dt_LINE.Columns.Add("ISA_PRICING_UOM", typeof(string));
            dt_LINE.Columns.Add("ITM_ID_VNDR", typeof(string));
            dt_LINE.Columns.Add("MFG_ID", typeof(string));
            dt_LINE.Columns.Add("MFG_ITM_ID", typeof(string));
            dt_LINE.Columns.Add("DESCR254", typeof(string));
            dt_LINE.Columns.Add("SHIP_TO_NAME", typeof(string));
            dt_LINE.Columns.Add("SHIPTO_ADDRESS1", typeof(string));
            dt_LINE.Columns.Add("SHIPTO_ADDRESS2", typeof(string));
            dt_LINE.Columns.Add("SHIPTO_CITY", typeof(string));
            dt_LINE.Columns.Add("SHIPTO_STATE", typeof(string));
            dt_LINE.Columns.Add("SHIPTO_POSTAL", typeof(string));
            dt_LINE.Columns.Add("ISA_LINE_NOTES", typeof(string));
            dt_LINE.Columns.Add("ADD_DTTM", typeof(string));
            dt_LINE.Columns.Add("PROCESS_FLAG", typeof(string));
            dt_LINE.Columns.Add("PROCESS_INSTANCE", typeof(string));

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
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                appPath = appPath.Substring(0, appPath.LastIndexOf("bin"));

                string logFilePath = appPath + @"Logs\SIPDFProcessLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
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

                FindItemsResults<Item> findResults = service.FindItems(fidProcessedParent, SetFilter(), view);

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
                if (findResults.Items.Count() > 0)
                {
                    foreach (Item item in findResults.Items)
                    {
                        int filePDFcount = 0;
                        //Information
                        string DocNo = " ";
                        string PODate = " ";
                        string ContactName = " ";
                        string Phone = " ";
                        string Fax = " ";
                        string Email = " ";
                        string TaxInformation = " ";
                        string DeliveryDate = " ";
                        string VendorID = " ";

                        string currentText = string.Empty;
                        string Ship_Name = " ";
                        string Ship_Addr1 = " ";
                        string Ship_Addr2 = " ";
                        string Ship_City = " ";
                        string Ship_State = " ";
                        string Ship_Postal = " ";
                        string HDRShipAddrLFlag = "N";

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

                            if (fileExtension == "pdf" || fileExtension == "PDF")
                            {
                                filePDFcount++;
                                try
                                {
                                    dt_HDR.Clear();
                                    dt_LINE.Clear();
                                    log.WriteLine("Begin Processing the Email from : " + from_name + " for File name " + fileAttachment.Name + "");
                                    fileAttachment.Load(appPath + @"PDFAttachments\" + fileAttachment.Name);
                                    if (System.IO.File.Exists(appPath + @"PDFAttachments\" + fileAttachment.Name))
                                    {
                                        log.WriteLine("Downloaded the attachment file " + fileAttachment.Name);
                                    }
                                    else
                                    {
                                        log.WriteLine("Error in Downloading the attachment file " + fileAttachment.Name + " in attachment folder.");
                                        log.WriteLine("-------------------------------------------------------------");
                                        break;
                                    }

                                    StringBuilder text = new StringBuilder();

                                    List<string> strlst = new List<string>();
                                    DataRow dr_HDR = null;
                                    dr_HDR = dt_HDR.NewRow();
                                    using (PdfReader reader = new PdfReader(appPath + @"PDFAttachments\" + fileAttachment.Name))
                                    {
                                        string prevPage = "";
                                        for (int i = 1; i <= reader.NumberOfPages; i++)
                                        {
                                            ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                                            currentText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                                            currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.UTF8.GetBytes(currentText)));
                                            text.Append(currentText);
                                            strlst.Add(currentText);
                                        }
                                        reader.Close();
                                    }
                                    ShipTo = GetShipAddress();

                                    if (strlst.Count() > 0)
                                    {
                                        log.WriteLine("Parsed the PDF file for reading the texts.");
                                        string checkFile = text.ToString();
                                        if (checkFile.Trim() != "")
                                        {
                                            if (!checkFile.Contains("Change to Purchase order"))
                                            {
                                                for (int i = 0; i <= strlst.Count - 1; i++)
                                                {
                                                    if (i == 0)
                                                    {//Page1 Processing
                                                        string fullfile = strlst[i];
                                                        //Processing the Header info
                                                        try
                                                        {
                                                            if (fullfile.Contains("Document Number"))
                                                            {
                                                                string fullLine = fullfile;

                                                                try
                                                                {
                                                                    int line_no = fullfile.IndexOf("Please note our purchase order and material numbers on your order confirmations, shipping papers and invoices !\n");
                                                                    fullLine = fullLine.Substring(0, line_no);
                                                                }
                                                                catch (Exception ex)
                                                                {

                                                                }
                                                                string[] lines = null;
                                                                lines = fullLine.Split('\n');
                                                                foreach (string line in lines)
                                                                {
                                                                    if (line.StartsWith("Document Number"))
                                                                    {
                                                                        try
                                                                        {
                                                                            DocNo = line.Replace("Document Number ", "");
                                                                            dr_HDR["PO_ID"] = DocNo.Trim();
                                                                            //log.WriteLine("Extracted the Header Date for LINE_NO: " + ItemNo);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("Date "))
                                                                    {
                                                                        try
                                                                        {
                                                                            PODate = line.Replace("Date ", "");
                                                                            DateTime dDate;
                                                                            if (DateTime.TryParse(PODate, out dDate))
                                                                            {
                                                                                dr_HDR["PO_DT"] = PODate.Trim();
                                                                            }
                                                                            else
                                                                            {
                                                                                dr_HDR["PO_DT"] = "";
                                                                            }
                                                                            //log.WriteLine("Extracted the Header Document Number for LINE_NO: " + ItemNo);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("Contact "))
                                                                    {
                                                                        try
                                                                        {
                                                                            ContactName = line.Replace("Contact ", "");
                                                                            dr_HDR["CONTACT_NAME"] = ContactName;
                                                                            //log.WriteLine("Extracted the Header Contact for LINE_NO: " + ItemNo);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("Phone "))
                                                                    {
                                                                        try
                                                                        {
                                                                            Phone = line.Replace("Phone ", "");
                                                                            dr_HDR["CONTACT_PHONE"] = Phone.Trim();
                                                                            //log.WriteLine("Extracted the Header Phone for LINE_NO: " + ItemNo);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("Fax "))
                                                                    {
                                                                        try
                                                                        {
                                                                            Fax = line.Replace("Fax ", "");
                                                                            dr_HDR["CONTACT_FAX"] = Fax.Trim();
                                                                            //log.WriteLine("Extracted the Header Fax for LINE_NO: " + ItemNo);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("Vendor Information "))
                                                                    {
                                                                        if (line.Contains("E-mail"))
                                                                        {
                                                                            try
                                                                            {
                                                                                int getVndr = line.IndexOf("E-mail ");
                                                                                int totallen = line.Length;
                                                                                string vndrinfo = line.Substring(0, getVndr);
                                                                                string EmailInfo = line.Substring(getVndr);

                                                                                VendorID = vndrinfo.Replace("Vendor Information ", "");
                                                                                Email = EmailInfo.Replace("E-mail ", "");
                                                                                dr_HDR["CONTACT_EMAIL"] = Email.Trim();
                                                                                //log.WriteLine("Extracted the Header Email info & Vendor Info for LINE_NO: " + ItemNo);
                                                                            }
                                                                            catch (Exception ex)
                                                                            {

                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            try
                                                                            {
                                                                                int getVndr = line.IndexOf("Vendor Information") + Convert.ToString("Vendor Information").Length;
                                                                                string vndrinfo = line.Substring(getVndr);
                                                                                VendorID = vndrinfo.Replace("Vendor Information ", "");
                                                                                VendorID = VendorID.Trim();
                                                                                //log.WriteLine("Extracted the Header Vendor Information for LINE_NO: " + ItemNo);
                                                                            }
                                                                            catch (Exception ex)
                                                                            {

                                                                            }
                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("E-mail address"))
                                                                    {
                                                                        //ignore
                                                                    }
                                                                    else if (line.StartsWith("E-mail "))
                                                                    {
                                                                        try
                                                                        {
                                                                            int getemail = line.IndexOf("E-mail") + Convert.ToString("E-mail").Length;
                                                                            string emailinfo = line.Substring(getemail);
                                                                            Email = emailinfo.Replace("E-mail", "");
                                                                            Email = Email.Trim();
                                                                            dr_HDR["CONTACT_EMAIL"] = Email.Trim();
                                                                            //log.WriteLine("Extracted the Header Email info for LINE_NO: " + ItemNo);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("Tax Information "))
                                                                    {
                                                                        try
                                                                        {
                                                                            //TaxInformation = line.Replace("Tax Information ", "");
                                                                            //dr_HDR["TAXABLE_CD"] = TaxInformation;
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("Delivery Date "))
                                                                    {
                                                                        try
                                                                        {
                                                                            DeliveryDate = line.Replace("Delivery Date ", "");
                                                                            DateTime dDate;
                                                                            if (DateTime.TryParse(DeliveryDate, out dDate))
                                                                            {
                                                                                dr_HDR["DUE_DT"] = DeliveryDate.Trim();
                                                                            }
                                                                            else
                                                                            {
                                                                                dr_HDR["DUE_DT"] = "";
                                                                            }
                                                                            //log.WriteLine("Extracted the Header Delivery Date for LINE_NO: " + ItemNo);
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (line.StartsWith("Shipping Address "))
                                                                    {
                                                                        try
                                                                        {
                                                                            int getshipno = fullLine.IndexOf("Shipping Address INVOICE MAIL ADDRESS") + Convert.ToString("Shipping Address INVOICE MAIL ADDRESS\n").Length;
                                                                            string chkShipping = fullLine.Substring(getshipno);
                                                                            chkShipping = chkShipping.Replace(",", " ");
                                                                            if (chkShipping.Trim() != "")
                                                                            {
                                                                                string[] shipdatas = null;
                                                                                shipdatas = chkShipping.Split('\n');
                                                                                foreach (string strchek in shipdatas)
                                                                                {
                                                                                    if (!strchek.ToUpper().Contains("SI GROUP"))
                                                                                    {
                                                                                        string[] checkShipAddr = strchek.Split(' ');
                                                                                        foreach (string str in checkShipAddr)
                                                                                        {
                                                                                            if (HDRShipAddrLFlag == "Y")
                                                                                            {
                                                                                                break;
                                                                                            }
                                                                                            if (str.Trim() != "")
                                                                                            {
                                                                                                if (ShipTo != null)
                                                                                                {
                                                                                                    foreach (DataRow rw in ShipTo.Rows)
                                                                                                    {
                                                                                                        string checkdata = Convert.ToString(rw["ADDRESS"]);
                                                                                                        if (checkdata.ToUpper().Contains(" " + str.ToUpper()))
                                                                                                        {
                                                                                                            Ship_Name = Convert.ToString(rw["DESCR"]);
                                                                                                            Ship_Addr1 = Convert.ToString(rw["ADDRESS1"]);
                                                                                                            Ship_Addr2 = Convert.ToString(rw["ADDRESS2"]);
                                                                                                            Ship_City = Convert.ToString(rw["CITY"]);
                                                                                                            Ship_State = Convert.ToString(rw["STATE"]);
                                                                                                            Ship_Postal = Convert.ToString(rw["POSTAL"]);
                                                                                                            HDRShipAddrLFlag = "Y";
                                                                                                            //log.WriteLine("Extracted the Header Shipping Address info for LINE_NO: " + ItemNo);
                                                                                                            break;
                                                                                                        }
                                                                                                        if (HDRShipAddrLFlag == "Y")
                                                                                                        {
                                                                                                            break;
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                                //HDRShipAddrLFlag = "Y";
                                                                            }
                                                                            else
                                                                            {
                                                                                HDRShipAddrLFlag = "N";
                                                                            }
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (line.Contains("Document Number"))
                                                                        {
                                                                            try
                                                                            {
                                                                                int getDoc = line.IndexOf("Document Number") + Convert.ToString("Document Number").Length;
                                                                                string Docinfo = line.Substring(getDoc);
                                                                                DocNo = Docinfo.Replace("Document Number", "");
                                                                                DocNo = DocNo.Trim();
                                                                                dr_HDR["PO_ID"] = DocNo.Trim();
                                                                            }
                                                                            catch (Exception ex)
                                                                            {

                                                                            }
                                                                        }
                                                                        else if (line.Contains("Delivery Date"))
                                                                        {
                                                                            try
                                                                            {
                                                                                int getDelivery = line.IndexOf("Delivery Date") + Convert.ToString("Delivery Date").Length;
                                                                                string Docinfo = line.Substring(getDelivery);
                                                                                DeliveryDate = Docinfo.Replace("Document Number", "");
                                                                                DeliveryDate = DeliveryDate.Trim();
                                                                                DateTime dDate;
                                                                                if (DateTime.TryParse(DeliveryDate, out dDate))
                                                                                {
                                                                                    dr_HDR["DUE_DT"] = DeliveryDate.Trim();
                                                                                }
                                                                                else
                                                                                {
                                                                                    dr_HDR["DUE_DT"] = "";
                                                                                }
                                                                            }
                                                                            catch (Exception ex)
                                                                            {

                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            log.WriteLine("Processed the header information");
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            log.WriteLine("Error while extracting the header information");
                                                        }
                                                        dr_HDR["TAXABLE_CD"] = "Y";
                                                        dt_HDR.Rows.Add(dr_HDR);

                                                        fullfile = strlst[i];

                                                        try
                                                        {
                                                            int currentLine = Convert.ToInt32(Convert.ToString("Invoice should be sent via -mail address referenced in the INVOICE MAIL ADDRESS. ").Length);
                                                            int lineno = fullfile.IndexOf("Invoice should be sent via -mail address referenced in the INVOICE MAIL ADDRESS.") + currentLine;

                                                            fullfile = fullfile.Substring(lineno);
                                                            int val = fullfile.CompareTo("Invoice should be sent via -mail address referenced in the INVOICE MAIL ADDRESS.");
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            log.WriteLine("Error while extracting the Line items info information in first page");
                                                        }

                                                        if (fullfile.Contains("Item Material/Description Quantity UM Unit Price Net Amount"))
                                                        {
                                                            //log.WriteLine("Started th processing the Page: " + (i + 1));
                                                            int current_Line = Convert.ToInt32(Convert.ToString("Item Material/Description Quantity UM Unit Price Net Amount ").Length);
                                                            fullfile = fullfile.Substring(current_Line);
                                                            string[] datas = fullfile.Split('\n');
                                                            int LineItemsCount = 0;
                                                            foreach (string data in datas)
                                                            {
                                                                if (data.StartsWith("   "))
                                                                {
                                                                    LineItemsCount += 1;
                                                                }
                                                            }
                                                            string[] datas1 = fullfile.Split('\n');
                                                            List<int> lstItems = new List<int>();
                                                            int LinesCount = 0;
                                                            int LineNumbers = 0;

                                                            List<List<string>> myList = new List<List<string>>();

                                                            foreach (string data in datas1)
                                                            {
                                                                LineNumbers += 1;
                                                                if (data.StartsWith("   "))
                                                                {
                                                                    LinesCount += 1;
                                                                    lstItems.Add(LineNumbers - 1);
                                                                    myList.Add(new List<string> { data });
                                                                }
                                                                else
                                                                {
                                                                    if (LinesCount != 0)
                                                                    {
                                                                        lstItems.Add(LineNumbers - 1);
                                                                        myList[LinesCount - 1].Add(data);
                                                                    }
                                                                }
                                                            }
                                                            foreach (List<string> list in myList)
                                                            {
                                                                string ItemNo = " ";
                                                                string ItemID = " ";
                                                                string Qtys = " ";
                                                                string UM = " ";
                                                                string UnitPrice = " ";
                                                                string ItemDescription = " ";
                                                                string MFGITEMID = " ";

                                                                string ShipName = " ";
                                                                string ShipAddr1 = " ";
                                                                string ShipAddr2 = " ";
                                                                string ShipCity = " ";
                                                                string ShipState = " ";
                                                                string ShipPostal = " ";
                                                                foreach (string data in list)
                                                                {
                                                                    if (data.StartsWith("   "))
                                                                    {
                                                                        string newstr = data.Replace("   ", "").ToString();
                                                                        int z = newstr.IndexOf(" ") + 1;

                                                                        ItemNo = newstr.Substring(0, z);
                                                                        newstr = newstr.Substring(z);

                                                                        //\d+.+\d

                                                                        string[] wordslist = data.Split(' ');
                                                                        string getqtys = "";
                                                                        foreach (string word in wordslist)
                                                                        {
                                                                            bool result = false;
                                                                            result = Regex.IsMatch(word, @"^\d*\.\d*$");
                                                                            if (result)
                                                                            {
                                                                                getqtys = word;
                                                                                break;
                                                                            }
                                                                        }
                                                                        Qtys = getqtys;
                                                                        int getint = newstr.IndexOf(getqtys);
                                                                        string splitstrs = newstr.Substring(0, getint);
                                                                        string descriptionsValue = splitstrs;
                                                                        newstr = newstr.Substring(newstr.IndexOf(getqtys) + getqtys.Length);

                                                                        newstr = newstr.Trim();

                                                                        int getUOM = newstr.IndexOf(" ") + 1;
                                                                        UM = newstr.Substring(0, getUOM);
                                                                        newstr = newstr.Substring(newstr.IndexOf(UM) + UM.Length);

                                                                        newstr = newstr.Trim();

                                                                        string[] wordslist2 = newstr.Split(' ');
                                                                        string get_Unitprice = "";
                                                                        foreach (string word in wordslist2)
                                                                        {
                                                                            bool result = false;
                                                                            result = Regex.IsMatch(word, @"(?=.*\d)^\$?(([1-9]\d{0,20}(,\d{3})*)|0)?(\.\d{1,5})?$");
                                                                            if (result)
                                                                            {
                                                                                get_Unitprice = word;
                                                                                break;
                                                                            }
                                                                        }
                                                                        UnitPrice = get_Unitprice.Replace(",", "");

                                                                        string strcurrenttext = data.Replace("   ", "").ToString();
                                                                        if(strcurrenttext.Trim() != "")
                                                                        {
                                                                            string itemno = "";
                                                                            itemno = strcurrenttext.Substring(0, strcurrenttext.IndexOf(Qtys));
                                                                            itemno = itemno.Replace(ItemNo, "");
                                                                            itemno = itemno.Replace(Qtys, "");
                                                                            itemno = itemno.Replace("  ", "");
                                                                            itemno = itemno.Trim();

                                                                            if (itemno.Length > 18)
                                                                            {
                                                                                ItemID = itemno.Substring(0, 18).Trim();
                                                                            }
                                                                            else {
                                                                                ItemID = itemno.Trim();
                                                                            }
                                                                        }

                                                                        //log.WriteLine("Extracted the LINE_NO, PRICE, UM column values for LINE_NO: " + ItemNo);
                                                                        int LinesforgetDesc = 0;
                                                                        string strbrkline = "";
                                                                        foreach (string chkdesc in list)
                                                                        {
                                                                            LinesforgetDesc++;
                                                                            if (chkdesc.Contains("Your material Number"))
                                                                            {
                                                                                strbrkline = "Y";
                                                                                break;
                                                                            }
                                                                            else if (chkdesc.Contains("Item partially delivered"))
                                                                            {
                                                                                strbrkline = "Y";
                                                                                break;
                                                                            }
                                                                            else if (chkdesc.Contains("Item completely delivered"))
                                                                            {
                                                                                strbrkline = "Y";
                                                                                break;
                                                                            }
                                                                            else if (chkdesc.Contains("Manufacturing Part Number Manuf. Manuf. pl. Rev. l"))
                                                                            {
                                                                                strbrkline = "Y";
                                                                                break;
                                                                            }
                                                                            else if (chkdesc.Contains("Shipping Address"))
                                                                            {
                                                                                strbrkline = "Y";
                                                                                break;
                                                                            }
                                                                            else if (chkdesc.Contains("Net value incl. disc"))
                                                                            {
                                                                                strbrkline = "Y";
                                                                                break;
                                                                            }
                                                                        }
                                                                        int MarkLns = 0;
                                                                        if (strbrkline == "Y")
                                                                        {
                                                                            for (int l = 1; l < LinesforgetDesc - 1; l++)
                                                                            {
                                                                                ItemDescription += list[l] + " ";
                                                                            }
                                                                            //log.WriteLine("Extracted the Descriptions for LINE_NO: " + ItemNo);
                                                                        }
                                                                        else
                                                                        {
                                                                            for (int l = 1; l < LinesforgetDesc; l++)
                                                                            {
                                                                                ItemDescription += list[l] + " ";
                                                                            }
                                                                            string GetDescNextPage = "";
                                                                            if ((strlst.Count()) > (i + 1))
                                                                            {
                                                                                GetDescNextPage = strlst[i + 1];
                                                                                GetDescNextPage = GetDescNextPage.Substring(GetDescNextPage.IndexOf("Item Material/Description Quantity UM Unit Price Net Amount") + Convert.ToString("Item Material/Description Quantity UM Unit Price Net Amount").Length);

                                                                                int Lines_forgetDesc = 0;
                                                                                string strbrk_line = "";
                                                                                string[] Getdesclists = null;
                                                                                if (!GetDescNextPage.StartsWith("\n   "))
                                                                                {
                                                                                    Getdesclists = GetDescNextPage.Split('\n');
                                                                                }
                                                                                if (Getdesclists != null)
                                                                                {
                                                                                    foreach (string chkdesc in Getdesclists)
                                                                                    {
                                                                                        Lines_forgetDesc++;
                                                                                        if (chkdesc.Contains("Your material Number"))
                                                                                        {
                                                                                            strbrk_line = "Y";
                                                                                            break;
                                                                                        }
                                                                                        else if (chkdesc.Contains("Item partially delivered"))
                                                                                        {
                                                                                            strbrk_line = "Y";
                                                                                            break;
                                                                                        }
                                                                                        else if (chkdesc.Contains("Item completely delivered"))
                                                                                        {
                                                                                            strbrk_line = "Y";
                                                                                            break;
                                                                                        }
                                                                                        else if (chkdesc.Contains("Manufacturing Part Number Manuf. Manuf. pl. Rev. l"))
                                                                                        {
                                                                                            strbrk_line = "Y";
                                                                                            break;
                                                                                        }
                                                                                        else if (chkdesc.Contains("Shipping Address"))
                                                                                        {
                                                                                            strbrk_line = "Y";
                                                                                            break;
                                                                                        }
                                                                                        else if (chkdesc.Contains("Net value incl. disc"))
                                                                                        {
                                                                                            strbrk_line = "Y";
                                                                                            break;
                                                                                        }
                                                                                    }
                                                                                }
                                                                                int Mark_Lns = 0;
                                                                                ItemDescription = ItemDescription.Trim();
                                                                                if (strbrk_line == "Y")
                                                                                {
                                                                                    for (int l = 0; l < Lines_forgetDesc - 1; l++)
                                                                                    {
                                                                                        ItemDescription += Getdesclists[l].Trim() + " ";
                                                                                    }
                                                                                }
                                                                            }
                                                                            //log.WriteLine("Extracted the Complete Descriptions for LINE_NO: " + ItemNo);
                                                                        }
                                                                        ItemDescription = ItemDescription.Trim();
                                                                    }
                                                                    else if (data.StartsWith("Manufacturing Part Number Manuf. Manuf. pl. Rev. l"))
                                                                    {
                                                                        try
                                                                        {

                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }
                                                                    else if (data.StartsWith("Your material Number:"))
                                                                    {
                                                                        //ItemID = data.Replace("Your material Number:", "").Trim();
                                                                        //if (ItemID.Length > 18)
                                                                        //{
                                                                        //    ItemID = ItemID.Substring(0, 18).Trim();
                                                                        //}
                                                                        //log.WriteLine("Extracted the Material_NO(Item ID) for LINE_NO: " + ItemNo);
                                                                    }
                                                                    else if (data.StartsWith("Shipping Address:"))
                                                                    {
                                                                        if (HDRShipAddrLFlag == "N")
                                                                        {
                                                                            string strFulladdr = string.Join(",", datas);
                                                                            string fullText = text.ToString();
                                                                            string fullText2 = "";
                                                                            strFulladdr = strFulladdr.Substring(strFulladdr.IndexOf("Shipping Address:") + Convert.ToString("Shipping Address:").Length);
                                                                            try
                                                                            {
                                                                                fullText2 = strlst[i + 1];
                                                                                fullText2 = fullText2.Substring(fullText2.IndexOf("Item Material/Description Quantity UM Unit Price Net Amount") + Convert.ToString("Item Material/Description Quantity UM Unit Price Net Amount").Length).Trim();
                                                                                fullText2 = fullText2.Substring(0, fullText2.IndexOf("   ")).Trim();
                                                                                fullText2 = fullText2.Replace(",", " ");
                                                                                string[] leftAddr = null;
                                                                                leftAddr = fullText2.Split('\n');
                                                                                int LinesforgetDesc = 0;
                                                                                string strbrkline = "";
                                                                                foreach (string chkdesc in leftAddr)
                                                                                {
                                                                                    LinesforgetDesc++;
                                                                                    if (chkdesc.Contains("Your material Number"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Item partially delivered"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Item completely delivered"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Manufacturing Part Number Manuf. Manuf. pl. Rev. l"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Shipping Address"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Net value incl. disc"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                }
                                                                                int MarkLns = 0;
                                                                                if (strbrkline == "Y")
                                                                                {
                                                                                    foreach (var getdata in leftAddr)
                                                                                    {
                                                                                        MarkLns++;
                                                                                        if (LinesforgetDesc > MarkLns)
                                                                                        {
                                                                                            strFulladdr += " " + getdata + " ";
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            break;
                                                                                        }
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    foreach (var getdata in leftAddr)
                                                                                    {
                                                                                        MarkLns++;
                                                                                        if (LinesforgetDesc > MarkLns)
                                                                                        {
                                                                                            strFulladdr += " " + getdata + " ";
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            break;
                                                                                        }
                                                                                    }
                                                                                }

                                                                            }
                                                                            catch
                                                                            {

                                                                            }
                                                                            strFulladdr = strFulladdr.Replace(",", " ").Trim();
                                                                            strFulladdr = strFulladdr.ToUpper().Replace("SI GROUP", " ").Trim();
                                                                            strFulladdr = strFulladdr.ToUpper().Replace("INC", " ").Trim();
                                                                            string[] splitstr = strFulladdr.Split(' ');
                                                                            string InnerFlagShip = "";
                                                                            foreach (string strchek in splitstr)
                                                                            {
                                                                                if (InnerFlagShip == "Y")
                                                                                {
                                                                                    break;
                                                                                }
                                                                                if (!strchek.ToUpper().Contains("SI GROUP"))
                                                                                {
                                                                                    string[] checkShipAddr = strchek.Split(' ');
                                                                                    foreach (string str in checkShipAddr)
                                                                                    {
                                                                                        if (InnerFlagShip == "Y")
                                                                                        {
                                                                                            break;
                                                                                        }
                                                                                        if (str.Trim() != "")
                                                                                        {
                                                                                            if (ShipTo != null)
                                                                                            {
                                                                                                foreach (DataRow rw in ShipTo.Rows)
                                                                                                {
                                                                                                    string checkdata = Convert.ToString(rw["ADDRESS"]);
                                                                                                    if (checkdata.ToUpper().Contains(str.ToUpper()))
                                                                                                    {
                                                                                                        ShipName = Convert.ToString(rw["DESCR"]);
                                                                                                        ShipAddr1 = Convert.ToString(rw["ADDRESS1"]);
                                                                                                        ShipAddr2 = Convert.ToString(rw["ADDRESS2"]);
                                                                                                        ShipCity = Convert.ToString(rw["CITY"]);
                                                                                                        ShipState = Convert.ToString(rw["STATE"]);
                                                                                                        ShipPostal = Convert.ToString(rw["POSTAL"]);
                                                                                                        InnerFlagShip = "Y";
                                                                                                        break;
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                if (LineItemsCount != 0)
                                                                {
                                                                    DataRow dr_LINE = null;
                                                                    dr_LINE = dt_LINE.NewRow();
                                                                    if (DocNo.Trim() != "")
                                                                    {
                                                                        dr_LINE["PO_ID"] = DocNo.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["PO_ID"] = " ";
                                                                    }

                                                                    if (ItemNo.Trim() != "")
                                                                    {
                                                                        dr_LINE["LINE_NBR"] = ItemNo.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["LINE_NBR"] = "0";
                                                                    }

                                                                    if (Qtys.Trim() != "")
                                                                    {
                                                                        dr_LINE["QTY_REQUESTED"] = Qtys.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["QTY_REQUESTED"] = "0";
                                                                    }

                                                                    if (UnitPrice.Trim() != "")
                                                                    {
                                                                        dr_LINE["PRICE"] = UnitPrice.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["PRICE"] = "0";
                                                                    }
                                                                    if (UM.Trim() != "")
                                                                    {
                                                                        dr_LINE["ISA_CUSTOMER_UOM"] = UM.Trim();
                                                                        dr_LINE["ISA_PRICING_UOM"] = UM.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["ISA_CUSTOMER_UOM"] = " ";
                                                                        dr_LINE["ISA_PRICING_UOM"] = " ";
                                                                    }

                                                                    if (ItemID.Trim() != "")
                                                                    {
                                                                        dr_LINE["ISA_ITEM"] = ItemID.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["ISA_ITEM"] = " ";
                                                                    }
                                                                    ItemDescription = ItemDescription.Replace("_________________", "");
                                                                    if (ItemDescription.Length > 254)
                                                                    {
                                                                        ItemDescription = ItemDescription.Replace("'", " ");
                                                                        ItemDescription = ItemDescription.Substring(0, 254).Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        ItemDescription = ItemDescription.Replace("'", " ");
                                                                    }
                                                                    if (ItemDescription.Trim() != "")
                                                                    {
                                                                        dr_LINE["DESCR254"] = ItemDescription.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["DESCR254"] = " ";
                                                                    }
                                                                    if (VendorID.Trim() != "")
                                                                    {
                                                                        dr_LINE["ITM_ID_VNDR"] = VendorID.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["ITM_ID_VNDR"] = " ";
                                                                    }

                                                                    if (HDRShipAddrLFlag == "N")
                                                                    {
                                                                        if (ShipName.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIP_TO_NAME"] = ShipName.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIP_TO_NAME"] = " ";
                                                                        }

                                                                        if (ShipAddr1.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS1"] = ShipAddr1.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS1"] = " ";
                                                                        }

                                                                        if (ShipAddr2.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS2"] = ShipAddr2.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS2"] = " ";
                                                                        }

                                                                        if (ShipCity.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_CITY"] = ShipCity.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_CITY"] = " ";
                                                                        }

                                                                        if (ShipState.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_STATE"] = ShipState.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_STATE"] = " ";
                                                                        }

                                                                        if (ShipPostal.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_POSTAL"] = ShipPostal.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_POSTAL"] = " ";
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (Ship_Name.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIP_TO_NAME"] = Ship_Name.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIP_TO_NAME"] = " ";
                                                                        }

                                                                        if (Ship_Addr1.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS1"] = Ship_Addr1.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS1"] = " ";
                                                                        }

                                                                        if (Ship_Addr2.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS2"] = Ship_Addr2.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS2"] = " ";
                                                                        }

                                                                        if (Ship_City.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_CITY"] = Ship_City.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_CITY"] = " ";
                                                                        }

                                                                        if (Ship_State.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_STATE"] = Ship_State.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_STATE"] = " ";
                                                                        }

                                                                        if (Ship_Postal.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_POSTAL"] = Ship_Postal.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_POSTAL"] = " ";
                                                                        }

                                                                    }
                                                                    dr_LINE["PREV_INV_ITEM_ID"] = " ";
                                                                    dr_LINE["MFG_ID"] = " ";
                                                                    dr_LINE["MFG_ITM_ID"] = " ";
                                                                    dr_LINE["ISA_LINE_NOTES"] = " ";
                                                                    dt_LINE.Rows.Add(dr_LINE);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            log.WriteLine("PDF Page has not info.");
                                                        }
                                                    }
                                                    else
                                                    {//Otherthan the 1st page
                                                        string full_file = strlst[i];
                                                        if (full_file.Contains("Item Material/Description Quantity UM Unit Price Net Amount"))
                                                        {
                                                            int current_Line = Convert.ToInt32(Convert.ToString("Item Material/Description Quantity UM Unit Price Net Amount ").Length);
                                                            full_file = full_file.Substring(current_Line);
                                                            if (full_file.Contains("The foregoing product brand is a trademark of one or more SI Group, Inc. affiliated companies. ADDIVANT is a trademark of SI Group"))
                                                            {
                                                                int removecontent = full_file.IndexOf("The foregoing product brand is a trademark of one or more SI Group, Inc. affiliated companies. ADDIVANT is a trademark of SI Group");
                                                                full_file = full_file.Substring(0, removecontent);
                                                            }
                                                            string[] datas = full_file.Split('\n');
                                                            List<int> lstItems = new List<int>();
                                                            int LinesCount = 0;
                                                            int LineNumbers = 0;

                                                            List<List<string>> myList = new List<List<string>>();

                                                            foreach (string data in datas)
                                                            {
                                                                LineNumbers += 1;
                                                                if (data.StartsWith("   "))
                                                                {
                                                                    LinesCount += 1;
                                                                    lstItems.Add(LineNumbers - 1);
                                                                    myList.Add(new List<string> { data });
                                                                }
                                                                else
                                                                {
                                                                    if (LinesCount != 0)
                                                                    {
                                                                        lstItems.Add(LineNumbers - 1);
                                                                        myList[LinesCount - 1].Add(data);
                                                                    }
                                                                }
                                                            }

                                                            if (LinesCount != 0)
                                                            {
                                                                foreach (List<string> list in myList)
                                                                {
                                                                    string ItemNo = " ";
                                                                    string ItemID = " ";
                                                                    string Qtys = " ";
                                                                    string UM = " ";
                                                                    string UnitPrice = " ";
                                                                    string ItemDescription = " ";

                                                                    string ShipName = " ";
                                                                    string ShipAddr1 = " ";
                                                                    string ShipAddr2 = " ";
                                                                    string ShipCity = " ";
                                                                    string ShipState = " ";
                                                                    string ShipPostal = " ";

                                                                    foreach (string data in list)
                                                                    {
                                                                        if (data.StartsWith("   "))
                                                                        {

                                                                            string newstr = data.Replace("   ", "").ToString();
                                                                            int z = newstr.IndexOf(" ") + 1;

                                                                            ItemNo = newstr.Substring(0, z);
                                                                            newstr = newstr.Substring(z);

                                                                            string[] wordslist = data.Split(' ');
                                                                            string getqtys = "";
                                                                            foreach (string word in wordslist)
                                                                            {
                                                                                bool result = false;
                                                                                result = Regex.IsMatch(word, @"^\d*\.\d*$");
                                                                                if (result)
                                                                                {
                                                                                    getqtys = word;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            Qtys = getqtys;
                                                                            int getint = newstr.IndexOf(getqtys);
                                                                            string splitstrs = newstr.Substring(0, getint);
                                                                            string descriptionsValue = splitstrs;
                                                                            newstr = newstr.Substring(newstr.IndexOf(getqtys) + getqtys.Length);

                                                                            newstr = newstr.Trim();

                                                                            int getUOM = newstr.IndexOf(" ") + 1;
                                                                            UM = newstr.Substring(0, getUOM);
                                                                            newstr = newstr.Substring(newstr.IndexOf(UM) + UM.Length);

                                                                            newstr = newstr.Trim();

                                                                            string[] wordslist2 = newstr.Split(' ');
                                                                            string get_Unitprice = "";
                                                                            foreach (string word in wordslist2)
                                                                            {
                                                                                bool result = false;
                                                                                result = Regex.IsMatch(word, @"^\d+(?:[\,]?\d+([\.]\d+))?$");
                                                                                if (result)
                                                                                {
                                                                                    get_Unitprice = word;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            if (get_Unitprice.Trim() == "")
                                                                            {
                                                                                if (wordslist2.Count() > 0)
                                                                                {
                                                                                    get_Unitprice = wordslist2[0];
                                                                                    get_Unitprice = get_Unitprice.Trim();
                                                                                }
                                                                            }
                                                                            UnitPrice = get_Unitprice.Replace(",", "");

                                                                            string strcurrenttext = data.Replace("   ", "").ToString();
                                                                            if (strcurrenttext.Trim() != "")
                                                                            {
                                                                                string itemno = "";
                                                                                itemno = strcurrenttext.Substring(0, strcurrenttext.IndexOf(Qtys));
                                                                                itemno = itemno.Replace(ItemNo, "");
                                                                                itemno = itemno.Replace(Qtys, "");
                                                                                itemno = itemno.Replace("  ", "");
                                                                                itemno = itemno.Trim();

                                                                                if (itemno.Length > 18)
                                                                                {
                                                                                    ItemID = itemno.Substring(0, 18).Trim();
                                                                                }
                                                                                else {
                                                                                    ItemID = itemno.Trim();
                                                                                }
                                                                            }

                                                                            int LinesforgetDesc = 0;
                                                                            string strbrkline = "";
                                                                            foreach (string chkdesc in list)
                                                                            {
                                                                                LinesforgetDesc++;
                                                                                if (chkdesc.Contains("Your material Number"))
                                                                                {
                                                                                    strbrkline = "Y";
                                                                                    break;
                                                                                }
                                                                                else if (chkdesc.Contains("Item partially delivered"))
                                                                                {
                                                                                    strbrkline = "Y";
                                                                                    break;
                                                                                }
                                                                                else if (chkdesc.Contains("Item completely delivered"))
                                                                                {
                                                                                    strbrkline = "Y";
                                                                                    break;
                                                                                }
                                                                                else if (chkdesc.Contains("Manufacturing Part Number Manuf. Manuf. pl. Rev. l"))
                                                                                {
                                                                                    strbrkline = "Y";
                                                                                    break;
                                                                                }
                                                                                else if (chkdesc.Contains("Shipping Address"))
                                                                                {
                                                                                    strbrkline = "Y";
                                                                                    break;
                                                                                }
                                                                                else if (chkdesc.Contains("Net value incl. disc"))
                                                                                {
                                                                                    strbrkline = "Y";
                                                                                    break;
                                                                                }
                                                                            }
                                                                            int MarkLns = 0;
                                                                            if (strbrkline == "Y")
                                                                            {
                                                                                for (int l = 1; l < LinesforgetDesc - 1; l++)
                                                                                {
                                                                                    ItemDescription += list[l] + " ";
                                                                                }
                                                                            }
                                                                            else
                                                                            {

                                                                                for (int l = 1; l < LinesforgetDesc; l++)
                                                                                {
                                                                                    ItemDescription += list[l] + " ";
                                                                                }

                                                                                string GetDescNextPage = "";
                                                                                if ((strlst.Count() - 1) > (i + 1))
                                                                                {
                                                                                    GetDescNextPage = strlst[i + 1];
                                                                                    GetDescNextPage = GetDescNextPage.Substring(GetDescNextPage.IndexOf("Item Material/Description Quantity UM Unit Price Net Amount") + Convert.ToString("Item Material/Description Quantity UM Unit Price Net Amount").Length).Trim();
                                                                                    string[] Getdesclists = null;
                                                                                    if (!GetDescNextPage.StartsWith("\n   "))
                                                                                    {
                                                                                        Getdesclists = GetDescNextPage.Split('\n');
                                                                                    }
                                                                                    int Lines_forgetDesc = 0;
                                                                                    string strbrk_line = "";

                                                                                    if (Getdesclists != null)
                                                                                    {
                                                                                        foreach (string chkdesc in Getdesclists)
                                                                                        {
                                                                                            Lines_forgetDesc++;
                                                                                            if (chkdesc.Contains("Your material Number"))
                                                                                            {
                                                                                                strbrk_line = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Item partially delivered"))
                                                                                            {
                                                                                                strbrk_line = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Item completely delivered"))
                                                                                            {
                                                                                                strbrk_line = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Manufacturing Part Number Manuf. Manuf. pl. Rev. l"))
                                                                                            {
                                                                                                strbrk_line = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Shipping Address"))
                                                                                            {
                                                                                                strbrk_line = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Net value incl. disc"))
                                                                                            {
                                                                                                strbrk_line = "Y";
                                                                                                break;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    ItemDescription = ItemDescription.Trim();
                                                                                    if (strbrk_line == "Y")
                                                                                    {
                                                                                        for (int l = 0; l < Lines_forgetDesc; l++)
                                                                                        {
                                                                                            ItemDescription += Getdesclists[l].Trim() + " ";
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                            ItemDescription = ItemDescription.Trim();
                                                                        }
                                                                        else if (data.StartsWith("Your material Number:"))
                                                                        {
                                                                            //ItemID = data.Replace("Your material Number:", "").Trim();
                                                                            //if (ItemID.Length > 18)
                                                                            //{
                                                                            //    ItemID = ItemID.Substring(0, 18).Trim();
                                                                            //}
                                                                        }
                                                                        else if (data.StartsWith("Shipping Address:"))
                                                                        {
                                                                            string OverAllChlforShipAddr = "";
                                                                            if (HDRShipAddrLFlag == "N")
                                                                            {
                                                                                string strFulladdr = string.Join(",", list);
                                                                                strFulladdr = strFulladdr.Substring(strFulladdr.IndexOf("Shipping Address:") + Convert.ToString("Shipping Address:").Length);
                                                                                string[] left_Addr = strFulladdr.Split(',');

                                                                                int LinesforgetDesc = 0;
                                                                                string strbrkline = "";
                                                                                foreach (string chkdesc in left_Addr)
                                                                                {
                                                                                    LinesforgetDesc++;
                                                                                    if (chkdesc.Contains("Your material Number"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Item partially delivered"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Item completely delivered"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Manufacturing Part Number Manuf. Manuf. pl. Rev. l"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Shipping Address"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                    else if (chkdesc.Contains("Net value incl. disc"))
                                                                                    {
                                                                                        strbrkline = "Y";
                                                                                        break;
                                                                                    }
                                                                                }
                                                                                int MarkLns = 0;
                                                                                string getshipaddr = "";
                                                                                foreach (var getdata in left_Addr)
                                                                                {
                                                                                    MarkLns++;
                                                                                    if (LinesforgetDesc > MarkLns)
                                                                                    {
                                                                                        getshipaddr += " " + getdata + " ";
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        break;
                                                                                    }
                                                                                }
                                                                                getshipaddr = getshipaddr.Replace(",", " ").Trim();
                                                                                getshipaddr = getshipaddr.ToUpper().Replace("SI GROUP", " ").Trim();
                                                                                getshipaddr = getshipaddr.ToUpper().Replace("INC", " ").Trim();
                                                                                string[] split_str = getshipaddr.Split(' ');
                                                                                string InnerFlagShip = "";
                                                                                foreach (string strchek in split_str)
                                                                                {
                                                                                    if (InnerFlagShip == "Y")
                                                                                    {
                                                                                        break;
                                                                                    }
                                                                                    if (strchek.Trim() != "")
                                                                                    {
                                                                                        if (strchek.Trim() != ".")
                                                                                        {
                                                                                            if (!strchek.ToUpper().Contains("SI GROUP"))
                                                                                            {
                                                                                                string[] checkShipAddr = strchek.Split(' ');
                                                                                                foreach (string str in checkShipAddr)
                                                                                                {
                                                                                                    if (InnerFlagShip == "Y")
                                                                                                    {
                                                                                                        break;
                                                                                                    }
                                                                                                    if (str.Trim() != "")
                                                                                                    {
                                                                                                        if (ShipTo != null)
                                                                                                        {
                                                                                                            foreach (DataRow rw in ShipTo.Rows)
                                                                                                            {
                                                                                                                string checkdata = Convert.ToString(rw["ADDRESS"]);
                                                                                                                if (checkdata.ToUpper().Contains(str.ToUpper()))
                                                                                                                {
                                                                                                                    ShipName = Convert.ToString(rw["DESCR"]);
                                                                                                                    ShipAddr1 = Convert.ToString(rw["ADDRESS1"]);
                                                                                                                    ShipAddr2 = Convert.ToString(rw["ADDRESS2"]);
                                                                                                                    ShipCity = Convert.ToString(rw["CITY"]);
                                                                                                                    ShipState = Convert.ToString(rw["STATE"]);
                                                                                                                    ShipPostal = Convert.ToString(rw["POSTAL"]);
                                                                                                                    InnerFlagShip = "Y";
                                                                                                                    OverAllChlforShipAddr = "Y";
                                                                                                                    break;
                                                                                                                }

                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                            if (OverAllChlforShipAddr == "")
                                                                            {
                                                                                if (HDRShipAddrLFlag == "N")
                                                                                {
                                                                                    string str_Fulladdr = "";
                                                                                    try
                                                                                    {
                                                                                        string fulltext3 = "";
                                                                                        fulltext3 = strlst[i + 1];
                                                                                        fulltext3 = fulltext3.Substring(fulltext3.IndexOf("Item Material/Description Quantity UM Unit Price Net Amount") + Convert.ToString("Item Material/Description Quantity UM Unit Price Net Amount").Length).Trim();
                                                                                        fulltext3 = fulltext3.Substring(0, fulltext3.IndexOf("   ")).Trim();
                                                                                        string[] leftAddr = null;
                                                                                        leftAddr = fulltext3.Split('\n');
                                                                                        int LinesforgetDesc = 0;
                                                                                        string strbrkline = "";
                                                                                        foreach (string chkdesc in leftAddr)
                                                                                        {
                                                                                            LinesforgetDesc++;
                                                                                            if (chkdesc.Contains("Your material Number"))
                                                                                            {
                                                                                                strbrkline = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Item partially delivered"))
                                                                                            {
                                                                                                strbrkline = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Item completely delivered"))
                                                                                            {
                                                                                                strbrkline = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Manufacturing Part Number Manuf. Manuf. pl. Rev. l"))
                                                                                            {
                                                                                                strbrkline = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Shipping Address"))
                                                                                            {
                                                                                                strbrkline = "Y";
                                                                                                break;
                                                                                            }
                                                                                            else if (chkdesc.Contains("Net value incl. disc"))
                                                                                            {
                                                                                                strbrkline = "Y";
                                                                                                break;
                                                                                            }
                                                                                        }
                                                                                        int MarkLns = 0;
                                                                                        foreach (var getdata in leftAddr)
                                                                                        {
                                                                                            MarkLns++;
                                                                                            if (LinesforgetDesc > MarkLns)
                                                                                            {
                                                                                                str_Fulladdr += " " + getdata + " ";
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                break;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    catch (Exception ex)
                                                                                    {

                                                                                    }
                                                                                    str_Fulladdr = str_Fulladdr.Replace(",", " ").Trim();
                                                                                    str_Fulladdr = str_Fulladdr.ToUpper().Replace("SI GROUP", " ").Trim();
                                                                                    str_Fulladdr = str_Fulladdr.ToUpper().Replace("INC", " ").Trim();
                                                                                    string[] splitstr = str_Fulladdr.Split(' ');
                                                                                    string InnerFlagShip = "";
                                                                                    foreach (string strchek in splitstr)
                                                                                    {
                                                                                        if (InnerFlagShip == "Y")
                                                                                        {
                                                                                            break;
                                                                                        }
                                                                                        if (!strchek.ToUpper().Contains("SI GROUP"))
                                                                                        {
                                                                                            string[] checkShipAddr = strchek.Split(' ');
                                                                                            foreach (string str in checkShipAddr)
                                                                                            {
                                                                                                if (InnerFlagShip == "Y")
                                                                                                {
                                                                                                    break;
                                                                                                }
                                                                                                if (str.Trim() != "")
                                                                                                {
                                                                                                    if (ShipTo != null)
                                                                                                    {
                                                                                                        foreach (DataRow rw in ShipTo.Rows)
                                                                                                        {
                                                                                                            string checkdata = Convert.ToString(rw["ADDRESS"]);
                                                                                                            if (checkdata.ToUpper().Contains(str.ToUpper()))
                                                                                                            {
                                                                                                                ShipName = Convert.ToString(rw["DESCR"]);
                                                                                                                ShipAddr1 = Convert.ToString(rw["ADDRESS1"]);
                                                                                                                ShipAddr2 = Convert.ToString(rw["ADDRESS2"]);
                                                                                                                ShipCity = Convert.ToString(rw["CITY"]);
                                                                                                                ShipState = Convert.ToString(rw["STATE"]);
                                                                                                                ShipPostal = Convert.ToString(rw["POSTAL"]);
                                                                                                                InnerFlagShip = "Y";
                                                                                                                break;
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    DataRow dr_LINE = null;
                                                                    dr_LINE = dt_LINE.NewRow();
                                                                    if (DocNo.Trim() != "")
                                                                    {
                                                                        dr_LINE["PO_ID"] = DocNo.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["PO_ID"] = " ";
                                                                    }
                                                                    if (ItemNo.Trim() != "")
                                                                    {
                                                                        dr_LINE["LINE_NBR"] = ItemNo.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["LINE_NBR"] = "0";
                                                                    }
                                                                    if (Qtys.Trim() != "")
                                                                    {
                                                                        dr_LINE["QTY_REQUESTED"] = Qtys.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["QTY_REQUESTED"] = "0";
                                                                    }
                                                                    if (UnitPrice.Trim() != "")
                                                                    {
                                                                        dr_LINE["PRICE"] = UnitPrice.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["PRICE"] = "0";
                                                                    }
                                                                    if (UM.Trim() != "")
                                                                    {
                                                                        dr_LINE["ISA_CUSTOMER_UOM"] = UM.Trim();
                                                                        dr_LINE["ISA_PRICING_UOM"] = UM.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["ISA_CUSTOMER_UOM"] = " ";
                                                                        dr_LINE["ISA_PRICING_UOM"] = " ";
                                                                    }

                                                                    if (ItemID.Trim() != "")
                                                                    {
                                                                        dr_LINE["ISA_ITEM"] = ItemID.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["ISA_ITEM"] = " ";
                                                                    }
                                                                    ItemDescription = ItemDescription.Replace("_________________", "");
                                                                    if (ItemDescription.Length > 254)
                                                                    {
                                                                        ItemDescription = ItemDescription.Replace("'", " ");
                                                                        ItemDescription = ItemDescription.Substring(0, 254).Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        ItemDescription = ItemDescription.Replace("'", " ");
                                                                    }
                                                                    if (ItemDescription.Trim() != "")
                                                                    {
                                                                        dr_LINE["DESCR254"] = ItemDescription.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["DESCR254"] = " ";
                                                                    }
                                                                    if (VendorID.Trim() != "")
                                                                    {
                                                                        dr_LINE["ITM_ID_VNDR"] = VendorID.Trim();
                                                                    }
                                                                    else
                                                                    {
                                                                        dr_LINE["ITM_ID_VNDR"] = " ";
                                                                    }

                                                                    if (HDRShipAddrLFlag == "N")
                                                                    {
                                                                        if (ShipName.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIP_TO_NAME"] = ShipName.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIP_TO_NAME"] = " ";
                                                                        }

                                                                        if (ShipAddr1.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS1"] = ShipAddr1.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS1"] = " ";
                                                                        }

                                                                        if (ShipAddr2.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS2"] = ShipAddr2.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS2"] = " ";
                                                                        }

                                                                        if (ShipCity.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_CITY"] = ShipCity.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_CITY"] = " ";
                                                                        }

                                                                        if (ShipState.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_STATE"] = ShipState.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_STATE"] = " ";
                                                                        }

                                                                        if (ShipPostal.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_POSTAL"] = ShipPostal.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_POSTAL"] = " ";
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (Ship_Name.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIP_TO_NAME"] = Ship_Name.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIP_TO_NAME"] = " ";
                                                                        }

                                                                        if (Ship_Addr1.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS1"] = Ship_Addr1.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS1"] = " ";
                                                                        }

                                                                        if (Ship_Addr2.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS2"] = Ship_Addr2.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_ADDRESS2"] = " ";
                                                                        }

                                                                        if (Ship_City.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_CITY"] = Ship_City.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_CITY"] = " ";
                                                                        }

                                                                        if (Ship_State.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_STATE"] = Ship_State.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_STATE"] = " ";
                                                                        }

                                                                        if (Ship_Postal.Trim() != "")
                                                                        {
                                                                            dr_LINE["SHIPTO_POSTAL"] = Ship_Postal.Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            dr_LINE["SHIPTO_POSTAL"] = " ";
                                                                        }

                                                                    }
                                                                    dr_LINE["PREV_INV_ITEM_ID"] = " ";
                                                                    dr_LINE["MFG_ID"] = " ";
                                                                    dr_LINE["MFG_ITM_ID"] = " ";
                                                                    dr_LINE["ISA_LINE_NOTES"] = " ";
                                                                    dt_LINE.Rows.Add(dr_LINE);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            log.WriteLine("PDF Page " + (i + 1) + " has no informations.");
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                log.WriteLine("PDF contains 'Change to Purchase order' data so we are forwarding the mail.");
                                                string paths = appPath + @"PDFAttachments\" + fileAttachment.Name;
                                                FileStream fs = new FileStream(paths, FileMode.Open, FileAccess.Read);
                                                BinaryReader br = new BinaryReader(fs);
                                                Byte[] bytesval = br.ReadBytes(Convert.ToInt32(fs.Length));
                                                br.Close();
                                                fs.Close();
                                                string sampleBody = "We have receivied the 'Change to Purchase order' PDF file so we are forwarding it to you.";
                                                Send_MailAttachmentEmail(null, "", Convert.ToString(""), "", item, attch, bytesval, sampleBody);
                                                item.Move(processedFolderID);
                                                log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                                            }
                                        }
                                        else
                                        {
                                            log.WriteLine("PDF files does not have any datas.");
                                            string paths = appPath + @"PDFAttachments\" + fileAttachment.Name;
                                            FileStream fs = new FileStream(paths, FileMode.Open, FileAccess.Read);
                                            BinaryReader br = new BinaryReader(fs);
                                            Byte[] bytesval = br.ReadBytes(Convert.ToInt32(fs.Length));
                                            br.Close();
                                            fs.Close();

                                            Send_MailAttachmentEmail(null, "", Convert.ToString(""), "", item, attch, bytesval);
                                            item.Move(processedFolderID);
                                            log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                                        }
                                    }
                                    else
                                    {
                                        log.WriteLine("PDF attachment is invalid.");
                                    }
                                    int insertedHDRTbl = 0;
                                    int insertedLNTbl = 0;
                                    if (dt_HDR.Rows.Count > 0)
                                    {
                                        foreach (DataRow drs in dt_HDR.Rows)
                                        {
                                            bool HDR_inserted = false;
                                            string strInsertHDRQuery = "INSERT Into SYSADM8.PS_ISA_SI_PO_HDR (PO_ID, PO_DT, CONTACT_NAME, " + System.Environment.NewLine +
                                                                        "CONTACT_PHONE, CONTACT_FAX, CONTACT_EMAIL, TAXABLE_CD, DUE_DT, ADD_DTTM, PROCESS_FLAG, PROCESS_INSTANCE) " + System.Environment.NewLine +
                                                                        "Values('" + drs["PO_ID"] + "', TO_DATE('" + drs["PO_DT"] + "','MM-DD-YYYY'), " + System.Environment.NewLine +
                                                                        "'" + drs["CONTACT_NAME"] + "', '" + drs["CONTACT_PHONE"] + "', '" + drs["CONTACT_FAX"] + "', " + System.Environment.NewLine +
                                                                        "'" + drs["CONTACT_EMAIL"] + "', 'Y', TO_DATE('" + drs["DUE_DT"] + "','MM-DD-YYYY'), SYSDATE, 'N', 0)";
                                            HDR_inserted = InsertTbl_HDR(drs);
                                            if (HDR_inserted)
                                            {
                                                log.WriteLine("INSERTED into the HDR table for the Document No " + Convert.ToString(drs["PO_ID"]));
                                                insertedHDRTbl++;
                                            }
                                            else
                                            {
                                                log.WriteLine("Error while INSERTING into the HDR table for the Document No " + Convert.ToString(drs["PO_ID"]));
                                                log.WriteLine("HDR Insert Query: " + strInsertHDRQuery);
                                            }
                                        }
                                    }
                                    if (dt_LINE.Rows.Count > 0)
                                    {
                                        foreach (DataRow drs in dt_LINE.Rows)
                                        {
                                            bool LN_inserted = false;
                                            string strInsertLNQuery = "INSERT INTO SYSADM8.PS_ISA_SI_PO_LN (PO_ID, LINE_NBR, ISA_ITEM, PREV_INV_ITEM_ID, " + System.Environment.NewLine +
                                                                        "QTY_REQUESTED, PRICE, ISA_CUSTOMER_UOM, ISA_PRICING_UOM, ITM_ID_VNDR, MFG_ID, MFG_ITM_ID, DESCR254, " + System.Environment.NewLine +
                                                                        "SHIP_TO_NAME, SHIPTO_ADDRESS1, SHIPTO_ADDRESS2, SHIPTO_CITY, SHIPTO_STATE, SHIPTO_POSTAL, ISA_LINE_NOTES, " + System.Environment.NewLine +
                                                                        "ADD_DTTM, PROCESS_FLAG, PROCESS_INSTANCE) " + System.Environment.NewLine +
                                                                        "VALUES('" + drs["PO_ID"] + "', " + drs["LINE_NBR"] + ", " + System.Environment.NewLine +
                                                                        "'" + drs["ISA_ITEM"] + "', '" + drs["PREV_INV_ITEM_ID"] + "', " + drs["QTY_REQUESTED"] + ", " + drs["PRICE"] + ", " + System.Environment.NewLine +
                                                                        "'" + drs["ISA_CUSTOMER_UOM"] + "', '" + drs["ISA_PRICING_UOM"] + "', '" + drs["ITM_ID_VNDR"] + "', ' ', ' ', " + System.Environment.NewLine +
                                                                        "'" + drs["DESCR254"] + "', '" + drs["SHIP_TO_NAME"] + "', '" + drs["SHIPTO_ADDRESS1"] + "', " + System.Environment.NewLine +
                                                                        "'" + drs["SHIPTO_ADDRESS2"] + "', '" + drs["SHIPTO_CITY"] + "', '" + drs["SHIPTO_STATE"] + "', " + System.Environment.NewLine +
                                                                        "'" + drs["SHIPTO_POSTAL"] + "', ' ', SYSDATE, 'N', 0)";
                                            LN_inserted = InsertTbl_LN(drs);
                                            if (LN_inserted)
                                            {
                                                log.WriteLine("INSERTED into the Line table for the Line No " + Convert.ToString(drs["LINE_NBR"]));
                                                insertedLNTbl++;
                                            }
                                            else
                                            {
                                                log.WriteLine("Error while INSERTING into the Line table for the Line No " + Convert.ToString(drs["LINE_NBR"]));
                                                log.WriteLine("LINE Insert Query: " + strInsertLNQuery);
                                            }
                                        }
                                    }
                                    //Email moved to Processed folder
                                    if (insertedHDRTbl > 0 && insertedLNTbl > 0)
                                    {
                                        item.Move(processedFolderID);
                                        log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                                        //log.WriteLine("-------------------------------------------------------------");

                                    }
                                    else {
                                        string paths = appPath + @"PDFAttachments\" + fileAttachment.Name;
                                        FileStream fs = new FileStream(paths, FileMode.Open, FileAccess.Read);
                                        BinaryReader br = new BinaryReader(fs);
                                        Byte[] bytesval = br.ReadBytes(Convert.ToInt32(fs.Length));
                                        br.Close();
                                        fs.Close();
                                        Send_MailAttachmentEmail(null, "", "", "", item, attch, bytesval);
                                        item.Move(processedFolderID);
                                        log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                                    }
                                    //Delete the Attachment file in AttachmentFile folder
                                    System.IO.DirectoryInfo Attchment_Folder = new DirectoryInfo(appPath + @"PDFAttachments");
                                    foreach (FileInfo file in Attchment_Folder.GetFiles())
                                    {
                                        file.Delete();
                                    }
                                    //System.IO.File.Delete(appPath + @"AttachmentFile\" + fileAttachment.Name);
                                    log.WriteLine("Deleted the Attachment file from the PDF Attachments folder");
                                    log.WriteLine("-------------------------------------------------------------");
                                }
                                catch (Exception ex)
                                {
                                    log.WriteLine("Error while processing the PDF file. Error : " + ex.Message);
                                    SendErrorEmail(ex, ex.Message, Convert.ToString(ex.InnerException), "");

                                    string paths = appPath + @"PDFAttachments\" + fileAttachment.Name;
                                    FileStream fs = new FileStream(paths, FileMode.Open, FileAccess.Read);
                                    BinaryReader br = new BinaryReader(fs);
                                    Byte[] bytesval = br.ReadBytes(Convert.ToInt32(fs.Length));
                                    br.Close();
                                    fs.Close();
                                    Send_MailAttachmentEmail(ex, ex.Message, Convert.ToString(ex.InnerException), "", item, attch, bytesval);
                                    item.Move(processedFolderID);
                                    log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                                    continue;
                                }
                            }//IF PDF condition ENDs
                        }
                        if (filePDFcount == 0)
                        {
                            log.WriteLine("Mail Doesnt contains any PDF attachments. So we are moving to processed folder.");
                            item.Move(processedFolderID);
                            log.WriteLine("Moved the email to " + ProcessedFolderName + " folder.");
                            log.WriteLine("-------------------------------------------------------------");
                        }
                    }
                }
                else
                {
                    log.WriteLine("No Mails availible at this times.");
                }
                log.Close();
            }
            catch (Exception ex)
            {
                SendErrorEmail(ex, ex.Message, Convert.ToString(ex.InnerException), "");
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

        private static Boolean InsertTbl_HDR(DataRow dr_HDR)
        {
            Boolean reslt = false;
            try
            {
                int rowsaffected;
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();

                string strInsertHDRQuery = "INSERT Into SYSADM8.PS_ISA_SI_PO_HDR (PO_ID, PO_DT, CONTACT_NAME, " + System.Environment.NewLine +
                    "CONTACT_PHONE, CONTACT_FAX, CONTACT_EMAIL, TAXABLE_CD, DUE_DT, ADD_DTTM, PROCESS_FLAG, PROCESS_INSTANCE) " + System.Environment.NewLine +
                    "Values('" + dr_HDR["PO_ID"] + "', TO_DATE('" + dr_HDR["PO_DT"] + "','MM-DD-YYYY'), " + System.Environment.NewLine +
                    "'" + dr_HDR["CONTACT_NAME"] + "', '" + dr_HDR["CONTACT_PHONE"] + "', '" + dr_HDR["CONTACT_FAX"] + "', " + System.Environment.NewLine +
                    "'" + dr_HDR["CONTACT_EMAIL"] + "', 'Y', TO_DATE('" + dr_HDR["DUE_DT"] + "','MM-DD-YYYY'), SYSDATE, 'N', 0)";


                com = new OleDbCommand(strInsertHDRQuery, cn);
                rowsaffected = com.ExecuteNonQuery();

                cn.Close();
                cn.Dispose();
                if (rowsaffected == 1)
                {
                    reslt = true;

                }
                else
                {
                    reslt = false;
                }
                return reslt;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static Boolean InsertTbl_LN(DataRow dr_LN)
        {
            Boolean reslt = false;
            try
            {
                int rowsaffected;
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();

                string strInsertLNQuery = "INSERT INTO SYSADM8.PS_ISA_SI_PO_LN (PO_ID, LINE_NBR, ISA_ITEM, PREV_INV_ITEM_ID, " + System.Environment.NewLine +
                "QTY_REQUESTED, PRICE, ISA_CUSTOMER_UOM, ISA_PRICING_UOM, ITM_ID_VNDR, MFG_ID, MFG_ITM_ID, DESCR254, " + System.Environment.NewLine +
                "SHIP_TO_NAME, SHIPTO_ADDRESS1, SHIPTO_ADDRESS2, SHIPTO_CITY, SHIPTO_STATE, SHIPTO_POSTAL, ISA_LINE_NOTES, " + System.Environment.NewLine +
                "ADD_DTTM, PROCESS_FLAG, PROCESS_INSTANCE) " + System.Environment.NewLine +
                "VALUES('" + dr_LN["PO_ID"] + "', " + dr_LN["LINE_NBR"] + ", " + System.Environment.NewLine +
                "'" + dr_LN["ISA_ITEM"] + "', '" + dr_LN["PREV_INV_ITEM_ID"] + "', " + dr_LN["QTY_REQUESTED"] + ", " + dr_LN["PRICE"] + ", " + System.Environment.NewLine +
                "'" + dr_LN["ISA_CUSTOMER_UOM"] + "', '" + dr_LN["ISA_PRICING_UOM"] + "', '" + dr_LN["ITM_ID_VNDR"] + "', ' ', ' ', " + System.Environment.NewLine +
                "'" + dr_LN["DESCR254"] + "', '" + dr_LN["SHIP_TO_NAME"] + "', '" + dr_LN["SHIPTO_ADDRESS1"] + "', " + System.Environment.NewLine +
                "'" + dr_LN["SHIPTO_ADDRESS2"] + "', '" + dr_LN["SHIPTO_CITY"] + "', '" + dr_LN["SHIPTO_STATE"] + "', " + System.Environment.NewLine +
                "'" + dr_LN["SHIPTO_POSTAL"] + "', ' ', SYSDATE, 'N', 0)";


                com = new OleDbCommand(strInsertLNQuery, cn);
                rowsaffected = com.ExecuteNonQuery();

                cn.Close();
                cn.Dispose();
                if (rowsaffected == 1)
                {
                    reslt = true;

                }
                else
                {
                    reslt = false;
                }
                return reslt;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Boolean Send_MailAttachmentEmail(Exception exception, string strMessage, string InnerExcp, string MethodName, Item MailMsg, Microsoft.Exchange.WebServices.Data.Attachment attachmentsFile, Byte[] bytes, string Bodytext = "")
        {
            string strbodyhead;
            string strbodydetl = string.Empty;
            Boolean isEmailSent = false;
            SIPDFProcess.SDIEmailUtility.EmailServices SDIEmailService = new SIPDFProcess.SDIEmailUtility.EmailServices();
            String DbUrl = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper();
            string toaddress = ConfigurationManager.AppSettings["ToAddessForSend_MailAttachmentEmail"];

            try
            {
                MailMessage Mailer = new MailMessage();
                string FromAddress = "SDIExchADMIN@SDI.com";
                string Mailcc = "";
                string MailBcc = "webdev@sdi.com;";
                //StackTrace trace = new StackTrace(exception, true);
                //string fileName = trace.GetFrame(0).GetFileName();
                //int lineNo = trace.GetFrame(0).GetFileLineNumber();               
                strbodydetl = strbodydetl + "<div>";
                strbodydetl = strbodydetl + "<p >Team, </p>";
                //strbodydetl = strbodydetl + "&nbsp;<BR>";
                if (Bodytext != "")
                {
                    strbodydetl = strbodydetl + "<p >" + Bodytext;                    
                }
                else {
                    strbodydetl = strbodydetl + "<p >There was an issue while processing the PDF file. Please check if it’s a valid PDF.";
                }                
                //strbodydetl = strbodydetl + "Method Name :<span>    </span>" + MethodName + "<BR>";
                //strbodydetl = strbodydetl + "Error Message :<span>    </span>" + strMessage + "<BR>";
                //strbodydetl = strbodydetl + "Inner Exception :<span>    </span>" + InnerExcp + "<BR>";
                //strbodydetl = strbodydetl + "Error Line No :<span>    </span>" + lineNo + "<BR>";
                //strbodydetl = strbodydetl + "Date:<span>    </span>" + DateTime.Now + "<BR>";
                //strbodydetl = strbodydetl + "&nbsp;<br>";
                strbodydetl = strbodydetl + "&nbsp;</p>";
                strbodydetl = strbodydetl + "</div>";

                Mailer.Body = strbodydetl;

                if (DbUrl == "PLGR" | DbUrl == "STAR" | DbUrl == "DEVL" | DbUrl == "RPTG")
                {
                    Mailer.To.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.To.Add(new MailAddress("avacorp@sdi.com"));
                    Mailer.Subject = "<<TEST SITE>> PDF attachment processing Error";
                }
                else
                {
                    Mailer.To.Add(new MailAddress(toaddress));
                    Mailer.Subject = "PDF attachment processing Error";
                    Mailer.Bcc.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.Bcc.Add(new MailAddress("avacorp@sdi.com"));
                }

                OleDbConnection connectionEmail = new OleDbConnection((Convert.ToString(ConfigurationManager.AppSettings["ConString"])));
                string[] MailAttachmentName = new string[] { attachmentsFile.Name };

                List<byte[]> MailAttachmentbytes = new List<byte[]>();
                MailAttachmentbytes.Add(bytes);


                try
                {
                    SDIEmailService.EmailUtilityServices("MailandStore", "SDIExchange@SDI.com", Mailer.To.ToString(), Mailer.Subject, string.Empty, Mailer.Bcc.ToString(), Mailer.Body, "SDIERRMAIL", MailAttachmentName, MailAttachmentbytes.ToArray());
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

        public static Boolean SendErrorEmail(Exception exception, string strMessage, string InnerExcp, string MethodName)
        {
            string strbodyhead;
            string strbodydetl = string.Empty;
            Boolean isEmailSent = false;
            SIPDFProcess.SDIEmailUtility.EmailServices SDIEmailService = new SIPDFProcess.SDIEmailUtility.EmailServices();
            String DbUrl = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper();

            string toaddress = ConfigurationManager.AppSettings["UserName"];

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
                    Mailer.Subject = "<<TEST SITE>> PDF processing Error";
                }
                else
                {
                    Mailer.To.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.To.Add(new MailAddress("avacorp@sdi.com"));
                    Mailer.Subject = "PDF processing Error";
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

        public static DataTable GetShipAddress()
        {
            string strGetShip = "SELECT (DESCR || ' - ' || CITY) as ADDRESS,LOCATION, DESCR, ADDRESS1,ADDRESS2,CITY,STATE,POSTAL FROM PS_LOCATION_TBL WHERE DESCR LIKE '%SI Group%'";
            DataSet dsQuery = null;
            try
            {
                dsQuery = GetAdapter(strGetShip);
                if (dsQuery != null)
                {
                    if (dsQuery.Tables[0].Rows.Count > 0)
                    {
                        return dsQuery.Tables[0];
                    }
                    else
                    {
                        return dsQuery.Tables[0];
                    }
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static DataSet GetAdapter(string p_strQuery, bool bGoToErrPage = true, bool bThrowBackError = false)
        {
            // Gives us a reference to the current asp.net 
            // application executing the method.
            //HttpApplication currentApp = HttpContext.Current.ApplicationInstance;
            string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(Command);

                System.Data.DataSet UserdataSet = new System.Data.DataSet();

                dataAdapter.Fill(UserdataSet);
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
                return UserdataSet;
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
                //string errorMessage = objException.Message;
                //sendErrorEmail(objException.ToString() + "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery + " --- from ORDBData.vb, GetAdapter. User supposed to see DBErrorPage.aspx");
                //string sMsg66 = Strings.LCase(objException.ToString());
                //if (bGoToErrPage)
                //    ProcessError(sMsg66);
                //else if (bThrowBackError)
                //    throw objException;
                //else
                //    return null/* TODO Change to default(_) if this is not a reference type */;
                return null;
            }
        }

    }
}
