using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Xls;
using System.Data;
using System.IO;
using System.Configuration;
using UpsIntegration;
using Oracle.DataAccess.Client;
using System.Data.OleDb;
using System.Globalization; 
using UpsIntegration.SDiEmailUtilityService;

/*
 * This codebase runs under project build properties x64
 * The .csproj points to x64 version of Oracle.DataAccess v4.11 saved in the ConsoleUtilities bin by AvaSoft
 *  To use OraOleDB in provider, register the DLL with windows system
 * It relies on the universal tsnnames.ora 
 */
namespace UpsIntegration
{
    /** QuantumUtility
*  Processes CSV files based upon tracking information
*   
*  Comments:
*  Is there a global config file that holds connection string based upon prod/various test servers?
     *  Do you want the Notes_1000 field in the PS table updated or do you want to create an SDIX table that holds the updated tracking comment and then update expedite.aspx and expedite.aspx.vb to pull from the sdix_tracking.comment field?
 **/
    class QuantumView
    { 
        private static OleDbConnection dbConn = new OleDbConnection();
        private static OleDbConnection rptgConn = new OleDbConnection();
        private static String rptgStr = "Provider=OraOLEDB.Oracle;User Id=sdiexchange;Password=sd1exchange;Data Source=RPTG.WORLD;Connection Timeout=1200;";
        private static String connStr = rptgStr;

        private static ftpData testFtp = new ftpData("speedtest.tele2.net", "anonymous", "anonymous");
        private static ftpData upsData = new ftpData("ftp2.ups.com", "/", "sdiinc0318", "pR2cn9E");
        private static String prod_server = @"\\172.31.251.161\sdixdata\ftp";
        private static String prod_folder = @"\" + DateTime.Today.Month + "_" + DateTime.Today.Day + "_" + DateTime.Today.Year; 
        private static ftpData toFtp = new ftpData(@"C:\sdi\", @"csvfiles\", "anonymous", "anonymous");  

        public static void Main(String[] args)
        {
            try
            {
                //FTP Files  
                ftpData fromFtp = upsData;
                fromFtp.extension = ".txt";

                fromFtp.filesize = 800000;
                fromFtp.days = 0;

                //Run for each date entered at command line
                if (args.Length >= 1)
                    fromFtp.days = GetConsoleDays(args[0]);

                prod_folder += @"\" + QuantumUtility.stripChars(DateTime.Today.Subtract(new TimeSpan(fromFtp.days, 0, 0, 0)).ToShortDateString(), "DATE") + @"\";
                toFtp = new ftpData(prod_server, prod_folder, "anonymous", "anonymous");
                toFtp.filesize = 800000;

                QuantumUtility.logError("FTP FILES FROM: " + fromFtp.server + fromFtp.directory + " to " + toFtp.server + toFtp.directory);
                QuantumUtility.logErrorFile("FTP FILES FROM: " + fromFtp.server + fromFtp.directory + " to " + toFtp.server + toFtp.directory, toFtp.server + toFtp.directory);
               QuantumUtility.cleanDirectory(toFtp.server + toFtp.directory);
                QuantumUtility.winSCP(fromFtp, toFtp);

                QuantumDbUtility.openDb(connStr, dbConn);
                if (dbConn.State.ToString() == "Open")
                {
                    parseDirectory(toFtp.server + toFtp.directory);  // parseCsvFile(ShortMatchFile);
                    batchMail();
                }
                QuantumDbUtility.closeDb(dbConn);
                QuantumUtility.logError("Completed");
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
                if (dbConn.State.ToString() == "Open") QuantumDbUtility.logError(dbConn, e.ToString());
            }
        }

        public static void batchMail()
        {
            QuantumUtility.logError("Mailing Users ... "); 
            try
            {
                //using ps_isa_ord_intf_lN and/or PS_ISA_USERS_TBL  or PS_ISA_PODUEDTMON  based on ConsoleUtilities\PODueDtchangeEmail.vb 
                StringBuilder sql = new StringBuilder(@"
                         select distinct  
                         USR.LAST_NAME_SRCH as LAST_NAME,    USR.ISA_EMPLOYEE_EMAIL as ISA_EMPLOYEE_EMAIL,
                         DISTR.PO_ID as DISTR_PO_ID,
                         qv_LOG.PO_ID as PO_ID,
                         qv_LOG.isa_asn_track_no as isa_asn_track_no ,
                         qv_LOG.UPS_RECORD_TYPE as RECORD_TYPE,
                            qv_LOG.UPS_DELIVERY_TYPE as DELIVERY_TYPE,
                        /* DISTR.BUSINESS_UNIT, */
                         /* DISTR.req_id, Requestion ID ,*/
                         /* INTF_LN.ISA_EMPLOYEE_ID USER ID ,*/
                         /*INTF_LN.order_no requesition id  id ,*/
                         /*INTF_LN.OPRID_APPROVED_BY,
                         INTF_LN.SHIP_TO_CUST_ID ,
                        trunc(current_timestamp) as current_timestamp, */
                         /* USR.FIRST_NAME_SRCH,  USR.ISA_USER_NAME, commenting out as redundant first names with different spellings can can redundancy */                         
                         qv_LOG.USER_MESSAGE  as USER_MESSAGE 
                         /*  Line Nbr fields - commenting out to avoid redundancy 
                         INTF_LN.ISA_INTFC_LN , DISTR.LINE_NBR  Line nbr ,*/
                         FROM sdix_ups_quantumview_log qv_LOG 
                           JOIN PS_PO_LINE_DISTRIB DISTR on  qv_LOG.PO_ID =  DISTR.PO_ID 
                         LEFT JOIN  PS_ISA_ORD_INTF_LN INTF_LN on DISTR.req_id=INTF_LN.order_no and DISTR.LINE_NBR =  INTF_LN. isa_intfc_ln 
                         LEFT JOIN sdix_users_tbl USR on INTF_LN.isa_employee_id = USR.isa_employee_id  
                         WHERE
                         qv_LOG.DTTM_Added  >= '@0'   and   
                    qv_LOG.DTTM_Added<= '@1' /* Use date range as this process can take multiple hours */
                        and  USR.ISA_EMPLOYEE_EMAIL is not null 
                        order  by 
                         USR.LAST_NAME_SRCH,
                         USR.ISA_EMPLOYEE_EMAIL, 
                         qv_LOG.PO_ID, qv_LOG.isa_asn_track_no ");
                 
                OleDbDataReader dbReader = null;
                String email = "";
                String message = "";
                String lname = "";
                String newline = "<br>\n";
                String hdr = "";
                String poid = "";
                String track_no = "";
                String testurl = @"http://zeustest.sdi.com:8083/ZEUSRPTG/QuantumView.aspx";
                String url = @"https://www.sdizeus.com";
                int rowcount = 0;
                String toemail = "anita.nicholson@sdi.com";
                dbReader = QuantumDbUtility.executeDbReader(dbConn, sql.ToString(), new String[] { toFtp.startDate.ToString("dd-MMM-yy hh:mm:ss.fffffff00 tt").ToUpper(), DateTime.Now.ToString("dd-MMM-yy hh:mm:ss.fffffff00 tt").ToUpper() });
                EmailServices sdiemail = new EmailServices();
                if (dbReader != null && dbReader.HasRows)
                {
                    while (dbReader.Read())
                    {
                        toemail = dbReader["ISA_EMPLOYEE_EMAIL"].ToString();
                        if (dbReader["LAST_NAME"].ToString() != lname)
                        {
                            if (!String.IsNullOrEmpty(message))
                                sdiemail.EmailUtilityServices("Mail", "SDIExchADMIN@sdi.com", toemail,
                                      "SDI Purchase Order Update " + DateTime.Now.ToShortDateString(), "", "",
                                      hdr + "<ul>" + message + "</ul>",
                                      "SDIERRMAIL", new string[0], new Byte[0][]);
                            sdiemail.Dispose();
                            lname = dbReader["LAST_NAME"].ToString();
                            email = dbReader["ISA_EMPLOYEE_EMAIL"].ToString();

                            hdr = "To: " + lname + " (" + email + ")" + newline + newline;
                            hdr += "Below find the latest shipping information received from UPS on your products." +
                            "The below email lists the available shipping history for the available UPS tracking numbers grouped by Purchase Order Number. "  +
                            "You can continue to track your packages at " + url + newline + newline; 
                        }
                        if (poid != dbReader["PO_ID"].ToString())
                        {
                            poid = dbReader["PO_ID"].ToString();
                            message += "<li style='list-style-type: disc'><strong>SHIPPING HISTORY FOR PURCHASE ORDER: " + poid + "</strong></li>"; 
                        } 

                        //Sometimes files have multiple rows with duplicate track #s 12/30 
                        if (track_no != dbReader["isa_asn_track_no"].ToString())
                        {
                            track_no = dbReader["isa_asn_track_no"].ToString();
                            if (!String.IsNullOrEmpty(dbReader["USER_MESSAGE"].ToString()))
                            {
                                message += "<li style='margin-left: 25px;  list-style-type: circle;'><i>Package " + track_no + " is " + dbReader["DELIVERY_TYPE"].ToString()  + ".</i> " + 
                                        dbReader["USER_MESSAGE"].ToString() + "</li>";
                            }
                        }
                        QuantumUtility.logErrorFile("Mailing User " + rowcount++, toFtp.server + toFtp.directory );
                    }
                    //Send one last time - in case all under the same name
                    sdiemail.EmailUtilityServices("Mail", "SDIExchADMIN@sdi.com", toemail,
                         "SDI Purchase Order Update " + DateTime.Now.ToShortDateString(), "", "",
                         hdr + "<ul>" + message + "</ul>",
                         "SDIERRMAIL", new string[0], new Byte[0][]);
                    sdiemail.Dispose();
                }
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
                QuantumUtility.logErrorFile(e,  toFtp.server + toFtp.directory );
                if (dbConn.Equals("Open"))
                    QuantumDbUtility.logError(dbConn, e.ToString());
            }
        }

        /**
         * ParseCSVFile
         *  Run through each row 
         * Question: Should I use the ISA_ASN_SHIP_VIA like 'UPS%' filter? to avoid nabbing fedex - is this field always completed?
         * * select * from PS_ISA_XPD_COMMENT where (PO_id like  'E3919%' or po_id like '%8044%' or PO_ID like '%0210214800' or PO_ID='0210214800'  or DTTM_STAMP >='15-Nov-2019') and rownum < 10 2016 earliest date for PLGR & RPTG 
         * */
        public static void parseCsvFile(String filename)
        {
            try
            {
                QuantumUtility.logError(DateTime.Now.ToShortDateString() + " " + DateTime.Now.TimeOfDay + ": Parsing " + filename );
                QuantumUtility.logErrorFile(DateTime.Now.ToShortDateString() + " " + DateTime.Now.TimeOfDay + ": Parsing " + filename, toFtp.server + toFtp.directory);

                String currentRow;
                String[] header = null;
                String[] row = null;
                int rowCount = 0;
                int colCount = 0;
                StringReader rdr = new StringReader(File.ReadAllText(@filename));
                QuantumFile qf = new QuantumFile();
                List<KeyValuePair<String, int>> l_quantumFilePositions = new List<KeyValuePair<String, int>>();
                Char separator = '|';  //Sometimes they use pipes. Othertimes tabs

                
                String file = File.ReadAllText(filename);
                if (file.ToString().Contains("E1") || file.ToString().Contains("D1") || file.ToString().Contains("D2")) //12/27/19 - verify data is in file before processing
                {
                    while ((currentRow = rdr.ReadLine()) != null && rdr != null)
                    {
                        //1. Grab header use to identify column positions 
                        if (rowCount == 0)
                        {
                            header = currentRow.Split(separator);
                            colCount = header.Length;

                            //If this does not contain a pipe, then it uses a tab, so default to the predefined col positions
                            if (currentRow.Contains(separator))
                            {
                                foreach (KeyValuePair<String, int> q in qf.quantumFilePositions)
                                {
                                    if (Array.IndexOf(header, q.Key) >= 0)
                                        l_quantumFilePositions.Add(new KeyValuePair<String, int>(q.Key, Array.IndexOf(header, q.Key)));
                                }
                                qf.quantumFilePositions = l_quantumFilePositions;
                            }

                        }
                        else
                        {
                            //Verify row contains valid data or skip
                            if (currentRow.Contains("E1") || currentRow.Contains("D1") || currentRow.Contains("D2"))
                                parseRow(currentRow, new QuantumFile(qf.quantumFilePositions), row, filename); //Separated out in case users decide to parse the Package value ref fields that might contain more than one entry - passing in a new qf object  to avoid value reuse, while maintiang the qfp
                        }
                        rowCount++;
                    }
                }
                else
                {
                    QuantumDbUtility.logError(dbConn, filename + " No matches on E1, D1 or D2");
                    QuantumUtility.logErrorFile(" ---  No matches on E1, D1 or D2", toFtp.server + toFtp.directory);
                } 
                 
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
                QuantumUtility.logErrorFile(e, toFtp.server + toFtp.directory);
                QuantumDbUtility.logError(dbConn, e.ToString());
            }
        }

      

        /* Manually coded parser */
        public static void parseRow(String currentRow, QuantumFile qf, String[] row, String filename, String local_poid = "")
        {
            try
            {
                List<KeyValuePair<String, int>> l_quantumFilePositions = new List<KeyValuePair<String, int>>();
                OleDbDataReader dbReader = null;
                String[] dbParams;
                String sdix_ups_quantumview_log_sql = "insert into SDIX_UPS_QUANTUMVIEW_LOG  (ups_filename, po_id_options, isa_asn_track_no, ups_file_location,   po_id,  user_message, ups_record_type, ups_delivery_type, ups_sdi_match) values ('@0','@1','@2','@3','@4','@5','@6','@7','@8' )";
                String[] sdix_ups_quantumview_log_params = null;    //String[] ps_isa_xpd_comment_params = null;
                Char separator = '|';  //Sometimes they use pipes. Othertimes tabs 
                String poFromSql = " FROM PS_PO_LINE_SHIP  PO  LEFT JOIN PS_ISA_ASN_SHIPPED SH ON PO.PO_ID = SH.PO_ID "; //switched form ps_po_hdr to ps_po_line_shipped to ps_po_line_ship, ps_po_line_ship adds 15 sec, but it returns the estimated shipping due date   // String shFromSql = " FROM  PS_ISA_RECV_LN_ASN SH LEFT JOIN   PS_PO_HDR PO   ON SH.PO_ID =  PO.PO_ID  ";  //" FROM  PS_ISA_ASN_SHIPPED SH LEFT JOIN   PS_PO_HDR PO   ON SH.PO_ID =  PO.PO_ID  ";  switching from ps_isa_asn_shipped provided by m. randall to  PS_ISA_RECV_LN_ASN as reqpoststatus.aspx.vb uses that table
                String shFromSql = "   JOIN   PS_PO_HDR PS  ON PO.PO_ID =  PS.PO_ID  ";
                String comFromSql = shFromSql + "   LEFT JOIN PS_ISA_XPD_COMMENT COM ON PO.PO_ID = COM.PO_ID AND COM.BUSINESS_UNIT = PO.BUSINESS_UNIT AND COM.LINE_NBR=PO.LINE_NBR AND COM.SCHED_NBR=PO.SCHED_NBR   ";
                String asnSelectSql =  "  SELECT DISTINCT   PO.business_unit as BUSINESS_UNIT, PO.PO_ID as PO_ID,  PO.DUE_DT as DUE_DT " ; 
                         /* , PO.VENDOR_ID as VENDOR_ID,    PO.BUYER_ID as BUYER_ID,     SH.BUSINESS_UNIT as SH_BUSINESS_UNIT,   SH.PO_ID as SH_PO_ID,  
                         SH.ISA_ASN_TRACK_NO as SH_TRACK_NO,    SH.ISA_ASN_SHIP_DT as SH_SHIP_DT,   SH.LINE_NBR as SH_LINE_NBR,   SH.SCHED_NBR as SH_SCHED_NBR, 
                         SH.OPRID as SH_OPRID,   COM.ISA_PROBLEM_CODE as ISA_PROBLEM_CODE ,   COM.NOTES_1000 as NOTES_1000 ,  OM.LINE_NBR as COM_LINE_NBR,  
                       COM.SCHED_NBR  as  COM_SCHED_NBR,     PO.LINE_NBR as PO_LINE_NBR,    PO.SCHED_NBR  as  PO_SCHED_NBR  */ 

                //Note: Sometimes there are the same PO_Ids but different sched #s, I'm only pulling the most recent. Should I pull both? OR SHOULD I Just insert a new line altogether?
                String whereSql = "WHERE " +
                      "(TRIM(SH.ISA_ASN_TRACK_No) = '@0'  OR  TRIM(PO.PO_ID) = '@1' OR TRIM(PO.PO_ID) = '@2' OR " +
                      "TRIM(PO.PO_ID) = '@3' OR TRIM(PO.PO_ID) = '@4' OR   TRIM(PO.PO_ID) = '@5' OR TRIM(PO.PO_ID) = '@6' OR " +
                      "TRIM(PO.PO_ID) = '@7' OR TRIM(PO.PO_ID) = '@8' OR  TRIM(PO.PO_ID) = '@9' " +
                      " ) " +
                       "and rownum =1  ORDER BY PO.DUE_DT DESC";// ORDER BY SH.ISA_ASN_SHIP_DT desc"; 

                /* Process All the Sub Rows */
                if (currentRow.Contains(separator)) //only get if currentrow uses pipe
                {
                    row = currentRow.Split(separator);
                    qf.RecordType = row[qf.quantumFilePositions.First(x => x.Key == "RecordType").Value];
                }
                else
                    qf.RecordType = QuantumUtility.RegSearch(currentRow, "[DE][12]");

                if (!String.IsNullOrEmpty(local_poid)) //Use this poid if passed in 
                    qf.ps_po_id = local_poid;
                 
                if (qf.RecordType == "E1" || qf.RecordType == "D2" || qf.RecordType == "D1")
                {
                    if (currentRow.Contains(separator))
                    {
                        //Try to identify the POID and grab alld ata necessary to alert user
                        qf.TrackingNumber = row[qf.quantumFilePositions.First(x => x.Key == "TrackingNumber").Value];
                        qf.ExceptionResolutionDescription = row[qf.quantumFilePositions.First(x => x.Key == "ExceptionResolutionDescription").Value];
                        qf.ExceptionReasonDescription = row[qf.quantumFilePositions.First(x => x.Key == "ExceptionReasonDescription").Value];
                        qf.RescheduledDeliveryDate = row[qf.quantumFilePositions.First(x => x.Key == "RescheduledDeliveryDate").Value];
                        qf.SignedForBy = row[qf.quantumFilePositions.First(x => x.Key == "SignedForBy").Value];
                        qf.ShipperNumber = row[qf.quantumFilePositions.First(x => x.Key == "ShipperNumber").Value];
                        qf.ShipmentReferenceNumberValue1 = row[qf.quantumFilePositions.First(x => x.Key == "ShipmentReferenceNumberValue1").Value];
                        qf.ShipmentReferenceNumberValue2 = row[qf.quantumFilePositions.First(x => x.Key == "ShipmentReferenceNumberValue2").Value];
                        qf.PackageReferenceNumberValue1 = row[qf.quantumFilePositions.First(x => x.Key == "PackageReferenceNumberValue1").Value];
                        qf.PackageReferenceNumberValue2 = row[qf.quantumFilePositions.First(x => x.Key == "PackageReferenceNumberValue2").Value];
                        qf.UPSLocation = row[qf.quantumFilePositions.First(x => x.Key == "UPSLocation").Value];
                        qf.ScheduledDeliveryDate = row[qf.quantumFilePositions.First(x => x.Key == "ScheduledDeliveryDate").Value];
                        qf.PackageActivityDate = row[qf.quantumFilePositions.First(x => x.Key == "PackageActivityDate").Value];
                    }
                    else
                    {
                        qf.TrackingNumber = QuantumUtility.RegSearch(currentRow, "[A-Z0-9]{18}");
                        String rowNoSpace = (QuantumUtility.stripChars(QuantumUtility.stripChars(currentRow, "PO"), "PARTIAL")).Replace("\t", " ");
                        qf.PackageReferenceNumberValue1 = QuantumUtility.RegSearch(rowNoSpace, "\\b[A-Z]{1,3}[0-9]{1}[A-Z0-9]{5,8}\\b"); //FInd value closest to recent format of po id 
                    } 

                    QuantumUtility.logError(" - Processing " + qf.TrackingNumber + " " + qf.RecordType + " " + qf.PackageReferenceNumberValue1 + " " + qf.PackageReferenceNumberValue2 + " " + local_poid);
                    QuantumUtility.logErrorFile(" - Processing " + qf.TrackingNumber + " " + qf.RecordType + " " + qf.PackageReferenceNumberValue1 + " " + qf.PackageReferenceNumberValue2 + " " + local_poid, toFtp.server + toFtp.directory);

                    //Set Query params with resulting data - verify the various poid possibilities are at least greater than 8/10 (the expected po size is 10 and not just standard nums)
                    dbParams = new String[10]  {
                                       qf.TrackingNumber, 
                                       qf.PackageReferenceNumberValue1.Length >=10 ? qf.PackageReferenceNumberValue1 : "",
                                       qf.PackageReferenceNumberValue2.Length >=10 ? qf.PackageReferenceNumberValue2 :"" ,
                                        qf.PackageReferenceNumberValue1.Length >=10 ? QuantumUtility.stripChars(qf.PackageReferenceNumberValue1,"partial") : "",
                                        qf.PackageReferenceNumberValue2.Length >=10 ?QuantumUtility.stripChars(qf.PackageReferenceNumberValue2,"partial") : "",
                                       qf.PackageReferenceNumberValue1.Length >=10 ?QuantumUtility.stripChars(qf.PackageReferenceNumberValue1,"PO"): "",
                                       qf.PackageReferenceNumberValue2.Length >=10 ? QuantumUtility.stripChars(qf.PackageReferenceNumberValue2,"PO" ): "",
                                        qf.ShipmentReferenceNumberValue1.Length >= 10? QuantumUtility.stripChars(qf.ShipmentReferenceNumberValue1,"partial"): "",
                                       qf.ShipmentReferenceNumberValue2.Length >= 10 ? QuantumUtility.stripChars(qf.ShipmentReferenceNumberValue2,"partial"): "",
                                       QuantumUtility.stripChars( qf.PackageReferenceNumberValue1, "PO2")
                              };
                    dbParams = dbParams.Distinct().ToArray();
                    sdix_ups_quantumview_log_params = new String[9]      {  
                                        filename.Replace(toFtp.server + toFtp.directory,""),  
                                       String.Join(" || ", dbParams,1,dbParams.Length-1) ,
                                       qf.TrackingNumber,  
                                       toFtp.server + toFtp.directory,  
                                       qf.ps_po_id,
                                       qf.ps_notes_1000,
                                       qf.RecordType,
                                       getRecordDeliveryType(qf.RecordType),
                                       "FALSE"
                                    };
                    //Grab Matching Data
                    dbReader = QuantumDbUtility.executeDbReader(dbConn, asnSelectSql + poFromSql + shFromSql + whereSql, dbParams );

                    if (dbReader.HasRows)
                    {
                        while (dbReader.Read())
                        {
                            //Set PO_ID
                            if (!String.IsNullOrEmpty(dbReader["PO_ID"].ToString()))
                            {
                                qf.ps_po_id = dbReader["PO_ID"].ToString();
                                sdix_ups_quantumview_log_params[4] = qf.ps_po_id;
                            }
                          
                            if (!qf.ps_notes_1000.Contains("Delivered") && !String.IsNullOrEmpty(qf.DeliveryLocation))
                                qf.ps_notes_1000_new += " Delivered To: " + 
                                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(qf.DeliveryLocation.ToLower()) + " " + 
                                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(qf.UPSLocation.ToLower());
                            if (!qf.ps_notes_1000.Contains("Signed") && !String.IsNullOrEmpty(qf.SignedForBy))
                                qf.ps_notes_1000_new += " Signed By: " +   CultureInfo.CurrentCulture.TextInfo.ToTitleCase(qf.SignedForBy.ToLower());
                            if (!qf.ps_notes_1000.Contains("Reason:") && !String.IsNullOrEmpty(qf.ExceptionResolutionDescription))
                                qf.ps_notes_1000_new += " Rescheduled Delivery Reason: " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(qf.ExceptionResolutionDescription.ToLower()) + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(qf.ExceptionReasonDescription.ToLower());
                            if (!qf.ps_notes_1000.Contains("Delivery Date") && !String.IsNullOrEmpty(qf.RescheduledDeliveryDate))
                                qf.ps_notes_1000_new += " Rescheduled Delivery Date: " + qf.RescheduledDeliveryDate;
                            if (!qf.ps_notes_1000.Contains("Original Delivery Date") && !String.IsNullOrEmpty(qf.ScheduledDeliveryDate))
                                qf.ps_notes_1000_new += " Original Delivery Date: " + qf.ScheduledDeliveryDate;
                            if (!String.IsNullOrEmpty(qf.PackageActivityDate))
                                qf.ps_notes_1000_new += " Last Processed by UPS On: " + qf.PackageActivityDate;
                            sdix_ups_quantumview_log_params[8] = "TRUE";

                            if (String.IsNullOrEmpty(qf.ps_notes_1000_new) || qf.ps_notes_1000_new.Length < 1)
                                qf.ps_notes_1000_new = qf.ps_notes_1000;
                            sdix_ups_quantumview_log_params[5] = qf.ps_notes_1000_new;
 
                            QuantumDbUtility.executeDbUpdate(dbConn, sdix_ups_quantumview_log_sql, sdix_ups_quantumview_log_params);
                            QuantumUtility.logError("   -- Inserted using PO ID " + qf.ps_po_id ); //note: get all of the required fields to insert
                            QuantumUtility.logErrorFile("   -- Inserted using PO ID " + qf.ps_po_id, toFtp.server + toFtp.directory); //note: get all of the required fields to insert
                        }
                    }
                    else if (!dbReader.HasRows || dbReader == null)
                    {
                        QuantumUtility.logError("   -- Match not made on " + String.Join(",", dbParams) ); 
                        QuantumUtility.logErrorFile("   -- Match not made on " + String.Join(",", dbParams), toFtp.server + toFtp.directory);
                        QuantumDbUtility.executeDbUpdate(dbConn, sdix_ups_quantumview_log_sql, sdix_ups_quantumview_log_params);
                    }
                }
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
                QuantumUtility.logErrorFile(e, toFtp.server + toFtp.directory);
                QuantumDbUtility.logError(dbConn, e.ToString());
            }
        }

        public static String getRecordDeliveryType(String input)
        {
            switch (input)
            {
                case "E1":
                    return "Delayed";
                case "D1":
                    return "Delivered";
                case "D2":
                    return "Delivered";
                default:
                    return input;
            }
        }

        public static String GetConsoleDate(String input)
        {
            String days = "";
            try
            {
                //QuantumUtility.logError("What date do you want to FTP files for? Use format dd//mm//yyyy or hit enter for today");
                //String input = Console.ReadLine();
                if (!String.IsNullOrEmpty(input))
                {
                    DateTime consoleDate = DateTime.Parse(input);
                    if (consoleDate > DateTime.Today)
                    {
                        QuantumUtility.logError("Provided date " + consoleDate.ToShortDateString() + " is greater than today, " + DateTime.Today.ToShortDateString());
                        //return GetConsoleDate();
                    }
                    else days = QuantumUtility.stripChars(consoleDate.ToShortDateString(), "DATE");
                    if (DateTime.Today.Subtract(consoleDate).TotalDays > 15)
                        QuantumUtility.logError("Provided date " + days + " days ago. The FTP server might not have files over 15-19 days ago");
                }
            }
            catch (Exception e)
            {
                QuantumUtility.logError("Format incorrect." + e.ToString());
                //return GetConsoleDate();
            }
            QuantumUtility.logError("Grabbing files from " + days + " days ago ");
            return days;
        }

        public static int GetConsoleDays(String input)
        {
            int days = 0;

            try
            {
                //QuantumUtility.logError("What date do you want to FTP files for? Use format dd//mm//yyyy or hit enter for today");
                //String input = Console.ReadLine(); 
                if (!String.IsNullOrEmpty(input))
                {
                    DateTime consoleDate = DateTime.Parse(input);
                    if (consoleDate > DateTime.Today)
                    {
                        QuantumUtility.logError("Provided date " + consoleDate.ToShortDateString() + " is greater than today, " + DateTime.Today.ToShortDateString());
                        //return GetConsoleDays();
                    }
                    if (consoleDate <= DateTime.Today)
                        days = Convert.ToInt32(DateTime.Today.Subtract(consoleDate).TotalDays);
                    if (days > 19)
                        QuantumUtility.logError("Provided date " + days + " days ago. The FTP server might not have files over 15-19 days ago");
                }
            }
            catch (Exception e)
            {
                QuantumUtility.logError("Format incorrect. " + e.ToString());
                //return GetConsoleDays();
            }
            QuantumUtility.logError("Grabbing files from " + days + " days ago ");
            return days;
        }
        /* ParseDirectory
         *  Processes all downloaded CSV files
         * */
        public static void parseDirectory(String dir)
        {
            QuantumUtility.logError("Parsing local directory " + dir);
            QuantumUtility.logErrorFile("Parsing local directory " + dir, toFtp.server + toFtp.directory);
            if (Directory.Exists(dir))
            {
                foreach (System.IO.FileInfo file in (new DirectoryInfo(@dir)).GetFiles())
                {
                    if (file.ToString().Contains(".txt"))
                    {
                        parseCsvFile(dir + file.ToString());
                    }
                }
            }
        }
    }

   

    /* QuantumFile: 
     *  This class holds row data from the CSV file and relevant data from the PS tables
     *  Each field represents a relevant column 
     *  As each CSV/TXT file might vary in the column data provided, the enumeration quantumFilePositions will identify the relevant columns
     */

    public class QuantumFile
    {
        public String receiverid;
        public String SubscriptionNumber = "";
        public String SubscriptionFileName = "";
        public String RecordType = "";
        public String ShipperNumber = "";
        public String ShipmentReferenceNumberValue1 = "";
        public String ShipmentReferenceNumberValue2 = "";
        public String TrackingNumber = "";
        public String PackageReferenceNumberValue1 = "";
        public String PackageReferenceNumberValue2 = "";
        public String ExceptionResolutionType = "";
        public String ExceptionResolutionDescription = "";
        public String RescheduledDeliveryDate = "";
        public String RescheduledDeliveryTime = "";
        public String DeliveryLocation = "";
        public String BillToAccountNumber = "";
        public String SignedForBy = "";
        public String ps_notes_1000 = "No additional shipping data provided by UPS outside delivery status";
        public String ps_notes_1000_new = "";
        public String ps_po_id = "";
        public String business_unit = "";
        public String isa_problem_code = "SH";
        public String ps_sched_nbr = "0";
        public String ps_line_nbr = "0";
        public String ExceptionReasonDescription = "";
        public String UPSLocation = "";
        public String ScheduledDeliveryDate = "";
        public String PackageActivityDate = "";

        public List<KeyValuePair<String, int>> quantumFilePositions;

        public QuantumFile()
        {
            quantumFilePositions = new List<KeyValuePair<String, int>>();
            quantumFilePositions.Add(new KeyValuePair<String, int>("RecordType", 68)); //COL 85 without white space reduced
            quantumFilePositions.Add(new KeyValuePair<String, int>("ShipperNumber", 89));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ShipmentReferenceNumberValue1", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ShipmentReferenceNumberValue2", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("TrackingNumber", 277));
            quantumFilePositions.Add(new KeyValuePair<String, int>("PackageReferenceNumberValue1", 76)); //COL 177 with tabs 
            quantumFilePositions.Add(new KeyValuePair<String, int>("PackageReferenceNumberValue2", 317));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ExceptionResolutionType", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ExceptionResolutionDescription", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("RescheduledDeliveryDate", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("RescheduledDeliveryTime", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("DeliveryLocation", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("SignedForBy", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("BillToAccountNumber", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ExceptionReasonDescription", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("UPSLocation", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ScheduledDeliveryDate", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("PackageActivityDate", 0));
        }

        public QuantumFile(List<KeyValuePair<String, int>> qfp)
        {
            quantumFilePositions = qfp;
            quantumFilePositions.Add(new KeyValuePair<String, int>("RecordType", 68)); //COL 85 without white space reduced
            quantumFilePositions.Add(new KeyValuePair<String, int>("ShipperNumber", 89));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ShipmentReferenceNumberValue1", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ShipmentReferenceNumberValue2", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("TrackingNumber", 277));
            quantumFilePositions.Add(new KeyValuePair<String, int>("PackageReferenceNumberValue1", 76)); //COL 177 with tabs 
            quantumFilePositions.Add(new KeyValuePair<String, int>("PackageReferenceNumberValue2", 317));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ExceptionResolutionType", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ExceptionResolutionDescription", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("RescheduledDeliveryDate", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("RescheduledDeliveryTime", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("DeliveryLocation", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("SignedForBy", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("BillToAccountNumber", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ExceptionReasonDescription", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("UPSLocation", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("ScheduledDeliveryDate", 0));
            quantumFilePositions.Add(new KeyValuePair<String, int>("PackageActivityDate", 0));
        }
    }
}
