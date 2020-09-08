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

            if (ds_OpentPOs.Tables[0].Rows.Count != 0)
            {
                foreach (DataRow rw in ds_OpentPOs.Tables[0].Rows)
                {
                    ItemCount++;
                    string POID = Convert.ToString(rw["PO_ID"]);
                    string VendorID = Convert.ToString(rw["VENDOR_ID"]);
                    string PO_BU = Convert.ToString(rw["BUSINESS_UNIT"]);

                    string strEncrypt_POID = Encrypt(POID, "bautista");
                    string strEncrypt_VendorID = Encrypt(VendorID, "bautista");
                    string strEncrypt_PO_BU = Encrypt(PO_BU, "bautista");

                    string strBuyerEmail = getBuyerEmail(POID, PO_BU);

                    string UsrTblQuery = "SELECT * FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" + VendorID + "'";
                    DataSet ds_UserDetail = GetAdapter(UsrTblQuery);
                    if (ds_UserDetail.Tables[0].Rows.Count != 0)
                    {
                        string Vendr_UN = Convert.ToString(ds_UserDetail.Tables[0].Rows[0]["ISA_USER_NAME"]);
                        string Vendr_Email = Convert.ToString(ds_UserDetail.Tables[0].Rows[0]["ISA_EMPLOYEE_EMAIL"]);
                        string Vendr_OprID = Convert.ToString(ds_UserDetail.Tables[0].Rows[0]["ISA_USER_ID"]);
                        Vendr_OprID = Encrypt(Vendr_OprID, "bautista");

                        string EmailBasueURL = baseWebURL + "Supplier/PODetails.aspx?POID=" + strEncrypt_POID + "&vendorid=" + strEncrypt_VendorID + "&PO_BU=" + strEncrypt_PO_BU + "&OPR_ID=" + Vendr_OprID + "";

                        Boolean EmailSent = SendEmail(VendorID, Vendr_UN, Vendr_Email, POID, PO_BU, EmailBasueURL, strBuyerEmail);

                        
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
                }
            }
            log.Close();
            fileStream.Close();
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

        public static Boolean SendEmail(string VendorID, string VendorUN, string VendorEmail, string POID, string PO_BU, string strURL, string strBuyerEmail)
        {
            string strbodyhead;
            string strbodydetl = "";
            Boolean isEmailSent = false;

            String DbUrl = Convert.ToString(ConfigurationSettings.AppSettings["OLEDBconString"]);
            DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper();

            try
            {
                MailMessage Mailer = new MailMessage();
                string FromAddress = "SDIExchange@SDI.com";
                string Mailcc = "";
                string MailBcc = "webdev@sdi.com;Tony.Smith@sdi.com";

                strbodyhead = "<table bgcolor='black' Width='100%'><tbody><tr><td style='width:1%;'><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td>";
                strbodyhead = strbodyhead + "<td style='width:50% ;'><center><span style='font-weight:bold;color:white;font-size:24px;'>SDI Marketplace</span></center>";
                strbodyhead = strbodyhead + "<center><span style='color:white;'>SDiExchange - Purchase Order Notification</span></center></td></tr></tbody></table>";
                strbodyhead = strbodyhead + "<HR width='100%' SIZE='1'>";
                strbodydetl = "<div>";

                strbodydetl = "&nbsp;";
                strbodydetl = strbodydetl + "<div>";
                strbodydetl = strbodydetl + "<p>Dear <span>" + VendorUN + "</span><br>";
                strbodydetl = strbodydetl + "&nbsp;<br>";
                //   strbodydetl = strbodydetl + "<p>Need approval for PO ID: " + POID + ". Please click on this following  <a href=" + strURL + ">Link</a> to go through the approval process.</p>";
                //strbodydetl = strbodydetl + "<p>Purchase Order " + POID + " has been created for " + VendorUN + ". Please select the <a href=" + strURL + ">Link</a> to review and confirm this purchase order, <span style='color: red;'>price and anticipated delivery date</span>.</p>";
                strbodydetl = strbodydetl + "<p>Purchase Order " + POID + " has been created for " + VendorUN + ". Please select the <a href=" + strURL + ">Link</a> to review and confirm<span style='color: red;'> pricing </span>and<span style='color: red;'> anticipated delivery date</span> for this purchase order.</p>";
                // strbodydetl = strbodydetl + "<br>";

                if (strBuyerEmail != "")
                {
                    strbodydetl = strbodydetl + "<p style='font-weight: bold;'>If you have any questions, please e-mail: " + strBuyerEmail + "  </p>";
                }

                strbodydetl = strbodydetl + "&nbsp;<br>";
                strbodydetl = strbodydetl + "Sincerely,<br>";
                strbodydetl = strbodydetl + "&nbsp;<br>";
                strbodydetl = strbodydetl + "SDI Customer Care<br>";
                strbodydetl = strbodydetl + "&nbsp;<br>";
                strbodydetl = strbodydetl + "</p>";
                strbodydetl = strbodydetl + "</div>";

                strbodydetl = strbodydetl + "<HR width='100%' SIZE='1'>";
                strbodydetl = strbodydetl + "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />";

                Mailer.Body = strbodyhead + strbodydetl;

                if (DbUrl == "SNBX" | DbUrl == "STAR" | DbUrl == "DEVL" | DbUrl == "RPTG" | DbUrl == "STST" | DbUrl == "SUAT")
                {
                    Mailer.To.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.To.Add(new MailAddress("avacorp@sdi.com"));
                    Mailer.Subject = "<<TEST SITE>> SDiExchange - Purchase Order " + POID + "";
                }
                else
                {
                    Mailer.To.Add(new MailAddress(VendorEmail));
                    Mailer.Subject = "SDiExchange - Purchase Order " + POID + "";
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
            catch (Exception)
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
                SDIEmailService.EmailUtilityServices("MailandStore", "SDIExchange@SDI.com", mailer.To.ToString(), mailer.Subject, string.Empty, string.Empty, mailer.Body, "SDIERRMAIL", null, null);
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
