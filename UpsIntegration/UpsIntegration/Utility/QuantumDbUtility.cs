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
        private static String defaultStr = "Provider=OraOLEDB.Oracle;User Id=sdiexchange;Password=sd1exchange;Data Source=STAR.WORLD;Connection Timeout=310;"; 
        private static OleDbConnection dbConn  = new OleDbConnection();
        public QuantumDbUtility()
        {
            dbConn= openDb(defaultStr);
        }
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

     /*   public static OracleConnection openDb(String connStr)
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
        */
        public static OleDbConnection  openDb(String connStr)
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
        } 

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
                SqlCommand command = new SqlCommand(getSql(sql,param), dbConn);
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

        public static OleDbDataReader executeDbReader(OleDbConnection dbConn, String sql, String[] param=null)
        {
            OleDbDataReader  dbReader = null;
            String newSql = sql;
            try
            {
                if (param ==null || param.Length > 0)
                    newSql = getSql(newSql, param);
               // QuantumUtility.logError("-- SQL: " + newSql );//+ "\n" + dbConn.ConnectionString ); 
                OleDbCommand command = new OleDbCommand(newSql, dbConn);
                command.CommandTimeout = 310; //Adding as 45 sec queries in Sql Developer timeout via the utility
                dbReader = command.ExecuteReader(); 
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return dbReader;
        }

        /* Checks to see if table exists. If they don't, then run table creation scripts */
        public static String checkDb(OleDbConnection dbConn, String[] tablenames, String  folder)
        {
            OleDbDataReader dbReader = null;
            String sql = "";
            String result = "";
            String filename = "";

            try
            {
                foreach (String tablename in tablenames)
                {
                    dbReader = executeDbReader(dbConn, "select count(*) as count from  ALL_TABLES where TABLE_NAME='@0'", new String[1] { tablename });

                    if (dbReader.HasRows)
                        while (dbReader.Read())
                        {
                            Console.WriteLine("DOES " + tablename + " exist: " + dbReader["count"].ToString());
                            filename = folder +  tablename + ".sql";
                            if (File.Exists(filename) && dbReader["count"].ToString() == "0")
                            {
                                Console.WriteLine("RUN " + filename + " : " + File.Exists(filename).ToString());
                                sql = File.ReadAllText(@filename, Encoding.UTF8);
                                sql = System.Text.RegularExpressions.Regex.Replace(sql, @"[\s]+", " ");
                                if (!String.IsNullOrEmpty(sql))
                                    result += executeDbUpdate(dbConn, @sql);
                                else
                                    result += "Could not find SQL";
                            }
                             
                        }
                }
                return result;
            } 
            catch (Exception e)
            {
             //  QuantumUtility.logError(e);
                logError(dbConn, e.ToString());
                return e.ToString() + e.StackTrace.ToString();
            }
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

        public static String  executeDbUpdate(OleDbConnection dbConn, String sql, String[] param=null)
        {
            String newSql = sql;
            String result = "";
            int res = 0;
            try 
            { 
                if (param != null && param.Length > 0)
                  newSql = getSql(newSql, param);
            // QuantumUtility.logError("** " + DateTime.Now.ToShortDateString() + " " +  DateTime.Now.TimeOfDay + " DB UPDATE SQL: " + newSql);
                OleDbCommand updateCommand = new OleDbCommand(newSql, dbConn);
                res = updateCommand.ExecuteNonQuery();
                if (res == 0)
                    return "";
                else
                    return res.ToString();
            }
            catch (Exception e)
            {
               // QuantumUtility.logError(e);
                return e.ToString() + e.StackTrace.ToString();
            }
        }

        public static void logError(OleDbConnection dbConn, String error)
        {
            try
            {
                executeDbUpdate(dbConn,  "insert into SDIX_UPS_QUANTUMVIEW_ERROR (errormsg) values ('@0')",   new String[1]   { error.Replace("'","") });
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
                if (sqlParams != null)
                {
                  
                    for (int i = 0; i < sqlParams.Length; i++)
                    {

                        newSql = newSql.Replace('@' + i.ToString(), sqlParams[i]);

                        // for (int i = 0; i < param.Length; i++)
                        // command.Parameters.Add(new OleDbParameter( i.ToString(), param[i]));  
                    }
                }
                newSql = System.Text.RegularExpressions.Regex.Replace(newSql, @"'@\d'", "''");
                newSql = System.Text.RegularExpressions.Regex.Replace(newSql, @"[\s]+", " ");
                newSql = newSql.Replace("OR TRIM(PO.PO_ID) = ''", ""); 
                newSql = newSql.Replace("TRIM(SH.ISA_ASN_TRACK_No) = '' OR", "");
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
            return newSql;
        }

    }
}
