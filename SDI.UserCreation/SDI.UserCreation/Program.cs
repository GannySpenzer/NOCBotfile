using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Data.OleDb;
using System.Configuration;
using System.Web;
using System.Security.Cryptography;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;

namespace SDI.UserCreation
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

            string FirstName = string.Empty;
            string LastName = string.Empty;
            string Email = string.Empty;
            string PhoneNo = string.Empty;
            string BU = string.Empty;
            string Pwd = string.Empty;
            string RoleType = string.Empty;
            string UserType = string.Empty;
            string VendorId = string.Empty;
            string CreatorUserID = string.Empty;
            string UserID = string.Empty;
            string strFullName40 = string.Empty;
            string RoleID = string.Empty;
            int count = 0;
            string Logpath = ConfigurationManager.AppSettings["LogFilePath"];
            //string UploadPath = ConfigurationManager.AppSettings["UploadFilePath"];
            string UploadPath = Convert.ToString(dialog.FileName);
            string logFilePath = "C:\\Logs\\";
            logFilePath = Logpath + "UserCreationUtilityLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) + "." + "txt";
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
            log.WriteLine("Started the User Creations");

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(UploadPath))
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
                            csvData.Rows.Add(rotw);
                        }
                    }
                    else if (UploadPath.Contains(".csv"))
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
                    else
                    {
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
                                count = count + 1;
                                FirstName = Convert.ToString(rows[0]).Trim();
                                LastName = Convert.ToString(rows[1]).Trim();
                                Email = Convert.ToString(rows[2]).Trim();
                                PhoneNo = Convert.ToString(rows[3]).Trim();
                                //Pwd = Convert.ToString(rows[4]);
                                Pwd = "Welcome";
                                BU = Convert.ToString(rows[4]).Trim();
                                UserType = Convert.ToString(rows[5]).ToUpper().Trim();
                                RoleType = Convert.ToString(rows[6]).ToUpper().Trim();
                                RoleID = Convert.ToString(rows[8]).Trim();
                                VendorId = string.Empty;
                                if (FirstName.Length > 30)
                                {
                                    FirstName = FirstName.Substring(0, 30);
                                }
                                if (LastName.Length > 30)
                                {
                                    LastName = LastName.Substring(0, 30);
                                }
                                if (Email.Length > 60)
                                {
                                    Email = Email.Substring(0, 60);
                                }
                                if (PhoneNo.Length == 0)
                                {
                                    PhoneNo = "111-111-1111";
                                }
                                if (PhoneNo.Length > 14)
                                {
                                    PhoneNo = PhoneNo.Substring(0, 14);
                                }
                                if (BU.Length > 5)
                                {
                                    BU = BU.Substring(0, 5);
                                }
                                if (UserType.Length == 0)
                                {
                                    UserType = "C";
                                }
                                if (RoleType.Length == 0)
                                {
                                    RoleType = "USER";
                                }
                                if (UserType.Length > 1)
                                {
                                    UserType = UserType.Substring(0, 1);
                                }
                                string strFullName = LastName + "," + FirstName;
                                strFullName40 = LastName + "," + FirstName;
                                if (strFullName40.Length > 50)
                                {
                                    strFullName = strFullName.Substring(0, 50);
                                }
                                if (strFullName40.Length > 40)
                                {
                                    strFullName = strFullName40.Substring(0, 40);
                                }
                                if (UserType == "C" || UserType == "S")
                                {
                                    bool VerifyBU = CheckBU(BU.ToUpper());
                                    if (!VerifyBU)
                                    {
                                        throw new Exception("Business Unit does not exist");
                                    }
                                }
                                else if (UserType == "V")
                                {
                                    VendorId = Convert.ToString(rows[7]).Trim();
                                    if (BU == "ISA00" || BU == "SDM00")
                                    {

                                    }
                                    else
                                    {
                                        throw new Exception("Vendor business Unit is invalid.");
                                    }
                                    if (VendorId == "")
                                    {
                                        throw new Exception("Vendor ID is required to create a New Vendor user");
                                    }
                                    else
                                    {
                                        bool vendrCheck = checkVendorId(VendorId);
                                        if (!vendrCheck)
                                        {
                                            throw new Exception("Given Vendor ID is invalid");
                                        }
                                    }
                                }

                                if (UserType == "S")
                                {
                                    if (RoleType != "SUPER" && RoleType != "ADMIN" && RoleType != "USER")
                                    {
                                        throw new Exception("Given Role type is incorrect SDI Employee roles either Super or Admin or User.");
                                    }
                                }
                                else if (UserType == "C" || UserType == "V")
                                {
                                    if (RoleType != "ADMIN" && RoleType != "USER")
                                    {
                                        throw new Exception("Given Role type is incorrect. Role either be Admin or User.");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Given User type value is incorrect.");
                                }
                                UserID = Auto_UserIdGenerate_Flags(FirstName, LastName);
                                string strPasswEncrp = GenerateHash(Pwd);

                                string strSqlString = string.Empty;
                                string strSQLPW = string.Empty;
                                int rowsaffected;
                                long lngIsaUserId = GetNextUserId(UserID, BU);
                                strSqlString = @"INSERT INTO SDIX_USERS_TBL
                                             (ISA_USER_ID, ISA_USER_NAME,
                                             ISA_PASSWORD_ENCR, FIRST_NAME_SRCH,
                                            LAST_NAME_SRCH, BUSINESS_UNIT,
                                            ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME, PHONE_NUM,
                                            ISA_DAILY_ALLOW, ISA_EMPLOYEE_PASSW,
                                            ISA_EMPLOYEE_EMAIL, ISA_EMPLOYEE_ACTYP,
                                            CUST_ID, ISA_SESSION,
                                            ISA_LAST_SYNC_DATE, ISA_SDI_EMPLOYEE, ISA_CUST_SERV_FLG,
                                            LASTUPDOPRID, LASTUPDDTTM, ACTIVE_STATUS, PWDRESET, LAST_ACTIVITY, ISA_VENDOR_ID,ROLENUM)
                                            VALUES(" + lngIsaUserId + ",'" + strFullName.ToUpper() + "','" + strPasswEncrp + "', '"
                                                         + FirstName.ToUpper() + "', '" + LastName.ToUpper() + "', '" + BU + "', '" + UserID + "', '"
                                                         + strFullName40.ToUpper() + "', '" + PhoneNo + "', 0, ' ', '"
                                                         + Email + "', '" + RoleType.ToUpper() + "', '0', 0, '', '" + UserType.ToUpper() + "', ' ', 'SYSLOAD', TO_DATE('" +
                                                         DateTime.Now.ToString() + "', 'MM/DD/YYYY HH:MI:SS AM'), 'A', 'N', TO_DATE('" +
                                                         DateTime.Now.ToString() + "', 'MM/DD/YYYY HH:MI:SS AM'), '" + VendorId + "','" + RoleID + "')";

                                strSQLPW = @"INSERT INTO SDIX_ISOL_PW
                                        (ISA_USER_ID, ISA_EMPLOYEE_ID,
                                        ISA_ISOL_PW1, ISA_ISOL_PW_DATE1,
                                        ISA_ISOL_PW2, ISA_ISOL_PW_DATE2,
                                        ISA_ISOL_PW3, ISA_ISOL_PW_DATE3)
                                        VALUES (" + lngIsaUserId + ",'" +
                                                      UserID + "','" + strPasswEncrp + "', TO_DATE('" +
                                                      DateTime.Now.ToString("d") + "', 'MM/DD/YYYY'), ' ', '', ' ','')";

                                com = new OleDbCommand(strSqlString, cn);
                                rowsaffected = com.ExecuteNonQuery();
                                //rowsaffected = ExecNonQuery(strSQLPW);
                                if (rowsaffected == 0)
                                {
                                    throw new Exception("Error in Updating the SDIX_USERS_TBL");
                                }
                                if (UserType == "C")
                                {
                                    if (!checkCustEmpTbl(BU, UserID))
                                    {
                                        strSqlString = @"Insert Into PS_ISA_EMPL_TBL
                                                    ( BUSINESS_UNIT, ISA_EMPLOYEE_ID,
                                                    ISA_EMPLOYEE_NAME, ISA_DAILY_ALLOW,
                                                    ISA_EMPLOYEE_PASSW, ISA_EMPLOYEE_EMAIL,
                                                    ISA_EMPLOYEE_ACTYP, CUST_ID, EFF_STATUS)
                                                   Values('" + BU + "', '" + UserID + "', '" +
                                                                 strFullName40.ToUpper() + "',0,' ',' ',' ', ' ', 'A')";
                                        com = new OleDbCommand(strSqlString, cn);
                                        rowsaffected = com.ExecuteNonQuery();
                                        //rowsaffected = ExecNonQuery(strSqlString);
                                        if (rowsaffected == 0)
                                        {
                                            throw new Exception("Error in Updating the PS_ISA_EMPL_TBL");
                                        }
                                    }
                                }

                                if (BU != "SDI00" && BU != "SDM00")
                                {

                                }
                                else
                                {
                                    string strbunit = " ";
                                    if (UserID.Substring(0, 2) == "M0" || UserID.Substring(0, 2) == "MU")
                                    {
                                        strbunit = "SDM00";
                                    }
                                    else
                                    {
                                        strbunit = "ISA00";
                                    }
                                    bool bolUserPrivs = checkUserPrivs(UserID);
                                    if (!bolUserPrivs)
                                    {
                                        strSqlString = @"INSERT INTO SDIX_USERS_PRIVS 
                                                     (ISA_EMPLOYEE_ID,
                                                     BUSINESS_UNIT,
                                                    ISA_IOL_OP_NAME,
                                                    ISA_IOL_OP_VALUE,
                                                    ISA_IOL_OP_TYPE,
                                                    LASTUPDOPRID,
                                                    LASTUPDDTTM)
                                                    VALUES('" + UserID + "','" + BU + "','ASN','Y','SUP','SYSLOAD', TO_DATE('" + DateTime.Now.ToString() + "', 'MM/DD/YYYY HH:MI:SS AM'))";
                                        com = new OleDbCommand(strSqlString, cn);
                                        rowsaffected = com.ExecuteNonQuery();
                                        //rowsaffected = ExecNonQuery(strSqlString);
                                        if (rowsaffected == 0)
                                        {
                                            throw new Exception("Error in Updating the SDIX_USERS_PRIVS");
                                        }
                                    }
                                }
                                log.WriteLine("{0}. User Name - '{1}' has been Created succesfully with UserID - {2} \n", count, strFullName40.ToUpper(), UserID);
                            }
                            catch (Exception e)
                            {
                                log.WriteLine("{0}. User Name - '{1}' has having an issue while creating a user Error : {2} \n", count, strFullName40.ToUpper(), e.Message);
                                continue;
                            }
                        }
                        cn.Close();

                    }
                    else
                    {
                        log.WriteLine("File does not contain any datas");
                    }
                }
            }
            catch (System.IndexOutOfRangeException e)  // CS0168
            {
                log.WriteLine("Error while User creation, {0}", e);
                throw new System.ArgumentOutOfRangeException("index parameter is out of range.", e);
            }
            log.Close();
        }

        //public void getverified(DataRow rows)
        //{
        //    List<string> errlst = new List<string>();
        //    if (Convert.ToString(rows[0]) == "")
        //    {
        //        errlst.Add("Fisrt Name");
        //    }
        //    if (Convert.ToString(rows[1]) == "")
        //    {
        //        errlst.Add("Last Name");
        //    }
        //    if (Convert.ToString(rows[2]) == "")
        //    {
        //        errlst.Add("Email ID");
        //    }
        //    if (Convert.ToString(rows[3]) == "")
        //    {
        //        errlst.Add("Phone Number");
        //    }
        //    if (Convert.ToString(rows[4]) == "")
        //    {
        //        errlst.Add("Business unit");
        //    }
        //    if (Convert.ToString(rows[5]) == "")
        //    {
        //        errlst.Add("User type");
        //    }
        //    if(Convert.ToString(rows[]) != "S" ||Convert.ToString(rows[6]) != "C"||Convert.ToString(rows[6]) != "V")            
        //    {
        //        errlst.Add("");
        //    }
        //    if (Convert.ToString(rows[7]) == "")
        //    {
        //        errlst.Add("Role type");
        //    }            
        //}

        public static string GenerateHash(string SourceText)
        {
            // Create an encoding object to ensure the encoding standard for the source text
            UnicodeEncoding Ue = new UnicodeEncoding();
            // Retrieve a byte array based on the source text
            byte[] ByteSourceText = Ue.GetBytes(SourceText);
            // Instantiate an MD5 Provider object
            MD5CryptoServiceProvider Md5 = new MD5CryptoServiceProvider();
            // Compute the hash value from the source
            byte[] ByteHash = Md5.ComputeHash(ByteSourceText);
            // And convert it to String format for return
            return Convert.ToBase64String(ByteHash);
        }

        public static bool CheckBU(string BU)
        {
            bool rslt = false;
            string strSQLstring = string.Empty;
            strSQLstring = @"SELECT * From SYSADM8.PS_ISA_ENTERPRISE where ISA_BUSINESS_UNIT = '" + BU + "'";
            DataSet dsCustUserid = GetAdapter(strSQLstring);
            if (dsCustUserid.Tables[0].Rows.Count == 0)
            {

            }
            else
            {
                rslt = true;
            }
            return rslt;
        }

        private static bool checkCustEmpTbl(string strBU, string userid)
        {
            string strSQLstring = @"Select ISA_EMPLOYEE_ID 
                                    FROM PS_ISA_EMPL_TBL 
                                    WHERE UPPER(isa_employee_id) = '" +
                                    userid + "' AND BUSINESS_UNIT = '" + strBU + "'";

            DataSet dsCustUserid = GetAdapter(strSQLstring);

            if (dsCustUserid.Tables[0].Rows.Count == 0)
                return false;
            else
                return true;
        }

        public static long GetNextUserId(string strUserId, string strBU)
        {
            long lngIsaUserId = 0;
            long lngPKNextVal = 0;
            long lngMaxIsaUserId = 0;
            bool bSendErrorEmail = true;
            string strSqlStr54 = "SELECT SDIX_SEQ_ISA_USER_ID_PK.NEXTVAL FROM DUAL";
            // get SEQ_ISA_USER_ID_PK.nextval 
            try
            {
                lngPKNextVal = System.Convert.ToInt64(GetScalar(strSqlStr54, false));
            }
            catch (Exception ex)
            {
                lngPKNextVal = 0;
            }
            // get MAX(ISA_USER_ID) from PS_ISA_USERS_TBL
            string strSqlStr54N = "SELECT MAX(ISA_USER_ID) FROM SDIX_USERS_TBL";
            try
            {
                lngMaxIsaUserId = System.Convert.ToInt64(GetScalar(strSqlStr54N, false));
            }
            catch (Exception ex)
            {
                lngMaxIsaUserId = 0;
            }
            // compare - if nextval less then max ISA_USER_ID then send email alert 
            if (lngMaxIsaUserId > 0)
            {
                if (lngPKNextVal > 0)
                {
                    if (lngPKNextVal > lngMaxIsaUserId)
                    {
                        lngIsaUserId = lngPKNextVal;
                        bSendErrorEmail = false;
                    }
                    else
                    {
                        long lngGap = lngMaxIsaUserId - lngPKNextVal + 1;
                        lngIsaUserId = lngMaxIsaUserId + 1;
                        if (lngGap > 0)
                        {
                            string strIncreaseNextVal = "";
                            int iCtr = 0;
                            for (iCtr = 0; iCtr <= lngGap - 1; iCtr++)
                                strIncreaseNextVal = GetScalar(strSqlStr54, false);
                        }
                    }
                }
            }

            return lngIsaUserId;
        }


        public static string Auto_UserIdGenerate_Flags(string fname, string Lname)
        {
            string AutoGenerateUserID = string.Empty;
            string strFirst = fname.Trim();
            strFirst = strFirst.Replace("'", "");
            strFirst = strFirst.Replace(" ", "");
            string strLast = Lname.Trim();
            strLast = strLast.Replace("'", "");
            strLast = strLast.Replace(" ", "");
            string FirstPart_UserID = string.Empty;
            string SecondPart_UserID = string.Empty;
            try
            {
                if (strFirst.Length > 3 & strLast.Length > 3)
                {
                    FirstPart_UserID = strFirst.Substring(0, 3);
                    SecondPart_UserID = strLast.Substring(0, 3);
                }
                else if (strFirst.Length < 3 & strLast.Length < 3)
                {
                    FirstPart_UserID = strFirst.Substring(0, strFirst.Length);
                    SecondPart_UserID = strLast.Substring(0, strLast.Length);
                }
                else if (strFirst.Length > 3 | strLast.Length < 3)
                {
                    SecondPart_UserID = strLast.Substring(0, strLast.Length);
                    if (strFirst.Length != 3)
                    {
                        if (strLast.Length == 1)
                            FirstPart_UserID = strFirst.Substring(0, 5);
                        else if (strLast.Length == 2)
                            FirstPart_UserID = strFirst.Substring(0, 4);
                        else if (strLast.Length == 3)
                            FirstPart_UserID = strFirst.Substring(0, 3);
                    }
                    else
                        FirstPart_UserID = strFirst.Substring(0, strFirst.Length);
                }
                else if (strFirst.Length < 3 | strLast.Length > 3)
                {
                    FirstPart_UserID = strFirst.Substring(0, strFirst.Length);
                    if (strLast.Length != 3)
                    {
                        if (strFirst.Length == 1)
                            SecondPart_UserID = strLast.Substring(0, 5);
                        else if (strFirst.Length == 2)
                            SecondPart_UserID = strLast.Substring(0, 4);
                        else if (strFirst.Length == 3)
                            SecondPart_UserID = strLast.Substring(0, 3);
                    }
                    else
                        SecondPart_UserID = strLast.Substring(0, strLast.Length);
                }
                else
                {
                    FirstPart_UserID = strFirst.Substring(0, 3);
                    SecondPart_UserID = strLast.Substring(0, 3);
                }

                string usrid = FirstPart_UserID + SecondPart_UserID;

                if (usrid.Length < 6)
                {
                    if (usrid.Length == 5)
                        usrid = usrid + "0";
                    else if (usrid.Length == 4)
                        usrid = usrid + "00";
                    else if (usrid.Length == 3)
                        usrid = usrid + "000";
                    else if (usrid.Length == 2)
                        usrid = usrid + "0000";
                }

                bool VerifiedUsrID = false;

                // Dim dsUsertbl As DataSet
                string strSQLUserIdQuery = "Select USERID_SEQ from ISA_USERID_SEQ order by  USERID_SEQ desc";
                DataSet dsOREmp = GetAdapter(strSQLUserIdQuery);

                if (dsOREmp.Tables[0].Rows.Count > 0)
                {
                    string Prev_UserID = Convert.ToString(dsOREmp.Tables[0].Rows[0]["USERID_SEQ"]);
                    int Inc_Count = 1;
                    if (Prev_UserID != "")
                    {
                        do
                        {
                            int UserIDval = Convert.ToInt32(Prev_UserID) + Inc_Count;
                            string strUser_ID_Created = Convert.ToString(UserIDval);
                            if (strUser_ID_Created.Length == 1)
                                strUser_ID_Created = "000" + strUser_ID_Created;
                            else if (strUser_ID_Created.Length == 2)
                                strUser_ID_Created = "00" + strUser_ID_Created;
                            else if (strUser_ID_Created.Length == 3)
                                strUser_ID_Created = "0" + strUser_ID_Created;
                            else
                            {
                            }

                            //int strAutoIDLen = 10 -  strUser_ID_Created.Length;
                            //string strSliceUserID;


                            //if (strAutoIDLen != usrid.Length ){
                            //    strSliceUserID = usrid.Substring(0, strAutoIDLen);

                            //}
                           

                            string UserID_Created = usrid + strUser_ID_Created;
                            int userIDLen = UserID_Created.Length;

                            if (userIDLen > 10) {
                                userIDLen = userIDLen - 10;
                                usrid = usrid.Substring(0, usrid.Length - userIDLen);
                                UserID_Created = usrid + strUser_ID_Created;
                            }

                            AutoGenerateUserID = UserID_Created.ToUpper();
                            string strSQLstring = "Select ISA_USER_ID FROM SDIX_USERS_TBL WHERE isa_employee_id = '" + AutoGenerateUserID + "'";
                            DataSet dsUserid = GetAdapter(strSQLstring);
                            if (dsUserid.Tables[0].Rows.Count > 0)
                            {
                                VerifiedUsrID = true;
                                Inc_Count = Inc_Count + 1;
                            }
                            else
                                VerifiedUsrID = false;
                        }
                        while (!(VerifiedUsrID == false));

                        UpdateUserIDSeqTBL(Inc_Count);
                    }
                    //AutoGenID = "A";
                    //return AutoGenerateUserID;
                }
            }
            catch (Exception ex)
            {
                return "";
            }
            return AutoGenerateUserID;
        }

        public static void UpdateUserIDSeqTBL(int Inc_value)
        {
            try
            {
                string strUserIdQuery = "Select USERID_SEQ from ISA_USERID_SEQ order by USERID_SEQ desc";
                DataSet ds_PreviousUserIDSeq = GetAdapter(strUserIdQuery);
                int strPreviousUserIDSeq = Convert.ToInt32(ds_PreviousUserIDSeq.Tables[0].Rows[0]["USERID_SEQ"]);
                int strCurrentValue = strPreviousUserIDSeq + Inc_value;
                string UpdateQuery_UserIdSeq;
                UpdateQuery_UserIdSeq = "Update ISA_USERID_SEQ set USERID_SEQ = " + strCurrentValue + " where USERID_SEQ = " + strPreviousUserIDSeq + "";
                //if (Inc_value == 0)
                //    UpdateQuery_UserIdSeq = "Update ISA_USERID_SEQ set USERID_SEQ = " + strPreviousUserIDSeq + 1 + " where USERID_SEQ = " + strPreviousUserIDSeq + "";
                //else
                //    UpdateQuery_UserIdSeq = "Update ISA_USERID_SEQ set USERID_SEQ = " + strPreviousUserIDSeq + Inc_value + " where USERID_SEQ = " + strPreviousUserIDSeq + "";

                DataSet ds_UpdatedUserIDSeq = GetAdapter(UpdateQuery_UserIdSeq);
            }
            catch (Exception ex)
            {
            }
        }

        private static bool checkUserPrivs(string strUserid)
        {
            string strbunit = " ";
            // If Me.txtUserid.Text.Substring(0, 2) = "M0" Or Me.txtUserid.Text.Substring(0, 2) = "MU" Then
            if (strUserid.Substring(0, 2) == "M0" || strUserid.Substring(0, 2) == "MU")
                strbunit = "SDM00";
            else
                strbunit = "ISA00";
            string strSQLString = @"SELECT A.ISA_EMPLOYEE_ID FROM SDIX_USERS_PRIVS A 
                                    WHERE A.BUSINESS_UNIT = '" + strbunit + "' AND A.ISA_EMPLOYEE_ID = '" +
                                                               strUserid + "' AND A.ISA_IOL_OP_NAME = 'ASN' AND A.ISA_IOL_OP_VALUE = 'Y' AND A.ISA_IOL_OP_TYPE = 'SUP'";

            string strUserResults = GetScalar(strSQLString);
            if (strUserResults == null)
                return false;
            else
                return true;
        }

        private static bool checkVendorId(string strvendorid)
        {
            string strSQLString = @"SELECT DISTINCT ISA_VENDOR_ID FROM SDIX_USERS_TBL where ISA_VENDOR_ID = '" + strvendorid + "'";

            DataSet ds_vendorID = GetAdapter(strSQLString);
            if (ds_vendorID.Tables[0].Rows.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
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
