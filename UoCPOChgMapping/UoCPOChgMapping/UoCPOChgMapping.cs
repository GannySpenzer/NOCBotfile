using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UoCMapping;

namespace UoCPOChgMapping
{
    class UoCPOChgMapping
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
            m_oLogger = new Logger(sLogPath, "UoCPOChgMapping");
            m_oLogger.LogMessage("Main", "Started utility UoCPOChgMapping");

            UoCPOChgMappingAPIAccess objUoCPOChgMappingAPIAccess = new UoCPOChgMappingAPIAccess();

            UoCPOChgMappingDAL objUoCPOChgMappingDAL = new UoCPOChgMappingDAL();
            strResponse = objUoCPOChgMappingAPIAccess.postUoCPOChgMappingData(m_oLogger);
            if (strResponse.ToUpper() == "SUCCESS")
            {
                processFlag = "Y";
            }
            else
            {
                processFlag = "E"; //error
            }
            objUoCPOChgMappingDAL.UpdateUoCPOChgMappingData(m_oLogger, processFlag );

            m_oLogger.LogMessage("Main", "UoCPOChgMapping End");

        }
    }
}
