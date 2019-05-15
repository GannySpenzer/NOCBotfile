using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.OleDb;
using UoCMapping;

namespace UoCMinMaxMapping
{
    class UoCMinMaxMappingDAL
    {
        System.Data.OleDb.OleDbConnection MyConnection;
        System.Data.DataSet DtSet;
        DataTable dtTransaction;
        System.Data.OleDb.OleDbDataAdapter MyCommand;
        string strSQLstring = "";
        System.Data.OleDb.OleDbConnection MyOracleConn;
        string OracleConString = ConfigurationManager.AppSettings["OLEDBconString"];

        /// <summary>
        /// Get the purchase order data whose process flag is 'N' and cust id is 'SOLVAY'
        /// </summary>
        /// <returns></returns>
        public DataTable getUoCMinMaxMappingData(Logger m_oLogger)
        {

            DataTable dtResponse = new DataTable();
            try
            {

                //strSQLstring = "SELECT * FROM sysadm8.PS_ISA_O_MATR_MOVE WHERE PROCESS_FLAG = 'N' AND CUST_ID = 'PMC' AND TRANS_TYPE = 'REC'";
                strSQLstring = "SELECT * FROM sysadm8.PS_ISA_MXM_ITM_OUT WHERE PROCESS_FLAG = 'N' AND CUST_ID = 'UOC' AND ROWNUM < 2";
                m_oLogger.LogMessage("getUoCMinMaxMappingData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getUoCMinMaxMappingData", "Query To get the PO mapping date : " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                m_oLogger.LogMessage("getUoCMinMaxMappingData", "Number of rows Seleted " + dtResponse.Rows.Count);
            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("getUoCMinMaxMappingData", "Error trying to get the UoC MinMax Mapping data.", ex);

            }
            return dtResponse;
        }

        /// <summary>
        /// Update the process flag to I once the Solvay service transaction successfully submited.
        /// </summary>
        /// <returns></returns>
        public int UpdateUoCMinMaxMappingData(Logger m_oLogger)
        {

            DataTable dtResponse = new DataTable();
            int rowsAffected = 0;
            try
            {

                strSQLstring = "UPDATE SYSADM8.PS_ISA_MXM_ITM_OUT SET PROCESS_FLAG='I', DATE_PROCESSED = SYSDATE WHERE PROCESS_FLAG = 'N' AND CUST_ID = 'SOLVAY' AND ROWNUM < 2";

                m_oLogger.LogMessage("UpdateUoCMinMaxMappingData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("UpdateUoCMinMaxMappingData", "Query To Update the PO Receipt mapping date : " + strSQLstring);

                rowsAffected = OleDBExecuteNonQuery(strSQLstring);

                m_oLogger.LogMessage("UpdateUoCMinMaxMappingData", "Number of rows updated : " + rowsAffected);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("UpdateUoCMinMaxMappingData", "Error trying to Update the PO Receipt Mapping data.", ex); throw;
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
