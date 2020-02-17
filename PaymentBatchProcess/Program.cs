using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using BanquestUtility.Model;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Data.OleDb;
using System.Configuration;
using System.Net.Mail;
using System.Diagnostics;

namespace BanquestUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;
            try
            {
                string logpath = string.Empty;
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                appPath = appPath.Substring(0, appPath.LastIndexOf("bin"));

                string logFilePath = appPath + @"Logs\PaymentBatchLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
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

                string response = "";
                string response2 = "";
                string baseurl = ConfigurationManager.AppSettings["APIUrl"];
                List<BatchBO> batchbos = new List<BatchBO>();
                List<TransactionBO> transactionbos = new List<TransactionBO>();
                using (var client = new HttpClient())
                {
                    WebClient wbclient = new WebClient();
                    wbclient.Headers["Content-type"] = "application/json";
                    wbclient.Encoding = Encoding.UTF8;
                    DateTime dttm_current = DateTime.Now;
                    string date_from = "";
                    string date_to = "";
                    string mainurl = "";

                    date_from = ConfigurationManager.AppSettings["FromDate"];
                    date_to = ConfigurationManager.AppSettings["ToDate"];

                    if (date_from.Trim() == "" && date_to.Trim() == "")
                    {
                        date_from = dttm_current.AddDays(-1).ToString("yyyy-MM-dd");
                        date_to = dttm_current.ToString("yyyy-MM-dd");
                    }

                    mainurl = baseurl + "api/CreditCard/Getbatch?date_from=" + date_from + "&date_to=" + date_to + "";
                    response = wbclient.DownloadString(mainurl);
                    response = response.TrimStart('\"');
                    response = response.TrimEnd('\"');
                    response = response.Replace("\\", "");
                    batchbos = JsonConvert.DeserializeObject<List<BatchBO>>(response);
                }
                if (batchbos.Count > 0)
                {
                    log.WriteLine("Batch ID count : " + batchbos.Count);
                    foreach (BatchBO batchbo in batchbos)
                    {
                        WebClient wb_client = new WebClient();
                        wb_client.Headers["Content-type"] = "application/json";
                        wb_client.Encoding = Encoding.UTF8;
                        string url = baseurl + "api/CreditCard/GetTransaction?ID=" + Convert.ToString(batchbo.id);
                        response2 = wb_client.DownloadString(url);
                        response2 = response2.TrimStart('\"');
                        response2 = response2.TrimEnd('\"');
                        response2 = response2.Replace("\\", "");
                        transactionbos = JsonConvert.DeserializeObject<List<TransactionBO>>(response2);

                        if (transactionbos.Count > 0)
                        {
                            foreach (TransactionBO transactionbo in transactionbos)
                            {
                                log.WriteLine("--------------------------------------------");
                                log.WriteLine("*Batch ID : " + Convert.ToString(batchbo.id));
                                log.WriteLine("*Transaction ID : " + Convert.ToString(transactionbo.id));
                                string Batch_ID = "";
                                DateTime Added_DTTM;

                                DateTime Transaction_date;
                                string processing_status = "";
                                string confirmation_id = "";

                                Batch_ID = Convert.ToString(batchbo.id).Trim();
                                Added_DTTM = transactionbo.created_at;
                                Transaction_date = transactionbo.created_at;
                                confirmation_id = Convert.ToString(transactionbo.id);
                                processing_status = Convert.ToString(transactionbo.status_details.status);
                                string description = "";
                                description = Convert.ToString(transactionbo.transaction_details.description);
                                string[] descs;
                                if (description.Trim() != "")
                                {
                                    descs = description.Split('^');
                                    if (descs != null && descs.Count() != 0)
                                    {
                                        foreach (var desc in descs)
                                        {
                                            string Invoice = "";
                                            string Payment_Amt = "";
                                            string[] splitvalues = desc.Split('-');
                                            if (splitvalues.Count() > 0)
                                            {
                                                Invoice = splitvalues[0].ToString();
                                                Payment_Amt = splitvalues[1].ToString();
                                                Boolean results = false;
                                                log.WriteLine("Inovice number : " + Invoice);
                                                string sqlquery = "";
                                                string strBatchID = "";

                                                sqlquery = "select Batch_ID from SYSADM8.PS_ISA_AR_CR_CARD where batch_id='" + Batch_ID + "' and invoice='" + Invoice + "' and payment_amt = " + Payment_Amt + " and confirmation_id='" + confirmation_id + "'";

                                                strBatchID = GetScalar(sqlquery);

                                                if (strBatchID == "")
                                                {
                                                    sqlquery = "INSERT INTO SYSADM8.PS_ISA_AR_CR_CARD (BATCH_ID, DTTIME_ADDED, INVOICE, PAYMENT_AMT, " + System.Environment.NewLine +
                                                                "TRANSACTION_DATE, PROCESSING_STATUS, CONFIRMATION_ID) " + System.Environment.NewLine +
                                                                "Values('" + Batch_ID + "', TO_DATE('" + Added_DTTM.ToString("yyyy-MM-dd hh:mm:ss") + "', 'yyyy-MM-dd hh:mi:ss'), '" + Invoice + "', " + Payment_Amt + ", TO_DATE('" + Transaction_date.ToString("yyyy-MM-dd") + "', 'yyyy-MM-dd'), 'N', '" + confirmation_id + "')";
                                                    results = InsertTbl(sqlquery);
                                                    if (results)
                                                    {
                                                        log.WriteLine("Transaction details inserted succesfully.");
                                                        log.WriteLine(" ");
                                                    }
                                                    else
                                                    {
                                                        log.WriteLine("Error in inserting the transaction details. Query :" + sqlquery);
                                                        log.WriteLine(" ");
                                                    }
                                                }
                                                else {
                                                    log.WriteLine("Transaction details already exist in SYSADM8.PS_ISA_AR_CR_CARD table.");
                                                    log.WriteLine(" ");
                                                }
                                            }
                                            else
                                            {
                                                log.WriteLine("Error: Inovoice or amount details is missing.");
                                                log.WriteLine(" ");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        log.WriteLine("Description contains null value.");
                                        log.WriteLine(" ");
                                    }
                                }
                                else
                                {
                                    log.WriteLine("Description contains null value.");
                                    log.WriteLine(" ");
                                }
                            }
                        }
                        else
                        {
                            log.WriteLine("Transaction details not availible.");
                            log.WriteLine(" ");
                        }
                    }
                }
                else
                {
                    log.WriteLine("There is no batches are availible.");
                    log.WriteLine(" ");
                }
                log.Close();
            }
            catch (Exception ex)
            {
                SendErrorEmail(ex, ex.Message, ex.InnerException.ToString(), "");
            }
        }

        private static Boolean InsertTbl(string insertquery)
        {
            Boolean reslt = false;
            try
            {
                int rowsaffected;
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();
                string strInsertHDRQuery = insertquery;

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

        public static string GetScalar(string p_strQuery, bool bGoToErrPage = true)
        {
            // Gives us a reference to the current asp.net 
            // application executing the method.
            //HttpApplication currentApp = HttpContext.Current.ApplicationInstance;
            string strReturn = "";
            OleDbConnection connection = new OleDbConnection(ConfigurationManager.ConnectionStrings["ConString"].ConnectionString);
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

        public static Boolean SendErrorEmail(Exception exception, string strMessage, string InnerExcp, string MethodName)
        {
            string strbodyhead;
            string strbodydetl = string.Empty;
            Boolean isEmailSent = false;
            BanquestUtility.SDIEmailUtility.EmailServices SDIEmailService = new BanquestUtility.SDIEmailUtility.EmailServices();
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
                strbodydetl = strbodydetl + "<p>Sincerely,</p>";
                strbodydetl = strbodydetl + "<p>SDI Customer Care</p>";
                strbodydetl = strbodydetl + "</div>";

                Mailer.Body = strbodydetl;

                if (DbUrl == "PLGR" | DbUrl == "STAR" | DbUrl == "DEVL" | DbUrl == "RPTG")
                {
                    Mailer.To.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.To.Add(new MailAddress("avacorp@sdi.com"));
                    Mailer.Subject = "<<TEST SITE>> Payment batch processing error";
                }
                else
                {
                    Mailer.To.Add(new MailAddress("WebDev@sdi.com"));
                    Mailer.To.Add(new MailAddress("avacorp@sdi.com"));
                    Mailer.Subject = "Payment batch processing error";
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
