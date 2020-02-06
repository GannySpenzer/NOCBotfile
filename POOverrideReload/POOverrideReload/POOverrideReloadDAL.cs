using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace POOverrideReload1
{
    public class PODData
    {
        public List<string> ACTION_ITEM = new List<string>();
        public List<string> CLIENT = new List<string>();
        public List<string> SITE = new List<string>();
        public List<string> BUSINESS_UNIT = new List<string>();
        public List<string> PO_ID = new List<string>();
        public List<string> LINE_NUMBER = new List<string>();
        public List<DateTime> DATE_ACKNOWLEDGED = new List<DateTime>();
        public List<string> VENDOR_ID = new List<string>();
        public List<string> VENDOR_NAME = new List<string>();
        public List<DateTime> PO_DATE = new List<DateTime>();
        public List<string> BUYER_ID = new List<string>();
        public List<string> OPERATOR_ID = new List<string>();
        public List<string> SHIPTO_ID = new List<string>();
        public List<string> ITEM_ID = new List<string>();
        public List<int> PO_QUANTITY = new List<int>();
        public List<int> QTY_ACKNOWLEDGED = new List<int>();
        public List<string> PO_PRICE = new List<string>();
        public List<string> PRICE_ACKNOWLEDGED = new List<string>();
        public List<string> CURRENCY = new List<string>();
        public List<string> CURRENCY_ACKNOWLEDGED = new List<string>();
        public List<string> UNIT_MEASURE = new List<string>();
        public List<string> UOM_ACKNOWLEDGED = new List<string>();
        public List<DateTime> PO_DUE_DATE = new List<DateTime>();
        public List<DateTime> DUE_DATE_ACKNOWLEDGED = new List<DateTime>();
        public List<string> PRICE_UPDATE_BYPASS = new List<string>();
        public List<string> PRICE_UPDATE_OVERRIDE = new List<string>();
        public List<string> DUE_DATE_BYPASS = new List<string>();
        public List<string> DUE_DATE_OVERRIDE = new List<string>();
        public List<string> QTY_UPDATE_BYPASS = new List<string>();
        public List<string> QTY_OVERRIDE_STATUS = new List<string>();
        public List<string> REVIEW_FLAG = new List<string>();
        public List<string> PS_URL = new List<string>();
        public List<string> BUYER_TEAM = new List<string>();
    }

    public class POOverrideReloadDAL
    {
        System.Data.OleDb.OleDbConnection MyConnection;
        System.Data.DataSet DtSet;
        DataTable dtTransaction;
        System.Data.OleDb.OleDbDataAdapter MyCommand;
        string strSQLstring = "";
        System.Data.OleDb.OleDbConnection MyOracleConn;
        string OracleConString = ConfigurationManager.AppSettings["OLEDBconString"];

        int iLastVal = 0;
        int modValue = 1000;
        int oracleSendLimit = 3000;
        string strResp = "SUCCESS";

        public int dtResponseRowsCount = 0;
        public string gotAllData = "N";

        PODData pod = new PODData();

        // InitializeLogger start here
        //Logger m_oLogger;
        //string sLogPath = Environment.CurrentDirectory;

        DataTable dtResponse = new DataTable();

        public void CreateTable(Logger m_oLogger)
        {
            try
            {
                //check if table already exists
                //strSQLstring = "select table_name from user_tables where table_name='SDIX_POOVERRIDETEMP'";
                //dtResponse = oleDBExecuteReader(strSQLstring);
                //if (dtResponse.Rows.Count > 0)
                //{
                //    //if it does, drop the table
                    strSQLstring = "TRUNCATE TABLE SDIX_POOVERRIDETEMP";
                    dtResponse = oleDBExecuteReader(strSQLstring);
                //}

                strSQLstring = "INSERT INTO SDIX_POOVERRIDETEMP \n";
                strSQLstring += "(SELECT ' ' as ACTION_ITEM,\n";
                strSQLstring += "DECODE( T.ACCOUNTING_OWNER, ' ' , 'Inactive Site',  T.ACCOUNTING_OWNER) as Client,\n";
                strSQLstring += "T.DESCR as Site, \n";
                strSQLstring += "A.BUSINESS_UNIT, \n";
                strSQLstring += "A.PO_ID, \n";
                strSQLstring += "A.LINE_NBR as LINE_NUMBER, \n";
                strSQLstring += "A.DATETIME_ADDED as DATE_ACKNOWLEDGED, \n";
                strSQLstring += "A.VENDOR_ID, \n";
                strSQLstring += "V.NAME1 as VENDOR_NAME,\n";
                strSQLstring += "A.PO_DT as PO_DATE,\n";
                strSQLstring += "A.BUYER_ID, \n";
                strSQLstring += "A.ISA_ACK_OPRID as OPERATOR_ID, \n";
                strSQLstring += "A.SHIPTO_ID, \n";
                strSQLstring += "A.INV_ITEM_ID as ITEM_ID, \n";
                strSQLstring += "A.QTY_PO as PO_QUANTITY, \n";
                strSQLstring += "A.ISA_QTY_ACK as QTY_ACKNOWLEDGED, \n";
                strSQLstring += "A.PRICE_PO, \n";
                strSQLstring += "A.ISA_PRICE_ACK as PRICE_ACKNOWLEDGED, \n";
                strSQLstring += "A.ISA_CURRENCY_PO as CURRENCY, \n";
                strSQLstring += "A.ISA_CURRENCY_ACK as CURRENCY_ACKNOWLEDGED, \n";
                strSQLstring += "A.UOM_PO as UNIT_MEASURE, \n";
                strSQLstring += "A.ISA_UOM_ACK as UOM_ACKNOWLEDGED, \n";
                strSQLstring += "A.ISA_DUE_DT_PO as PO_DUE_DATE, \n";
                strSQLstring += "A.ISA_DUE_DT_ACK as DUE_DATE_ACKNOWLEDGED, \n";
                strSQLstring += "A.ISA_PRICE_UPDBYP as PRICE_UPDATE_BYPASS, \n";
                strSQLstring += "A.ISA_PRICE_UPD_OV as PRICE_UPDATE_OVERRIDE, \n";
                strSQLstring += "A.ISA_DUEDT_UPDBYP as DUE_DATE_BYPASS, \n";
                strSQLstring += "A.ISA_DUEDT_UPD_OV as DUE_DATE_OVERRIDE, \n";
                strSQLstring += "A.ISA_QTY_UPDBYP as QTY_UPDATE_BYPASS, \n";
                strSQLstring += "A.ISA_QTY_UPD_OV as QTY_OVERRIDE_STATUS, \n";
                strSQLstring += "A.ISA_STAT_RVW_FLG as REVIEW_FLAG,\n";
                strSQLstring += "U.url || '/EMPLOYEE/ERP/c/MANAGE_PURCHASE_ORDERS.PURCHASE_ORDER.GBL?Page=PO_LINE&Action=U&BUSINESS_UNIT=' || A.BUSINESS_UNIT || '&PO_ID=' || a.po_ID || '&TargetFrameName=None' as PS_URL,\n";
                strSQLstring += "NVL(P.BUYER_MANAGER, ' ') as BUYER_TEAM,\n";
                strSQLstring += "' ' AS PROCESS_FLAG\n";

                strSQLstring += "FROM sysadm8.PS_ISA_ACK_OVRD A,\n";
                strSQLstring += "sysadm8.PS_SHIPTO_TBL B,\n";
                strSQLstring += "sysadm8.PS_PO_LINE_DISTRIB D,\n";
                strSQLstring += "SYSADM8.PS_DEPT_TBL T,\n";
                strSQLstring += "SYSADM8.PS_PO_MANAGER_TBL P,\n";
                strSQLstring += "SYSADM8.PS_VENDOR V,\n";
                strSQLstring += "SYSADM8.ps_PTSF_URLDEFN_VW U\n";

                strSQLstring += "WHERE A.BUSINESS_UNIT IN('ISA00','CST00','SDM00')\n";
                strSQLstring += "AND A.DATETIME_ADDED > sysdate - 730\n";
                strSQLstring += "AND((A.ISA_PRICE_UPDBYP = 'Y'\n";
                strSQLstring += "AND A.ISA_PRICE_UPD_OV = ' '\n";
                strSQLstring += "OR A.ISA_DUEDT_UPDBYP = 'Y'\n";
                strSQLstring += "AND A.ISA_DUEDT_UPD_OV = ' '\n";
                strSQLstring += "OR A.ISA_STAT_RVW_FLG = 'W'))\n";
                strSQLstring += "AND A.SHIPTO_ID = B.SHIPTO_ID\n";
                strSQLstring += "and B.EFFDT =\n";
                strSQLstring += "(SELECT MAX(B_ED.EFFDT) FROM SYSADM8.PS_SHIPTO_TBL B_ED\n";
                strSQLstring += "WHERE(B.SETID = B_ED.SETID)\n";
                strSQLstring += "AND B.SHIPTO_ID = B_ED.SHIPTO_ID)\n";
                strSQLstring += "AND A.BUSINESS_UNIT = D.BUSINESS_UNIT\n";
                strSQLstring += "AND A.PO_ID = D.PO_ID\n";
                strSQLstring += "AND A.LINE_NBR = D.LINE_NBR\n";
                strSQLstring += "AND A.BUYER_ID = P.BUYER_ID(+)\n";
                strSQLstring += "AND D.DEPTID = T.DEPTID\n";
                strSQLstring += "AND A.VENDOR_ID = V.VENDOR_ID\n";
                strSQLstring += "AND U.URL_ID = 'EMP_SERVLET')";

                m_oLogger.LogMessage("CreateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("CreateTable", "Query To create the POOverride temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                strSQLstring = "SELECT Process_Flag from SDIX_POOVERRIDETEMP";
                dtResponse = oleDBExecuteReader(strSQLstring);
                dtResponseRowsCount = dtResponse.Rows.Count;

                m_oLogger.LogMessage("CreateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("CreateTable", "Error trying to create the POOverride temp data table.", ex);
            }

        }

        public void UpdateTable(Logger m_oLogger)
        {
            try
            {
                strSQLstring = "UPDATE SDIX_POOVERRIDETEMP SET Process_Flag = 'X' Where Process_Flag <> 'X' And rownum < " + (oracleSendLimit + 1);
                m_oLogger.LogMessage("UpdateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("UpdateTable", "Query To update the POOverride temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                m_oLogger.LogMessage("UpdateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("UpdateTable", "Error trying to update the POOverride temp data table.", ex);
            }

        }



        public DataTable getPOOverrideData(Logger m_oLogger)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                strSQLstring = "SELECT * FROM SDIX_POOVERRIDETEMP\n";
                strSQLstring += "where process_flag <> 'X' and rownum < " + (oracleSendLimit + 1);

                m_oLogger.LogMessage("getPOOverrideData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getPOOverrideData", "Query To get the POOverride data: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                m_oLogger.LogMessage("getPOOverrideData", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("getMatchExcepData", "Error trying to get the POOverride data.", ex);
            }
            return dtResponse;
        }


        public DataTable oleDBExecuteReader(string strQuery)
        {
            //  var isValidUser = false;
            DataSet dsResponse = new DataSet();
            DataTable dtResponse = new DataTable();
            try
            {
                using (MyOracleConn = new OleDbConnection(OracleConString))
                {
                    MyOracleConn.Open();
                    using (OleDbCommand myCommand = new OleDbCommand(strQuery, MyOracleConn))
                    {
                        dtResponse.Load(myCommand.ExecuteReader());
                    }
                }

            }
            catch (Exception ex)
            {
                dtResponse = null;
                // throw ex;
            }


            return dtResponse;
        }

        public Boolean OleDBExecuteReader(string strQuery)
        {
            var isValidUser = false;
            try
            {
                using (MyOracleConn = new OleDbConnection(OracleConString))
                {
                    MyOracleConn.Open();
                    using (OleDbCommand myCommand = new OleDbCommand(strQuery, MyOracleConn))
                    {
                        OleDbDataReader myReader = myCommand.ExecuteReader();
                        if (myReader.HasRows)
                        {
                            isValidUser = true;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                isValidUser = false;
                throw ex;
            }


            return isValidUser;

        }

        public int OleDBExecuteNonQuery(string strQuery)
        {
            int rowsaffected = 0;

            try
            {
                MyOracleConn = new OleDbConnection(OracleConString);
                OleDbCommand Command = new OleDbCommand(strQuery, MyOracleConn);
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(Command);
                System.Data.DataSet UserdataSet = new System.Data.DataSet();
                MyOracleConn.Open();
                rowsaffected = Command.ExecuteNonQuery();
                MyOracleConn.Close();
            }
            catch (Exception ex)
            {
                rowsaffected = 0;
            }

            return rowsaffected;
        }

        public PODData getData(Logger m_oLogger)
        {
            //Logger m_oLogger;
            //string sLogPath = Environment.CurrentDirectory;
            //if (!sLogPath.EndsWith(@"\"))
            //    sLogPath += @"\";
            //sLogPath += "Logs";
            //m_oLogger = new Logger(sLogPath, "MatchExcepReload");
            m_oLogger.LogMessage("BatchPOOverride", "Entered BatchPOOverride class");

            pod.ACTION_ITEM.Clear();
            pod.CLIENT.Clear();
            pod.SITE.Clear();
            pod.BUSINESS_UNIT.Clear();
            pod.PO_ID.Clear();
            pod.LINE_NUMBER.Clear();
            pod.DATE_ACKNOWLEDGED.Clear();
            pod.VENDOR_ID.Clear();
            pod.VENDOR_NAME.Clear();
            pod.PO_DATE.Clear();
            pod.BUYER_ID.Clear();
            pod.OPERATOR_ID.Clear();
            pod.SHIPTO_ID.Clear();
            pod.ITEM_ID.Clear();
            pod.PO_QUANTITY.Clear();
            pod.QTY_ACKNOWLEDGED.Clear();
            pod.PO_PRICE.Clear();
            pod.PRICE_ACKNOWLEDGED.Clear();
            pod.CURRENCY.Clear();
            pod.CURRENCY_ACKNOWLEDGED.Clear();
            pod.UNIT_MEASURE.Clear();
            pod.UOM_ACKNOWLEDGED.Clear();
            pod.PO_DUE_DATE.Clear();
            pod.DUE_DATE_ACKNOWLEDGED.Clear();
            pod.PRICE_UPDATE_BYPASS.Clear();
            pod.PRICE_UPDATE_OVERRIDE.Clear();
            pod.DUE_DATE_BYPASS.Clear();
            pod.DUE_DATE_OVERRIDE.Clear();
            pod.QTY_UPDATE_BYPASS.Clear();
            pod.QTY_OVERRIDE_STATUS.Clear();
            pod.REVIEW_FLAG.Clear();
            pod.PS_URL.Clear();
            pod.BUYER_TEAM.Clear();

            //STEP #3 - QUERY TABLE AND POST NEW DATA 
            m_oLogger.LogMessage("POOverrideReload", "Query table started");
            //MatchExcepReloadDAL objGetMatchExcepReloadDAL = new MatchExcepReloadDAL();
            //dtResponse = objGetMatchExcepReloadDAL.getMatchExcepData(m_oLogger);
            dtResponse = getPOOverrideData(m_oLogger);
            dtResponseRowsCount = dtResponse.Rows.Count;

            if (dtResponseRowsCount < oracleSendLimit)
            {
                gotAllData = "Y";
            }

            if (dtResponseRowsCount == 0)
            {
                m_oLogger.LogMessage("POOverrideReload", "Query returned no records.");
                return null; ;
            }
            else
                m_oLogger.LogMessage("POOverrideReload", "POST POOverrideReload data started.");
            for (int i = 0; i < dtResponseRowsCount; i++)
            {
                DataRow rowInit;
                rowInit = dtResponse.Rows[i];

                try
                {
                    pod.ACTION_ITEM.Add(rowInit["ACTION_ITEM"].ToString());
                    pod.CLIENT.Add(rowInit["CLIENT"].ToString());
                    pod.SITE .Add(rowInit["SITE"].ToString());
                    pod.BUSINESS_UNIT .Add(rowInit["BUSINESS_UNIT"].ToString());
                    pod.PO_ID.Add(rowInit["PO_ID"].ToString());
                    pod.LINE_NUMBER .Add(rowInit["LINE_NUMBER"].ToString());
                    pod.DATE_ACKNOWLEDGED.Add(Convert.ToDateTime(rowInit["DATE_ACKNOWLEDGED"]));
                    pod.VENDOR_ID .Add(rowInit["VENDOR_ID"].ToString());
                    pod.VENDOR_NAME .Add(rowInit["VENDOR_NAME"].ToString());
                    pod.PO_DATE .Add(Convert.ToDateTime(rowInit["PO_DATE"]));
                    pod.BUYER_ID.Add(rowInit["BUYER_ID"].ToString());
                    pod.OPERATOR_ID .Add(rowInit["OPERATOR_ID"].ToString());
                    pod.SHIPTO_ID .Add(rowInit["SHIPTO_ID"].ToString());
                    pod.ITEM_ID .Add(rowInit["ITEM_ID"].ToString());
                    pod.PO_QUANTITY .Add(Convert.ToInt32( rowInit["PO_QUANTITY"]));
                    pod.QTY_ACKNOWLEDGED .Add(Convert.ToInt32( rowInit["QTY_ACKNOWLEDGED"]));
                    pod.PO_PRICE .Add( rowInit["PRICE_PO"].ToString());
                    pod.PRICE_ACKNOWLEDGED .Add(rowInit["PRICE_ACKNOWLEDGED"].ToString());
                    pod.CURRENCY .Add(rowInit["CURRENCY"].ToString());
                    pod.CURRENCY_ACKNOWLEDGED .Add(rowInit["CURRENCY_ACKNOWLEDGED"].ToString());
                    pod.UNIT_MEASURE.Add(rowInit["UNIT_MEASURE"].ToString ());
                    pod.UOM_ACKNOWLEDGED .Add(rowInit["UOM_ACKNOWLEDGED"].ToString());
                    pod.PO_DUE_DATE .Add(Convert.ToDateTime ( rowInit["PO_DUE_DATE"]));
                    pod.DUE_DATE_ACKNOWLEDGED .Add(Convert.ToDateTime(rowInit["DUE_DATE_ACKNOWLEDGED"]));
                    pod.PRICE_UPDATE_BYPASS .Add(rowInit["PRICE_UPDATE_BYPASS"].ToString());
                    pod.PRICE_UPDATE_OVERRIDE .Add(rowInit["PRICE_UPDATE_OVERRIDE"].ToString());
                    pod.DUE_DATE_BYPASS .Add(rowInit["DUE_DATE_BYPASS"].ToString());
                    pod.DUE_DATE_OVERRIDE .Add(rowInit["DUE_DATE_OVERRIDE"].ToString());
                    pod.QTY_UPDATE_BYPASS .Add(rowInit["QTY_UPDATE_BYPASS"].ToString());
                    pod.QTY_OVERRIDE_STATUS .Add(rowInit["QTY_OVERRIDE_STATUS"].ToString());
                    pod.REVIEW_FLAG .Add(rowInit["REVIEW_FLAG"].ToString());
                    pod.PS_URL .Add(rowInit["PS_URL"].ToString());
                    pod.BUYER_TEAM .Add(rowInit["BUYER_TEAM"].ToString());

                }
                catch (Exception ex)
                {
                    m_oLogger.LogMessage("POOverrideReload", "Error trying to parse data at line " + i.ToString(), ex);

                }

            }

            m_oLogger.LogMessage("POOverrideReload", "Query table and parse successful.");
            return pod;

        }


    }
}
