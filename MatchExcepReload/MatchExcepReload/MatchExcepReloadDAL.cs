using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using MatchExcepReload1;
using System.Threading.Tasks;

namespace MatchExcepReload
{
    public class MEData
    {
        public List<string> CLIENT = new List<string>();
        public List<string> BUYER_TEAM = new List<string>();
        public List<string> SITE = new List<string>();
        public List<string> PS_URL = new List<string>();
        public List<string> ME_ROLE = new List<string>();
        public List<string> SHIPTO_DESC = new List<string>();
        public List<string> SHIPTO_ID = new List<string>();
        public List<string> ASSIGNED_TO = new List<string>();
        public List<string> TASK_TYPE = new List<string>();
        public List<int> ME_LINES = new List<int>();
        public List<int> DAYS_OVERALL = new List<int>();
        public List<string> OVERALL_AGING = new List<string>();
        public List<DateTime> REPORTING_DATE = new List<DateTime>();
        public List<string> MATCH_RULE = new List<string>();
        public List<string> SUPPLIER_ID = new List<string>();
        public List<string> SUPPLIER_NAME = new List<string>();
        public List<string> BUYER_ID = new List<string>();
        public List<string> PO_BUSINESS_UNIT = new List<string>();
        public List<string> PO_NO = new List<string>();
        public List<string> DISPATCH_METHOD = new List<string>();
        public List<string> INVOICE_ID = new List<string>();
        public List<DateTime> INVOICE_DATE = new List<DateTime>();
        public List<string> TOTAL_INVOICED_AMT = new List<string>();
        public List<DateTime> SCAN_DATE = new List<DateTime>();
        public List<DateTime> TASK_DATE = new List<DateTime>();
        public List<int> TASK_DAYS = new List<int>();
        public List<string> TASK_AGING = new List<string>();
        public List<DateTime> DATE_ASSIGNED = new List<DateTime>();
        public List<int> DAYS_ASSIGNED = new List<int>();
        public List<string> ASSIGNED_AGING = new List<string>();

    }

    public class MatchExcepReloadDAL
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

        MEData med = new MEData();

        //public List<string> CLIENT 
        //{
        //    get { return CLIENT; }
        //    set {CLIENT = value;}
        //}
        List<string> CLIENT = new List<string>();
        List<string> BUYER_TEAM = new List<string>();
        List<string> SITE = new List<string>();
        List<string> PS_URL = new List<string>();
        List<string> ME_ROLE = new List<string>();
        List<string> SHIPTO_DESC = new List<string>();
        List<string> SHIPTO_ID = new List<string>();
        List<string> ASSIGNED_TO = new List<string>();
        List<string> TASK_TYPE = new List<string>();
        List<int> ME_LINES = new List<int>();
        List<int> DAYS_OVERALL = new List<int>();
        List<string> OVERALL_AGING = new List<string>();
        List<DateTime> REPORTING_DATE = new List<DateTime>();
        List<string> MATCH_RULE = new List<string>();
        List<string> SUPPLIER_ID = new List<string>();
        List<string> SUPPLIER_NAME = new List<string>();
        List<string> BUYER_ID = new List<string>();
        List<string> PO_BUSINESS_UNIT = new List<string>();
        List<string> PO_NO = new List<string>();
        List<string> DISPATCH_METHOD = new List<string>();
        List<string> INVOICE_ID = new List<string>();
        List<DateTime> INVOICE_DATE = new List<DateTime>();
        List<string> TOTAL_INVOICED_AMT = new List<string>();
        List<DateTime> SCAN_DATE = new List<DateTime>();
        List<DateTime> TASK_DATE = new List<DateTime>();
        List<int> TASK_DAYS = new List<int>();
        List<string> TASK_AGING = new List<string>();
        List<DateTime> DATE_ASSIGNED = new List<DateTime>();
        List<int> DAYS_ASSIGNED = new List<int>();
        List<string> ASSIGNED_AGING = new List<string>();

        // InitializeLogger start here
        //Logger m_oLogger;
        //string sLogPath = Environment.CurrentDirectory;

        DataTable dtResponse = new DataTable();

        public void CreateTable(Logger m_oLogger)
        {
            try 
            {
                //check if table already exists
                //strSQLstring="select table_name from user_tables where table_name='SDIX_MATCHEXCEPTEST'";
                //strSQLstring = "select count(1) from SDIX_MATCHEXCEPTEST";
                //dtResponse = oleDBExecuteReader(strSQLstring);
                //if (dtResponse.Rows.Count > 0)
                //{
                    //if it does, drop the table
                    strSQLstring = "TRUNCATE TABLE SDIX_MATCHEXCEPTEST";
                    dtResponse = oleDBExecuteReader(strSQLstring);
                //}


                strSQLstring = "INSERT INTO SDIX_MATCHEXCEPTEST \n";
                strSQLstring += "(SELECT DISTINCT\n";
                strSQLstring += "DECODE(T.ACCOUNTING_OWNER, ' ', 'Inactive Site', NVL(T.ACCOUNTING_OWNER, ' ')) as CLIENT,\n";
                strSQLstring += "NVL(T.descr, ' ') as SITE,\n";
                strSQLstring += "CASE WHEN  U.ROLENAME IS NULL THEN 'Unassigned'\n";
                strSQLstring += "WHEN A.ASSIGNED_TO = 'sdiapxd' and A.DWR_MATCH_RULE <> 'Wait for Receipts' then 'Buyer'\n";
                strSQLstring += "else  U.ROLENAME END  as ME_ROLE,\n";
                strSQLstring += "NVL(C.DESCR, ' ') as SHIPTO_DESC,\n";
                strSQLstring += "NVL(B.SHIPTO_ID, ' ') as SHIPTO_ID,\n";
                strSQLstring += "NVL(A.ASSIGNED_TO, ' ') as ASSIGNED_TO,\n";
                strSQLstring += "A.TYPE_DESCR as TASK_TYPE,\n";
                strSQLstring += "A.LINE_COUNT as ME_LINES,\n";
                strSQLstring += "A.DWR_DAYS_OVERALL as DAYS_OVERALL,\n";
                strSQLstring += "case when A.DWR_AGING = '0 to 7 days' then '00 - 07'\n";
                strSQLstring += "when A.DWR_AGING = '8 to 14 days' then '08 - 14'\n";
                strSQLstring += "when A.DWR_AGING = '15 to 21 days' then '15 - 21'\n";
                strSQLstring += "when A.DWR_AGING = '22 to 30 days' then '22 - 30'\n";
                strSQLstring += "when A.DWR_AGING = '31 to 60 days' then '31 - 60'\n";
                strSQLstring += "when A.DWR_AGING = '61 to 90 days' then '61 - 90'\n";
                strSQLstring += "when A.DWR_AGING like '%> 90 days' then '90+'\n";
                strSQLstring += "else  A.DWR_AGING END as OVERALL_AGING,\n";
                strSQLstring += "TO_CHAR(to_date(TO_CHAR(A.DWR_REPORTING_DT, 'YYYY-MM-DD'), 'YYYY-MM-DD') + 1, 'MM-DD-YYYY HH24:MI:SS') as REPORTING_DATE,\n";
                strSQLstring += "A.DWR_MATCH_RULE as MATCH_RULE,\n";
                strSQLstring += "A.VENDOR_ID as SUPPLIER_ID,\n";
                strSQLstring += "V.NAME1 as SUPPLIER_NAME,\n";
                strSQLstring += "NVL(A.BUYER_ID, ' ') as BUYER_ID,\n";
                strSQLstring += "A.BUSINESS_UNIT as PO_BUSINESS_UNIT,\n";
                strSQLstring += "NVL(A.PO_ID, ' ') as PO_NO,\n";
                strSQLstring += "NVL(A.DISP_METHOD, ' ') as DISPATCH_METHOD,\n";
                strSQLstring += "A.INVOICE_ID as INVOICE_ID,\n";
                strSQLstring += "TO_CHAR(A.INVOICE_DT, 'YYYY-MM-DD') as INVOICE_DATE,\n";
                strSQLstring += "A.TOTAL_INVOICED_AMT as TOTAL_INVOICED_AMT,\n";
                strSQLstring += "NVL(TO_CHAR(CAST((A.DWR_SCAN_DATE)AS TIMESTAMP), 'YYYY-MM-DD HH24:MI:SS'), sysdate) as SCAN_DATE,\n";
                strSQLstring += "NVL(TO_CHAR(CAST((A.DWR_TASK_DATE)AS TIMESTAMP), 'YYYY-MM-DD HH24:MI:SS'), sysdate) as TASK_DATE,\n";
                strSQLstring += "NVL(A.DWR_TASK_DAYS, 0) as TASK_DAYS,\n";
                strSQLstring += "A.DWR_AGING2 as TASK_AGING,\n";
                strSQLstring += "TO_CHAR(CAST((A.DWR_ASSIGNED_DT)AS TIMESTAMP), 'YYYY-MM-DD HH24:MI:SS') as DATE_ASSIGNED,\n";
                strSQLstring += "A.DWR_DAYS_ASSIGNED as DAYS_ASSIGNED,\n";
                strSQLstring += "A.DWR_AGING3 as ASSIGNED_AGING,\n";
                strSQLstring += "NVL(P.BUYER_MANAGER, ' ') as BUYER_TEAM,\n";
                strSQLstring += "UU.url || '/EMPLOYEE/ERP/c/MANAGE_PURCHASE_ORDERS.PURCHASE_ORDER.GBL?Page=PO_LINE&Action=U&BUSINESS_UNIT=' || B.BUSINESS_UNIT || '&PO_ID=' || b.po_ID || '&TargetFrameName=None' as PS_URL,\n";
                strSQLstring += "' ' as PROCESS_FLAG\n";

                strSQLstring += "FROM\n";
                strSQLstring += "sysadm8.DW_APXME A,\n";
                strSQLstring += "sysadm8.PS_PO_LINE_SHIP B,\n";
                strSQLstring += "sysadm8.PS_LOCATION_TBL C,\n";
                strSQLstring += "sysadm8.ps_po_line_distrib D,\n";
                strSQLstring += "sysadm8.DW_APX_USERS U,\n";
                strSQLstring += "sysadm8.PS_VENDOR V,\n";
                strSQLstring += "sysadm8.ps_dept_Tbl T,\n";
                strSQLstring += "sysadm8.PS_PO_MANAGER_TBL P,\n";
                strSQLstring += "SYSADM8.ps_PTSF_URLDEFN_VW UU\n";

                strSQLstring += "WHERE\n";
                strSQLstring += "A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)\n";
                strSQLstring += "AND A.PO_ID = B.PO_ID(+)\n";
                strSQLstring += "and B.BUSINESS_UNIT = D.BUSINESS_UNIT(+)\n";
                strSQLstring += "AND B.PO_ID = D.PO_ID(+)\n";
                strSQLstring += "AND B.LINE_NBR = D.LINE_NBR(+)\n";
                strSQLstring += "AND B.SHIPTO_SETID = C.SETID(+)\n";
                strSQLstring += "AND B.SHIPTO_ID = C.LOCATION(+)\n";
                strSQLstring += "AND A.VENDOR_ID = V.VENDOR_ID\n";
                strSQLstring += "AND A.ASSIGNED_TO = U.NAME(+)\n";
                strSQLstring += "AND A.DWR_REPORTING_DT > trunc(sysdate) - 3\n";
                strSQLstring += "AND(C.EFFDT =\n";
                strSQLstring += "        (SELECT MAX(C_ED.EFFDT) FROM sysadm8.PS_LOCATION_TBL C_ED\n";
                strSQLstring += "              WHERE C.SETID = C_ED.SETID\n";
                strSQLstring += "                  AND C.LOCATION = C_ED.LOCATION\n";
                strSQLstring += "                  AND C_ED.EFFDT <= SYSDATE)\n";
                strSQLstring += "         OR C.EFFDT IS NULL)\n";
                strSQLstring += "     and d.deptid = t.deptid(+)\n";
                strSQLstring += "and substr(A.ASSIGNED_TO, instr(A.ASSIGNED_TO, ', ') + 2) || '.' || substr(A.ASSIGNED_TO, 1, instr(A.ASSIGNED_TO, ', ') - 1) = P.BUYER_ID(+)\n";
                strSQLstring += "AND UU.url_id = 'EMP_SERVLET')";
                m_oLogger.LogMessage("CreateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("CreateTable", "Query To create the MatchExcep temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                strSQLstring = "SELECT Process_Flag from SDIX_MATCHEXCEPTEST";
                dtResponse = oleDBExecuteReader(strSQLstring);
                dtResponseRowsCount = dtResponse.Rows.Count;

                 m_oLogger.LogMessage("CreateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("CreateTable", "Error trying to create the MatchExcep temp data table.", ex);
            }

        }

        public void UpdateTable(Logger m_oLogger)
        {
            try
            {
                strSQLstring = "UPDATE SDIX_MATCHEXCEPTEST SET Process_Flag = 'X' Where Process_Flag <> 'X' And rownum < 3001";
                m_oLogger.LogMessage("UpdateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("UpdateTable", "Query To update the MatchExcep temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                m_oLogger.LogMessage("UpdateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("UpdateTable", "Error trying to update the MatchExcep temp data table.", ex);
            }

        }


        
        public DataTable getMatchExcepData(Logger m_oLogger)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                //strSQLstring = "SELECT DISTINCT\n";
                //strSQLstring += "DECODE(T.ACCOUNTING_OWNER, ' ', 'Inactive Site', NVL(T.ACCOUNTING_OWNER, ' ')) as CLIENT,\n";
                //strSQLstring += "NVL(T.descr, ' ') as SITE,\n";
                //strSQLstring += "CASE WHEN  U.ROLENAME IS NULL THEN 'Unassigned'\n";
                //strSQLstring += "WHEN A.ASSIGNED_TO = 'sdiapxd' and A.DWR_MATCH_RULE <> 'Wait for Receipts' then 'Buyer'\n";
                //strSQLstring += "else  U.ROLENAME END  as ME_ROLE,\n";
                //strSQLstring += "NVL(C.DESCR, ' ') as SHIPTO_DESC,\n";
                //strSQLstring += "NVL(B.SHIPTO_ID, ' ') as SHIPTO_ID,\n";
                //strSQLstring += "NVL(A.ASSIGNED_TO, ' ') as ASSIGNED_TO,\n";
                //strSQLstring += "A.TYPE_DESCR as TASK_TYPE,\n";
                //strSQLstring += "A.LINE_COUNT as ME_LINES,\n";
                //strSQLstring += "A.DWR_DAYS_OVERALL as DAYS_OVERALL,\n";
                //strSQLstring += "case when A.DWR_AGING = '0 to 7 days' then '00 - 07'\n";
                //strSQLstring += "when A.DWR_AGING = '8 to 14 days' then '08 - 14'\n";
                //strSQLstring += "when A.DWR_AGING = '15 to 21 days' then '15 - 21'\n";
                //strSQLstring += "when A.DWR_AGING = '22 to 30 days' then '22 - 30'\n";
                //strSQLstring += "when A.DWR_AGING = '31 to 60 days' then '31 - 60'\n";
                //strSQLstring += "when A.DWR_AGING = '61 to 90 days' then '61 - 90'\n";
                //strSQLstring += "when A.DWR_AGING like '%> 90 days' then '90+'\n";
                //strSQLstring += "else  A.DWR_AGING END as OVERALL_AGING,\n";
                ////strSQLstring += "to_date(TO_CHAR(A.DWR_REPORTING_DT, 'YYYY-MM-DD'), 'YYYY-MM-DD') + 1 as REPORTING_DATE,\n";
                //strSQLstring += "TO_CHAR(to_date(TO_CHAR(A.DWR_REPORTING_DT, 'YYYY-MM-DD'), 'YYYY-MM-DD') + 1, 'MM-DD-YYYY HH24:MI:SS') as REPORTING_DATE,\n";
                //strSQLstring += "A.DWR_MATCH_RULE as MATCH_RULE,\n";
                //strSQLstring += "A.VENDOR_ID as SUPPLIER_ID,\n";
                //strSQLstring += "V.NAME1 as SUPPLIER_NAME,\n";
                //strSQLstring += "NVL(A.BUYER_ID, ' ') as BUYER_ID,\n";
                //strSQLstring += "A.BUSINESS_UNIT as PO_BUSINESS_UNIT,\n";
                //strSQLstring += "NVL(A.PO_ID, ' ') as PO_NO,\n";
                //strSQLstring += "NVL(A.DISP_METHOD, ' ') as DISPATCH_METHOD,\n";
                //strSQLstring += "A.INVOICE_ID as INVOICE_ID,\n";
                //strSQLstring += "TO_CHAR(A.INVOICE_DT, 'YYYY-MM-DD') as INVOICE_DATE,\n";
                //strSQLstring += "A.TOTAL_INVOICED_AMT as TOTAL_INVOICED_AMT,\n";
                //strSQLstring += "NVL(TO_CHAR(CAST((A.DWR_SCAN_DATE)AS TIMESTAMP), 'YYYY-MM-DD HH24:MI:SS'), sysdate) as SCAN_DATE,\n";
                //strSQLstring += "NVL(TO_CHAR(CAST((A.DWR_TASK_DATE)AS TIMESTAMP), 'YYYY-MM-DD HH24:MI:SS'), sysdate) as TASK_DATE,\n";
                //strSQLstring += "NVL(A.DWR_TASK_DAYS, 0) as TASK_DAYS,\n";
                //strSQLstring += "A.DWR_AGING2 as TASK_AGING,\n";
                //strSQLstring += "TO_CHAR(CAST((A.DWR_ASSIGNED_DT)AS TIMESTAMP), 'YYYY-MM-DD HH24:MI:SS') as DATE_ASSIGNED,\n";
                //strSQLstring += "A.DWR_DAYS_ASSIGNED as DAYS_ASSIGNED,\n";
                //strSQLstring += "A.DWR_AGING3 as ASSIGNED_AGING,\n";
                //strSQLstring += "NVL(R.ROLEUSER, ' ') as BUYER_TEAM,\n";
                //strSQLstring += "' ' as PS_URL\n";
                //strSQLstring += "FROM\n";
                //strSQLstring += "sysadm8.DW_APXME A,\n";
                //strSQLstring += "sysadm8.PS_PO_LINE_SHIP B,\n";
                //strSQLstring += "sysadm8.PS_LOCATION_TBL C,\n";
                //strSQLstring += "sysadm8.ps_po_line_distrib D,\n";
                //strSQLstring += "sysadm8.DW_APX_USERS U,\n";
                //strSQLstring += "sysadm8.PS_VENDOR V,\n";
                //strSQLstring += "sysadm8.ps_dept_Tbl T,\n";
                //strSQLstring += "sysadm8.ps_rte_cntl_ruser R\n";
                //strSQLstring += "WHERE\n";
                //strSQLstring += "A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)\n";
                //strSQLstring += "AND A.PO_ID = B.PO_ID(+)\n";
                //strSQLstring += "and B.BUSINESS_UNIT = D.BUSINESS_UNIT(+)\n";
                //strSQLstring += "AND B.PO_ID = D.PO_ID(+)\n";
                //strSQLstring += "AND B.LINE_NBR = D.LINE_NBR(+)\n";
                //strSQLstring += "AND B.SHIPTO_SETID = C.SETID(+)\n";
                //strSQLstring += "AND B.SHIPTO_ID = C.LOCATION(+)\n";
                //strSQLstring += "AND A.VENDOR_ID = V.VENDOR_ID\n";
                //strSQLstring += "AND A.ASSIGNED_TO = U.NAME(+)\n";
                //strSQLstring += "AND A.DWR_REPORTING_DT > trunc(sysdate) - 3\n";
                //strSQLstring += "AND(C.EFFDT =\n";
                //strSQLstring += "        (SELECT MAX(C_ED.EFFDT) FROM sysadm8.PS_LOCATION_TBL C_ED\n";
                //strSQLstring += "              WHERE C.SETID = C_ED.SETID\n";
                //strSQLstring += "                  AND C.LOCATION = C_ED.LOCATION\n";
                //strSQLstring += "                  AND C_ED.EFFDT <= SYSDATE)\n";
                //strSQLstring += "         OR C.EFFDT IS NULL)\n";
                //strSQLstring += "     and d.deptid = t.deptid(+)\n";
                //strSQLstring += "and d.deptid = r.rte_cntl_profile(+)\n";
                //strSQLstring += "and case \n";
                //strSQLstring += "when d.business_unit = 'ISA00' then 'SDI_PROCURE_SUPERVISOR'\n";
                //strSQLstring += "when d.business_unit = 'SDM00' then 'SDI_SITE_MANAGER_SDM'\n";
                //strSQLstring += "else 'OTHER' end = r.rolename(+)\n";
                //strSQLstring += "and process_flag <> 'X'\n";
                //strSQLstring += "and rownum < 13351";

                strSQLstring = "SELECT * FROM SDIX_MATCHEXCEPTEST\n";
                strSQLstring += "where process_flag <> 'X' and rownum < " + (oracleSendLimit + 1) ;

                //strSQLstring = "SELECT * FROM SDIX_MATCHEXCEPTEST where buyer_team = 'Jaclyn.Quattrone'";
                //strSQLstring = "SELECT * FROM SDIX_MATCHEXCEPTEST where buyer_team in ('Maria.Richman', 'Jaclyn.Quattrone') ";  //6231 count fail
                //strSQLstring = "SELECT * FROM SDIX_MATCHEXCEPTEST where buyer_team in (' ', 'Jenny.Kelly', 'Jesus.Concha', 'Jose.Carrion', 'Juan.Romero') ";   //1711
                //strSQLstring = "SELECT * FROM SDIX_MATCHEXCEPTEST where buyer_team in ('Stephanie.Zuelsdorf', 'Ulises.Romero', 'Victor.Sanchez') ";   

                m_oLogger.LogMessage("getMatchExcepData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getMatchExcepData", "Query To get the MatchExcep data: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                 m_oLogger.LogMessage("getMatchExcepData", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("getMatchExcepData", "Error trying to get the MatchExcep data.", ex);
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

        public MEData getData(Logger m_oLogger)
        {
            //Logger m_oLogger;
            //string sLogPath = Environment.CurrentDirectory;
            //if (!sLogPath.EndsWith(@"\"))
            //    sLogPath += @"\";
            //sLogPath += "Logs";
            //m_oLogger = new Logger(sLogPath, "MatchExcepReload");
            m_oLogger.LogMessage("BatchMatchExcep", "Entered BatchMatchExcep class");

            med.CLIENT.Clear();
            med.BUYER_TEAM.Clear();
            med.SITE.Clear();
            med.PS_URL.Clear();
            med.ME_ROLE.Clear();
            med.SHIPTO_DESC.Clear();
            med.SHIPTO_ID.Clear();
            med.ASSIGNED_TO.Clear();
            med.TASK_TYPE.Clear();
            med.ME_LINES.Clear();
            med.DAYS_OVERALL.Clear();
            med.OVERALL_AGING.Clear();
            med.REPORTING_DATE.Clear();
            med.MATCH_RULE.Clear();
            med.SUPPLIER_ID.Clear();
            med.SUPPLIER_NAME.Clear();
            med.BUYER_ID.Clear();
            med.PO_BUSINESS_UNIT.Clear();
            med.PO_NO.Clear();
            med.DISPATCH_METHOD.Clear();
            med.INVOICE_ID.Clear();
            med.INVOICE_DATE.Clear();
            med.TOTAL_INVOICED_AMT.Clear();
            med.SCAN_DATE.Clear();
            med.TASK_DATE.Clear();
            med.TASK_DAYS.Clear();
            med.TASK_AGING.Clear();
            med.DATE_ASSIGNED.Clear();
            med.DAYS_ASSIGNED.Clear();
            med.ASSIGNED_AGING.Clear();

            //STEP #3 - QUERY TABLE AND POST NEW DATA 
            m_oLogger.LogMessage("MatchExcepReload", "Query table started");
            //MatchExcepReloadDAL objGetMatchExcepReloadDAL = new MatchExcepReloadDAL();
            //dtResponse = objGetMatchExcepReloadDAL.getMatchExcepData(m_oLogger);
            dtResponse = getMatchExcepData(m_oLogger);
            dtResponseRowsCount = dtResponse.Rows.Count;

            if (dtResponseRowsCount < oracleSendLimit)
            {
                gotAllData = "Y";
            }

            if (dtResponseRowsCount == 0)
            {
                m_oLogger.LogMessage("MatchExcepReload", "Query returned no records.");
                return null; ;
            }
            else
                m_oLogger.LogMessage("MatchExcepReload", "POST MatchExcepReload data started.");
                for (int i = 0; i < dtResponseRowsCount; i++)
                {
                DataRow rowInit;
                rowInit = dtResponse.Rows[i];

                try
                {
                    med.CLIENT.Add(rowInit["CLIENT"].ToString());
                    med.BUYER_TEAM.Add(rowInit["BUYER_TEAM"].ToString());
                    med.SITE.Add(rowInit["SITE"].ToString());
                    med.PS_URL.Add(rowInit["PS_URL"].ToString());
                    med.ME_ROLE.Add(rowInit["ME_ROLE"].ToString());
                    med.SHIPTO_DESC.Add(rowInit["SHIPTO_DESC"].ToString());
                    med.SHIPTO_ID.Add(rowInit["SHIPTO_ID"].ToString());
                    med.ASSIGNED_TO.Add(rowInit["ASSIGNED_TO"].ToString());
                    med.TASK_TYPE.Add(rowInit["TASK_TYPE"].ToString());
                    med.ME_LINES.Add(Convert.ToInt32(rowInit["ME_LINES"]));
                    med.DAYS_OVERALL.Add(Convert.ToInt32(rowInit["DAYS_OVERALL"]));
                    med.OVERALL_AGING.Add(rowInit["OVERALL_AGING"].ToString());

                    med.REPORTING_DATE.Add(Convert.ToDateTime(rowInit["REPORTING_DATE"]));
                    //REPORTING_DATE.Add(rowInit["REPORTING_DATE"].ToString ());
                    //string REPORTING_DATEtest = rowInit["REPORTING_DATE"].ToString();
                    //dateparse = DateTime.Parse(REPORTING_DATEtest);
                    ////LAST_COMMENT_DATE.Add(dateparse.ToString("yyyy-MM-ddTHH:mm:ss.000Z"));
                    //REPORTING_DATE.Add(dateparse);

                    med.MATCH_RULE.Add(rowInit["MATCH_RULE"].ToString());
                    med.SUPPLIER_ID.Add(rowInit["SUPPLIER_ID"].ToString());
                    med.SUPPLIER_NAME.Add(rowInit["SUPPLIER_NAME"].ToString());
                    med.BUYER_ID.Add(rowInit["BUYER_ID"].ToString());
                    med.PO_BUSINESS_UNIT.Add(rowInit["PO_BUSINESS_UNIT"].ToString());
                    med.PO_NO.Add(rowInit["PO_NO"].ToString());
                    med.DISPATCH_METHOD.Add(rowInit["DISPATCH_METHOD"].ToString());

                    med.INVOICE_ID.Add(rowInit["INVOICE_ID"].ToString().Replace("\"", ""));
                    med.INVOICE_DATE.Add(Convert.ToDateTime(rowInit["INVOICE_DATE"]));

                    med.TOTAL_INVOICED_AMT.Add(rowInit["TOTAL_INVOICED_AMT"].ToString());
                    med.SCAN_DATE.Add(Convert.ToDateTime(rowInit["SCAN_DATE"]));
                    med.TASK_DATE.Add(Convert.ToDateTime(rowInit["TASK_DATE"]));
                    med.TASK_DAYS.Add(Convert.ToInt32(rowInit["TASK_DAYS"]));
                    med.TASK_AGING.Add(rowInit["TASK_AGING"].ToString());
                    med.DATE_ASSIGNED.Add(Convert.ToDateTime(rowInit["DATE_ASSIGNED"]));
                    med.DAYS_ASSIGNED.Add(Convert.ToInt32(rowInit["DAYS_ASSIGNED"]));
                    med.ASSIGNED_AGING.Add(rowInit["ASSIGNED_AGING"].ToString());

                }
                catch (Exception ex)
                {
                    m_oLogger.LogMessage("MatchExcepReload", "Error trying to parse data at line " + i.ToString(), ex);

                }

            }

            m_oLogger.LogMessage("MatchExcepReload", "Query table and parse successful.");
            return med;

        }


    }
}
