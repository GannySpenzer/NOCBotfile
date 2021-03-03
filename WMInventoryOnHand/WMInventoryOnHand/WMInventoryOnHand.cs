using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMInventoryOnHand;

namespace WMInventoryOnHand
{
    class WMInventoryOnHand
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
            m_oLogger = new Logger(sLogPath, "WMInventoryOnHand");
            m_oLogger.LogMessage("Main", "Started utility WMInventoryOnHand");

            WMInventoryOnHandAPIAccess objWMInventoryOnHandAPIAccess = new WMInventoryOnHandAPIAccess();
            WMInventoryOnHandReDAL objWMInventoryOnHandReDAL = new WMInventoryOnHandReDAL();
            strResponse = objWMInventoryOnHandAPIAccess.postWMInventoryOnHandData(m_oLogger);
            if (strResponse.ToUpper() != " ")
            {

                if (strResponse.ToUpper() == "SUCCESS")
                {
                    m_oLogger.LogMessage("Main", "Successful Response");
                }
                //if (strResponse.ToUpper() == "SUCCESS")
                //{
                //    processFlag = "I";
                //}
                //else
                //{
                //    processFlag = "E"; //error
                //}
                //objWMInventoryOnHandReDAL.UpdateWMInventoryOnHandData(m_oLogger, processFlag);
            }
            else
            {
                m_oLogger.LogMessage("Main", "No response");
            }
            m_oLogger.LogMessage("Main", "WMInventoryOnHand End");
        }
    }
}


