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
    class MatchExcepReloadDAL
    {
        System.Data.OleDb.OleDbConnection MyConnection;
        System.Data.DataSet DtSet;
        DataTable dtTransaction;
        System.Data.OleDb.OleDbDataAdapter MyCommand;
        string strSQLstring = "";
        System.Data.OleDb.OleDbConnection MyOracleConn;
        string OracleConString = ConfigurationManager.AppSettings["OLEDBconString"];

        public DataTable getMatchExcepData(Logger m_oLogger)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                strSQLstring = "SELECT DISTINCT\n";
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
                //strSQLstring += "to_date(TO_CHAR(A.DWR_REPORTING_DT, 'YYYY-MM-DD'), 'YYYY-MM-DD') + 1 as REPORTING_DATE,\n";
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
                strSQLstring += "NVL(R.ROLEUSER, ' ') as BUYER_TEAM,\n";
                strSQLstring += "' ' as PS_URL\n";

                strSQLstring += "FROM\n";
                strSQLstring += "sysadm8.DW_APXME A,\n";
                strSQLstring += "sysadm8.PS_PO_LINE_SHIP B,\n";
                strSQLstring += "sysadm8.PS_LOCATION_TBL C,\n";
                strSQLstring += "sysadm8.ps_po_line_distrib D,\n";
                strSQLstring += "sysadm8.DW_APX_USERS U,\n";
                strSQLstring += "sysadm8.PS_VENDOR V,\n";
                strSQLstring += "sysadm8.ps_dept_Tbl T,\n";
                strSQLstring += "sysadm8.ps_rte_cntl_ruser R\n";

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
                strSQLstring += "and d.deptid = r.rte_cntl_profile(+)\n";
                strSQLstring += "and case \n";
                strSQLstring += "when d.business_unit = 'ISA00' then 'SDI_PROCURE_SUPERVISOR'\n";
                strSQLstring += "when d.business_unit = 'SDM00' then 'SDI_SITE_MANAGER_SDM'\n";
                strSQLstring += "else 'OTHER' end = r.rolename(+)\n";
                strSQLstring += "and rownum < 15001";
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


    }
}
