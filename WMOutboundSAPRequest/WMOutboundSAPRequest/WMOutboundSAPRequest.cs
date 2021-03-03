using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMOutboundSAPRequest;

namespace WMOutboundSAPRequest
{
    class WMOutboundSAPRequest
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
            m_oLogger = new Logger(sLogPath, "WMOutboundSAPRequest");
            m_oLogger.LogMessage("Main", "Started utility WMOutboundSAPRequest");

            WMOutboundSAPRequestAPIAccess objWMOutboundSAPRequestAPIAccess = new WMOutboundSAPRequestAPIAccess();
            WMOubtboundSAPRequestDAL objWMOubtboundSAPRequestDAL = new WMOubtboundSAPRequestDAL();
            strResponse = objWMOutboundSAPRequestAPIAccess.postWMOutboundSAPRequestData(m_oLogger);
            //if (strResponse.ToUpper() == "SUCCESS")
            //{
            //    processFlag = "I";
            //}
            //else
            //{
            //    processFlag = "E"; //error
            //}
            //objWMOubtboundSAPRequestDAL.UpdateWMOutboundSAPRequestData(m_oLogger, processFlag);

            m_oLogger.LogMessage("Main", "WMOutboundSAPRequest End");
        }
    }
}
