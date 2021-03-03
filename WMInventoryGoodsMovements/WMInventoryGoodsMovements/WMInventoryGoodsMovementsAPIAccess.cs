using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WMInventoryGoodsMovements;
using System.Configuration;
using System.IO;

namespace WMInventoryGoodsMovements
{
    class WMInventoryGoodsMovementsAPIAccess
    {
        /// <summary>
        /// POST Receiving data to the Solvay service
        /// </summary>
        /// <returns></returns>
        public string postWMInventoryGoodsMovementsData(Logger m_oLogger)
        {
            string testOrProd = " ";
            string authorization = " ";
            string serviceURL = " ";
            var strResponse = " ";
            string responseErrorText = " ";
            string RCVPOR = " ";
            string RCVPRN = " ";
            string SNDPRN = " ";
            string processFlag = " ";

            testOrProd = ConfigurationManager.AppSettings["TestOrProd"];
            if (testOrProd == "TEST")
            {
                serviceURL = ConfigurationManager.AppSettings["testServiceURL"];
                authorization = ConfigurationManager.AppSettings["testAuthorization"];
                SNDPRN = "FRPARWM1";
                RCVPOR = "SAPWQ1";
                RCVPRN = "WQ1_400";
            }
            else if (testOrProd == "PROD")
            {
                serviceURL = ConfigurationManager.AppSettings["prodServiceURL"];
                authorization = ConfigurationManager.AppSettings["prodAuthorization"];
                SNDPRN = "FRPARWM1";
                RCVPOR = "SAPWP1";
                RCVPRN = "WP1_400";
            }

            DataTable dtResponse = new DataTable();
            DataTable dtResponse2 = new DataTable();
            try
            {

                WMInventoryGoodsMovementsDAL objWMInventoryGoodsMovementsDAL = new WMInventoryGoodsMovementsDAL();
                m_oLogger.LogMessage("getWMInventoryGoodsMovementsData", "Getting WM Inventory Goods Movements Data starts here");
                dtResponse = objWMInventoryGoodsMovementsDAL.getWMInventoryGoodsMovementsData(m_oLogger);
                dtResponse2.Columns.Add("Line");

                DataRow rowInit2 = dtResponse2.NewRow();
                dtResponse2.Rows.Add(rowInit2);

                if (dtResponse.Rows.Count != 0)
                {

                    for (int i = 0; i <= dtResponse.Rows.Count - 1; i++)
                    {

                        DataRow rowInit;
                        rowInit = dtResponse.Rows[i];

                        string ISA_IDENTIFIER = rowInit["ISA_IDENTIFIER"].ToString();
                        string DOC_NUM = rowInit["ISA_IDENTIFIER"].ToString();
                        DOC_NUM = DOC_NUM.PadLeft(14, '0');//i.e. "0000000000000004"
                        string LOGDAT = System.DateTime.Now.ToString("yyyyMMdd");
                        string LOGTIM = System.DateTime.Now.ToString("HHmmss");
                        string REFGRP = rowInit["PLANT"].ToString();
                        string REFMES = DOC_NUM; //rowInit["ISA_IDENTIFIER"].ToString();
                        DateTime PSTNG_DATEcnv = Convert.ToDateTime(rowInit["DT_TIMESTAMP"]);
                        string PSTNG_DATE = PSTNG_DATEcnv.ToString("yyyyMMdd");
                        string DOC_DATE = PSTNG_DATEcnv.ToString("yyyyMMdd");
                        string REF_DOC_NO = rowInit["REFERENCE_ID"].ToString();                              //?
                        //string GM_CODE = "03";
                        string MATERIAL = rowInit["ISA_ITEM"].ToString();
                        string PLANT = REFGRP;
                        string STGE_LOC = rowInit["STORAGE_AREA"].ToString();
                        string MOVE_TYPE = rowInit["TRANS_TYPE"].ToString();
                        string VENDOR = rowInit["VENDOR_ID"].ToString();
                        string ENTRY_QNT = rowInit["QTY"].ToString();
                        string ENTRY_UOM_ISO = rowInit["ISA_CUSTOMER_UOM"].ToString();
                        string PO_NUMBER = rowInit["ISA_CUST_PO_NBR"].ToString();
                        string PO_ITEM = rowInit["ISA_SAP_PO_LN"].ToString();
                        string RESERV_NO = rowInit["ORDER_NO"].ToString();
                        string RES_ITEM = rowInit["ORDER_INT_LINE_NO"].ToString();
                        string COSTCENTER = rowInit["ISA_COST_CENTER"].ToString();
                        string ORDERID = rowInit["ISA_WORK_ORDER_NO"].ToString();
                        string ITEM_TEXT = "";
                        string WITHDRAWN = "";
                        string MOVE_STLOC = "";
                        string MVT_IND = "B";
                        //string MOVE_REAS = "0999";

                        string MOVE_REAS = rowInit["ISA_SAP_RJCT_CODE"].ToString();
                        string ST_UN_QTYY_1 = rowInit["EXTENDED_UNIT_COST"].ToString();
                        string ST_UN_QTYY_1_ISO = rowInit["ISA_CUSTOMER_UOM"].ToString();

                        //string WBS_ELEM = "";
                        string WBS_ELEM = rowInit["ISA_WBS_ELMNT"].ToString();
                        string REF_DOC = rowInit["REF_DOCUMENT_ID"].ToString();

                        string BILL_OF_LADING = ""; //unused?
                        string GR_GI_SLIP_NO = "";  //unused?
                        string HEADER_TXT = "";     //unused?
                        string BATCH = "";          //unused?
                        string NO_MORE_GR = "";     //unused?

                        StringBuilder sbInit = new StringBuilder();
                        string xmlStr = string.Empty;
                        string xmlStringInit = string.Empty;
                        //using (StreamReader sr = new StreamReader(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "/ZWIM_MBGMCR2-oneline-mapping3.xml"))
                        string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                        if (MOVE_TYPE != "101")
                        {

                            if (MOVE_TYPE == "261")
                            {
                                using (StreamReader sr = new StreamReader(dir + "/Sample1/SAMPLE_137876614.xml"))
                                {
                                    xmlStr = sr.ReadToEnd();
                                    if (RESERV_NO == " ")
                                    {
                                        RESERV_NO = "";
                                    }
                                    else
                                    {
                                        ORDERID = "";
                                    }
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, RES_ITEM, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, COSTCENTER, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else if (MOVE_TYPE == "201")
                            {
                                using (StreamReader sc = new StreamReader(dir + "/Sample3/SAMPLE_137877229.xml"))
                                {
                                    xmlStr = sc.ReadToEnd();
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, RES_ITEM, COSTCENTER, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else if (MOVE_TYPE == "202")
                            {
                                using (StreamReader sc = new StreamReader(dir + "/Sample6/SAMPLE_202.xml"))
                                {
                                    xmlStr = sc.ReadToEnd();
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, RES_ITEM, COSTCENTER, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else if (MOVE_TYPE == "222")
                            {
                                using (StreamReader sc = new StreamReader(dir + "/Sample10/SAMPLE_222.xml"))
                                {
                                    xmlStr = sc.ReadToEnd();
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, RES_ITEM, WBS_ELEM, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else if (MOVE_TYPE == "262")
                            {
                                using (StreamReader sc = new StreamReader(dir + "/Sample8/SAMPLE_262.xml"))
                                {
                                    xmlStr = sc.ReadToEnd();
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, RES_ITEM, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else if (MOVE_TYPE == "221")
                            {
                                using (StreamReader sr = new StreamReader(dir + "/Sample5/SAMPLE_221.xml"))
                                {
                                    xmlStr = sr.ReadToEnd();
                                    if (RESERV_NO == " ")
                                    {
                                        RESERV_NO = "";
                                    }
                                    else
                                    {
                                        WBS_ELEM = "";
                                    }
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, RES_ITEM, WBS_ELEM, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, COSTCENTER, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else if (MOVE_TYPE == "701")
                            {
                                using (StreamReader sc = new StreamReader(dir + "/Sample7/701_INVENTORY_SDI.xml"))
                                {
                                    xmlStr = sc.ReadToEnd();
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, MOVE_REAS, ST_UN_QTYY_1, ST_UN_QTYY_1_ISO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else if (MOVE_TYPE == "702")
                            {
                                using (StreamReader sc = new StreamReader(dir + "/Sample9/702_INVENTORY_SDI.xml"))
                                {
                                    xmlStr = sc.ReadToEnd();
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, MOVE_REAS, ST_UN_QTYY_1, ST_UN_QTYY_1_ISO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else if (MOVE_TYPE == "122")                              //122
                            {
                                using (StreamReader sd = new StreamReader(dir + "/Sample4/SAMPLE_137877278.xml"))
                                {
                                    xmlStr = sd.ReadToEnd();
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                                    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, COSTCENTER, SNDPRN, RCVPOR, RCVPRN);
                                    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                                    xmlStringInit = sbInit.ToString();
                                }
                            }
                            else
                            {

                            }

                            //using (StreamReader sa = new StreamReader(dir + "/Sample2/SAMPLE_137877226.xml"))                        
                            //{
                            //    xmlStr = sa.ReadToEnd();
                            //    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, RESERV_NO, SNDPRN, RCVPOR, RCVPRN);
                            //    sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, ORDERID, SNDPRN, RCVPOR, RCVPRN);
                            //    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, STGE_LOC, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, COSTCENTER, SNDPRN, RCVPOR, RCVPRN);
                            //    //sbInit.AppendFormat(xmlStr, DOC_NUM, LOGDAT, LOGTIM, REFGRP, REFMES, PSTNG_DATE, DOC_DATE, REF_DOC_NO, MATERIAL, PLANT, MOVE_TYPE, ENTRY_QNT, ENTRY_UOM_ISO, PO_NUMBER, PO_ITEM, SNDPRN, RCVPOR, RCVPRN);
                            //    xmlStringInit = sbInit.ToString();
                            //}

                            if (xmlStringInit != "")
                            {

                                List<WMInventoryGoodsMovementsBO> target = dtResponse2.AsEnumerable()
                                    .Select(row => new WMInventoryGoodsMovementsBO
                                    {
                                        Organization = "SolvaySDI",
                                        SharedSecret = "SolvaySDI",
                                        TimeStamp = System.DateTime.Now.ToString(),
                                        IDOC_TYPE = "MBGMCR03",
                                        xmlString = xmlStringInit

                                    //XXPMC_SDI_RECORD_ID = ((Decimal)(row["ISA_IDENTIFIER"])).ToString(),
                                    //PROCESSING_STATUS_CODE = ReplacePipe((String)(row["STATUS_DESCR"])),
                                    //RECEIPT_SOURCE_CODE = ReplacePipe((String)(row["RECEIPT_SOURCE"])),
                                    //// TRANSACTION_TYPE = ReplacePipe((String)(row["TRANS_TYPE"])),
                                    //HEADER_TRANSACTION_TYPE = ReplacePipe((String)(row["HDR_TRANS_TYPE"])),
                                    //VENDOR_ID = ReplacePipe((String)(row["ISA_VENDOR_NUM"])),
                                    //EXPECTED_RECEIPT_DATE = ((DateTime)(row["ISA_RECEIVING_DATE"])).ToString("yyyy/MM/dd HH:mm:ss"),
                                    //VALIDATION_FLAG = ReplacePipe((String)(row["VALID_FLAG"])),
                                    //TRANSACTION_DATE = ((DateTime)(row["TRANSACTION_DATE"])).ToString("yyyy/MM/dd HH:mm:ss"),
                                    //PROCESSING_MODE_CODE = ReplacePipe((String)(row["PROC_DESCR"])),
                                    ////STATUS = ReplacePipe((String)(row["STATUS1"])),
                                    //EBS_PO_NUMBER = ReplacePipe((String)(row["ISA_CUST_PO_ID"])),
                                    //EBS_PO_LINE_NUMBER = ReplacePipe((String)(row["CUSTOMER_PO_LINE"])),
                                    //LINE_TRANSACTION_TYPE = ReplacePipe((String)(row["ISA_TRANS_NAME"])),
                                    //ITEM = ReplacePipe((String)(row["ISA_ITEM"])),
                                    //ITEM_ID = ReplacePipe((String)(row["CUSTOMER_ITEM_NBR"])),
                                    //QUANTITY = ((Decimal)(row["QTY"])).ToString(),
                                    //UNIT_OF_MEASURE = ReplacePipe((String)(row["ISA_CUSTOMER_UOM"])),
                                    //EBS_PO_LINE_LOC_NBR = ReplacePipe((String)(row["ISA_LOCATOR_ID"])),
                                    //AUTO_TRANSACT_CODE = ReplacePipe((String)(row["ISA_AUTO_TRANS_CD"])),
                                    //TO_ORGANIZATION_CODE = ReplacePipe((String)(row["PLANT"])),
                                    //SOURCE_DOCUMENT_CODE = ReplacePipe((String)(row["SOURCE_DOC"])),
                                    //DOCUMENT_NUM = ReplacePipe((String)(row["PO_ID"])),
                                    //DESTINATION_TYPE_CODE = ReplacePipe((String)(row["ISA_DEST_TYPE_CODE"])),
                                    //DELIVER_TO_PERSON_ID = ReplacePipe((String)(row["RECIPIENT"])),
                                    //DELIVER_TO_LOCATION_CODE = ReplacePipe((String)(row["ISA_UNLOADING_PT"])),
                                    //DELIVER_TO_LOCATION_ID = ReplacePipe((String)(row["DELIVERY_OPT"])),
                                    //SUBINVENTORY = ReplacePipe((String)(row["SUB_ITEM_ID"])),
                                    //WIP_ENTITY_ID = ReplacePipe((String)(row["ISA_WORK_ORDER"])),
                                    //WIP_ENTITY_NAME = ReplacePipe((String)(row["ORDER_NO"])),
                                    //WIP_OPERATION_SEQ_NUM = ReplacePipe((String)(row["ACTIVITY_ID"])),
                                    //ATTRIBUTE1 = ReplacePipe((String)(row["ISA_ATTRIBUTE_1"])),
                                    //ATTRIBUTE2 = ReplacePipe((String)(row["ISA_ATTRIBUTE_2"])),
                                    //ATTRIBUTE3 = ReplacePipe((String)(row["ISA_ATTRIBUTE_3"])),
                                    //ATTRIBUTE4 = ReplacePipe((String)(row["ISA_ATTRIBUTE_4"])),
                                    //ATTRIBUTE5 = ReplacePipe((String)(row["ISA_ATTRIBUTE_5"])),
                                    //ATTRIBUTE6 = ReplacePipe((String)(row["ISA_ATTRIBUTE_6"])),
                                    //ATTRIBUTE7 = ReplacePipe((String)(row["ISA_ATTRIBUTE_7"])),
                                    //ATTRIBUTE8 = ReplacePipe((String)(row["ISA_ATTRIBUTE_8"])),
                                    //ATTRIBUTE9 = ReplacePipe((String)(row["ISA_ATTRIBUTE_9"])),
                                    //ATTRIBUTE10 = ReplacePipe((String)(row["ISA_ATTRIBUTE_10"])),
                                    //TRANS_STATUS_DESCRIPTION = row["ISA_COMMENTS_1333"] == DBNull.Value ? null : ReplacePipe((String)(row["ISA_COMMENTS_1333"])),
                                    //TRANSACTION_STATUS = ReplacePipe((String)(row["STATUS_MSG"])),
                                    //TRANSACTION_STATUS_CODE = "PENDING"
                                }).ToList();




                                //string jsontest = JsonConvert.SerializeObject(new
                                //{
                                //    _postwmreceipt = target
                                //});
                                string jsontest = JsonConvert.SerializeObject(target, Formatting.None);
                                //jsontest = jsontest.Remove(0, 5);
                                //jsontest = jsontest.Remove(jsontest.Length - 3);
                                jsontest = jsontest.Remove(0, 1);
                                jsontest = jsontest.Remove(jsontest.Length - 1);

                                StringBuilder sb = new StringBuilder();
                                //sb.Append("{'_postwmreceipt_batch_req':");
                                //sb.Append("{");
                                sb.Append(jsontest);
                                //sb.Append("}");

                                //JObject resultSet = JObject.Parse(sb.ToString());
                                string resultSet = jsontest;

                                //JObject resultSet = JObject.Parse(jsonSampleData);
                                using (var client = new WebClient())
                                {
                                    //testOrProd = ConfigurationManager.AppSettings["TestOrProd"];
                                    //if (testOrProd == "TEST")
                                    //{
                                    //    serviceURL = ConfigurationManager.AppSettings["testServiceURL"];
                                    //    authorization = ConfigurationManager.AppSettings["testAuthorization"];
                                    //}
                                    //else
                                    //{
                                    //    serviceURL = ConfigurationManager.AppSettings["prodServiceURL"];
                                    //    authorization = ConfigurationManager.AppSettings["prodAuthorization"];
                                    //}


                                    m_oLogger.LogMessage("postWMInventoryGoodsMovementsData", "POST WM Inventory Goods Movements to Solvay starts here");
                                    m_oLogger.LogMessage("postWMInventoryGoodsMovementsData", "POST WM Inventory Goods Movements Data" + resultSet.ToString());
                                    //m_oLogger.LogMessage("postWMReceiptMappingData", "POST WMMapping Data URL : https://10.118.13.27:8243/SDIOutboundWMReceiptAPI/v1_0");
                                    m_oLogger.LogMessage("postWMInventoryGoodsMovementsData", "POST WM Inventory Goods Movements Data URL : " + serviceURL);

                                    try
                                    {
                                        string basicAuthBase641;
                                        basicAuthBase641 = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", authorization, authorization)));
                                        //req.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641))
                                        client.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641));
                                        //client.Headers.Add("Authorization: Basic " + authorization);
                                        client.Headers.Add("Content-Type:application/json");
                                    }
                                    catch (WebException ex)
                                    {
                                        m_oLogger.LogMessage("postWMInventoryGoodsMovementsData", "Authenication failed " + ex.Message);
                                    }

                                    //client.Headers.Add("Accept:application/json");
                                    //System.Net.ServicePointManager.CertificatePolicy = new AlwaysIgnoreCertPolicy();
                                    //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                                    //var result = client.UploadString("https://10.118.13.27:8243/SDIOutboundWMReceiptAPI/v1_0", resultSet.ToString());
                                    var result = client.UploadString(serviceURL, resultSet.ToString());

                                    // Console.WriteLine(result);
                                    var parsed = JObject.Parse(result);
                                    strResponse = parsed.SelectToken("RequestStatus").Value<string>();

                                    m_oLogger.LogMessage("postWMInventoryGoodsMovementsData", "POST WMInventoryGoodsMovements data to Solvay server status " + strResponse);

                                    //if (strResponse.ToUpper() != "SUCCESS")
                                    //{
                                    //    break;
                                    //}

                                    if (strResponse.ToUpper() != " ")
                                    {

                                        if (strResponse.ToUpper() == "SUCCESS")
                                        {
                                            processFlag = "I";
                                        }
                                        else
                                        {
                                            processFlag = "E"; //error
                                        }
                                        objWMInventoryGoodsMovementsDAL.UpdateWMInventoryGoodsMovementsData(m_oLogger, processFlag, ISA_IDENTIFIER);
                                    }
                                    else
                                    {
                                        m_oLogger.LogMessageWeb("postWMInventoryGoodsMovementsData", "Error trying to POST data to Solvay server.", responseErrorText); //ex
                                    }
                                    // strResponse = JsonConvert.SerializeObject(result);
                                }
                            }
                        }
                        else
                        {
                            m_oLogger.LogMessage("postWMInventoryGoodsMovementsData", "The transaction is belong to 101");
                        }
                    }
                }

            }

            //catch (Exception ex)
            catch (WebException ex)
            {

                var responseStream = ex.Response.GetResponseStream();
                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseErrorText = reader.ReadToEnd();
                    }
                }

                m_oLogger.LogMessageWeb("postWMInventoryGoodsMovementsData", "Error trying to POST data to Solvay server.", responseErrorText); //ex
            }
            return strResponse;
        }


        public string ReplacePipe(string x)
        {
            x = x.Trim();
            return x == "|" ? "" : x;
        }

    }
}

