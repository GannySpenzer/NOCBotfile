using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace ReqReviewReload1
{
    public class REQData 
    {
        public List<string> ACTION_ITEM = new List<string>();
        public List<string> CLIENT = new List<string>();
        public List<string> SITE_NAME = new List<string>();
        public List<string> SHIP_TO_ID = new List<string>();
        public List<string> REQUESTOR_ID = new List<string>();
        public List<string> BUYER_ID = new List<string>();
        public List<DateTime> REQUISITION_DATE = new List<DateTime>();
        public List<string> REQUISITION_ID = new List<string>();
        public List<string> LINE_NUMBER = new List<string>();
        public List<string> INVENTORY_ITEM_ID = new List<string>();
        public List<int> QTY_REQ = new List<int>();
        public List<int> QTY_OPEN = new List<int>();
        public List<string> UNIT_OF_MEASURE = new List<string>();
        public List<string> PRICE_REQ = new List<string>();
        public List<string> SELL_PRICE = new List<string>();
        public List<string> VENDOR_ID = new List<string>();
        public List<string> VENDOR_NAME = new List<string>();
        public List<string> PROBLEM_CODE = new List<string>();
        public List<string> REQ_HOLD_FLAG = new List<string>();
        public List<DateTime> REQ_LINE_ADD_DATE = new List<DateTime>();
        public List<string> MANUFACTURER = new List<string>();
        public List<string> MANUFACTURER_PART_NUMBER = new List<string>();
        public List<string> DESCRIPTION = new List<string>();
        public List<string> REQUISITION_COMMENTS = new List<string>();
        public List<string> CHARGE_CD = new List<string>();
        public List<string> WORKORDER = new List<string>();
        public List<string> PRIORITY_FLAG = new List<string>();
        public List<int> STATUS_AGE = new List<int>();
        public List<string> PS_URL = new List<string>();
        public List<string> BUYER_TEAM = new List<string>();
    }

    public class ReqReviewReloadDAL
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

        REQData req = new REQData();

        // InitializeLogger start here
        //Logger m_oLogger;
        //string sLogPath = Environment.CurrentDirectory;

        DataTable dtResponse = new DataTable();

        public void CreateTable(Logger m_oLogger)
        {
            try
            {
                //check if table already exists
                //strSQLstring = "select table_name from user_tables where table_name='SDIX_REQREVIEWTEMP'";
                //dtResponse = oleDBExecuteReader(strSQLstring);
                //if (dtResponse.Rows.Count > 0)
                //{
                    //if it does, drop the table
                    strSQLstring = "TRUNCATE TABLE SDIX_REQREVIEWTEMP";
                    dtResponse = oleDBExecuteReader(strSQLstring);
                //}

                strSQLstring = "INSERT INTO SDIX_REQREVIEWTEMP \n";
                strSQLstring += "(SELECT distinct ' ' as Action_Item, \n";
                strSQLstring += "G.ACCOUNTING_OWNER as Client, \n";
                strSQLstring += "G.DESCR as Site_Name, \n";
                strSQLstring += "C.SHIPTO_ID as Ship_to_ID, \n";
                strSQLstring += "A.REQUESTOR_ID as Requestor_ID, \n";
                strSQLstring += "NVL(B.BUYER_ID, ' ') as Buyer_ID, \n";
                strSQLstring += "TO_CHAR(A.REQ_DT, 'YYYY-MM-DD') as Requisition_Date, \n";
                strSQLstring += "B.REQ_ID as Requisition_ID, \n";
                strSQLstring += "B.LINE_NBR as Line_Number, \n";
                strSQLstring += "B.INV_ITEM_ID as Inventory_Item_ID, \n";
                strSQLstring += "B.QTY_REQ as Qty_Req, \n";
                strSQLstring += "e.qty_open as Qty_Open,\n";
                strSQLstring += "B.UNIT_OF_MEASURE as Unit_Of_Measure, \n";
                strSQLstring += "B.PRICE_REQ AS Price_Req, \n";
                strSQLstring += "NVL(D.ISA_SELL_PRICE, 0) as Sell_Price,\n";
                strSQLstring += "B.VENDOR_ID as Vendor_ID,\n";
                strSQLstring += "NVL(V.NAME1, ' ') as Vendor_Name,\n";
                strSQLstring += "decode(B.CURR_STATUS, 'H', 'Vendor Info', 'D', 'Customer Info', 'P', 'Pending Buyer') as Problem_Code,\n";
                strSQLstring += "A.HOLD_STATUS AS Req_Hold_Flag,\n";
                //strSQLstring += "TO_CHAR(CAST((D.ADD_DTTM)AS TIMESTAMP), 'YYYY-MM-DD-HH24.MI.SS.FF') AS Req_Line_Add_Date,\n";
                strSQLstring += "NVL(D.ADD_DTTM, A.REQ_DT) AS Req_Line_Add_Date,\n";
                strSQLstring += "B.MFG_ID as Manufacturer, \n";
                strSQLstring += "B.MFG_ITM_ID as Manufacturer_Part_Number, \n";
                strSQLstring += "B.DESCR254_MIXED AS Description,  \n";
                strSQLstring += "NVL(to_char(K.COMMENTS_2000), ' ') AS Requisition_Comments,\n";
                strSQLstring += "NVL(D.ISA_CUST_CHARGE_CD, ' ') AS Charge_Code, \n";
                strSQLstring += "NVL(D.ISA_WORK_ORDER_NO, ' ') AS Workorder,\n";
                strSQLstring += "NVL(Q.ISA_PRIORITY_FLAG, ' ') AS Priority_Flag,\n";
                strSQLstring += "NVL(TRUNC(SYSDATE) - TRUNC(D.ADD_DTTM), 0) AS Status_Age,\n";
                strSQLstring += "U.url || '/EMPLOYEE/ERP/c/REQUISITION_ITEMS.REQUISITIONS.GBL?Page=REQ_FORM&Action=U&BUSINESS_UNIT=' || B.BUSINESS_UNIT || '&REQ_ID=' || B.REQ_ID || '&TargetFrameName=None' as PS_URL,\n";
                strSQLstring += "NVL(P.BUYER_MANAGER, ' ') AS Buyer_Team,\n";
                strSQLstring += "' ' as PROCESS_FLAG\n";

                strSQLstring += "FROM SYSADM8.PS_REQ_HDR A,\n";
                strSQLstring += "SYSADM8.PS_ISA_REQ_HDR_CMT K,\n";
                strSQLstring += "SYSADM8.PS_REQ_LINE B,\n";
                strSQLstring += "SYSADM8.PS_REQ_LINE_SHIP C,\n";
                strSQLstring += "SYSADM8.PS_ISA_ORD_INTF_LN D,\n";
                strSQLstring += "SYSADM8.PS_REQ_LN_DISTRIB E,\n";
                strSQLstring += "SYSADM8.PS_DEPT_TBL G,\n";
                strSQLstring += "SYSADM8.PS_DEPT_TBL T,\n";
                strSQLstring += "SYSADM8.PS_ISA_REQ_BI_INFO Q,\n";
                strSQLstring += "SYSADM8.PS_PO_MANAGER_TBL P,\n";
                strSQLstring += "SYSADM8.PS_VENDOR V,\n";
                strSQLstring += "SYSADM8.ps_PTSF_URLDEFN_VW U \n";

                strSQLstring += "WHERE A.BUSINESS_UNIT IN('ISA00','SDM00','CST00','SDC00')\n";
                strSQLstring += "and A.BUSINESS_UNIT = K.BUSINESS_UNIT(+)\n";
                strSQLstring += "AND A.REQ_ID = K.REQ_ID(+)\n";
                strSQLstring += "AND A.REQ_DT > TO_DATE('2017-01-01', 'YYYY-MM-DD')\n";
                strSQLstring += "AND A.BUSINESS_UNIT = B.BUSINESS_UNIT\n";
                strSQLstring += "AND A.REQ_ID = B.REQ_ID\n";
                strSQLstring += "AND B.CURR_STATUS IN('D','H','O','P')\n";
                strSQLstring += "AND B.BUSINESS_UNIT = C.BUSINESS_UNIT\n";
                strSQLstring += "AND B.REQ_ID = C.REQ_ID\n";
                strSQLstring += "AND B.LINE_NBR = C.LINE_NBR\n";
                strSQLstring += "AND C.BUSINESS_UNIT = E.BUSINESS_UNIT\n";
                strSQLstring += "AND C.REQ_ID = E.REQ_ID\n";
                strSQLstring += "AND C.LINE_NBR = E.LINE_NBR\n";
                strSQLstring += "AND C.SCHED_NBR = E.SCHED_NBR\n";
                strSQLstring += "and e.qty_open > 0\n";
                strSQLstring += "AND E.BUSINESS_UNIT = Q.BUSINESS_UNIT(+)\n";
                strSQLstring += "AND E.REQ_ID = Q.REQ_ID(+)\n";
                strSQLstring += "AND E.LINE_NBR = Q.LINE_NBR(+)\n";
                strSQLstring += "and Q.Business_unit_om = D.business_unit_om(+)\n";
                strSQLstring += "and Q.REQ_ID = D.ORDER_NO(+)\n";
                strSQLstring += "AND Q.LINE_NBR = D.ISA_INTFC_LN(+)\n";
                strSQLstring += "And (d.isa_line_status not in ('RFA','RFR','RFX', 'RFC', 'RFY', 'RFZ', 'QTS', 'QTW', 'QTC', 'CNC', 'QTR',' ','MR')\n";
                strSQLstring += "or d.isa_line_status is null)\n";
                strSQLstring += "AND G.SETID = 'MAIN1'\n";
                strSQLstring += "AND G.DEPTID = E.DEPTID\n";
                strSQLstring += "AND G.EFFDT = (SELECT MAX(G_ED.EFFDT)\n";
                strSQLstring += "                            FROM SYSADM8.PS_DEPT_TBL G_ED\n";
                strSQLstring += "                            WHERE G.SETID = G_ED.SETID\n";
                strSQLstring += "                            AND G.DEPTID = G_ED.DEPTID)\n";
                strSQLstring += "AND B.VENDOR_ID = V.VENDOR_ID(+)\n";
                strSQLstring += "and B.BUYER_ID = P.BUYER_ID(+)\n";
                strSQLstring += "AND e.DEPTID = T.DEPTID\n";
                strSQLstring += "AND U.url_id = 'EMP_SERVLET')";

                m_oLogger.LogMessage("CreateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("CreateTable", "Query To create the ReqReview temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                strSQLstring = "SELECT Process_Flag from SDIX_REQREVIEWTEMP";
                dtResponse = oleDBExecuteReader(strSQLstring);
                dtResponseRowsCount = dtResponse.Rows.Count;

                m_oLogger.LogMessage("CreateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("CreateTable", "Error trying to create the ReqReview temp data table.", ex);
            }

        }

        public void UpdateTable(Logger m_oLogger)
        {
            try
            {
                strSQLstring = "UPDATE SDIX_REQREVIEWTEMP SET Process_Flag = 'X' Where Process_Flag <> 'X' And rownum < " + (oracleSendLimit + 1);
                m_oLogger.LogMessage("UpdateTable", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("UpdateTable", "Query To update the ReqReview temp data table: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);

                m_oLogger.LogMessage("UpdateTable", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("UpdateTable", "Error trying to update the ReqReview temp data table.", ex);
            }

        }



        public DataTable getReqReviewData(Logger m_oLogger)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                strSQLstring = "SELECT * FROM SDIX_REQREVIEWTEMP\n";
                strSQLstring += "where process_flag <> 'X' and rownum < " + (oracleSendLimit + 1);

                m_oLogger.LogMessage("getReqReviewData", "PeopleSoft connection string : " + OracleConString);
                m_oLogger.LogMessage("getReqReviewData", "Query To get the ReqReview data: " + strSQLstring);
                dtResponse = oleDBExecuteReader(strSQLstring);
                m_oLogger.LogMessage("getReqReviewData", "Number of rows Selected " + dtResponse.Rows.Count);

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("getMatchExcepData", "Error trying to get the ReqReview data.", ex);
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

        public REQData getData(Logger m_oLogger)
        {
            //Logger m_oLogger;
            //string sLogPath = Environment.CurrentDirectory;
            //if (!sLogPath.EndsWith(@"\"))
            //    sLogPath += @"\";
            //sLogPath += "Logs";
            //m_oLogger = new Logger(sLogPath, "MatchExcepReload");
            m_oLogger.LogMessage("BatchReqReview", "Entered BatchReqReview class");

            req.ACTION_ITEM.Clear();
            req.CLIENT .Clear();
            req.SITE_NAME .Clear();
            req.SHIP_TO_ID .Clear();
            req.REQUESTOR_ID .Clear();
            req.BUYER_ID .Clear();
            req.REQUISITION_DATE .Clear();
            req.REQUISITION_ID .Clear();
            req.LINE_NUMBER .Clear();
            req.INVENTORY_ITEM_ID .Clear();
            req.QTY_REQ .Clear();
            req.QTY_OPEN .Clear();
            req.UNIT_OF_MEASURE .Clear();
            req.PRICE_REQ .Clear();
            req.SELL_PRICE .Clear();
            req.VENDOR_ID .Clear();
            req.VENDOR_NAME.Clear();
            req.PROBLEM_CODE .Clear();
            req.REQ_HOLD_FLAG .Clear();
            req.REQ_LINE_ADD_DATE .Clear();
            req.MANUFACTURER .Clear();
            req.MANUFACTURER_PART_NUMBER .Clear();
            req.DESCRIPTION .Clear();
            req.REQUISITION_COMMENTS .Clear();
            req.CHARGE_CD .Clear();
            req.WORKORDER .Clear();
            req.PRIORITY_FLAG .Clear();
            req.STATUS_AGE .Clear();
            req.PS_URL.Clear();
            req.BUYER_TEAM.Clear();

            //STEP #3 - QUERY TABLE AND POST NEW DATA 
            m_oLogger.LogMessage("ReqReviewReload", "Query table started");
            //MatchExcepReloadDAL objGetMatchExcepReloadDAL = new MatchExcepReloadDAL();
            //dtResponse = objGetMatchExcepReloadDAL.getMatchExcepData(m_oLogger);
            dtResponse = getReqReviewData(m_oLogger);
            dtResponseRowsCount = dtResponse.Rows.Count;

            if (dtResponseRowsCount < oracleSendLimit)
            {
                gotAllData = "Y";
            }

            if (dtResponseRowsCount == 0)
            {
                m_oLogger.LogMessage("ReqReviewReload", "Query returned no records.");
                return null; ;
            }
            else
                m_oLogger.LogMessage("ReqReviewReload", "POST ReqReviewReload data started.");
            for (int i = 0; i < dtResponseRowsCount; i++)
            {
                DataRow rowInit;
                rowInit = dtResponse.Rows[i];

                try
                {
                    req.ACTION_ITEM.Add(rowInit["ACTION_ITEM"].ToString());
                    req.CLIENT.Add(rowInit["CLIENT"].ToString());
                    req.SITE_NAME.Add(rowInit["SITE_NAME"].ToString());
                    req.SHIP_TO_ID .Add(rowInit["SHIP_TO_ID"].ToString());
                    req.REQUESTOR_ID .Add(rowInit["REQUESTOR_ID"].ToString());
                    req.BUYER_ID .Add(rowInit["BUYER_ID"].ToString());
                    req.REQUISITION_DATE .Add(Convert.ToDateTime(rowInit["REQUISITION_DATE"]));
                    req.REQUISITION_ID .Add(rowInit["REQUISITION_ID"].ToString());
                    req.LINE_NUMBER .Add(rowInit["LINE_NUMBER"].ToString());
                    req.INVENTORY_ITEM_ID .Add(rowInit["INVENTORY_ITEM_ID"].ToString ());
                    req.QTY_REQ .Add(Convert.ToInt32 ( rowInit["QTY_REQ"]));
                    req.QTY_OPEN .Add(Convert.ToInt32( rowInit["QTY_OPEN"]));
                    req.UNIT_OF_MEASURE .Add(rowInit["UNIT_OF_MEASURE"].ToString());
                    req.PRICE_REQ .Add(rowInit["PRICE_REQ"].ToString());
                    req.SELL_PRICE .Add(rowInit["SELL_PRICE"].ToString());
                    req.VENDOR_ID .Add(rowInit["VENDOR_ID"].ToString());
                    req.VENDOR_NAME .Add(rowInit["VENDOR_NAME"].ToString());
                    req.PROBLEM_CODE .Add(rowInit["PROBLEM_CODE"].ToString());
                    req.REQ_HOLD_FLAG .Add(rowInit["REQ_HOLD_FLAG"].ToString());
                    req.REQ_LINE_ADD_DATE .Add(Convert.ToDateTime( rowInit["REQ_LINE_ADD_DATE"]));
                    req.MANUFACTURER .Add(rowInit["MANUFACTURER"].ToString());
                    req.MANUFACTURER_PART_NUMBER .Add(rowInit["MANUFACTURER_PART_NUMBER"].ToString());
                    req.DESCRIPTION .Add(rowInit["DESCRIPTION"].ToString());
                    req.REQUISITION_COMMENTS .Add(rowInit["REQUISITION_COMMENTS"].ToString());
                    req.CHARGE_CD .Add(rowInit["CHARGE_CODE"].ToString());
                    req.WORKORDER .Add(rowInit["WORKORDER"].ToString());
                    req.PRIORITY_FLAG .Add(rowInit["PRIORITY_FLAG"].ToString());
                    req.STATUS_AGE .Add(Convert.ToInt32 ( rowInit["STATUS_AGE"]));
                    req.PS_URL.Add(rowInit["PS_URL"].ToString());
                    req.BUYER_TEAM.Add(rowInit["BUYER_TEAM"].ToString());

                }
                catch (Exception ex)
                {
                    m_oLogger.LogMessage("ReqReviewReload", "Error trying to parse data at line " + i.ToString(), ex);

                }

            }

            m_oLogger.LogMessage("ReqReviewReload", "Query table and parse successful.");
            return req;

        }


    }
}
