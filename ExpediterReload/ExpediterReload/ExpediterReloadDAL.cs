using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using ExpediterReload1;
using System.Threading.Tasks;

namespace ExpediterReload
{
    public class BEData
    {
        public List<string> ACTION_ITEMS = new List<string>();
        public List<string> BUSINESS_UNIT = new List<string>();
        public List<string> BUYER_ID = new List<string>();
        public List<string> BUYER_TEAM = new List<string>();
        public List<string> CLIENT = new List<string>();
        public List<string> DESCRIPTION = new List<string>();
        public List<string> EXPEDITING_COMMENTS = new List<string>();
        public List<string> INVENTORY_BUSINESS_UNIT = new List<string>();
        public List<string> ITEM = new List<string>();
        public List<DateTime> LAST_COMMENT_DATE = new List<DateTime>();
        public List<string> LAST_OPERATOR = new List<string>();
        public List<string> LINE_NUMBER = new List<string>();
        public List<DateTime> PO_DATE = new List<DateTime>();
        public List<string> PO_ID = new List<string>();
        public List<string> PS_URL = new List<string>();
        public List<string> PRIORITY_FLAG = new List<string>();
        public List<string> PROBLEM_CODE = new List<string>();
        public List<string> SITE_NAME = new List<string>();
        public List<int> STATUS_AGE = new List<int>();
        public List<string> VENDOR_ID = new List<string>();
        public List<string> VENDOR_NAME = new List<string>();
        
    }

    public class ExpediterReloadDAL
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
        DateTime dateparse;

        BEData bed = new BEData ();

        DataTable dtResponse = new DataTable();

        public void CreateTable(Logger m_oLogger)
        {

            try
            {
                //check if table already exists
                strSQLstring = "select table_name from user_tables where table_name='SDIX_BUYEXPTEMP'";
                dtResponse = oleDBExecuteReader(strSQLstring);
                if (dtResponse.Rows.Count > 0)
                {
                    //if it does, drop the table
                    strSQLstring = "DROP TABLE SDIX_BUYEXPTEMP";
                    dtResponse = oleDBExecuteReader(strSQLstring);
                }

                strSQLstring = "CREATE TABLE SDIX_BUYEXPTEMP as\n";
                strSQLstring += "(SELECT ' ' as ACTION_ITEMS,";
                strSQLstring += "DECODE(T.ACCOUNTING_OWNER, ' ', 'Inactive Site', T.ACCOUNTING_OWNER) as CLIENT, \n";
                //strSQLstring += "T.descr as DESCRIPTION,\n";
                strSQLstring += "E.PO_DT AS PO_DATE, \n";
                strSQLstring += "A.BUSINESS_UNIT as BUSINESS_UNIT, \n";
                strSQLstring += "A.PO_ID as PO_ID, \n";
                strSQLstring += "A.LINE_NBR as LINE_NBR, \n";
                strSQLstring += "D.INV_ITEM_ID as ITEM, \n";
                strSQLstring += "D.DESCR254_MIXED as DESCRIPTION, \n";
                strSQLstring += "Z.descr as PROBLEM_CODE, \n";
                strSQLstring += "B.NOTES_1000 as EXPEDITING_COMMENTS, \n";
                strSQLstring += "NVL(E.buyer_id, ' ') as BUYER_ID, \n";
                strSQLstring += "E.VENDOR_ID as VENDOR_ID, \n";
                strSQLstring += "V.NAME1 as VENDOR,\n";
                strSQLstring += "B.OPRID as LAST_OPERATOR, \n";
                strSQLstring += "B.DTTM_STAMP as LAST_COMMENT_DATE,\n";
                strSQLstring += "S.BUSINESS_UNIT_IN as BUSINESS_UNIT_IN,\n";
                strSQLstring += "q.ISA_PRIORITY_FLAG as PRIORITY_FLAG,\n";
                strSQLstring += "trunc(sysdate) - trunc(b.DTTM_STAMP) as STATUS_AGE,\n";
                strSQLstring += "NVL(R.ROLEUSER, ' ') as BUYER_TEAM,\n";
                strSQLstring += "U.url || '/EMPLOYEE/ERP/c/MANAGE_PURCHASE_ORDERS.PURCHASE_ORDER.GBL?Page=PO_LINE&Action=U&BUSINESS_UNIT=' || A.BUSINESS_UNIT || '&PO_ID=' || A.PO_ID || '&TargetFrameName=None' as PS_URL,\n";
                strSQLstring += "' ' as PROCESS_FLAG\n";

                strSQLstring += "FROM sysadm8.PS_ISA_EXPED_XREF A,\n";
                strSQLstring += "sysadm8.PS_ISA_XPD_COMMENT B,\n";
                strSQLstring += "sysadm8.PS_PO_LINE D,\n";
                strSQLstring += "sysadm8.ps_PO_line_distrib S,\n";
                strSQLstring += "sysadm8.PS_PO_HDR E,\n";
                strSQLstring += "sysadm8.PS_VENDOR V,\n";
                strSQLstring += "SDIEXCHANGE.sdix_problm_code Z,\n";
                strSQLstring += "sysadm8.ps_isa_req_bi_info Q,\n";
                strSQLstring += "sysadm8.ps_rte_cntl_ruser R,\n";
                strSQLstring += "SYSADM8.PS_DEPT_TBL T,\n";
                strSQLstring += "SYSADM8.ps_PTSF_URLDEFN_VW U\n";

                strSQLstring += "WHERE A.BUSINESS_UNIT = B.BUSINESS_UNIT\n";
                strSQLstring += "AND A.PO_ID = B.PO_ID\n";
                strSQLstring += "AND A.LINE_NBR = B.LINE_NBR\n";
                strSQLstring += "AND A.SCHED_NBR = B.SCHED_NBR\n";
                strSQLstring += "AND B.PO_ID = D.PO_ID\n";
                strSQLstring += "AND B.BUSINESS_UNIT = D.BUSINESS_UNIT\n";
                strSQLstring += "AND B.LINE_NBR = D.LINE_NBR\n";
                strSQLstring += "AND D.PO_ID = S.PO_ID\n";
                strSQLstring += "AND D.BUSINESS_UNIT = S.BUSINESS_UNIT\n";
                strSQLstring += "AND D.LINE_NBR = S.LINE_NBR\n";
                strSQLstring += "AND S.BUSINESS_UNIT = E.BUSINESS_UNIT\n";
                strSQLstring += "AND S.PO_ID = E.PO_ID\n";
                strSQLstring += "AND E.VENDOR_ID = V.VENDOR_ID\n";
                strSQLstring += "AND V.SETID = 'MAIN1'\n";
                strSQLstring += "AND(B.ISA_PROBLEM_CODE IN('CP', 'PP', 'VI', 'SI')\n";
                strSQLstring += "OR B.ISA_PROBLEM_CODE IN('DL', 'RP', 'RT', 'SP', 'WS', 'RC', 'CR'))\n";
                strSQLstring += "AND B.DTTM_STAMP = (SELECT MAX(C.DTTM_STAMP)\n";
                strSQLstring += "FROM sysadm8.PS_ISA_XPD_COMMENT C\n";
                strSQLstring += "WHERE C.BUSINESS_UNIT = A.BUSINESS_UNIT\n";
                strSQLstring += "AND C.PO_ID = A.PO_ID\n";
                strSQLstring += "AND C.LINE_NBR = A.LINE_NBR\n";
                strSQLstring += "AND C.SCHED_NBR = A.SCHED_NBR)\n";
                strSQLstring += "AND b.isa_problem_code = z.isa_problem_code\n";
                strSQLstring += "AND S.BUSINESS_UNIT = Q.BUSINESS_UNIT\n";
                strSQLstring += "AND S.REQ_ID = Q.REQ_ID\n";
                strSQLstring += "AND S.REQ_LINE_NBR = Q.LINE_NBR\n";
                strSQLstring += "and s.deptid = r.rte_cntl_profile(+)\n";
                strSQLstring += "and case \n";
                strSQLstring += "when S.business_unit = 'ISA00' then 'SDI_PROCURE_SUPERVISOR'\n";
                strSQLstring += "when S.business_unit = 'SDM00' then 'SDI_SITE_MANAGER_SDM'\n";
                strSQLstring += "else 'OTHER' end = r.rolename(+)\n";
                strSQLstring += "AND S.DEPTID = T.DEPTID\n";
                strSQLstring += "AND U.URL_ID = 'EMP_SERVLET') "; // AND ROWNUM < 1501";
                // AND ROWNUM < 1501";


                m_oLogger.LogMessage("getExpediterData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getExpediterData", "Query To get the Expediter data: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                m_oLogger.LogMessage("getExpediterData", "Number of rows Selected " + dtResponse.Rows.Count);

                strSQLstring = "SELECT Process_Flag from SDIX_BUYEXPTEMP";
                dtResponse = oleDBExecuteReader(strSQLstring);
                dtResponseRowsCount = dtResponse.Rows.Count;

                m_oLogger.LogMessage("CreateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("CreateTable", "Error trying to create the BuyExp temp data table.", ex);
            }


        }

        public void UpdateTable(Logger m_oLogger)
        {
            try
            {
                strSQLstring = "UPDATE SDIX_BUYEXPTEMP SET Process_Flag = 'X' Where Process_Flag <> 'X' And rownum < " + (oracleSendLimit + 1);
                m_oLogger.LogMessage("UpdateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("UpdateTable", "Query To update the BuyExp temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                m_oLogger.LogMessage("UpdateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("UpdateTable", "Error trying to update the BuyExp temp data table.", ex);
            }

        }




        public DataTable getExpediterData(Logger m_oLogger)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                strSQLstring = "SELECT * FROM SDIX_BUYEXPTEMP\n";
                strSQLstring += "where process_flag <> 'X' and rownum < " + (oracleSendLimit + 1);

                m_oLogger.LogMessage("getExpediterData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getExpediterData", "Query To get the Expediter data: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                m_oLogger.LogMessage("getExpediterData", "Number of rows Selected " + dtResponse.Rows.Count);


            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("getExpediterData", "Error trying to get the Expediter data.", ex);
            }
            return dtResponse;
        }

        public BEData getData(Logger m_oLogger)
        {
            m_oLogger.LogMessage("BatchBuyExp", "Entered BatchBuyExp class");

            
            bed.ACTION_ITEMS.Clear();
            bed.BUSINESS_UNIT .Clear();
            bed.BUYER_ID .Clear();
            bed.BUYER_TEAM .Clear();
            bed.CLIENT .Clear();
            bed.DESCRIPTION .Clear();
            bed.EXPEDITING_COMMENTS .Clear();
            bed.INVENTORY_BUSINESS_UNIT .Clear();
            bed.ITEM .Clear();
            bed.LAST_COMMENT_DATE .Clear();
            bed.LAST_OPERATOR.Clear();
            bed.LINE_NUMBER.Clear();
            bed.PO_DATE .Clear();
            bed.PO_ID .Clear();
            bed.PS_URL .Clear();
            bed.PRIORITY_FLAG .Clear();
            bed.PROBLEM_CODE .Clear();
            bed.SITE_NAME .Clear();
            bed.STATUS_AGE .Clear();
            bed.VENDOR_ID .Clear();
            bed.VENDOR_NAME .Clear();

            //STEP #3 - QUERY TABLE AND POST NEW DATA 
            m_oLogger.LogMessage("BuyExpReload", "Query table started");
            //MatchExcepReloadDAL objGetMatchExcepReloadDAL = new MatchExcepReloadDAL();
            //dtResponse = objGetMatchExcepReloadDAL.getMatchExcepData(m_oLogger);
            dtResponse = getExpediterData (m_oLogger);
            dtResponseRowsCount = dtResponse.Rows.Count;

            if (dtResponseRowsCount < oracleSendLimit)
            {
                gotAllData = "Y";
            }

            if (dtResponseRowsCount == 0)
            {
                m_oLogger.LogMessage("BuyExpReload", "Query returned no records.");
                return null; 
            }
            else
                m_oLogger.LogMessage("BuyExpReload", "POST BuyExpReload data started.");
            for (int i = 0; i < dtResponseRowsCount; i++)
            {
                DataRow rowInit;
                rowInit = dtResponse.Rows[i];

                try
                {
                    bed.ACTION_ITEMS.Add( rowInit["ACTION_ITEMS"].ToString());
                    bed.BUSINESS_UNIT.Add(rowInit["BUSINESS_UNIT"].ToString());
                    bed.BUYER_ID.Add( rowInit["BUYER_ID"].ToString());
                    bed.BUYER_TEAM.Add(rowInit["BUYER_TEAM"].ToString());
                    bed.CLIENT.Add(rowInit["CLIENT"].ToString());
                    bed.DESCRIPTION.Add(rowInit["DESCRIPTION"].ToString());
                    bed.EXPEDITING_COMMENTS.Add(rowInit["EXPEDITING_COMMENTS"].ToString());
                    bed.INVENTORY_BUSINESS_UNIT.Add(rowInit["BUSINESS_UNIT_IN"].ToString());
                    bed.ITEM.Add(rowInit["ITEM"].ToString());

                    string LAST_COMMENT_DATEtest = rowInit["LAST_COMMENT_DATE"].ToString();
                    dateparse = DateTime.Parse(LAST_COMMENT_DATEtest);
                    bed.LAST_COMMENT_DATE.Add(dateparse);

                    bed.LAST_OPERATOR.Add(rowInit["LAST_OPERATOR"].ToString());
                    bed.LINE_NUMBER.Add(rowInit["LINE_NBR"].ToString());

                    string PO_DATEtest = rowInit["PO_DATE"].ToString();
                    dateparse = DateTime.Parse(PO_DATEtest);
                    bed.PO_DATE.Add(dateparse);

                    bed.PO_ID.Add(rowInit["PO_ID"].ToString());
                    bed.PS_URL.Add(rowInit["PS_URL"].ToString());                                        //?????
                    bed.PRIORITY_FLAG.Add(rowInit["PRIORITY_FLAG"].ToString());  
                    bed.PROBLEM_CODE.Add(rowInit["PROBLEM_CODE"].ToString());
                    bed.SITE_NAME.Add(" ");                                     //?????

                    bed.STATUS_AGE.Add(Convert.ToInt32(rowInit["STATUS_AGE"]));
                    bed.VENDOR_ID.Add(rowInit["VENDOR_ID"].ToString());
                    bed.VENDOR_NAME.Add(rowInit["VENDOR"].ToString());

                }
                catch (Exception ex)
                {
                    m_oLogger.LogMessage("ExpediterReload", "Error trying to parse data at line " + i.ToString(), ex);

                }

            }

            m_oLogger.LogMessage("ExpediterReload", "Query table and parse successful.");
            return bed;

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


    }
}
