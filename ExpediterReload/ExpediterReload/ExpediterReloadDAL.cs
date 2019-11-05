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

    class ExpediterReloadDAL
    {
        System.Data.OleDb.OleDbConnection MyConnection;
        System.Data.DataSet DtSet;
        DataTable dtTransaction;
        System.Data.OleDb.OleDbDataAdapter MyCommand;
        string strSQLstring = "";
        System.Data.OleDb.OleDbConnection MyOracleConn;
        string OracleConString = ConfigurationManager.AppSettings["OLEDBconString"];

        public DataTable getExpediterData(Logger m_oLogger)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                strSQLstring = "SELECT ' ' as Action_Items,' ' As Client,E.PO_DT AS PO_DATE, A.BUSINESS_UNIT, A.PO_ID, A.LINE_NBR, D.INV_ITEM_ID as ITEM, D.DESCR254_MIXED as DESCRIPTION, Z.descr  as PROBLEM_CODE  , B.NOTES_1000 as EXPEDITING_COMMENTS, E.buyer_id, E.VENDOR_ID, V.NAME1 as Vendor,B.OPRID as Last_Operator, B.DTTM_STAMP as Last_Comment_Date,' ' as Exception_Date, ' ' as Exception_Num_Days,S.BUSINESS_UNIT_IN,s.deptid" +
                       " FROM sysadm8.PS_ISA_EXPED_XREF A, sysadm8.PS_ISA_XPD_COMMENT B, sysadm8.PS_PO_LINE D, sysadm8.ps_PO_line_distrib S, sysadm8.PS_PO_HDR E, sysadm8.PS_VENDOR V,SDIEXCHANGE.sdix_problm_code Z" +
                       " WHERE A.BUSINESS_UNIT = B.BUSINESS_UNIT" +
                       " AND A.PO_ID = B.PO_ID" +
                       " AND A.LINE_NBR = B.LINE_NBR" +
                       " AND A.SCHED_NBR = B.SCHED_NBR" +
                       " AND B.PO_ID = D.PO_ID " +
                       " AND B.BUSINESS_UNIT = D.BUSINESS_UNIT" +
                       " AND B.LINE_NBR = D.LINE_NBR" +
                       " AND D.PO_ID = S.PO_ID " +
                       " AND D.BUSINESS_UNIT = S.BUSINESS_UNIT" +
                       " AND D.LINE_NBR = S.LINE_NBR" +
                       " AND S.BUSINESS_UNIT = E.BUSINESS_UNIT" +
                       " AND S.PO_ID = E.PO_ID " +
                       " AND E.VENDOR_ID = V.VENDOR_ID" +
                       " AND V.SETID = 'MAIN1' " +
                       " AND (B.ISA_PROBLEM_CODE IN ('CP', 'PP','VI', 'SI') " +
                       " OR B.ISA_PROBLEM_CODE IN ('DL','RP', 'RT','SP', 'WS', 'RC', 'CR'))" +
                       " AND B.DTTM_STAMP = (SELECT MAX( C.DTTM_STAMP)" +
                       " FROM sysadm8.PS_ISA_XPD_COMMENT C" +
                       " WHERE C.BUSINESS_UNIT = A.BUSINESS_UNIT" +
                       " AND C.PO_ID = A.PO_ID" +
                       " AND C.LINE_NBR = A.LINE_NBR" +
                       " AND C.SCHED_NBR = A.SCHED_NBR)" +
                       " AND b.isa_problem_code = z.isa_problem_code AND ROWNUM < 8";
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
