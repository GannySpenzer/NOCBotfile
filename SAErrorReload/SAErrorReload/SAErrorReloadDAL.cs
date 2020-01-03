using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace SAErrorReload1
{
    public class SAEData
    {
        public List<string> ACTION_ITEM = new List<string>();
        public List<string> CLIENT = new List<string>();
        public List<string> DESCRIPTION = new List<string>();
        public List<string> BUYER_ID = new List<string>();
        public List<string> ITEM = new List<string>();
        public List<string> STOCK_TYPE = new List<string>();
        public List<string> STAGE_STATUS = new List<string>();
        public List<string> MESSAGE = new List<string>();
        public List<string> REQ_ID= new List<string>();
        public List<string> REQ_LINE = new List<string>();
        public List<DateTime> REQ_DATE = new List<DateTime>();
        public List<string> VENDOR_ID = new List<string>();
        public List<string> VENDOR_NAME = new List<string>();
        public List<string> REQUISITION_PRICE = new List<string>();
        public List<DateTime> SOURCE_DATE = new List<DateTime>();
        public List<DateTime> TODAYS_DATE = new List<DateTime>();
        public List<DateTime> DATE_LAST_MODIFIED = new List<DateTime>();
        public List<int> DAYS_SINCE_SOURCE_DATE = new List<int>();
        public List<int> DAYS_SINCE_LAST_MODIFIED = new List<int>();
        public List<DateTime> EXCEPTION_DATE = new List<DateTime>();
        public List<int> EXCEPTION_NUM_DAYS = new List<int>();
        public List<string> SHIPTO_ID = new List<string>();
        public List<string> PRIORITY_FLAG = new List<string>();
        public List<string> SITE_NAME = new List<string>();
        public List<string> PS_URL = new List<string>();
        public List<string> BUYER_TEAM = new List<string>();
    }

    public class SAErrorReloadDAL
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

        SAEData sae = new SAEData();

        // InitializeLogger start here
        //Logger m_oLogger;
        //string sLogPath = Environment.CurrentDirectory;

        DataTable dtResponse = new DataTable();

        public void CreateTable(Logger m_oLogger)
        {
            try
            {
                //check if table already exists
                strSQLstring = "select table_name from user_tables where table_name='SDIX_SAERRORTEMP'";
                dtResponse = oleDBExecuteReader(strSQLstring);
                if (dtResponse.Rows.Count > 0)
                {
                    //if it does, drop the table
                    strSQLstring = "DROP TABLE SDIX_SAERRORTEMP";
                    dtResponse = oleDBExecuteReader(strSQLstring);
                }

                strSQLstring = "CREATE TABLE SDIX_SAERRORTEMP as\n";
                strSQLstring += "(Select DISTINCT ' ' as Action_Item,\n";
                strSQLstring += "DECODE(E.ACCOUNTING_OWNER, ' ', 'Inactive Site', E.ACCOUNTING_OWNER) as Client, \n";
                strSQLstring += "e.descr as Site_Name,\n";
                strSQLstring += "e.descr as DESCRIPTION,\n";
                strSQLstring += "NVL(d.BUYER_ID, ' ') as Buyer_ID, \n";
                strSQLstring += "d.INV_ITEM_ID as Item, \n";
                strSQLstring += "DECODE(d.INV_ITEM_ID, ' ', 'NSTK', 'STK') as Stock_Type, \n";
                strSQLstring += "c.xlatlongname as Stage_Status, \n";
                strSQLstring += "NVL(g.message_text, ' ') as Message,\n";
                strSQLstring += "d.REQ_ID as Req_ID, \n";
                strSQLstring += "d.line_nbr as Req_Line, \n";
                strSQLstring += "TO_CHAR(B.REQ_DT, 'YYYY-MM-DD') as Req_Date,\n";
                strSQLstring += "d.vendor_id as Vendor_ID,\n";
                strSQLstring += "NVL(v.name1, ' ') as Vendor_Name,\n";
                strSQLstring += "d.price_req as Requisition_Price,\n";
                strSQLstring += "TO_CHAR(D.SOURCE_DATE, 'YYYY-MM-DD') as Source_Date, \n";
                strSQLstring += "SYSDATE As Todays_Date, \n";
                strSQLstring += "SUBSTR(TO_CHAR(D.DATETIME_MODIFIED, 'YYYY-MM-DD'), 1, 10) as Date_Last_Modified, \n";
                strSQLstring += "ROUND(SYSDATE - TO_DATE(TO_CHAR(D.SOURCE_DATE, 'YYYY-MM-DD'), 'YYYY-MM-DD')) as Days_Since_Source_Date, \n";
                strSQLstring += "MAX(ROUND(SYSDATE - TO_DATE(SUBSTR(TO_CHAR(D.DATETIME_MODIFIED, 'YYYY-MM-DD'), 1, 10), 'YYYY-MM-DD'))) as Days_Since_Last_Modified,\n";
                strSQLstring += "' ' as EXCEPTION_DATE,\n";
                strSQLstring += "' ' as EXCEPTION_NUM_DAYS,\n";
                strSQLstring += "d.SHIPTO_ID as Shipto_ID, \n";
                strSQLstring += "q.isa_priority_flag as Priority_Flag,\n";
                strSQLstring += "NVL(r.roleuser, ' ') as Buyer_Team,\n";
                strSQLstring += "U.url || '/EMPLOYEE/ERP/c/MANAGE_PURCHASE_ORDERS.PO_SRC_ANALYSIS.GBL?Page=PO_SRC_ANALYSIS&Action=U&BUSINESS_UNIT=' || B.BUSINESS_UNIT || '&REQ_ID=' || B.REQ_ID || '&TargetFrameName=None' as PS_URL,\n";
                strSQLstring += "' ' as PROCESS_FLAG\n";

                strSQLstring += "FROM sysadm8.PS_REQ_HDR B, sysadm8.PS_PO_ITM_STG D, sysadm8.PS_DEPT_TBL E, sysadm8.ps_req_ln_distrib A, sysadm8.XLATTABLE_VW c, sysadm8.ps_msg_Cat_VW G,\n";
                strSQLstring += " sysadm8.ps_rte_cntl_ruser R, sysadm8.ps_vendor V, sysadm8.ps_isa_req_bi_info q, sysadm8.ps_ptsf_urldefn_vw u \n";

                strSQLstring += "  WHERE d.BUSINESS_UNIT IN('ISA00','CST00','SDM00')\n";
                strSQLstring += "     AND D.STAGE_STATUS IN('E','V' )\n";
                strSQLstring += "     AND d.BUSINESS_UNIT = B.BUSINESS_UNIT\n";
                strSQLstring += "     AND d.REQ_ID = B.REQ_ID\n";
                strSQLstring += "     and b.business_unit = A.business_unit\n";
                strSQLstring += "     and b.req_id = a.req_id\n";
                strSQLstring += "     and d.stage_status = c.fieldvalue\n";
                strSQLstring += "     and c.fieldname = 'STAGE_STATUS'\n";
                strSQLstring += "     AND E.DEPTID = a.DEPTID\n";
                strSQLstring += "     AND E.EFFDT =\n";
                strSQLstring += "        (SELECT MAX(E_ED.EFFDT) FROM sysadm8.PS_DEPT_TBL E_ED\n";
                strSQLstring += "        WHERE E.SETID = E_ED.SETID\n";
                strSQLstring += "          AND E.DEPTID = E_ED.DEPTID)\n";
                strSQLstring += "AND d.message_set_nbr = g.message_set_nbr(+)\n";
                strSQLstring += "AND d.message_nbr = g.message_nbr(+)\n";
                strSQLstring += "and a.deptid = r.rte_cntl_profile(+)\n";
                strSQLstring += "and case \n";
                strSQLstring += "when a.business_unit = 'ISA00' then 'SDI_PROCURE_SUPERVISOR'\n";
                strSQLstring += "when a.business_unit = 'SDM00' then 'SDI_SITE_MANAGER_SDM'\n";
                strSQLstring += "else 'OTHER' end = r.rolename(+)\n";
                strSQLstring += "and d.vendor_id = v.vendor_id(+)\n";
                strSQLstring += "and a.business_unit = q.business_unit\n";
                strSQLstring += "and a.req_id = q.req_id\n";
                strSQLstring += "and a.line_nbr = q.line_nbr\n";
                strSQLstring += "AND U.url_id = 'EMP_SERVLET'\n";

                //strSQLstring += "AND ROWNUM < 11\n";
                strSQLstring += "  GROUP BY  'Error',  DECODE(E.ACCOUNTING_OWNER, ' ', 'Inactive Site', E.ACCOUNTING_OWNER),  E.DESCR,  d.BUYER_ID,  \n";
                strSQLstring += "  d.INV_ITEM_ID,  DECODE(d.INV_ITEM_ID, ' ', 'NSTK', 'STK'),  c.xlatlongname, g.message_text, d.REQ_ID, d.line_nbr ,  d.SCHED_NBR,d.vendor_id, v.name1, d.price_req,  \n";
                strSQLstring += "  TO_CHAR(B.REQ_DT, 'YYYY-MM-DD'),  TO_CHAR(D.SOURCE_DATE, 'YYYY-MM-DD'),  SYSDATE,  SUBSTR(TO_CHAR(D.DATETIME_MODIFIED, 'YYYY-MM-DD'), 1, 10),  \n";
                strSQLstring += "  ROUND(SYSDATE - TO_DATE(TO_CHAR(D.SOURCE_DATE, 'YYYY-MM-DD'), 'YYYY-MM-DD')),d.SHIPTO_ID, q.isa_priority_flag, r.roleuser, \n";
                strSQLstring +="   U.url || '/EMPLOYEE/ERP/c/MANAGE_PURCHASE_ORDERS.PO_SRC_ANALYSIS.GBL?Page=PO_SRC_ANALYSIS&Action=U&BUSINESS_UNIT=' || B.BUSINESS_UNIT || '&REQ_ID=' || B.REQ_ID || '&TargetFrameName=None' \n";
                strSQLstring += ")"; // order by 17";

                m_oLogger.LogMessage("CreateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("CreateTable", "Query To create the SAError temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                strSQLstring = "SELECT Process_Flag from SDIX_SAERRORTEMP";
                dtResponse = oleDBExecuteReader(strSQLstring);
                dtResponseRowsCount = dtResponse.Rows.Count;

                m_oLogger.LogMessage("CreateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("CreateTable", "Error trying to create the SAError temp data table.", ex);
            }

        }

        public void UpdateTable(Logger m_oLogger)
        {
            try
            {
                strSQLstring = "UPDATE SDIX_SAERRORTEMP SET Process_Flag = 'X' Where Process_Flag <> 'X' And rownum < " + (oracleSendLimit + 1);
                m_oLogger.LogMessage("UpdateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("UpdateTable", "Query To update the SAError temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                m_oLogger.LogMessage("UpdateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("UpdateTable", "Error trying to update the SAError temp data table.", ex);
            }

        }



        public DataTable getSAErrorData(Logger m_oLogger)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                strSQLstring = "SELECT * FROM SDIX_SAERRORTEMP\n";
                strSQLstring += "where process_flag <> 'X' and rownum < " + (oracleSendLimit + 1);

                m_oLogger.LogMessage("getSAErrorData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getSAErrorData", "Query To get the SAError data: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                m_oLogger.LogMessage("getSAErrorData", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("getMatchExcepData", "Error trying to get the SAError data.", ex);
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

        public SAEData getData(Logger m_oLogger)
        {
            //Logger m_oLogger;
            //string sLogPath = Environment.CurrentDirectory;
            //if (!sLogPath.EndsWith(@"\"))
            //    sLogPath += @"\";
            //sLogPath += "Logs";
            //m_oLogger = new Logger(sLogPath, "MatchExcepReload");
            m_oLogger.LogMessage("BatchSAError", "Entered BatchSAError class");

            sae.ACTION_ITEM.Clear();
            sae.CLIENT.Clear();
            sae.DESCRIPTION.Clear();
            sae.BUYER_ID.Clear();
            sae.ITEM.Clear();
            sae.STOCK_TYPE.Clear();
            sae.STAGE_STATUS.Clear();
            sae.MESSAGE.Clear();
            sae.REQ_ID.Clear();
            sae.REQ_LINE.Clear();
            sae.REQ_DATE.Clear();
            sae.VENDOR_ID.Clear();
            sae.VENDOR_NAME.Clear();
            sae.REQUISITION_PRICE.Clear();
            sae.SOURCE_DATE.Clear();
            sae.TODAYS_DATE.Clear();
            sae.DATE_LAST_MODIFIED.Clear();
            sae.DAYS_SINCE_SOURCE_DATE.Clear();
            sae.DAYS_SINCE_LAST_MODIFIED.Clear();
            sae.EXCEPTION_DATE.Clear();
            sae.EXCEPTION_NUM_DAYS.Clear();
            sae.SHIPTO_ID.Clear();
            sae.PRIORITY_FLAG.Clear();
            sae.SITE_NAME.Clear();
            sae.PS_URL.Clear();
            sae.BUYER_TEAM.Clear();

            //STEP #3 - QUERY TABLE AND POST NEW DATA 
            m_oLogger.LogMessage("SAErrorReload", "Query table started");
            //MatchExcepReloadDAL objGetMatchExcepReloadDAL = new MatchExcepReloadDAL();
            //dtResponse = objGetMatchExcepReloadDAL.getMatchExcepData(m_oLogger);
            dtResponse = getSAErrorData(m_oLogger);
            dtResponseRowsCount = dtResponse.Rows.Count;

            if (dtResponseRowsCount < oracleSendLimit)
            {
                gotAllData = "Y";
            }

            if (dtResponseRowsCount == 0)
            {
                m_oLogger.LogMessage("SAErrorReload", "Query returned no records.");
                return null; ;
            }
            else
                m_oLogger.LogMessage("SAErrorReload", "POST SAErrorReload data started.");
            for (int i = 0; i < dtResponseRowsCount; i++)
            {
                DataRow rowInit;
                rowInit = dtResponse.Rows[i];

                try
                {
                    sae.ACTION_ITEM.Add(rowInit["ACTION_ITEM"].ToString());
                    sae.CLIENT.Add(rowInit["CLIENT"].ToString());
                    sae.DESCRIPTION.Add(rowInit["DESCRIPTION"].ToString());
                    sae.BUYER_ID.Add(rowInit["BUYER_ID"].ToString());
                    sae.ITEM.Add(rowInit["ITEM"].ToString());
                    sae.STOCK_TYPE.Add(rowInit["STOCK_TYPE"].ToString());
                    sae.STAGE_STATUS.Add(rowInit["STAGE_STATUS"].ToString());
                    sae.MESSAGE.Add(rowInit["MESSAGE"].ToString());
                    sae.REQ_ID.Add(rowInit["REQ_ID"].ToString());
                    sae.REQ_LINE.Add(rowInit["REQ_LINE"].ToString());
                    sae.REQ_DATE.Add(Convert.ToDateTime(rowInit["REQ_DATE"]));
                    sae.VENDOR_ID.Add(rowInit["VENDOR_ID"].ToString());
                    sae.VENDOR_NAME.Add(rowInit["VENDOR_NAME"].ToString());
                    sae.REQUISITION_PRICE.Add(rowInit["REQUISITION_PRICE"].ToString());
                    sae.SOURCE_DATE.Add(Convert.ToDateTime(rowInit["SOURCE_DATE"]));
                    sae.TODAYS_DATE.Add(Convert.ToDateTime(rowInit["TODAYS_DATE"]));
                    sae.DATE_LAST_MODIFIED.Add(Convert.ToDateTime(rowInit["DATE_LAST_MODIFIED"]));
                    sae.DAYS_SINCE_SOURCE_DATE.Add(Convert.ToInt32(rowInit["DAYS_SINCE_SOURCE_DATE"]));
                    sae.DAYS_SINCE_LAST_MODIFIED.Add(Convert.ToInt32(rowInit["DAYS_SINCE_LAST_MODIFIED"]));

                    //if (rowInit["EXCEPTION_DATE"].ToString().Trim() != "")
                    //{
                    //    sae.EXCEPTION_DATE.Add(Convert.ToDateTime(rowInit["EXCEPTION_DATE"]));
                    //}
                    //else
                    //{
                    //    sae.EXCEPTION_DATE.Add(DateTime.Now);
                    //}

                    //if (rowInit["EXCEPTION_NUM_DAYS"].ToString().Trim() == "")
                    //{
                    //    sae.EXCEPTION_NUM_DAYS.Add(0);
                    //}
                    //else
                    //{
                    //    sae.EXCEPTION_NUM_DAYS.Add(Convert.ToInt32(rowInit["EXCEPTION_NUM_DAYS"]));
                    //}

                    sae.SHIPTO_ID .Add(rowInit["SHIPTO_ID"].ToString());
                    sae.PRIORITY_FLAG .Add(rowInit["PRIORITY_FLAG"].ToString());
                    sae.SITE_NAME .Add(rowInit["SITE_NAME"].ToString());
                    sae.PS_URL .Add(rowInit["PS_URL"].ToString());
                    sae.BUYER_TEAM .Add(rowInit["BUYER_TEAM"].ToString());

                }
                catch (Exception ex)
                {
                    m_oLogger.LogMessage("SAErrorReload", "Error trying to parse data at line " + i.ToString(), ex);

                }

            }

            m_oLogger.LogMessage("SAErrorReload", "Query table and parse successful.");
            return sae;

        }


    }
}
