using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceMapping;

namespace InvoiceMapping
{
    class InvoiceMapping
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
            InvoiceMappingAPIAccess objInvoiceMappingAPIAccess = new InvoiceMappingAPIAccess();
            InvoiceMappingDAL objInvoiceMappingDAL = new InvoiceMappingDAL();


            strResponse = objInvoiceMappingAPIAccess.postInvoiceMappingData(m_oLogger);
            if (strResponse == "SUCCESSFUL")
            {
                objInvoiceMappingDAL.UpdateInvoiceMappingData(m_oLogger);
            }
            m_oLogger.LogMessage("Main", "POReceiptsMapping End");
        }
    }
}
