using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POIssueMapping
{
    class POIssueMapping
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
            m_oLogger = new Logger(sLogPath, "POIssueMapping");

            POIssueMappingAPIAccess objPOIssueMappingAPIAccess = new POIssueMappingAPIAccess();
            POIssueMappingDAL objPOIssueMappingDAL = new POIssueMappingDAL();
            m_oLogger.LogMessage("Main", "Started utility POIssueMapping");

            strResponse = objPOIssueMappingAPIAccess.postPOIssueMappingData(m_oLogger);

            if (strResponse == "SUCCESSFUL")
            {
                objPOIssueMappingDAL.UpdatePOIssueMappingData(m_oLogger);
            }
            m_oLogger.LogMessage("Main", "POIssueMapping End");
        }
    }
}
