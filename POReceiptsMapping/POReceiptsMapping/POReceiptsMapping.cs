using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POMapping;

namespace POReceiptsMapping
{
    class POReceiptsMapping
    {
        static void Main(string[] args)
        {
            var strResponse = "";

            // InitializeLogger start here

            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "POReceiptsMapping");
            m_oLogger.LogMessage("Main", "Started utility POReceiptsMapping");

            POReceiptsMappingAPIAccess objPOReceiptsMappingAPIAccess = new POReceiptsMappingAPIAccess();
            POReceiptsMappingDAL objPOReceiptsMappingDAL = new POReceiptsMappingDAL();
            strResponse = objPOReceiptsMappingAPIAccess.postPOReceiptMappingData(m_oLogger);
            if (strResponse == "SUCCESSFUL")
            {
                objPOReceiptsMappingDAL.UpdatePOReceiptMappingData(m_oLogger);
            }
            m_oLogger.LogMessage("Main", "POReceiptsMapping End");
        }
    }
}
