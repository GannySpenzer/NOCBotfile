using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace POMapping
{
    class POMapping
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
            m_oLogger = new Logger(sLogPath, "POMapping");

            POMappingAPIAccess objPOMappingAPIAccess = new POMappingAPIAccess();
            POMappingDAL objPOMappingDAL = new POMappingDAL();
            m_oLogger.LogMessage("Main", "Started utility POMapping");

            strResponse = objPOMappingAPIAccess.postPOMappingData(m_oLogger);
          
            if (strResponse == "SUCCESSFUL")
            {
                objPOMappingDAL.UpdatePOMappingData(m_oLogger);
            }
            m_oLogger.LogMessage("Main", "POMapping End");

        }



    }

    //private void InitializeLogger()
    //   {
    //       string sLogPath = Environment.CurrentDirectory;
    //       if (!sLogPath.EndsWith(@"\"))
    //           sLogPath += @"\";
    //       sLogPath += "Logs";
    //       m_oLogger = new Logger(sLogPath, "Populate_BU_UNSPSC_Tbl");
    //   }


}
