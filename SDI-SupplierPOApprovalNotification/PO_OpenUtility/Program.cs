using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Security.Cryptography;
using System.IO;

namespace PO_OpenUtility
{
    class Program
    {

        static void Main(string[] args)
        {
            StreamWriter log;
            FileStream fileStream = null;
            FileInfo logFileInfo;
            DirectoryInfo logDirInfo = null;
            int ItemCount = 0;
            string baseWebURL = ConfigurationSettings.AppSettings["WebAppName"];
            string logFilePath = ConfigurationSettings.AppSettings["LogFilePath"];
            //Mythili -- SP-404 Embedding new supplier portal link in the email
            string DBurl = ConfigurationSettings.AppSettings["OLEDBconString"];
            DBurl = DBurl.Substring(DBurl.Length - 4).ToUpper();

            string strPT = ConfigurationSettings.AppSettings["Disp_Method"];

            logFilePath = logFilePath + "OpenPOUtility_" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
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
            log.WriteLine("Open PO Utility Logs: ");

            string Sqlstring = "SELECT *  FROM SYSADM8.PS_ISA_PO_DISP_PTL WHERE DTTM_OPENED IS NULL";
            DataSet ds_OpentPOs = GetAdapter(Sqlstring);
            List<string> PODetails = new List<string>();
            if (ds_OpentPOs.Tables[0].Rows.Count != 0)
            {
                foreach (DataRow rw in ds_OpentPOs.Tables[0].Rows)
                {
                    ItemCount++;
                    string POID = Convert.ToString(rw["PO_ID"]);
                    if (!PODetails.Contains(Convert.ToString(POID)))
                    {
                        PODetails.Add(Convert.ToString(POID));
                        string VendorID = Convert.ToString(rw["VENDOR_ID"]);
                        string PO_BU = Convert.ToString(rw["BUSINESS_UNIT"]);
                        string Email = Convert.ToString(rw["ISA_VENDOR_EMAIL"]);
                        string strEncrypt_POID = Encrypt(POID, "bautista");
                        //Mythili -- SP-404 Embedding new supplier portal link in the email
                        string strPT2 = string.Empty;
                        Boolean IsPT2 = false;
                        if (DBNull.Value.Equals(rw["DISP_METHOD"]))
                        {
                            strPT2 = " ";
                        }
                        else
                        {
                            strPT2  = Convert.ToString(rw["DISP_METHOD"]);
                        }
                        if (strPT2 == strPT)
                        {
                            IsPT2 = true;
                        }
                        else
                        {
                            IsPT2 = false;
                        }
                        string techPhno = string.Empty;
                        string techEmail = string.Empty;
                        string Notes = string.Empty;
                        string strBuyerEmail = string.Empty;
                        string vendorname1 = string.Empty;
                        string Vendr_UN = string.Empty;
                        DataSet ds_UserDetail = new DataSet();
                        try
                        {
                            baseWebURL = GetWebBaseUrl(DBurl, IsPT2);

                        }
                        catch(Exception ex)
                        {

                        }
                        string EmailBasueURL = string.Empty;
                        string Vendr_Email = String.Empty;
                        string Vendr_OprID = string.Empty;
                        Boolean EmailSent = false;
                        if (IsPT2 == true)
                        {
                            string UsrTblQuery = "SELECT NAME1 FROM PS_VENDOR WHERE VENDOR_STATUS = 'A' AND VENDOR_ID = '" + VendorID + "'";
                            vendorname1 = GetScalar(UsrTblQuery);
                            if (vendorname1.Trim() != "")
                            {
                                if (PO_BU == "WAL00" || PO_BU == "EMC00")
                                {
                                    strBuyerEmail = getpurchasingEmailFrom(PO_BU);
                                }
                                else
                                {
                                    strBuyerEmail = getBuyerEmail(POID, PO_BU);
                                }

                                DataSet dsTech = getTechDetails(POID, PO_BU);

                                if (dsTech.Tables[0].Rows.Count != 0)
                                {
                                    techPhno = Convert.ToString(dsTech.Tables[0].Rows[0]["PHONE_NUM"]);
                                    techEmail = Convert.ToString(dsTech.Tables[0].Rows[0]["ISA_EMPLOYEE_EMAIL"]);
                                }
                                try
                                {
                                    string CTblQuery = "select A.BUSINESS_UNIT, A.PO_ID, A.LINE_NBR, B.COMMENTS_2000 from sysadm8.ps_po_comments A, ps_comments_tbl B where A.business_unit = 'WAL00' AND A.OPRID = B.OPRID AND A.COMMENT_ID = B.COMMENT_ID AND A.RANDOM_CMMT_NBR = B.RANDOM_CMMT_NBR AND A.LINE_NBR <> 0 AND B.SHARED_FLG = 'Y' AND A.PO_ID= '" + POID + "'";
                                    DataSet Comments = GetAdapter(CTblQuery);
                                    if (Comments.Tables[0].Rows.Count != 0)
                                    {
                                        Notes = Convert.ToString(Comments.Tables[0].Rows[0]["COMMENTS_2000"]);
                                    }
                                }
                                catch (Exception)
                                {
                                    Notes = " ";
                                }
                               
                               EmailBasueURL = baseWebURL + "po-dispatch/" + POID;
                                try
                                {
                                    EmailSent = SendEmail(VendorID, vendorname1, Vendr_Email, POID, PO_BU, EmailBasueURL, strBuyerEmail, Email, techPhno, techEmail, Notes);
                                    if (EmailSent)
                                    {
                                        log.WriteLine("{0}. PO ID - {1}, Generated email link {2} notification sent to - {3} \n", ItemCount, POID, EmailBasueURL, vendorname1.ToUpper());
                                        string strUpdQuery = "UPDATE SYSADM8.PS_ISA_PO_DISP_PTL  SET DTTM_OPENED = SYSDATE WHERE PO_ID = '" + POID + "' AND VENDOR_ID = '" + VendorID + "' ";
                                        Boolean results = false;
                                        results = ExecuteNonQuery(strUpdQuery);
                                    }
                                    else
                                    {
                                        log.WriteLine("{0}. PO ID - {1}, Error in {2} notification sent to - {3} \n", ItemCount, POID, EmailBasueURL, vendorname1.ToUpper());
                                    }
                                }
                                catch (Exception)
                                {

                                }

                            }
                            else
                            {
                                log.WriteLine("{0}No data found for  PO ID - {1} \n", ItemCount, POID, EmailBasueURL, Vendr_UN.ToUpper());

                            }
                        }
                        else
                        {
                          
                            string UsrTblQuery = "SELECT * FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" + VendorID + "'";
                            ds_UserDetail = GetAdapter(UsrTblQuery);
                            if (ds_UserDetail.Tables[0].Rows.Count != 0)
                            {
                                //SP-282 Replacing Plus with %2b to pass in query string[Change By vishalini]
                                if (strEncrypt_POID.Contains("+"))
                                {
                                    strEncrypt_POID = strEncrypt_POID.Replace("+", "%2b");
                                }
                                string strEncrypt_VendorID = Encrypt(VendorID, "bautista");
                                //SP-282 Replacing Plus with %2b to pass in query string[Change By vishalini]
                                if (strEncrypt_VendorID.Contains("+"))
                                {
                                    strEncrypt_VendorID = strEncrypt_VendorID.Replace("+", "%2b");
                                }
                                string strEncrypt_PO_BU = Encrypt(PO_BU, "bautista");
                                //SP-282 Replacing Plus with %2b to pass in query string[Change By vishalini]
                                if (strEncrypt_PO_BU.Contains("+"))
                                {
                                    strEncrypt_PO_BU = strEncrypt_PO_BU.Replace("+", "%2b");
                                }
                                if (PO_BU == "WAL00" || PO_BU == "EMC00")
                                {
                                    strBuyerEmail = getpurchasingEmailFrom(PO_BU);
                                }
                                else
                                {
                                    strBuyerEmail = getBuyerEmail(POID, PO_BU);
                                }

                                DataSet dsTech = getTechDetails(POID, PO_BU);

                                if (dsTech.Tables[0].Rows.Count != 0)
                                {
                                    techPhno = Convert.ToString(dsTech.Tables[0].Rows[0]["PHONE_NUM"]);
                                    techEmail = Convert.ToString(dsTech.Tables[0].Rows[0]["ISA_EMPLOYEE_EMAIL"]);
                                }
                                Vendr_UN = Convert.ToString(ds_UserDetail.Tables[0].Rows[0]["ISA_USER_NAME"]);
                                Vendr_Email = Convert.ToString(ds_UserDetail.Tables[0].Rows[0]["ISA_EMPLOYEE_EMAIL"]);
                                Vendr_OprID = Convert.ToString(ds_UserDetail.Tables[0].Rows[0]["ISA_USER_ID"]);
                                Vendr_OprID = Encrypt(Vendr_OprID, "bautista");
                                //SP-282 Replacing Plus with %2b to pass in query string[Change By vishalini]
                                if (Vendr_OprID.Contains("+"))
                                {
                                    Vendr_OprID = Vendr_OprID.Replace("+", "%2b");
                                }
                                try
                                {
                                    string CTblQuery = "select A.BUSINESS_UNIT, A.PO_ID, A.LINE_NBR, B.COMMENTS_2000 from sysadm8.ps_po_comments A, ps_comments_tbl B where A.business_unit = 'WAL00' AND A.OPRID = B.OPRID AND A.COMMENT_ID = B.COMMENT_ID AND A.RANDOM_CMMT_NBR = B.RANDOM_CMMT_NBR AND A.LINE_NBR <> 0 AND B.SHARED_FLG = 'Y' AND A.PO_ID= '" + POID + "'";
                                    DataSet Comments = GetAdapter(CTblQuery);
                                    if (Comments.Tables[0].Rows.Count != 0)
                                    {
                                        Notes = Convert.ToString(Comments.Tables[0].Rows[0]["COMMENTS_2000"]);
                                    }
                                }
                                catch (Exception)
                                {
                                    Notes = " ";
                                }
                                EmailBasueURL = baseWebURL + "Supplier/PODetails.aspx?POID=" + strEncrypt_POID + "&vendorid=" + strEncrypt_VendorID + "&PO_BU=" + strEncrypt_PO_BU + "&OPR_ID=" + Vendr_OprID + "";
                                try
                                {
                                    EmailSent = SendEmail(VendorID, Vendr_UN, Vendr_Email, POID, PO_BU, EmailBasueURL, strBuyerEmail, Email, techPhno, techEmail, Notes);
                                    if (EmailSent)
                                    {
                                        log.WriteLine("{0}. PO ID - {1}, Generated email link {2} notification sent to - {3} \n", ItemCount, POID, EmailBasueURL, Vendr_UN.ToUpper());
                                        string strUpdQuery = "UPDATE SYSADM8.PS_ISA_PO_DISP_PTL  SET DTTM_OPENED = SYSDATE WHERE PO_ID = '" + POID + "' AND VENDOR_ID = '" + VendorID + "' ";
                                        Boolean results = false;
                                        results = ExecuteNonQuery(strUpdQuery);
                                    }
                                    else
                                    {
                                        log.WriteLine("{0}. PO ID - {1}, Error in {2} notification sent to - {3} \n", ItemCount, POID, EmailBasueURL, Vendr_UN.ToUpper());
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                            else
                            {
                                log.WriteLine("{0}No data found for  PO ID - {1} \n", ItemCount, POID, EmailBasueURL, Vendr_UN.ToUpper());

                            }

                        }
                    }
                }
            }
            log.Close();
            fileStream.Close();
        }
        //Mythili -- SP-404 Embedding new supplier portal link in the email -- get site url based on 
        public static string GetWebBaseUrl(string strDB, Boolean IsPT2)
        {
            string strSQLString = "";
            string strPT2 = "";
            string strGetWebBaseUrl = "";
            DataSet dssites = new DataSet();
            if (IsPT2 == true)
            {
                strPT2 = "SP2";
            }
            else
            {
                strPT2 = "ZEUS1";
            }
            try
            {
                strSQLString = @"Select * from SDIX_SITE_URL WHERE SITENAME = '" + strPT2 + "'";

                dssites = GetAdapter(strSQLString);
                if (strDB == "DEVL")
                {
                    strGetWebBaseUrl = dssites.Tables[0].Rows[0]["TEST_SITE"].ToString();
                }
                else if (strDB == "SNBX" || strDB == "SUAT")
                {
                    strGetWebBaseUrl = dssites.Tables[0].Rows[0]["DEMO_SITE"].ToString();
                }
                else if (strDB == "PROD" || strDB == "SPRD")
                {
                    strGetWebBaseUrl = dssites.Tables[0].Rows[0]["PROD_SITE"].ToString();
                }
                else
                {
                    strGetWebBaseUrl = dssites.Tables[0].Rows[0]["PRE_PROD_SITE"].ToString();
                }
                return strGetWebBaseUrl;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string getBuyerEmail(string strPO, string strPOBU)
        {
            string strSQLString = "";
            string strBuyerEmail = "";
            try
            {
                strSQLString = @"SELECT DECODE(E.EMAILID, null, ' ', E.EMAILID) AS EMAILID 
                  FROM PS_BUYER_TBL A, PS_PO_HDR B,(SELECT C.EMAILID, C.ROLEUSER, D.BUYER_ID 
                  FROM SYSADM8.PS_ROLEXLATOPR C,PS_BUYER_TBL D
                  WHERE  D.BUYER_ID= C.ROLEUSER (+))E 
                  WHERE  A.BUYER_ID = B.BUYER_ID 
                  AND   A.BUYER_ID = E.ROLEUSER (+)  
                  AND   A.BUYER_ID = E.BUYER_ID (+) 
                  AND   B.BUSINESS_UNIT = '" + strPOBU + "' AND B.PO_ID = '" + strPO + "'";

                strBuyerEmail = GetScalar(strSQLString);

                return strBuyerEmail;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static DataSet getTechDetails(string strPO, string strPOBU)
        {
            string strSQLString = "";
            DataSet dsEmp = new DataSet();
            try
            {
                strSQLString = @" select distinct b.ISA_EMPLOYEE_EMAIL, b.PHONE_NUM from ps_isa_po_line a,
ps_isa_users_tbl b where a.business_unit_om = b.business_unit (+) 
and a.ISA_EMPLOYEE_ID = b.isa_employee_id (+)
and a.business_unit = '" + strPOBU + "' and a.po_id='" + strPO + "'";

                dsEmp = GetAdapter(strSQLString);

                return dsEmp;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private static Boolean ExecuteNonQuery(string insertquery)
        {
            Boolean reslt = false;
            try
            {
                int rowsaffected;
                string connectionString = ConfigurationSettings.AppSettings["OLEDBconString"];
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();
                string strInsertHDRQuery = insertquery;

                com = new OleDbCommand(strInsertHDRQuery, cn);
                rowsaffected = com.ExecuteNonQuery();

                cn.Close();
                cn.Dispose();
                if (rowsaffected != 0)
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


        public static DataSet GetAdapter(string p_strQuery, bool bGoToErrPage = true, bool bThrowBackError = false)
        {
            // Gives us a reference to the current asp.net 
            // application executing the method.
            //HttpApplication currentApp = new HttpApplication();
            //currentApp = HttpContext.Current.ApplicationInstance;
            OleDbConnection connection = new OleDbConnection(ConfigurationSettings.AppSettings["OLEDBconString"]);
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
            }
            return UserdataSet;
        }

        public static string GetScalar(string p_strQuery, bool bGoToErrPage = true)
        {
            // Gives us a reference to the current asp.net 
            // application executing the method.
            //HttpApplication currentApp = HttpContext.Current.ApplicationInstance;
            string strReturn = "";
            OleDbConnection connection = new OleDbConnection(ConfigurationSettings.AppSettings["OLEDBconString"]);
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

                }
            }

            return strReturn;
        }
        //SP-391 Purchasing Email from address change[By Vishalini]
        public static string getpurchasingEmailFrom(string sBU)
        {
            string sqlStringEmailFrom = "";
            string PurchasingEmailFrom = "";
            try
            {
                sqlStringEmailFrom = "Select ISA_PURCH_EML_FROM from PS_ISA_BUS_UNIT_PM where BUSINESS_UNIT_PO ='" + sBU + "'";
                PurchasingEmailFrom = GetScalar(sqlStringEmailFrom);
            }
            catch (Exception ex)
            {
                if (sBU == "I0W01" || sBU == "WAL00")
                    PurchasingEmailFrom = "WalmartPurchasing@sdi.com";
                else if (sBU == "EMC00" || sBU == "I0631")
                    PurchasingEmailFrom = "Emcorpurchasing@sdi.com";
                else
                    PurchasingEmailFrom = "SDIExchange@SDI.com";
            }
            return PurchasingEmailFrom;
        }
        //Madhu-INC0015106-Removed avacorp in Email flow
        public static Boolean SendEmail(string VendorID, string VendorUN, string VendorEmail, string POID, string PO_BU, string strURL, string strBuyerEmail, string Email, string techPhno, string techEmail, string Notes)
        {
            string strbodyhead;
            string strbodydetl = "";
            Boolean isEmailSent = false;
            string MailerFrom = "";

            String DbUrl = Convert.ToString(ConfigurationSettings.AppSettings["OLEDBconString"]);
            DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper();
            //SDI - 50231 Spanish Transalation for Mexican Suppliers[Change by Vishalini]
            String Vendor = VendorID.Substring(0, 1);

            try
            {
                //SP-136: Change the email address from sdiexchange@sdi.com to sdizeus@sdi.com. Change made by Venkat
                //SP-391: Change the email address from sdiexchange@sdi.com to respective email id[ Change made by Vishalini]
                MailMessage Mailer = new MailMessage();
                MailerFrom = getpurchasingEmailFrom(PO_BU);
                MailAddress From = new MailAddress(MailerFrom);
                Mailer.From = From;
                //string FromAddress = "SDIZEUS@SDI.COM";
                string Mailcc = "";
                string MailBcc = "webdev@sdi.com;Tony.Smith@sdi.com";

                strbodyhead = "<table bgcolor='black' Width='100%'><tbody><tr><td style='width:1%;'><img src='http://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td>";
                strbodyhead = strbodyhead + "<td style='width:50% ;'><center><span style='font-weight:bold;color:white;font-size:24px;'>SDI Marketplace</span></center>";
                //SDI - 50231 Spanish Transalation for Mexican Suppliers[Change by Vishalini]
                if (Vendor == "M")
                {
                    strbodyhead = strbodyhead + "<center><span style='color:white;'>SDIZEUS – Notificación de Orden de Compra</span></center></td></tr></tbody></table>";
                }
                else
                {
                    strbodyhead = strbodyhead + "<center><span style='color:white;'>SDIZEUS - Purchase Order Notification</span></center></td></tr></tbody></table>";
                }
                strbodyhead = strbodyhead + "<HR width='100%' SIZE='1'>";
                strbodydetl = "<div>";

                strbodydetl = "&nbsp;";
                strbodydetl = strbodydetl + "<div>";
                //SDI - 50231 Spanish Transalation for Mexican Suppliers[Change by Vishalini]
                if (Vendor == "M")
                {
                    strbodydetl = strbodydetl + "<p>Estimado <span>" + VendorUN + "</span><br>";
                }
                else
                {
                    strbodydetl = strbodydetl + "<p>Dear <span>" + VendorUN + "</span><br>";
                }
                strbodydetl = strbodydetl + "&nbsp;<br>";
                //   strbodydetl = strbodydetl + "<p>Need approval for PO ID: " + POID + ". Please click on this following  <a href=" + strURL + ">Link</a> to go through the approval process.</p>";
                //strbodydetl = strbodydetl + "<p>Purchase Order " + POID + " has been created for " + VendorUN + ". Please select the <a href=" + strURL + ">Link</a> to review and confirm this purchase order, <span style='color: red;'>price and anticipated delivery date</span>.</p>";
                //SDI - 50231 Spanish Transalation for Mexican Suppliers[Change by Vishalini]
                if (Vendor == "M")
                {
                    strbodydetl = strbodydetl + "<p>Orden de Compra " + POID + " ha sido creada para " + VendorUN + ". Por favor seleccione la <a href=" + strURL + ">Liga</a> para revisar, confirmar<span style='color: red;'> el precio</span>,<span style='color: red;'> la cantidad y la fecha de entrega</span> para esta Orden de Compra.</p>";
                }
                else
                {
                    strbodydetl = strbodydetl + "<p>Purchase Order " + POID + " has been created for " + VendorUN + ". Please select the <a href=" + strURL + ">Link</a> to review and confirm<span style='color: red;'> pricing </span>and<span style='color: red;'> anticipated delivery date</span> for this purchase order.</p>";
                }
                // strbodydetl = strbodydetl + "<br>";
                if (Notes != string.Empty)
                {
                    strbodydetl = strbodydetl + "<p>Note: " + Notes + "  </p>";
                }

                if (strBuyerEmail != "")
                {
                    //SDI - 50231 Spanish Transalation for Mexican Suppliers[Change by Vishalini]
                    if (Vendor == "M")
                    {
                        strbodydetl = strbodydetl + "<p style='font-weight: bold;'>Si tiene cualquier duda, favor de enviar correo a: " + strBuyerEmail + "  </p>";
                    }
                    else
                    {
                        strbodydetl = strbodydetl + "<p style='font-weight: bold;'>If you have any questions, please e-mail: " + strBuyerEmail + "  </p>";
                    }
                }
                if (techPhno != "" && techEmail != "")
                {
                    string PhNo = string.Empty;
                    try
                    {
                        PhNo = String.Format("{0:(###)-###-####}", double.Parse(techPhno));
                    }
                    catch (Exception)
                    {
                        PhNo = techPhno;
                    }
                    if (PO_BU == "WAL00" || PO_BU == "EMC00")
                    {
                        strbodydetl = strbodydetl + "<p style='font-weight: bold;'>Tech e-mail: " + techEmail + " </p>";
                        strbodydetl = strbodydetl + "<p style='font-weight: bold;'>Tech Phone No: " + PhNo + "  </p>";
                    }
                }

                strbodydetl = strbodydetl + "&nbsp;<br>";
                //SDI - 50231 Spanish Transalation for Mexican Suppliers[Change by Vishalini]
                if (Vendor == "M")
                {
                    strbodydetl = strbodydetl + "Sinceramente,<br>";
                }
                else
                {
                    strbodydetl = strbodydetl + "Sincerely,<br>";
                }
                strbodydetl = strbodydetl + "&nbsp;<br>";
                //SDI - 50231 Spanish Transalation for Mexican Suppliers[Change by Vishalini]
                if (Vendor == "M")
                {
                    strbodydetl = strbodydetl + "SDM Servicio al Cliente<br>";
                }
                else
                {
                    strbodydetl = strbodydetl + "SDI Customer Care<br>";
                }
                strbodydetl = strbodydetl + "&nbsp;<br>";
                strbodydetl = strbodydetl + "</p>";
                strbodydetl = strbodydetl + "</div>";

                strbodydetl = strbodydetl + "<HR width='100%' SIZE='1'>";
                strbodydetl = strbodydetl + "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />";

                Mailer.Body = strbodyhead + strbodydetl;

                if (DbUrl == "SNBX" | DbUrl == "STAR" | DbUrl == "DEVL" | DbUrl == "RPTG" | DbUrl == "STST" | DbUrl == "SUAT")
                {
                    Mailer.To.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.Subject = "<<TEST SITE>> SDIZEUS - Purchase Order " + POID + "";
                }
                else
                {
                    //As per SP-57 ticket request, commented the mail sending to Portal email
                    //Mailer.To.Add(new MailAddress(VendorEmail));
                    try
                    {
                        string[] values = Email.Split(';');
                        for (int i = 0; i < values.Length; i++)
                        {
                            Mailer.To.Add(new MailAddress(values[i].Trim()));
                        }
                    }
                    catch (Exception)
                    {
                    }
                    Mailer.Subject = "SDIZEUS - Purchase Order " + POID + "";
                }

                OleDbConnection connectionEmail = new OleDbConnection(ConfigurationSettings.AppSettings["OLEDBconString"]);

                try
                {
                    sendemails(Mailer);

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
            catch (Exception ex)
            {

                isEmailSent = false;
            }
            return isEmailSent;
        }

        public static void sendemails(MailMessage mailer)
        {
            com.sdi.ims.EmailServices SDIEmailService = new com.sdi.ims.EmailServices();
            //string fName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            string fName = "";
            string[] MailAttachmentName = new string[] { fName };


            List<byte[]> MailAttachmentbytes = new List<byte[]>();
            //byte[] bytes = Encoding.ASCII.GetBytes(fileStream);
            //MailAttachmentbytes.Add(bytes);

            try
            {
                SDIEmailService.EmailUtilityServices("MailandStore", mailer.From.ToString(), mailer.To.ToString(), mailer.Subject, string.Empty, string.Empty, mailer.Body, "SDIERRMAIL", null, null);
            }
            catch (Exception ex)
            {
                string strErr = ex.Message;
            }
        }

        public static string Encrypt(string stringToEncrypt, string SEncryptionKey)
        {
            var key = new byte[] { };
            var IV = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            string sDefaultKey = "#sdi.default";
            try
            {
                string sKey = SEncryptionKey.Trim();

                if (sKey.Length > 0)
                {
                    if (sKey.Length < 8)
                        sKey = sKey.PadRight(8, System.Convert.ToChar("%"));
                }
                else
                    sKey = sDefaultKey;

                key = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8));

                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = System.Text.Encoding.UTF8.GetBytes(stringToEncrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write);

                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        //public string Decrypt(string stringToDecrypt, string sEncryptionKey)
        //{
        //    string sDefaultKey = "#sdi.default";
        //    var key = new byte[] { };
        //    try
        //    {
        //        byte[] inputByteArray = new byte[stringToDecrypt.Length + 1];
        //        string sKey = sEncryptionKey.Trim();

        //        if (sKey.Length > 0)
        //        {
        //            if (sKey.Length < 8)
        //                sKey = sKey.PadRight(8, System.Convert.ToChar("%"));
        //        }
        //        else
        //            sKey = sDefaultKey;

        //        key = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8));

        //        DESCryptoServiceProvider des = new DESCryptoServiceProvider();

        //        inputByteArray = Convert.FromBase64String(stringToDecrypt);

        //        MemoryStream ms = new MemoryStream();
        //        CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write);

        //        cs.Write(inputByteArray, 0, inputByteArray.Length);
        //        cs.FlushFinalBlock();

        //        return System.Text.Encoding.UTF8.GetString(ms.ToArray());
        //    }
        //    catch (Exception e)
        //    {
        //        return "";
        //    }
        //}

    }
}
