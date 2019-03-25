using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace SDI.ApprovalsCreation
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Upload File with Excel or Csv format",
                Filter = "Excel Document|*.xlsx;*.csv"
            };
            using (dialog)
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {

                }
            }
            var excel = dialog.FileName;
            if (Convert.ToString(dialog.FileName) == "")
            {
                Environment.Exit(0);
            }
            DataTable csvData = new DataTable();
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string EmployeeID = string.Empty;
            string BU = string.Empty;
            string strApprover = string.Empty;
            string strOrderLimit = string.Empty;
            string strAltAppr = string.Empty;
            
            int count = 0;
            string Logpath = ConfigurationManager.AppSettings["LogFilePath"];
            //string UploadPath = ConfigurationManager.AppSettings["UploadFilePath"];
            string UploadPath = Convert.ToString(dialog.FileName);
            string logFilePath = "C:\\Logs\\";
            //string logFilePath = Path.GetDirectoryName(Application.ExecutablePath);
            logFilePath = Logpath + "ApprovalsCreationUtilityLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine("Started the Approvals Creation");

            try 
            {
                if (UploadPath.Contains(".xlsx"))
                {
                    Excel.Application xlApp = new Excel.Application();
                    Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(UploadPath);
                    int sheeetcount = xlWorkbook.Sheets.Count;
                    Excel.Worksheet xlSheet = xlWorkbook.Sheets[1];
                    Excel.Range xlRange = xlSheet.UsedRange;

                    int numberOfRows = xlRange.Rows.Count;
                    int numberOfCols = xlRange.Columns.Count;
                    List<int> columnsToRead = new List<int>();
                    for (int i = 1; i <= numberOfCols; i++)
                    {
                        if (xlRange.Cells[1, i].Value2 != null) // ADDED IN EDIT
                        {
                            csvData.Columns.Add(Convert.ToString(xlRange.Cells[1, i].Value2));
                            columnsToRead.Add(i);

                        }
                    }
                    List<string> columnValue = new List<string>();
                    // loop over each column number and add results to the list
                    foreach (var c in xlRange.Rows)
                    {
                        var val = c;
                    }
                    // start at 2 because the first row is 1 and the header row
                    for (int r = 2; r <= numberOfRows; r++)
                    {
                        columnValue.Clear();
                        foreach (int c in columnsToRead)
                        {
                            columnValue.Add(Convert.ToString(xlRange.Cells[r, c].Value2));
                        }
                        DataRow rotw = csvData.NewRow();
                        for (int i = 0; i < numberOfCols; i++)
                        {
                            rotw[i] = columnValue[i];
                        }
                        //rotw[0] = columnValue[0];
                        //rotw[1] = columnValue[1];
                        //rotw[2] = columnValue[2];
                        //rotw[3] = columnValue[3];
                        csvData.Rows.Add(rotw);
                    }
                }
                else if (UploadPath.Contains(".csv"))
                {
                    using (TextFieldParser csvReader = new TextFieldParser(UploadPath))
                    {
                        csvReader.SetDelimiters(new string[] { "," });
                        csvReader.HasFieldsEnclosedInQuotes = true;
                        string[] colFields = csvReader.ReadFields();
                        if (colFields != null)
                        {
                            foreach (string column in colFields)
                            {
                                DataColumn datecolumn = new DataColumn(column);
                                datecolumn.AllowDBNull = true;
                                csvData.Columns.Add(datecolumn);
                            }
                        }
                        else
                        {
                            throw new Exception("File does not contain any datas");
                        }
                        while (!csvReader.EndOfData)
                        {
                            string[] fieldData = csvReader.ReadFields();
                            //Making empty value as null
                            for (int i = 0; i < fieldData.Length; i++)
                            {
                                if (fieldData[i] == "")
                                {
                                    fieldData[i] = null;
                                }
                            }
                            csvData.Rows.Add(fieldData);
                        }
                    }
                }
                else {
                    log.WriteLine("Invalid File format.");                    
                    throw new Exception("Invalid File format.");
                }
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();
                if (csvData.Rows.Count != 0)
                {
                    foreach (DataRow rows in csvData.Rows)
                    {
                        try
                        {
                            bool Valid_UserID = false;
                            bool Valid_BU = false;
                            bool Valid_ApprID = false;                            
                            bool CheckForExist = false;
                            bool ValidUserID = false;
                            string Valid_ApprList = string.Empty;
                            string strresult = string.Empty;
                            count = count + 1;
                            EmployeeID = Convert.ToString(rows[0]).ToUpper();
                            BU = Convert.ToString(rows[1]).ToUpper();
                            strApprover = Convert.ToString(rows[2]).ToUpper();
                            strOrderLimit = Convert.ToString(rows[3]);
                            if (EmployeeID == "") 
                            {
                                throw new Exception("Employee ID is requied.");
                            }                            
                            if(BU == "")
                            {
                                throw new Exception("Business Unit is requied.");
                            }                            
                            if(strApprover == "")
                            {
                                throw new Exception("Approver ID is requied.");
                            }                            
                            if (strOrderLimit == "") 
                            {
                                throw new Exception("Order Limit is requied.");
                            }                       
                            Valid_UserID = CheckUserID(EmployeeID);
                            if (!Valid_UserID)
                            {
                                throw new Exception("Given Employee ID is not Valid.");
                            }
                            Valid_BU = CheckBU(BU);
                            if (!Valid_BU)
                            {
                                throw new Exception("Given Business Unit is not Valid.");
                            }
                            Valid_ApprID = CheckApprID(strApprover);
                            if (!Valid_ApprID)
                            {
                                throw new Exception("Given Approver ID is not Valid. And Approver must be the customer.");
                            }
                            Valid_ApprList = CheckAppr_List(EmployeeID, BU, strApprover);
                            if (Valid_ApprList != "")
                            {
                                throw new Exception(Valid_ApprList);
                            }
                            string strSqlString = string.Empty;
                            int rowsaffected;
                            CheckForExist = CheckApproversTbl(EmployeeID, BU);
                            if (!CheckForExist)
                            {
                                strSqlString = @"INSERT INTO SDIX_USERS_APPRV (ISA_EMPLOYEE_ID, BUSINESS_UNIT, ISA_IOL_APR_EMP_ID, ISA_IOL_APR_LIMIT, 
                                                ISA_IOL_APR_ALT, LASTUPDOPRID, LASTUPDDTTM)
                                                 VALUES('"+ EmployeeID +"', '"+ BU +"', '"+ strApprover +"', "+ strOrderLimit +", '"+ strApprover +"', 'SYSADMIN', TO_DATE('"+ DateTime.Now.ToString() +"', 'MM/DD/YYYY HH:MI:SS AM'))";
                                com = new OleDbCommand(strSqlString, cn);
                                rowsaffected = com.ExecuteNonQuery();
                                if (rowsaffected == 0)
                                {
                                    strresult = "Error occured While Inserting in the SDIX_USERS_APPRV table.";
                                }
                                else { 
                                    strresult = "Approver inserted successfully."; 
                                }
                            }
                            else {
                                //strSqlString = @"UPDATE SDIX_USERS_APPRV SET ISA_IOL_APR_EMP_ID = '"+ strApprover +"', ISA_IOL_APR_ALT = '"+
                                //    strApprover +"', ISA_IOL_APR_LIMIT = "+ strOrderLimit +", LASTUPDOPRID = 'SYSADMIN', LASTUPDDTTM = TO_DATE('"+
                                //    DateTime.Now.ToString() +"', 'MM/DD/YYYY HH:MI:SS AM') WHERE ISA_EMPLOYEE_ID = '"+
                                //    EmployeeID +"' AND BUSINESS_UNIT = '"+ BU +"'";
                                //com = new OleDbCommand(strSqlString, cn);
                                //rowsaffected = com.ExecuteNonQuery();
                                //if (rowsaffected == 0)
                                //{
                                //    strresult = "Error occured While Updating in the SDIX_USERS_APPRV table.";
                                //}
                                //else {
                                //    strresult = "Approver updated successfully.";
                                //}
                                strresult = "Approver already exists ";
                            }
                            log.WriteLine("{0}. {1} for the Employee ID - '{2}'. \n", count, strresult, EmployeeID);
                        }
                        catch (Exception ex)
                        {
                            log.WriteLine("{0}. Employee ID - '{1}' is having an issue while setting up an Approver. Error details : {2} \n", count, EmployeeID, ex.Message);
                            continue;
                        }
                    }                    
                }
                else 
                {
                    log.WriteLine("File does not contain any datas \n"); 
                }
            }catch(Exception ex)
            {
                log.WriteLine("Error while Synch {0}", ex);
                throw new System.ArgumentOutOfRangeException("index parameter is out of range.", ex);
            }
            log.Close();
        }

        public static bool CheckBU(string BU)
        {
            bool rslt = false;
            string strSQLstring = string.Empty;
            strSQLstring = @"SELECT * From SYSADM8.PS_ISA_ENTERPRISE where ISA_BUSINESS_UNIT = '" + BU + "'";
            DataSet dsCustUserid = GetAdapter(strSQLstring);
            if (dsCustUserid.Tables[0].Rows.Count == 0)
            {
                if (BU == "ISA00" || BU == "SDM00")
                {
                    rslt = true;
                }
                else
                {
                    rslt = false;
                }
            }
            else
            {
                rslt = true;
            }
            return rslt;
        }

        public static bool CheckUserID(string UserID)
        {
            bool rslt = false;
            string strSQLstring = string.Empty;
            strSQLstring = @"SELECT * FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '"+ UserID +"'";
            DataSet dsCustUserid = GetAdapter(strSQLstring);
            if (dsCustUserid.Tables[0].Rows.Count == 0)
            {
                rslt = false;
            }
            else
            {
                rslt = true;
            }
            return rslt;
        }

        public static bool CheckApprID(string UserID)
        {
            bool rslt = false;
            string strSQLstring = string.Empty;
            strSQLstring = @"SELECT * FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" + UserID + "' AND ISA_SDI_EMPLOYEE = 'C'";
            DataSet dsCustUserid = GetAdapter(strSQLstring);
            if (dsCustUserid.Tables[0].Rows.Count == 0)
            {
                rslt = false;
            }
            else
            {
                rslt = true;
            }
            return rslt;
        }

        public static string CheckAppr_List(string UserID, string BU, string strAppv)
        {
            string rslt = string.Empty;
            string strSQLstring = string.Empty;
            strSQLstring = @"SELECT A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A
                            JOIN SDIX_MULTI_SITE B ON A.ISA_EMPLOYEE_ID=B.ISA_EMPLOYEE_ID 
                            WHERE B.BUSINESS_UNIT='"+ BU +"' AND A.ISA_SDI_EMPLOYEE = 'C' AND NOT A.ISA_EMPLOYEE_ID = '"+ UserID +"' AND A.ACTIVE_STATUS = 'A' UNION SELECT A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A WHERE A.BUSINESS_UNIT='"+ BU +"' AND A.ISA_SDI_EMPLOYEE = 'C' AND NOT A.ISA_EMPLOYEE_ID = '"+ UserID +"' AND A.ACTIVE_STATUS = 'A'";
            DataSet dsCustUserid = GetAdapter(strSQLstring);
            if (dsCustUserid.Tables[0].Rows.Count == 0)
            {
                rslt = "There is not Approvers found for the Employee ID.";
            }
            else
            {
                string find = "ISA_EMPLOYEE_ID = '"+ strAppv +"'";
                DataRow[] foundRows = dsCustUserid.Tables[0].Select(find);
                var rrrr = foundRows.Count();
                if (foundRows.Count() > 0)
                {
                    rslt = "";
                }
                else {
                    rslt = "Approver was not in the same BU of the Employee ID.";
                }
            }
            return rslt;
        }

        public static bool CheckApproversTbl(string UserID, string BU)
        {
            bool rslt = false;
            string strSQLstring = string.Empty;
            strSQLstring = @"SELECT * FROM SDIX_USERS_APPRV WHERE ISA_EMPLOYEE_ID = '"+ UserID +"' AND BUSINESS_UNIT = '"+ BU +"'";
            DataSet dsCustUserid = GetAdapter(strSQLstring);
            if (dsCustUserid.Tables[0].Rows.Count == 0)
            {
                rslt = false;
            }
            else
            {
                rslt = true;
            }
            return rslt;
        }

        public static DataSet GetAdapter(string p_strQuery, bool bGoToErrPage = true, bool bThrowBackError = false)
        {
            // Gives us a reference to the current asp.net 
            // application executing the method.
            //HttpApplication currentApp = new HttpApplication();
            //currentApp = HttpContext.Current.ApplicationInstance;
            OleDbConnection connection = new OleDbConnection(ConfigurationManager.ConnectionStrings["ConString"].ConnectionString);
            DataSet UserdataSet = new DataSet();
            try
            {
                OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(Command);

                dataAdapter.Fill(UserdataSet);
                try
                {
                    dataAdapter.Dispose();
                }
                catch (Exception ex)
                {
                }
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex)
                {
                }
                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
                // connection.close()               
            }
            catch (Exception objException)
            {
                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {

                }
            }
            return UserdataSet;
        }

        public static string GetScalar(string p_strQuery, bool bGoToErrPage = true)
        {
            // Gives us a reference to the current asp.net 
            // application executing the method.
            //HttpApplication currentApp = HttpContext.Current.ApplicationInstance;
            string strReturn = "";
            OleDbConnection connection = new OleDbConnection(ConfigurationManager.ConnectionStrings["ConString"].ConnectionString);
            try
            {
                OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                try
                {
                    strReturn = System.Convert.ToString(Command.ExecuteScalar());
                }
                catch (Exception ex32)
                {
                    strReturn = "";
                }
                if (strReturn == null)
                    strReturn = "";
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex1)
                {
                }
                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex2)
                {
                }
            }
            // connection.close()
            catch (Exception objException)
            {
                strReturn = "";

                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex)
                {

                    // connection.close()

                }
            }

            return strReturn;
        }

        public static int ExecNonQuery(string p_strQuery, bool bGoToErrPage = true, bool bSendEmail = true)
        {
            int rowsAffected = 0;
            OleDbConnection connection = new OleDbConnection(ConfigurationManager.ConnectionStrings["ConString"].ConnectionString);

            try
            {
                OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                rowsAffected = Command.ExecuteNonQuery();
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex)
                {
                }
                try
                {
                    connection.Dispose();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
            }
            // connection.close()
            catch (Exception objException)
            {
                rowsAffected = 0;
                try
                {
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
            }

            return rowsAffected;
        }
    }
}
