using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpediterReload1;
using System.Net;

namespace ExpediterReload
{
    class ExpediterReload
    {
        static void Main(string[] args)
        {
            var strResponse = "";
            string processFlag = " ";

            // InitializeLogger start here

            
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "ExpediterReload");
            m_oLogger.LogMessage("Main", "Started utility ExpediterReload");

            //WMReceiptsMappingAPIAccess objWMReceiptsMappingAPIAccess = new WMReceiptsMappingAPIAccess();
            //WMReceiptsMappingDAL objWMReceiptsMappingDAL = new WMReceiptsMappingDAL();
            //strResponse = objWMReceiptsMappingAPIAccess.postWMReceiptMappingData(m_oLogger);
            if (strResponse.ToUpper() == "SUCCESS")
            {
                processFlag = "I";
            }
            else
            {
                processFlag = "E"; //error
            }
            //objWMReceiptsMappingDAL.UpdateWMReceiptMappingData(m_oLogger, processFlag);

            m_oLogger.LogMessage("Main", "ExpediterReload End");

        }
    }
}
