using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMInventoryGoodsMovements;

namespace WMInventoryGoodsMovements
{
    class WMInventoryGoodsMovements
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
            m_oLogger = new Logger(sLogPath, "WMInventoryGoodsMovements");
            m_oLogger.LogMessage("Main", "Started utility WMInventoryGoodsMovements");

            WMInventoryGoodsMovementsAPIAccess objWMInventoryGoodsMovementsAPIAccess = new WMInventoryGoodsMovementsAPIAccess();
            WMInventoryGoodsMovementsDAL objWMInventoryGoodsMovementsDAL = new WMInventoryGoodsMovementsDAL();
            strResponse = objWMInventoryGoodsMovementsAPIAccess.postWMInventoryGoodsMovementsData(m_oLogger);
            if (strResponse.ToUpper() != " ")
            {
            
                if (strResponse.ToUpper() == "SUCCESS")
            {
                processFlag = "I";
            }
            else
            {
                processFlag = "E"; //error
            }
            objWMInventoryGoodsMovementsDAL.UpdateWMInventoryGoodsMovementsData(m_oLogger, processFlag);
            }
            else
            {
                m_oLogger.LogMessage("Main", "The transaction is belong to 101");
            }
            m_oLogger.LogMessage("Main", "WMInventoryGoodsMovements End");
        }
    }
}

