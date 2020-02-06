using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace PODispatchReload1
{
    public class PODData
    {
        public List<string> ACTION_ITEM = new List<string>();
        public List<string> CLIENT = new List<string>();
        public List<string> VENDOR_NAME = new List<string>();
        public List<DateTime> PO_DATE = new List<DateTime>();
        public List<string> PO_ID = new List<string>();
        public List<string> LINE_NUMBER = new List<string>();
        public List<string> ITEM_ID = new List<string>();
        public List<string> INITIAL_DISP_METHOD = new List<string>();
        public List<string> INITIAL_USER = new List<string>();
        public List<DateTime> INITIAL_DIS_DTTM = new List<DateTime>();
        public List<string> BUYER_ID = new List<string>();
        public List<string> VENDOR_ID = new List<string>();
        public List<string> VENDOR_EMAIL = new List<string>();
        public List<string> VENDOR_DEFAULT = new List<string>();
        public List<string> PROBLEM_CODE = new List<string>();
        public List<string> COMMENTS = new List<string>();
        public List<string> USER_ID = new List<string>();
        public List<string> REQ_DISP_OVERRIDE = new List<string>();
        public List<string> PRIORITY_FLAG = new List<string>();
        public List<string> INVENTORY_BUSINESS_UNIT = new List<string>();
        public List<string> HDR_COMMENTS = new List<string>();
        public List<string> COMMENT_TYPE = new List<string>();
        public List<string> SITE_NAME = new List<string>();
        public List<string> PS_URL = new List<string>();
        public List<string> BUYER_TEAM = new List<string>();
    }

    public class PODispatchReloadDAL
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

        //List<string> ACTION_ITEM = new List<string>();
        //List<string> CLIENT = new List<string>();
        //List<string> VENDOR_NAME = new List<string>();
        //List<DateTime > PO_DATE = new List<DateTime >();
        //List<string> PO_ID = new List<string>();
        //List<string> LINE_NUMBER = new List<string>();
        //List<string> ITEM_ID = new List<string>();
        //List<string> INITIAL_DISP_METHOD = new List<string>();
        //List<string> INITIAL_USER = new List<string>();
        //List<DateTime> INITIAL_DIS_DTTM = new List<DateTime>();
        //List<string> BUYER_ID = new List<string>();
        //List<string> VENDOR_ID = new List<string>();
        //List<string> VENDOR_EMAIL = new List<string>();
        //List<string> VENDOR_DEFAULT = new List<string>();
        //List<string> PROBLEM_CODE = new List<string>();
        //List<string> COMMENTS = new List<string>();
        //List<string> USER_ID = new List<string>();
        //List<string> REQ_DISP_OVERRIDE = new List<string>();
        //List<string> PRIORITY_FLAG = new List<string>();
        //List<string> INVENTORY_BUSINESS_UNIT = new List<string>();
        //List<string> HDR_COMMENTS = new List<string>();
        //List<string> COMMENT_TYPE = new List<string>();
        //List<string> SITE_NAME = new List<string>();
        //List<string> PS_URL = new List<string>();
        //List<string> BUYER_TEAM = new List<string>();

        // InitializeLogger start here
        //Logger m_oLogger;
        //string sLogPath = Environment.CurrentDirectory;

        DataTable dtResponse = new DataTable();

        public void CreateTable(Logger m_oLogger)
        {
            try
            {
                //check if table already exists
                //strSQLstring = "select table_name from user_tables where table_name='SDIX_PODISPATCHTEMP'";
                //dtResponse = oleDBExecuteReader(strSQLstring);
                //if (dtResponse.Rows.Count > 0)
                //{
                    //if it does, drop the table
                    strSQLstring = "TRUNCATE TABLE SDIX_PODISPATCHTEMP";
                    dtResponse = oleDBExecuteReader(strSQLstring);
                //}

                strSQLstring = "INSERT INTO SDIX_PODISPATCHTEMP\n";
                strSQLstring += "(SELECT ' ' as ACTION_ITEM,\n";
                strSQLstring += "DECODE( T.ACCOUNTING_OWNER, ' ' , 'Inactive Site',  T.ACCOUNTING_OWNER) as CLIENT, \n";
                strSQLstring += "T.descr as SITE_NAME,\n";
                strSQLstring += "H.NAME1 as VENDOR_NAME, \n";
                strSQLstring += "B.PO_DT as PO_DATE, \n";
                strSQLstring += "B.PO_ID as PO_ID, \n";
                strSQLstring += "C.LINE_NBR as LINE_NUMBER, \n";
                strSQLstring += "C.INV_ITEM_ID as ITEM_ID, \n";
                strSQLstring += "A.DISP_METHOD as INITIAL_DISP_METHOD,\n";
                strSQLstring += "A.OPRID as INITIAL_USER, \n";
                strSQLstring += "A.DATETIME_DISP as INITIAL_DISPATCH_DTTM, \n";
                strSQLstring += "NVL(B.BUYER_ID, ' ') as BUYER_ID, \n";
                strSQLstring += "B.VENDOR_ID as VENDOR_ID,\n";
                strSQLstring += "I.EMAILID as VENDOR_EMAIL, \n";
                strSQLstring += "J.DISP_METHOD as VENDOR_DEFAULT, \n";
                strSQLstring += "NVL(F.ISA_PROBLEM_CODE, ' ') as PROBLEM_CODE, \n";
                strSQLstring += "NVL(F.NOTES_1000, ' ') as COMMENTS, \n";
                strSQLstring += "NVL(F.OPRID, ' ') as USER_ID, \n";
                strSQLstring += "L.DISP_METHOD as REQ_DISP_OVERRIDE, \n";
                strSQLstring += "L.ISA_PRIORITY_FlAG as PRIORITY_FLAG,\n";
                strSQLstring += "NVL(SUBSTR(D.BUSINESS_UNIT_IN,2,4), ' ') as INVENTORY_BUSINESS_UNIT,\n";
                strSQLstring += "NVL(O.COMMENTS_2000, ' ') as HDR_COMMENTS, \n";
                strSQLstring += "' 'as REQ_SHIP_METHOD,\n";
                strSQLstring += "B.hold_status as PO_HOLD_FLAG,\n";
                strSQLstring += "NVL(P.BUYER_MANAGER, ' ') as BUYER_TEAM,\n";
                strSQLstring += "U.url || '/EMPLOYEE/ERP/c/MANAGE_PURCHASE_ORDERS.PURCHASE_ORDER.GBL?Page=PO_LINE&Action=U&BUSINESS_UNIT=' || a.BUSINESS_UNIT || '&PO_ID=' || a.po_ID || '&TargetFrameName=None' as PS_URL,\n";
                strSQLstring += "NVL(O20X.XLATLONGNAME, ' ') as COMMENT_TYPE,\n";
                strSQLstring += "' ' as PROCESS_FLAG\n";

                strSQLstring += "FROM sysadm8.PS_PO_DISPATCHED A, \n";
                strSQLstring += "((sysadm8.PS_PO_HDR B \n";
                strSQLstring += "          LEFT OUTER JOIN  sysadm8.PS_PO_COMMENTS_FS O \n";
                strSQLstring += "          ON  B.BUSINESS_UNIT = O.BUSINESS_UNIT \n";
                strSQLstring += "          AND B.PO_ID = O.PO_ID \n";
                strSQLstring += "          AND O.COMMENT_TYPE = 'HDR' ) \n";
                strSQLstring += "   LEFT OUTER JOIN  SYSADM8.PS_PO_MANAGER_TBL P\n";
                strSQLstring += "          ON  P.BUYER_ID = B.BUYER_ID)\n";
                strSQLstring += "    LEFT OUTER JOIN sysadm8.PSXLATITEM O20X \n";
                strSQLstring += "          ON O20X.FIELDNAME='COMMENT_TYPE' \n";
                strSQLstring += "          AND O20X.FIELDVALUE= O.COMMENT_TYPE \n";
                strSQLstring += "          AND O20X.EFF_STATUS = 'A', \n";
                strSQLstring += "sysadm8.PS_PO_LINE C, \n";
                strSQLstring += "sysadm8.PS_PO_LINE_DISTRIB D, \n";
                strSQLstring += "(sysadm8.PS_ISA_EXPED_XREF E \n";
                strSQLstring += "LEFT OUTER JOIN  sysadm8.PS_ISA_XPD_COMMENT F \n";
                strSQLstring += "          ON  E.BUSINESS_UNIT = F.BUSINESS_UNIT \n";
                strSQLstring += "          AND E.PO_ID = F.PO_ID \n";
                strSQLstring += "          AND E.LINE_NBR = F.LINE_NBR \n";
                strSQLstring += "          AND E.SCHED_NBR = F.SCHED_NBR ), \n";
                strSQLstring += "sysadm8.PS_VENDOR H, \n";
                strSQLstring += "sysadm8.PS_VENDOR_ADDR I,\n";
                strSQLstring += "sysadm8.PS_VENDOR_LOC J, \n";
                strSQLstring += "sysadm8.PS_ISA_REQ_BI_INFO L,\n";
                strSQLstring += "sysadm8.ps_dept_tbl T, \n";
                strSQLstring += "SYSADM8.ps_PTSF_URLDEFN_VW U\n";

                strSQLstring += "WHERE (O20X.EFFDT = (SELECT MAX(EFFDT) \n";
                strSQLstring += "FROM sysadm8.PSXLATITEM TB WHERE TB.FIELDNAME=O20X.FIELDNAME \n";
                strSQLstring += "AND TB.FIELDVALUE=O20X.FIELDVALUE AND  TB.EFF_STATUS = 'A' \n";
                strSQLstring += "AND TB.EFFDT <= TO_DATE(TO_CHAR(SYSDATE,'YYYY-MM-DD'),'YYYY-MM-DD')) OR O20X.EFFDT IS NULL) \n";
                strSQLstring += "    AND A.BUSINESS_UNIT IN ('ISA00','SDM00','CST00')\n";
                strSQLstring += "     AND A.DATETIME_DISP = (SELECT MIN( M.DATETIME_DISP)\n";
                strSQLstring += "              FROM sysadm8.PS_PO_DISPATCHED M\n";
                strSQLstring += "              WHERE M.BUSINESS_UNIT = A.BUSINESS_UNIT\n";
                strSQLstring += "                 AND M.PO_ID = A.PO_ID)\n";
                strSQLstring += "     AND A.BUSINESS_UNIT = B.BUSINESS_UNIT\n";
                strSQLstring += "     AND A.PO_ID = B.PO_ID\n";
                strSQLstring += "     AND B.BUSINESS_UNIT = C.BUSINESS_UNIT\n";
                strSQLstring += "     AND B.PO_ID = C.PO_ID\n";
                strSQLstring += "     AND C.BUSINESS_UNIT = D.BUSINESS_UNIT\n";
                strSQLstring += "     AND C.PO_ID = D.PO_ID\n";
                strSQLstring += "     AND C.LINE_NBR = D.LINE_NBR\n";
                strSQLstring += "     AND D.BUSINESS_UNIT = E.BUSINESS_UNIT\n";
                strSQLstring += "     AND D.PO_ID = E.PO_ID\n";
                strSQLstring += "     AND D.LINE_NBR = E.LINE_NBR\n";
                strSQLstring += "     AND D.SCHED_NBR = E.SCHED_NBR\n";
                strSQLstring += "     AND ( F.DTTM_STAMP = (SELECT MAX( G.DTTM_STAMP)\n";
                strSQLstring += "            FROM sysadm8.PS_ISA_XPD_COMMENT G\n";
                strSQLstring += "            WHERE G.BUSINESS_UNIT = F.BUSINESS_UNIT\n";
                strSQLstring += "               AND G.PO_ID = F.PO_ID\n";
                strSQLstring += "               AND G.LINE_NBR = F.LINE_NBR\n";
                strSQLstring += "               AND G.SCHED_NBR = F.SCHED_NBR)\n";
                strSQLstring += "               OR F.PO_ID IS NULL)\n";
                strSQLstring += "     AND J.VENDOR_ID = B.VENDOR_ID\n";
                strSQLstring += "     AND B.VENDOR_SETID = H.SETID\n";
                strSQLstring += "     AND B.VENDOR_ID = H.VENDOR_ID\n";
                strSQLstring += "     AND I.VENDOR_ID = B.VENDOR_ID\n";
                strSQLstring += "     AND I.ADDRESS_SEQ_NUM = B.ADDRESS_SEQ_NUM\n";
                strSQLstring += "     AND I.EFFDT =\n";
                strSQLstring += "        (SELECT MAX(I_ED.EFFDT) FROM sysadm8.PS_VENDOR_ADDR I_ED\n";
                strSQLstring += "        WHERE I.SETID = I_ED.SETID\n";
                strSQLstring += "          AND I.VENDOR_ID = I_ED.VENDOR_ID\n";
                strSQLstring += "          AND I.ADDRESS_SEQ_NUM = I_ED.ADDRESS_SEQ_NUM\n";
                strSQLstring += "          AND I_ED.EFFDT <= SYSDATE)\n";
                strSQLstring += "     AND J.VNDR_LOC = B.VNDR_LOC\n";
                strSQLstring += "     AND J.EFFDT =\n";
                strSQLstring += "        (SELECT MAX(J_ED.EFFDT) FROM sysadm8.PS_VENDOR_LOC J_ED\n";
                strSQLstring += "        WHERE J.SETID = J_ED.SETID\n";
                strSQLstring += "          AND J.VENDOR_ID = J_ED.VENDOR_ID\n";
                strSQLstring += "          AND J.VNDR_LOC = J_ED.VNDR_LOC\n";
                strSQLstring += "          AND J_ED.EFFDT <= SYSDATE)\n";
                strSQLstring += "     AND NOT EXISTS (SELECT 'X'\n";
                strSQLstring += "              FROM sysadm8.PS_PO_DISPATCHED K\n";
                strSQLstring += "              WHERE K.DISP_METHOD IN ('EDX','EDI','FAX','EML','XML')\n";
                strSQLstring += "                 AND K.BUSINESS_UNIT = A.BUSINESS_UNIT\n";
                strSQLstring += "                 AND K.PO_ID = A.PO_ID)\n";
                strSQLstring += "     AND D.BUSINESS_UNIT = L.BUSINESS_UNIT\n";
                strSQLstring += "     AND D.REQ_ID = L.REQ_ID\n";
                strSQLstring += "     AND D.REQ_LINE_NBR = L.LINE_NBR\n";
                strSQLstring += "     AND ( F.ISA_PROBLEM_CODE <> 'BP'\n";
                strSQLstring += "     OR F.ISA_PROBLEM_CODE IS NULL)\n";
                strSQLstring += "     and d.deptid = t.deptid\n";
                strSQLstring += "     AND U.url_id = 'EMP_SERVLET')";

                m_oLogger.LogMessage("CreateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("CreateTable", "Query To create the PODispatch temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                strSQLstring = "SELECT Process_Flag from SDIX_PODISPATCHTEMP";
                dtResponse = oleDBExecuteReader(strSQLstring);
                dtResponseRowsCount = dtResponse.Rows.Count;

                m_oLogger.LogMessage("CreateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("CreateTable", "Error trying to create the PODispatch temp data table.", ex);
            }

        }

        public void UpdateTable(Logger m_oLogger)
        {
            try
            {
                strSQLstring = "UPDATE SDIX_PODISPATCHTEMP SET Process_Flag = 'X' Where Process_Flag <> 'X' And rownum < " + (oracleSendLimit + 1);
                m_oLogger.LogMessage("UpdateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("UpdateTable", "Query To update the PODispatch temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                m_oLogger.LogMessage("UpdateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("UpdateTable", "Error trying to update the PODispatch temp data table.", ex);
            }

        }



        public DataTable getPODispatchData(Logger m_oLogger)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                strSQLstring = "SELECT * FROM SDIX_PODISPATCHTEMP\n";
                strSQLstring += "where process_flag <> 'X' and rownum < " + (oracleSendLimit + 1);

                m_oLogger.LogMessage("getPODispatchData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getPODispatchData", "Query To get the PODispatch data: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                m_oLogger.LogMessage("getPODispatchData", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("getMatchExcepData", "Error trying to get the PODispatch data.", ex);
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
            m_oLogger.LogMessage("BatchPODispatch", "Entered BatchPODispatch class");

            pod.ACTION_ITEM.Clear ();
            pod.CLIENT.Clear();
            pod.VENDOR_NAME  .Clear();
            pod.PO_DATE .Clear();
            pod.PO_ID .Clear();
            pod.LINE_NUMBER.Clear();
            pod.ITEM_ID .Clear();
            pod.INITIAL_DISP_METHOD. Clear();
            pod.INITIAL_USER.Clear();
            pod.INITIAL_DIS_DTTM .Clear();
            pod.BUYER_ID. Clear();
            pod.VENDOR_ID .Clear();
            pod.VENDOR_EMAIL .Clear();
            pod.VENDOR_DEFAULT .Clear();
            pod.PROBLEM_CODE .Clear();
            pod.COMMENTS .Clear();
            pod.USER_ID .Clear();
            pod.REQ_DISP_OVERRIDE .Clear();
            pod.PRIORITY_FLAG .Clear();
            pod.INVENTORY_BUSINESS_UNIT .Clear();
            pod.HDR_COMMENTS .Clear();
            pod.COMMENT_TYPE .Clear();
            pod.SITE_NAME .Clear();
            pod.PS_URL .Clear();
            pod.BUYER_TEAM.Clear();

            //STEP #3 - QUERY TABLE AND POST NEW DATA 
            m_oLogger.LogMessage("PODispatchReload", "Query table started");
            //MatchExcepReloadDAL objGetMatchExcepReloadDAL = new MatchExcepReloadDAL();
            //dtResponse = objGetMatchExcepReloadDAL.getMatchExcepData(m_oLogger);
            dtResponse = getPODispatchData (m_oLogger);
            dtResponseRowsCount = dtResponse.Rows.Count;

            if (dtResponseRowsCount < oracleSendLimit)
            {
                gotAllData = "Y";
            }

            if (dtResponseRowsCount == 0)
            {
                m_oLogger.LogMessage("PODispatchReload", "Query returned no records.");
                return null; ;
            }
            else
                m_oLogger.LogMessage("PODispatchReload", "POST PODispatchReload data started.");
            for (int i = 0; i < dtResponseRowsCount; i++)
            {
                DataRow rowInit;
                rowInit = dtResponse.Rows[i];

                try
                {
                    pod.ACTION_ITEM .Add(rowInit["ACTION_ITEM"].ToString());
                    pod.CLIENT .Add(rowInit["CLIENT"].ToString());
                    pod.VENDOR_NAME .Add(rowInit["VENDOR_ID"].ToString());
                    pod.PO_DATE .Add(Convert.ToDateTime (rowInit["PO_DATE"].ToString()));
                    pod.PO_ID .Add(rowInit["PO_ID"].ToString());
                    pod.LINE_NUMBER .Add(rowInit["LINE_NUMBER"].ToString());
                    pod.ITEM_ID .Add(rowInit["ITEM_ID"].ToString());
                    pod.INITIAL_DISP_METHOD.Add(rowInit["INITIAL_DISP_METHOD"].ToString());
                    pod.INITIAL_USER.Add (rowInit["INITIAL_USER"].ToString());
                    pod.INITIAL_DIS_DTTM .Add(Convert.ToDateTime (rowInit["INITIAL_DISPATCH_DTTM"]));
                    pod.BUYER_ID.Add(rowInit["BUYER_ID"].ToString ());
                    pod.VENDOR_ID .Add(rowInit["VENDOR_ID"].ToString());
                    pod.VENDOR_EMAIL .Add(rowInit["VENDOR_EMAIL"].ToString ());
                    pod.VENDOR_DEFAULT.Add(rowInit["VENDOR_DEFAULT"].ToString());
                    pod.PROBLEM_CODE .Add(rowInit["PROBLEM_CODE"].ToString());
                    pod.COMMENTS .Add(rowInit["COMMENTS"].ToString());
                    pod.USER_ID .Add(rowInit["USER_ID"].ToString());
                    pod.REQ_DISP_OVERRIDE .Add(rowInit["REQ_DISP_OVERRIDE"].ToString());
                    pod.PRIORITY_FLAG .Add(rowInit["PRIORITY_FLAG"].ToString());
                    pod.INVENTORY_BUSINESS_UNIT .Add(rowInit["INVENTORY_BUSINESS_UNIT"].ToString());
                    pod.HDR_COMMENTS .Add(rowInit["HDR_COMMENTS"].ToString());
                    pod.COMMENT_TYPE.Add(rowInit ["COMMENT_TYPE"].ToString ()); 
                    pod.SITE_NAME .Add(rowInit["SITE_NAME"].ToString());
                    pod.PS_URL .Add(rowInit["PS_URL"].ToString());
                    pod.BUYER_TEAM .Add(rowInit["BUYER_TEAM"].ToString());

                }
                catch (Exception ex)
                {
                    m_oLogger.LogMessage("PODispatchReload", "Error trying to parse data at line " + i.ToString(), ex);

                }

            }

            m_oLogger.LogMessage("PODispatchReload", "Query table and parse successful.");
            return pod;

        }


    }
}
