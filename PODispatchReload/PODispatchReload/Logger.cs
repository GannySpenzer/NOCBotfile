using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic;
using System.Configuration;

namespace PODispatchReload1
{
    public class Logger
    {
        private string m_sLogFileSpec;

        public Logger(string sLogPath, string sFilePrefix)
        {
            try
            {
                if (!System.IO.Directory.Exists(sLogPath))
                    System.IO.Directory.CreateDirectory(sLogPath);

                m_sLogFileSpec = sLogPath + @"\" + sFilePrefix + DateTime.Now.ToString("_yyyyMMdd_HHmmtt") + ".log";
                if (LogFileSpec == "")
                {
                    LogFileSpec = sLogPath + @"\" + sFilePrefix + DateTime.Now.ToString("_yyyyMMdd_HHmmtt") + ".log";
                }
                else
                {
                    LogFileSpec = m_sLogFileSpec;
                }

            }
            catch (Exception ex)
            {
            }
        }

        private string itemField;
        public string LogFileName
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        public void WriteLine(string sMessage)
        {
            StreamWriter sw = new StreamWriter(File.Open(m_sLogFileSpec, FileMode.Append));

            string sLogLine = DateTime.Now.ToString("yyyyMMdd HH:mm:sstt") + ControlChars.Tab + sMessage;

            sw.WriteLine(sLogLine);
            sw.Flush();
            sw.Close();
        }

        public string LogFileSpec
        {
            get
            {
                return m_sLogFileSpec;
            }
            private set
            {
                m_sLogFileSpec = value;
            }
        }

        public void LogMessage(string sFunctionName, string sMessage, Exception ex)
        {
            string sLogMessage;
            sLogMessage = sFunctionName + " : " + sMessage + " " + Constants.vbCrLf + ex.Message + " " + Constants.vbCrLf;
            if (ex.InnerException != null)
                sLogMessage = sLogMessage + ex.InnerException.Message + " " + Constants.vbCrLf;
            sLogMessage = sLogMessage + ex.StackTrace;

            WriteLine(sLogMessage);

            SendEmailAlert(sLogMessage);
        }

        public void LogMessageWeb(string sFunctionName, string sMessage, string ex)
        {
            string sLogMessage;
            sLogMessage = sFunctionName + " : " + sMessage + " " + Constants.vbCrLf + ex + " " + Constants.vbCrLf;

            WriteLine(sLogMessage);

            SendEmailAlert(sLogMessage);
        }

        public void LogMessage(string sFunctionName, string sMessage)
        {
            WriteLine(sFunctionName + " : " + sMessage);
        }


        public void SendEmailAlert(string sLastErrorMessage)
        {
            try
            {
                string testOrProd = ConfigurationManager.AppSettings["TestOrProd"].ToUpper() == "TEST" ? "(TEST) " : "";

                //const string cErrMsg = testOrProd +  "Utility WMReceiptMapping had a critical error";
                string cErrMsg = testOrProd + "Utility PODispatchReload had a critical error";
                string strBodyhead = "";
                string strbodydetl = "";
                string strBody = "";
                PODispatchReload.SDiEmailUtilityService.EmailServices SDIEmailService = new PODispatchReload.SDiEmailUtilityService.EmailServices();
                string[] MailAttachmentName = null;
                List<byte[]> MailAttachmentbytes = new List<byte[]>();

                strBodyhead = strBodyhead + "<center><span style='font-family:Arial;font-size:X-Large;width:256px;Color:RED'><b>" + cErrMsg + "</b> </span><center>" + Constants.vbCrLf;
                strBodyhead = strBodyhead + "&nbsp;" + Constants.vbCrLf;
                strbodydetl = "&nbsp;" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<HR width='100%' SIZE='1'>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "&nbsp;" + Constants.vbCrLf;

                strbodydetl = strbodydetl + "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" + Constants.vbCrLf;

                strbodydetl = strbodydetl + "<TR>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<TD>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<b>Log File :</b><span> &nbsp;" + LogFileSpec + " </span></td></tr>";
                strbodydetl = strbodydetl + "<TR>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<TD>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<b>Last Error Message :</b><span> &nbsp;" + sLastErrorMessage + "</span></td></tr>";

                strbodydetl = strbodydetl + "<TD>" + Constants.vbCrLf;

                strbodydetl = strbodydetl + "<TR>" + Constants.vbCrLf;

                strbodydetl = strbodydetl + "<TR>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<TD>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<span>&nbsp;</span></td></tr>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<TR>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "<TD>" + Constants.vbCrLf;
                strbodydetl = strbodydetl + "&nbsp;<br>" + Constants.vbCrLf;
                strBody = strBodyhead + strbodydetl;
                try
                {
                    SDIEmailService.EmailUtilityServices("Mail", "SDIExchADMIN@sdi.com", "webdev@sdi.com", testOrProd + "Error from PODispatchReload Utility", "", "", strBody, "SDIERRMAIL", MailAttachmentName, MailAttachmentbytes.ToArray());
                }
                catch (Exception ex1)
                {
                }
            }
            catch (Exception e)
            {
            }
        }

 
    }
}
