using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using UoCMapping;
using System.Threading.Tasks;

namespace UoCPOChgMapping
{
    class UoCPOChgMappingDAL
    {

        System.Data.OleDb.OleDbConnection MyConnection;
        System.Data.DataSet DtSet;
        DataTable dtTransaction;
        System.Data.OleDb.OleDbDataAdapter MyCommand;
        string strSQLstring = "";
        System.Data.OleDb.OleDbConnection MyOracleConn;
        string OracleConString = ConfigurationManager.AppSettings["OLEDBconString"];

        /// <summary>
        /// Get the purchase order data whose process flag is 'N' and cust id is 'UCHICAGO'
        /// </summary>
        /// <returns></returns>
        public DataTable getUoCPOChgMappingData(Logger m_oLogger)
        {

            DataTable dtResponse = new DataTable();
            try
            {

                //strSQLstring = "SELECT * FROM sysadm8.PS_ISA_O_MATR_MOVE WHERE PROCESS_FLAG = 'N' AND CUST_ID = 'PMC' AND TRANS_TYPE = 'REC'";
                strSQLstring = "SELECT * FROM SYSADM8.PS_ISA_MXM_POCHG WHERE PROCESS_FLAG = 'N' AND CUST_ID = 'UCHICAGO'"; //AND ROWNUM < 2";
                //strSQLstring = "SELECT * FROM SYSADM8.PS_ISA_MXM_POCHG WHERE CUST_ID = 'UCHICAGO' AND ISA_IDENTIFIER IN (876)"; //AND ROWNUM < 2"; //test
                m_oLogger.LogMessage("getUoCPOChgMappingData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getUoCPOChgMappingData", "Query To get the PO mapping date : " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                m_oLogger.LogMessage("getUoCPOChgMappingData", "Number of rows Selected " + dtResponse.Rows.Count);
            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("getUoCMinMaxMappingData", "Error trying to get the UoC POChg Mapping data.", ex);

            }
            return dtResponse;
        }

        /// <summary>
        /// Update the process flag to I once the UOC service transaction successfully submited.
        /// </summary>
        /// <returns></returns>
        public int UpdateUoCPOChgMappingData(Logger m_oLogger, string procFlag, string PONum = "")
        {

            DataTable dtResponse = new DataTable();
            int rowsAffected = 0;
            try
            {

                strSQLstring = "UPDATE sysadm8.PS_ISA_MXM_POCHG SET PROCESS_FLAG='" + procFlag + "', LAST_UPDATE_DTTM = SYSDATE WHERE PROCESS_FLAG = 'N' AND CUST_ID = 'UCHICAGO'"; // AND ROWNUM < 2";
                if (PONum != "")
                {
                    strSQLstring += " AND ISA_CUST_PO_ID = '" + PONum + "'";
                }

                m_oLogger.LogMessage("UpdateUoCPOChgMappingData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("UpdateUoCPOChgMappingData", "Query To Update the PO Chg mapping date : " + strSQLstring);

                rowsAffected = OleDBExecuteNonQuery(strSQLstring);

                m_oLogger.LogMessage("UpdateUoCPOChgMappingData", "Number of rows updated : " + rowsAffected);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("UpdateUoCPOChgMappingData", "Error trying to Update the PO Chg Mapping data.", ex); throw;
            }
            return rowsAffected;
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
