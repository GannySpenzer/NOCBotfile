using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO; 
using System.Data.OleDb; //Used by AvaSoft and rest of VB code
using System.Configuration;
using Oracle.DataAccess.Client; // ODP.NET - Use the 4.11 version in the consoleutility bin 
//using Oracle.DataAccess.Types;
//using System.Data.OracleClient; //now deprecated
using UpsIntegration;


namespace UpsIntegration
{
    /** QuantumDbUtility
     *  Quick database utility to help with standard database tasks 
     **/

    class QuantumDbUtility
    {
        public static void openDb(String connStr, SqlConnection dbConn)
        {
            try
            {
                dbConn.ConnectionString = connStr;
                dbConn.Open();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            } 
        }

        public static OracleConnection openDb(String connStr, OracleConnection dbConn)
        {
            try
            {
                dbConn.ConnectionString = connStr;
                dbConn.Open();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return dbConn;
        }

        public static OracleConnection openDb(String connStr)
        {
            OracleConnection dbConn = new OracleConnection();
            try
            {
                dbConn.ConnectionString = connStr;
                dbConn.Open();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return dbConn;
        }

       /* public static OleDbConnection  openDb(String connStr)
        {
            OleDbConnection dbConn = new OleDbConnection();
            try
            {
                dbConn.ConnectionString = connStr;
                dbConn.Open();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return dbConn;
        }*/

        public static void  openDb(String connStr, OleDbConnection dbConn)
        { 
            try
            {
                dbConn.ConnectionString = connStr;
                dbConn.Open();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            } 
        }

        public static void closeDb( SqlConnection dbConn)
        {
            try
            {
                dbConn.Close();
                dbConn.Dispose();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
        }
        public static void closeDb( OracleConnection dbConn)
        {
            try
            {
                dbConn.Close();
                dbConn.Dispose();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
        }

        public static void closeDb( OleDbConnection  dbConn)
        {
            try
            {
                dbConn.Close();
                dbConn.Dispose();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
        }


        public static SqlDataReader  executeDbReader(SqlConnection dbConn, String sql, String[] param )
        {
            SqlDataReader dbReader = null;
            try
            {
              //  QuantumUtility.logError( getSql(sql, param) );
                SqlCommand command = new SqlCommand(getSql(sql,param), dbConn);
                /* for (int i=0; i < param.Length; i++)
                    command.Parameters.AddWithValue("@" + i.ToString(), param[i]);  //command.Parameters.Add(new SqlParameter( i.ToString(), param[i])); */
                dbReader = command.ExecuteReader();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return dbReader;
        }

        public static OracleDataReader executeDbReader(OracleConnection dbConn, String sql, String[] param)
        {
            OracleDataReader dbReader = null;
            try
            {
                OracleCommand command = new OracleCommand(sql, dbConn);
                for (int i = 0; i < param.Length; i++)
                    command.Parameters.Add(new OracleParameter( i.ToString(), param[i]));
                dbReader = command.ExecuteReader();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return dbReader;
        }

        public static OleDbDataReader executeDbReader(OleDbConnection dbConn, String sql, String[] param)
        {
            OleDbDataReader  dbReader = null;

            try
            {

                QuantumUtility.logErrorFile("******* " + DateTime.Today.ToLongDateString() + " " + DateTime.Today.ToLongTimeString() + " *************");
                QuantumUtility.logErrorFile("-- CONN: " + dbConn.ConnectionString);
               QuantumUtility.logErrorFile("-- SQL: " + getSql(sql, param) );
                OleDbCommand command = new OleDbCommand(getSql(sql, param ), dbConn);
               // for (int i = 0; i < param.Length; i++)
                   // command.Parameters.Add(new OleDbParameter( i.ToString(), param[i]));  
                dbReader = command.ExecuteReader(); 


            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return dbReader;
        }

        public static void executeDbUpdate(SqlConnection dbConn, String sql, String[]  param)
        {
            try
            {
                SqlCommand updateCommand = new SqlCommand(sql, dbConn);
                for (int i=0; i < param.Length; i++)
                    updateCommand.Parameters.Add(new SqlParameter( i.ToString(), param[i]));
                updateCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
        }

        public static void executeDbUpdate(OracleConnection dbConn, String sql, String[] param)
        {
            try
            {
                OracleCommand updateCommand = new OracleCommand(sql, dbConn);
                for (int i = 0; i < param.Length; i++)
                    updateCommand.Parameters.Add(new SqlParameter( i.ToString(), param[i]));
                updateCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
        }

        public static void executeDbUpdate(OleDbConnection dbConn, String sql, String[] param)
        {
            try
            {
               QuantumUtility.logErrorFile("** " + DateTime.Today.ToLongDateString() + " " + DateTime.Today.ToLongTimeString() + " DB UPDATE SQL: " + getSql(sql, param));
                OleDbCommand updateCommand = new OleDbCommand(getSql(sql, param), dbConn);
              /*  for (int i = 0; i < param.Length; i++)
                    updateCommand.Parameters.AddWithValue("@" + i.ToString(), param[i]); //updateCommand.Parameters.Add(new OleDbParameter( i.ToString(), param[i])); */
                updateCommand.ExecuteNonQuery();
               
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
        }

        public static void logError(OleDbConnection dbConn, String error)
        {
            try
            {
                executeDbUpdate(dbConn,  "insert into SDIX_UPS_QUANTUMVIEW_ERROR (error) values ('@0')",   new String[1]   { error });
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
        }

        public static String getSql(String oldSql, String[] sqlParams)
        {
            String newSql = oldSql;
            try
            {
                 
                for (int i = 0; i < sqlParams.Length; i++)
                {

                    newSql = newSql.Replace('@' + i.ToString() , sqlParams[i]); 
                    
                }
                 newSql = newSql.Replace("  ", " ");
                newSql = newSql.Replace("OR TRIM(PO.PO_ID) = ''", "");
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return newSql;
        }

    }
}
