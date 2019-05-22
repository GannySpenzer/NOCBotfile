using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.IO;
using UoCMapping;


namespace UoCMinMaxMapping
{
    class UoCMinMaxMappingAPIAccess
    {
        /// <summary>
        /// POST Receiving data to the Solvay service
        /// </summary>
        /// <returns></returns>
        public string postUoCMinMaxMappingData(Logger m_oLogger)
        {
            string testOrProd = " ";
            string authorization = " ";
            string username = " ";
            string password = " ";
            string serviceURL = " ";
            var strResponse = " ";
            string responseErrorText = " ";

            StringBuilder sbInit = new StringBuilder();
            string xmlStr = string.Empty;
            string xmlStringInit = string.Empty;

            DataTable dtResponse = new DataTable();
            try
            {

                string carriagereturn = "\r\n";

                string header = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns=""http://www.ibm.com/maximo"">" + carriagereturn +
                                 "<soapenv:Header/>" + carriagereturn +
                                 "   <soapenv:Body>" + carriagereturn +
                                 "<UpdateUCSDIINVENTORY>" + carriagereturn +
                                 "<UCSDIINVENTORYSet>" + carriagereturn;

                UoCMinMaxMappingDAL objUoCMinMaxMappingDAL = new UoCMinMaxMappingDAL();
                m_oLogger.LogMessage("getUoCMinMaxMappingData", "Getting UoC MinMax Mapping Data starts here");
                dtResponse = objUoCMinMaxMappingDAL.getUoCMinMaxMappingData(m_oLogger);
                if (dtResponse.Rows.Count != 0)
                {

                   for (int i = 0; i < dtResponse.Rows.Count; i++)
                   {
                       

                           DataRow rowInit;
                           rowInit = dtResponse.Rows[i];

                           string DOC_NUM = rowInit["ISA_IDENTIFIER"].ToString();
                           DOC_NUM = DOC_NUM.PadLeft(14, '0');//i.e. "0000000000000004"
                           string LOGDAT = System.DateTime.Now.ToString("yyyyMMdd");
                           string LOGTIM = System.DateTime.Now.ToString("HHmmss");
                           string SITEID = rowInit["PLANT"].ToString();
                           //string REFMES = DOC_NUM; //rowInit["ISA_IDENTIFIER"].ToString();
                           DateTime PSTNG_DATEcnv = Convert.ToDateTime(rowInit["ADD_DTTM"]);
                           string PSTNG_DATE = PSTNG_DATEcnv.ToString("yyyyMMdd");
                           string DOC_DATE = PSTNG_DATEcnv.ToString("yyyyMMdd");
                           string REF_DOC_NO = DOC_NUM;                              //?
                           //string GM_CODE = "01";
                           string ITEMNUM = rowInit["ISA_ITEM"].ToString();
                           //string PLANT = REFGRP;
                           string LOCATION = rowInit["IN_STOR_LOCATION"].ToString();
                           string MAXLEVEL = rowInit["QTY_MAXIMUM"].ToString();
                           string MINLEVEL = rowInit["REORDER_POINT"].ToString();
                           string ORDERQTY = rowInit["REORDER_QTY"].ToString();
                           string ORDERUNIT = rowInit["ISA_CUSTOMER_UOM"].ToString();
                           string AVGCOST = rowInit["TL_COST"].ToString();

                           //string ITEM_TEXT = rowInit["DESCR254"].ToString();
                           //string MVT_IND = "B";

                           using (StreamReader sr = new StreamReader(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "/UoCMinMaxInventory.txt"))
                           {
                               xmlStr = sr.ReadToEnd();
                               sbInit.AppendFormat(xmlStr, ITEMNUM, LOCATION, MAXLEVEL, MINLEVEL, ORDERQTY, ORDERUNIT, SITEID, AVGCOST);
                               xmlStringInit = sbInit.ToString() + carriagereturn;
                           }
                   }
                }

                string footer = @"         </UCSDIINVENTORYSet>" + carriagereturn +
                                       "</UpdateUCSDIINVENTORY>" + carriagereturn +
                                       "</soapenv:Body>" + carriagereturn +
                                       "</soapenv:Envelope>" + carriagereturn;

                       //List<UoCMinMaxMappingBO> target = dtResponse.AsEnumerable()
                       //    .Select(row => new UoCMinMaxMappingBO
                       //    {
                       //        Organization = "SolvaySDI",
                       //        SharedSecret = "SolvaySDI",
                       //        TimeStamp = System.DateTime.Now.ToString(),
                       //        IDOC_TYPE = "MBGMCR03",
                       //        xmlString = xmlStringInit
                       //        //XXPMC_SDI_RECORD_ID = ((Decimal)(row["ISA_IDENTIFIER"])).ToString(),
                       //        //PROCESSING_STATUS_CODE = ReplacePipe((String)(row["STATUS_DESCR"])),
                       //        //RECEIPT_SOURCE_CODE = ReplacePipe((String)(row["RECEIPT_SOURCE"])),
                       //        //// TRANSACTION_TYPE = ReplacePipe((String)(row["TRANS_TYPE"])),
                       //        //HEADER_TRANSACTION_TYPE = ReplacePipe((String)(row["HDR_TRANS_TYPE"])),
                       //        //VENDOR_ID = ReplacePipe((String)(row["ISA_VENDOR_NUM"])),
                       //        //EXPECTED_RECEIPT_DATE = ((DateTime)(row["ISA_RECEIVING_DATE"])).ToString("yyyy/MM/dd HH:mm:ss"),
                       //        //VALIDATION_FLAG = ReplacePipe((String)(row["VALID_FLAG"])),
                       //        //TRANSACTION_DATE = ((DateTime)(row["TRANSACTION_DATE"])).ToString("yyyy/MM/dd HH:mm:ss"),
                       //        //PROCESSING_MODE_CODE = ReplacePipe((String)(row["PROC_DESCR"])),
                       //        ////STATUS = ReplacePipe((String)(row["STATUS1"])),
                       //        //EBS_PO_NUMBER = ReplacePipe((String)(row["ISA_CUST_PO_ID"])),
                       //        //EBS_PO_LINE_NUMBER = ReplacePipe((String)(row["CUSTOMER_PO_LINE"])),
                       //        //LINE_TRANSACTION_TYPE = ReplacePipe((String)(row["ISA_TRANS_NAME"])),
                       //        //ITEM = ReplacePipe((String)(row["ISA_ITEM"])),
                       //        //ITEM_ID = ReplacePipe((String)(row["CUSTOMER_ITEM_NBR"])),
                       //        //QUANTITY = ((Decimal)(row["QTY"])).ToString(),
                       //        //UNIT_OF_MEASURE = ReplacePipe((String)(row["ISA_CUSTOMER_UOM"])),
                       //        //EBS_PO_LINE_LOC_NBR = ReplacePipe((String)(row["ISA_LOCATOR_ID"])),
                       //        //AUTO_TRANSACT_CODE = ReplacePipe((String)(row["ISA_AUTO_TRANS_CD"])),
                       //        //TO_ORGANIZATION_CODE = ReplacePipe((String)(row["PLANT"])),
                       //        //SOURCE_DOCUMENT_CODE = ReplacePipe((String)(row["SOURCE_DOC"])),
                       //        //DOCUMENT_NUM = ReplacePipe((String)(row["PO_ID"])),
                       //        //DESTINATION_TYPE_CODE = ReplacePipe((String)(row["ISA_DEST_TYPE_CODE"])),
                       //        //DELIVER_TO_PERSON_ID = ReplacePipe((String)(row["RECIPIENT"])),
                       //        //DELIVER_TO_LOCATION_CODE = ReplacePipe((String)(row["ISA_UNLOADING_PT"])),
                       //        //DELIVER_TO_LOCATION_ID = ReplacePipe((String)(row["DELIVERY_OPT"])),
                       //        //SUBINVENTORY = ReplacePipe((String)(row["SUB_ITEM_ID"])),
                       //        //WIP_ENTITY_ID = ReplacePipe((String)(row["ISA_WORK_ORDER"])),
                       //        //WIP_ENTITY_NAME = ReplacePipe((String)(row["ORDER_NO"])),
                       //        //WIP_OPERATION_SEQ_NUM = ReplacePipe((String)(row["ACTIVITY_ID"])),
                       //        //ATTRIBUTE1 = ReplacePipe((String)(row["ISA_ATTRIBUTE_1"])),
                       //        //ATTRIBUTE2 = ReplacePipe((String)(row["ISA_ATTRIBUTE_2"])),
                       //        //TRANS_STATUS_DESCRIPTION = row["ISA_COMMENTS_1333"] == DBNull.Value ? null : ReplacePipe((String)(row["ISA_COMMENTS_1333"])),
                       //        //TRANSACTION_STATUS = ReplacePipe((String)(row["STATUS_MSG"])),
                       //        //TRANSACTION_STATUS_CODE = "PENDING"
                       //    }).ToList();




                       //string jsontest = JsonConvert.SerializeObject(new
                       //{
                       //    _postUoCMinMax = target
                       //});
                       //string jsontest = JsonConvert.SerializeObject(target, Formatting.None);
                       //jsontest = jsontest.Remove(0, 1);
                       //jsontest = jsontest.Remove(jsontest.Length - 1);

                       //StringBuilder sb = new StringBuilder();
                       //sb.Append("{'_postUoCMinMax_batch_req':");
                       //sb.Append("{");

                       //sb.Append(jsontest);

                       //sb.Append("}");

                       //JObject resultSet = JObject.Parse(sb.ToString());
                       //string resultSet = jsontest;

                       string resultSet = header + xmlStringInit + footer;

                       //JObject resultSet = JObject.Parse(jsonSampleData);
                       using (var client = new WebClient())
                       {
                           testOrProd = ConfigurationManager.AppSettings["TestOrProd"];
                           if (testOrProd == "TEST")
                           {
                               serviceURL = ConfigurationManager.AppSettings["testServiceURL"];
                               //authorization = ConfigurationManager.AppSettings["testAuthorization"];
                               username = ConfigurationManager.AppSettings["testUsername"];
                               password = ConfigurationManager.AppSettings["testPassword"];
                           }
                           else
                           {
                               serviceURL = ConfigurationManager.AppSettings["prodServiceURL"];
                               authorization = ConfigurationManager.AppSettings["prodAuthorization"];
                           }


                           m_oLogger.LogMessage("postUoCMinMaxMappingData", "POST UoCMinMax Mapping Data to Solvay starts here");
                           m_oLogger.LogMessage("postUoCMinMaxMappingData", "POST UoCMinMaxMapping Data" + resultSet.ToString());
                           //m_oLogger.LogMessage("postUoCMinMaxMappingData", "POST WMMapping Data URL : https://10.118.13.27:8243/SDIOutboundWMReceiptAPI/v1_0");
                           m_oLogger.LogMessage("postUoCMinMaxMappingData", "POST UoCMinMaxMapping Data URL : " + serviceURL);

                           string basicAuthBase641;
                           //basicAuthBase641 = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", authorization, authorization)));
                           basicAuthBase641 = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", username , password)));

                           //req.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641))
                           client.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641));
                           //client.Headers.Add("Authorization: Basic " + authorization);
                           client.Headers.Add("Content-Type:application/json");
                           //client.Headers.Add("Accept:application/json");
                           //System.Net.ServicePointManager.CertificatePolicy = new AlwaysIgnoreCertPolicy();
                           //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                           var result = client.UploadString(serviceURL, resultSet.ToString());

                           // Console.WriteLine(result);
                           var parsed = JObject.Parse(result);
                           strResponse = parsed.SelectToken("RequestStatus").Value<string>();
                           m_oLogger.LogMessage("postUoCMinMaxMappingData", "POST UoCMinMaxMapping data to Solvay server status " + strResponse);

                           // strResponse = JsonConvert.SerializeObject(result);
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

                m_oLogger.LogMessageWeb("postUoCMinMaxMappingData", "Error trying to POST data to Solvay server.", responseErrorText); //ex
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
