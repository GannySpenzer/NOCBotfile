using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POIssueMapping;
using System.Configuration;
using System.IO;

namespace POIssueMapping
{
    class POIssueMappingAPIAccess
    {
        /// <summary>
        /// POST Purchase order Issue data to the PMC service
        /// </summary>
        /// <returns></returns>
        // Logger m_oLogger;
        public string postPOIssueMappingData(Logger m_oLogger)
        {
            string testOrProd = " ";
            string authorization = " ";
            string serviceURL = " ";
            string responseErrorText = " ";

            var strResponse = " ";
            DataTable dtResponse = new DataTable();
            try
            {

                POIssueMappingDAL objPOIssueMappingDAL = new POIssueMappingDAL();

                m_oLogger.LogMessage("getPOIssueMappingData", "Getting PO Issue Mapping Data starts here");
                dtResponse = objPOIssueMappingDAL.getPOIssueMappingData(m_oLogger);
                if (dtResponse.Rows.Count != 0)
                {
                    List<POIssueMappingBO> target = dtResponse.AsEnumerable()
                        .Select(row => new POIssueMappingBO
                        {
                            XXPMC_SDI_RECORD_ID = ((Decimal)(row["ISA_IDENTIFIER"])).ToString(),
                            SOURCE_CODE =ReplacePipe((String)(row["ISA_SOURCE_CODE"])),
                            ORGANIZATION_CODE =ReplacePipe((String)(row["PLANT"])),
                            ITEM =ReplacePipe((String)(row["ISA_ITEM"])),
                            //  INVENTORY_ITEM_ID = ((Decimal)(row["ITEM_FIELD_N15_A"])).ToString(), // "ITEM_FIELD_N15_A is missing in table"
                            INVENTORY_ITEM_ID = ReplacePipe((String)(row["CUSTOMER_ITEM_NBR"])),
                            ORGANIZATION_ID = ReplacePipe((String)(row["ISA_ORGANIZ_ID"])),
                            TRANSACTION_QUANTITY = ((Decimal)(row["QTY"])).ToString(),
                            PRIMARY_QUANTITY = ((Decimal)(row["QUANTITY"])).ToString(),
                            TRANSACTION_UOM =ReplacePipe((String)(row["ISA_CUSTOMER_UOM"])),
                            TRANSACTION_DATE = ((DateTime)(row["TRANSACTION_DATE"])).ToString("yyyy/MM/dd"),
                            SUBINVENTORY_CODE =ReplacePipe((String)(row["SUB_ITEM_ID"])),
                            LOCATOR_ID = ReplacePipe((String)(row["ISA_LOCATOR_ID"])),
                            LOCATOR_NAME =ReplacePipe((String)(row["ISA_LOCATOR_NAME"])),
                            WIP_ENTITY_NAME =ReplacePipe((String)(row["ORDER_NO"])),
                            WIP_ENTITY_ID = ((Decimal)(row["MAINJOBINSTANCE"])).ToString(),
                            TRANSACTION_TYPE_NAME =ReplacePipe((String)(row["ISA_TRANS_NAME"])),
                            TRANSACTION_TYPE_ID =ReplacePipe((String)(row["HDR_TRANS_TYPE"])),
                            //OPERATION_SEQ_NUM = ((Decimal)(row["MAINJOBSEQ"])).ToString(),
                            OPERATION_SEQ_NUM = ((String)(row["ACTIVITY_ID"])).ToString(),
                            TRANSACTION_REFERENCE =ReplacePipe((String)(row["REFERENCE"])),
                            ATTRIBUTE1 = ReplacePipe((String)(row["ISA_ATTRIBUTE_1"])),
                            ATTRIBUTE2 = ReplacePipe((String)(row["ISA_ATTRIBUTE_2"])),
                            ATTRIBUTE3 = ReplacePipe((String)(row["ISA_ATTRIBUTE_3"])),
                            ATTRIBUTE4 = ReplacePipe((String)(row["ISA_ATTRIBUTE_4"])),
                            ATTRIBUTE5 = ReplacePipe((String)(row["ISA_ATTRIBUTE_5"])),
                            ATTRIBUTE6 = ReplacePipe((String)(row["ISA_ATTRIBUTE_6"])),
                            ATTRIBUTE7 = ReplacePipe((String)(row["ISA_ATTRIBUTE_7"])),
                            ATTRIBUTE8 = ReplacePipe((String)(row["ISA_ATTRIBUTE_8"])),
                            ATTRIBUTE9 = ReplacePipe((String)(row["ISA_ATTRIBUTE_9"])),
                            ATTRIBUTE10 = ReplacePipe((String)(row["ISA_ATTRIBUTE_10"])),
                            TRANS_STATUS_DESCRIPTION = row["ISA_COMMENTS_1333"] == DBNull.Value ? null :ReplacePipe((String)(row["ISA_COMMENTS_1333"])),
                            TRANSACTION_STATUS =ReplacePipe((String)(row["STATUS_MSG"])),
                            SOURCE_HEADER_ID = ReplacePipe((String)(row["ISA_SOURCE_HDR_ID"])),
                            SOURCE_LINE_ID = ReplacePipe((String)(row["ISA_SOURCE_LN_ID"])),
                            TRANSACTION_ACTION_ID = ReplacePipe((String)(row["ISA_TRANS_ACTION"])),
                            TRANSACTION_SOURCE_ID = ReplacePipe((String)(row["ISA_TRANS_SRC_ID"])),
                            TRANSACTION_SOURCE_TYPE_ID = ReplacePipe((String)(row["ISA_TRANS_SRC_TYPE"])),
                            REASON_ID = ReplacePipe((String)(row["ISA_REASON_ID"])),
                            REASON_NAME = ReplacePipe((String)(row["REASON_CONFERRED"]))

                        }).ToList();



                    string jsontest = JsonConvert.SerializeObject(new
                     {
                         _postmaterialissuereturncyclecount = target
                     });



                    StringBuilder sb = new StringBuilder();

                    sb.Append("{'_postmaterialissuereturncyclecount_batch_req':");
                    sb.Append(jsontest);
                    sb.Append("}");

                    JObject resultSet = JObject.Parse(sb.ToString());
                   
                    using (var client = new WebClient())
                    {

                        string strTestOrProd = ConfigurationManager.AppSettings["TestOrProd"];
                        if (strTestOrProd == "TEST")
                        {
                            serviceURL = ConfigurationManager.AppSettings["testServiceURL"];
                            authorization = ConfigurationManager.AppSettings["testAuthorization"];
                        }
                        else
                        {
                            serviceURL = ConfigurationManager.AppSettings["prodServiceURL"];
                            authorization = ConfigurationManager.AppSettings["prodAuthorization"];
                        }


                        m_oLogger.LogMessage("postMatIssueMappingData", "POST PO Issue Mapping Data to PMC starts here");
                        //client.Headers.Add("Authorization: Basic YWRtaW46YWRtaW4=");
                        client.Headers.Add("Authorization: Basic " + authorization);
                        client.Headers.Add("Content-Type:application/json");
                        client.Headers.Add("Accept:application/json");
                        System.Net.ServicePointManager.CertificatePolicy = new AlwaysIgnoreCertPolicy();
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        m_oLogger.LogMessage("postMatIssueMappingData", "POST PO Issue Mapping Data" + resultSet.ToString());
                        //m_oLogger.LogMessage("postMatIssueMappingData", "POST PO Issue Mapping Data URL : https://10.118.13.27:8243/SDIOutboundPurchaseOrderAPI/v1_0");
                        m_oLogger.LogMessage("postMatIssueMappingData", "POST PO Issue Mapping Data URL : " + serviceURL);

                        //Need to change the URL after service available from client side

                        //var result = client.UploadString("https://10.118.13.27:8243/SDIOutboundMaterialIssueReturnCycleCountAPI/v1_0", resultSet.ToString());
                        var result = client.UploadString(serviceURL, resultSet.ToString());
                        
                        var parsed = JObject.Parse(result);
                        strResponse = parsed.SelectToken("REQUEST_STATUS").Value<string>();
                        m_oLogger.LogMessage("postMatIssueMappingData", "POST Material Issue data to PMC server status " + strResponse);
                    }
                }

            }
                                            //catch (Exception ex)
            catch (WebException ex)
            {

                var responseStream = ex.Response.GetResponseStream();
                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream ))
                    {
                        responseErrorText = reader.ReadToEnd();
                    }
                }

                m_oLogger.LogMessageWeb("postMatIssueMappingData", "Error trying to POST data to PMC server.", responseErrorText); //ex
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
