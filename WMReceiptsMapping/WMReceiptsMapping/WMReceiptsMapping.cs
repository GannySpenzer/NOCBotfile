using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMMapping;

namespace WMReceiptsMapping
{
    class WMReceiptsMapping
    {
        static void Main(string[] args)
        {
            var strResponse = "";
            string processFlag = " ";

            // InitializeLogger start here

            Logger m_oLogger;
            string sLogPath = ConfigurationManager.AppSettings["LogFilePath"];
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "WMReceiptsMapping");
            m_oLogger.LogMessage("Main", "Started utility WMReceiptsMapping");

            WMReceiptsMappingAPIAccess objWMReceiptsMappingAPIAccess = new WMReceiptsMappingAPIAccess();
            WMReceiptsMappingDAL objWMReceiptsMappingDAL = new WMReceiptsMappingDAL();
            strResponse = objWMReceiptsMappingAPIAccess.postWMReceiptMappingData(m_oLogger);
            //if (strResponse.ToUpper() != " ")
            //{


            //    if (strResponse.ToUpper() == "SUCCESS")
            //    {
            //        processFlag = "I";
            //    }
            //    else
            //    {
            //        processFlag = "E"; //error
            //    }
            //    objWMReceiptsMappingDAL.UpdateWMReceiptMappingData(m_oLogger, processFlag);
            //}
            //else
            //{
            //    m_oLogger.LogMessage("Main","The transaction is not belong to 101");
            //}
            m_oLogger.LogMessage("Main", "WMReceiptsMapping End");
        }
    }
}

