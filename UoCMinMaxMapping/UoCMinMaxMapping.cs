using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UoCMapping;

namespace UoCMinMaxMapping
{
    class UoCMinMaxMapping
    {
        static void Main(string[] args)
        {
            var strResponse = "";
            string processFlag = " ";

            //return;
 
            // InitializeLogger start here

            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "UoCMinMaxMapping");
            m_oLogger.LogMessage("Main", "Started utility UoCMinMaxMapping");

            UoCMinMaxMappingAPIAccess objUoCMinMaxMappingAPIAccess = new UoCMinMaxMappingAPIAccess();
            UoCMinMaxMappingDAL objUoCMinMaxMappingDAL = new UoCMinMaxMappingDAL();
            strResponse = objUoCMinMaxMappingAPIAccess.postUoCMinMaxMappingData(m_oLogger);
            if (strResponse.ToUpper() == "SUCCESS")
            {
                processFlag = "Y";
            }
            else
            {
                processFlag = "E"; //error
            }
            objUoCMinMaxMappingDAL.UpdateUoCMinMaxMappingData(m_oLogger, processFlag );

            m_oLogger.LogMessage("Main", "UoCMinMaxMapping End");

        }
    }
}
