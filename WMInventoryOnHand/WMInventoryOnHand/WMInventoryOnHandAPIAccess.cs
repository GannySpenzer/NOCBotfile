using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WMInventoryOnHand;
using System.Configuration;
using System.IO;

namespace WMInventoryOnHand
{
    class WMInventoryOnHandAPIAccess
    {
        /// <summary>
        /// POST Receiving data to the Solvay service
        /// </summary>
        /// <returns></returns>
        public string postWMInventoryOnHandData(Logger m_oLogger)
        {
            string testOrProd = " ";
            string authorization = " ";
            string serviceURL = " ";
            var strResponse = " ";
            string responseErrorText = " ";
            string RCVPOR = " ";
            string RCVPRN = " ";
            string SNDPRN = " ";

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
            try
            {

                WMInventoryOnHandReDAL objWMInventoryOnHandReDAL = new WMInventoryOnHandReDAL();
                m_oLogger.LogMessage("getWMInventoryOnHandData", "Getting WM Inventory On Hand Data starts here");
                dtResponse.Columns.Add("DOC_NUM");
                //dtResponse = objWMInventoryOnHandReDAL.getWMInventoryOnHandData(m_oLogger);
                //if (dtResponse.Rows.Count != 0)
                //{
                //DataTable table = new DataTable();
                DataRow rowInit = dtResponse.NewRow();
                dtResponse.Rows.Add(rowInit);
                int noOfTimes = 2;
                for (int j = 0; j < noOfTimes; j++)
                {


                    //       for (int i = 0; i <= dtResponse.Rows.Count - 1; i++)
                    //      {

                    //    DataRow rowInit;
                    //    rowInit = dtResponse.Rows[0];
                    //}

                    //string DOC_NUM = rowInit["ISA_IDENTIFIER"].ToString();
                    //DOC_NUM = DOC_NUM.PadLeft(14, '0');//i.e. "0000000000000004"
                    string LOGDAT = System.DateTime.Now.ToString("yyyyMMdd");
                    string LOGTIM = System.DateTime.Now.ToString("HHmmss");

                    //string REFGRP = rowInit["PLANT"].ToString();
                    //string REFMES = DOC_NUM; //rowInit["ISA_IDENTIFIER"].ToString();
                    //string WERKS = REFGRP;

                    //string MATNR = rowInit["ISA_ITEM"].ToString();
                    string[] WERKS = { "8192", "8894" };
                    string[] REFGRP = { "8192", "8894" };

                    //DateTime PSTNG_DATEcnv = Convert.ToDateTime(rowInit["DT_TIMESTAMP"]);
                    //string PSTNG_DATE = PSTNG_DATEcnv.ToString("yyyyMMdd");
                    //string DOC_DATE = PSTNG_DATEcnv.ToString("yyyyMMdd");

                    /*string REF_DOC_NO = rowInit["REFERENCE_ID"].ToString(); */                             //?

                    //string GM_CODE = "03";


                    //string STGE_LOC = rowInit["STORAGE_AREA"].ToString();
                    //string MOVE_TYPE = rowInit["TRANS_TYPE"].ToString();
                    //string VENDOR = rowInit["VENDOR_ID"].ToString();
                    //string ENTRY_QNT = rowInit["QTY"].ToString();
                    //string ENTRY_UOM_ISO = rowInit["ISA_CUSTOMER_UOM"].ToString();
                    //string PO_NUMBER = rowInit["ISA_CUST_PO_NBR"].ToString();
                    //string PO_ITEM = rowInit["ISA_SAP_PO_LN"].ToString();
                    //string RESERV_NO = rowInit["ORDER_NO"].ToString();
                    //string RES_ITEM = rowInit["ORDER_INT_LINE_NO"].ToString();
                    //string COSTCENTER = rowInit["ISA_COST_CENTER"].ToString();
                    //string ORDERID = rowInit["ISA_WORK_ORDER_NO"].ToString();
                    //string ITEM_TEXT = "";
                    //string WITHDRAWN = "";
                    //string MOVE_STLOC = "";
                    //string MVT_IND = "B";
                    //string MOVE_REAS = "0999";
                    ////string WBS_ELEM = "";
                    //string WBS_ELEM = rowInit["ISA_WBS_ELMNT"].ToString();
                    //string REF_DOC = rowInit["REF_DOCUMENT_ID"].ToString();

                    //string BILL_OF_LADING = ""; //unused?
                    //string GR_GI_SLIP_NO = "";  //unused?
                    //string HEADER_TXT = "";     //unused?
                    //string BATCH = "";          //unused?
                    //string NO_MORE_GR = "";     //unused?

                    //string datastr = "{";
                    //string datastr2 = ":";
                    //string datastr3 = ",";
                    //string datastr4 = "\"";
                    //string datastr5 = "}";
                    //string Organization = "Organization";
                    //string SolvaySDI = "SolvaySDI";
                    //string SharedSecret = "SharedSecret";
                    //string TimeStamp = "TimeStamp";
                    //string TimeStamp1 = System.DateTime.Now.ToString();
                    //string IDOC_TYPE = "IDOC_TYPE";
                    //string MBGMCR03 = "MBGMCR03";
                    //string xmlstr = "xmlString";

                    //string fulldata = datastr + datastr4 + Organization + datastr4 + datastr2 + datastr4 + SolvaySDI + datastr4 + datastr3 + datastr4 + SharedSecret + datastr4 + datastr2 + datastr4 + SolvaySDI + datastr4 + datastr3 + datastr4 + TimeStamp + datastr4 + datastr2 + datastr4 + TimeStamp1 + datastr4 + datastr3 + datastr4 + IDOC_TYPE + datastr4 + datastr2 + datastr4 + MBGMCR03 + datastr4 + datastr3 + datastr4 + xmlstr + datastr4 + datastr2;
                    //string fulldata2 = datastr4 + datastr5;



                    StringBuilder sbInit = new StringBuilder();
                    string xmlStr = string.Empty;
                    string xmlStringInit = string.Empty;
                    string xmlStringInit1 = string.Empty;
                    string xmlStringInit2 = string.Empty;
                    string xmlStringInit3 = string.Empty;
                    //using (StreamReader sr = new StreamReader(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "/ZWIM_MBGMCR2-oneline-mapping3.xml"))
                    string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    //int noOfTimes = 2;

                    //for (int j = 0; j < noOfTimes; j++)
                    //{
                    using (StreamReader sr = new StreamReader(dir + "/ZIM_LOISMO.xml"))
                    {
                        xmlStr = sr.ReadToEnd();
                        if (j == 0)
                        {
                            sbInit.AppendFormat(xmlStr, LOGDAT, LOGTIM, WERKS[0], SNDPRN, RCVPOR, RCVPRN, REFGRP[0]);

                            //xmlStringInit1 = fulldata + xmlStringInit1;
                        }
                        //else if(j == 1)
                        //{
                        //    sbInit.AppendFormat(xmlStr, LOGDAT, LOGTIM, WERKS[1], SNDPRN, RCVPOR, RCVPRN, REFGRP[1]);

                        //}
                        else
                        {
                            sbInit.AppendFormat(xmlStr, LOGDAT, LOGTIM, WERKS[1], SNDPRN, RCVPOR, RCVPRN, REFGRP[1]);
                        }


                    }
                    xmlStringInit = sbInit.ToString();

                    //}

                    //        for (int j = 0; j < noOfTimes; j++)
                    //        {
                    //    using (StreamReader sr = new StreamReader(dir + "/ZIM_LOISMO.xml"))
                    //    {
                    //        xmlStr = sr.ReadToEnd();
                    //        if (j != 1)
                    //        {
                    //            sbInit.AppendFormat(xmlStr, LOGDAT, LOGTIM, WERKS[0], SNDPRN, RCVPOR, RCVPRN);
                    //            xmlStringInit1 = sbInit.ToString();
                    //            //xmlStringInit1 = fulldata + xmlStringInit1;
                    //        }
                    //        else
                    //        {
                    //            sbInit.AppendFormat(xmlStr, LOGDAT, LOGTIM, WERKS[1], SNDPRN, RCVPOR, RCVPRN);
                    //            xmlStringInit2 = sbInit.ToString();
                    //        }


                    //    }
                    //    if (j != 1)
                    //    {

                    //    }
                    //    else
                    //    {
                    //        xmlStringInit = fulldata + xmlStringInit2 + fulldata2;
                    //    }

                    //}


                    //if (j != 1)
                    //{
                    //    using (StreamReader sr = new StreamReader(dir + "/ZIM_LOISMO.xml"))
                    //    {
                    //        xmlStr = sr.ReadToEnd();
                    //        sbInit.AppendFormat(xmlStr, LOGDAT, LOGTIM, WERKS[0], SNDPRN, RCVPOR, RCVPRN);
                    //    }
                    //    xmlStringInit1 = sbInit.ToString();
                    //}
                    //else
                    //{
                    //    using (StreamReader sr = new StreamReader(dir + "/ZIM_LOISMO.xml"))
                    //    {
                    //        xmlStr = sr.ReadToEnd();
                    //        sbInit.AppendFormat(xmlStr, LOGDAT, LOGTIM, WERKS[1], SNDPRN, RCVPOR, RCVPRN);
                    //    }
                    //    xmlStringInit2 = sbInit.ToString();

                    //}
                    //xmlStringInit = xmlStringInit1 + xmlStringInit2;




                    List<WMInventoryOnHandBO> target = dtResponse.AsEnumerable()
                        .Select(row => new WMInventoryOnHandBO
                        {
                            Organization = "SolvaySDI",
                            SharedSecret = "SolvaySDI",
                            TimeStamp = System.DateTime.Now.ToString(),
                            IDOC_TYPE = "MBGMCR03",
                            xmlString = xmlStringInit


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


                        m_oLogger.LogMessage("postWMInventoryOnHandData", "POST WM Inventory On Hand to Solvay starts here");
                        m_oLogger.LogMessage("postWMInventoryOnHandData", "POST WM Inventory On Hand Data" + resultSet.ToString());
                        //m_oLogger.LogMessage("postWMReceiptMappingData", "POST WMMapping Data URL : https://10.118.13.27:8243/SDIOutboundWMReceiptAPI/v1_0");
                        m_oLogger.LogMessage("postWMInventoryOnHandData", "POST WM Inventory On Hand Data URL : " + serviceURL);

                        string basicAuthBase641;
                        basicAuthBase641 = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", authorization, authorization)));
                        //req.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641))
                        client.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641));
                        //client.Headers.Add("Authorization: Basic " + authorization);
                        client.Headers.Add("Content-Type:application/json");
                        //client.Headers.Add("Accept:application/json");
                        //System.Net.ServicePointManager.CertificatePolicy = new AlwaysIgnoreCertPolicy();
                        //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                        //var result = client.UploadString("https://10.118.13.27:8243/SDIOutboundWMReceiptAPI/v1_0", resultSet.ToString());
                        var result = client.UploadString(serviceURL, resultSet.ToString());

                        // Console.WriteLine(result);
                        var parsed = JObject.Parse(result);
                        strResponse = parsed.SelectToken("RequestStatus").Value<string>();

                        m_oLogger.LogMessage("postWMInventoryOnHandData", "POST WMInventoryOnHand data to Solvay server status " + strResponse);

                        if (strResponse.ToUpper() != "SUCCESS")
                        {
                            //break;
                        }
                        // strResponse = JsonConvert.SerializeObject(result);
                    }

                    //    }
                    //}
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

                m_oLogger.LogMessageWeb("postWMInventoryOnHandData", "Error trying to POST data to Solvay server.", responseErrorText); //ex
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


