using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POMapping;

namespace POReceiptsMapping
{
    class POReceiptsMappingAPIAccess
    {
        /// <summary>
        /// POST Purchase order data to the PMC service
        /// </summary>
        /// <returns></returns>
        public string postPOReceiptMappingData(Logger m_oLogger)
        {
            var strResponse = " ";
            DataTable dtResponse = new DataTable();
            try
            {
                 
                POReceiptsMappingDAL objPOReceiptsMappingDAL = new POReceiptsMappingDAL();
                m_oLogger.LogMessage("getPOReceiptMappingData", "Getting PO Receipt Mapping Data starts here");
                dtResponse = objPOReceiptsMappingDAL.getPOReceiptMappingData(m_oLogger);
                if (dtResponse.Rows.Count != 0)
                {
                    List<POReceiptsMappingBO> target = dtResponse.AsEnumerable()
                        .Select(row => new POReceiptsMappingBO
                        {
                            XXPMC_SDI_RECORD_ID = ((Decimal)(row["ISA_IDENTIFIER"])).ToString(),
                            PROCESSING_STATUS_CODE = ReplacePipe((String)(row["STATUS_DESCR"])),
                            RECEIPT_SOURCE_CODE = ReplacePipe((String)(row["RECEIPT_SOURCE"])),
                           // TRANSACTION_TYPE = ReplacePipe((String)(row["TRANS_TYPE"])),
                            HEADER_TRANSACTION_TYPE = ReplacePipe((String)(row["HDR_TRANS_TYPE"])),
                            VENDOR_ID = ReplacePipe((String)(row["ISA_VENDOR_NUM"])),
                            EXPECTED_RECEIPT_DATE = ((DateTime)(row["ISA_RECEIVING_DATE"])).ToString("yyyy/MM/dd HH:mm:ss"),
                            VALIDATION_FLAG = ReplacePipe((String)(row["VALID_FLAG"])),
                            TRANSACTION_DATE = ((DateTime)(row["TRANSACTION_DATE"])).ToString("yyyy/MM/dd HH:mm:ss"),
                            PROCESSING_MODE_CODE = ReplacePipe((String)(row["PROC_DESCR"])),
                            //STATUS = ReplacePipe((String)(row["STATUS1"])),
                            EBS_PO_NUMBER = ReplacePipe((String)(row["ISA_CUST_PO_ID"])),
                            EBS_PO_LINE_NUMBER = ReplacePipe((String)(row["CUSTOMER_PO_LINE"])),
                            LINE_TRANSACTION_TYPE = ReplacePipe((String)(row["TRANSACTION_NAME"])),
                            ITEM = ReplacePipe((String)(row["ISA_ITEM"])),
                            ITEM_ID = ReplacePipe((String)(row["CUSTOMER_ITEM_NBR"])),
                            QUANTITY = ((Decimal)(row["QTY"])).ToString(),
                            UNIT_OF_MEASURE = ReplacePipe((String)(row["ISA_CUSTOMER_UOM"])),
                            EBS_PO_LINE_LOC_NBR = ReplacePipe((String)(row["PO_LINE_OPT"])),
                            AUTO_TRANSACT_CODE = ReplacePipe((String)(row["ISA_AUTO_TRANS_CD"])),
                            TO_ORGANIZATION_CODE = ReplacePipe((String)(row["PLANT"])),
                            SOURCE_DOCUMENT_CODE = ReplacePipe((String)(row["SOURCE_DOC"])),
                            DOCUMENT_NUM = ReplacePipe((String)(row["PO_ID"])),
                            DESTINATION_TYPE_CODE = ReplacePipe((String)(row["ISA_DEST_TYPE_CODE"])),
                            DELIVER_TO_PERSON_ID = ReplacePipe((String)(row["RECIPIENT"])),
                            DELIVER_TO_LOCATION_CODE = ReplacePipe((String)(row["ISA_UNLOADING_PT"])),
                            DELIVER_TO_LOCATION_ID = ReplacePipe((String)(row["DELIVERY_OPT"])),
                            SUBINVENTORY = ReplacePipe((String)(row["SUB_ITEM_ID"])),
                            WIP_ENTITY_ID = ReplacePipe((String)(row["ISA_WORK_ORDER"])),
                            WIP_ENTITY_NAME = ReplacePipe((String)(row["ORDER_NO"])),
                            WIP_OPERATION_SEQ_NUM = ReplacePipe((String)(row["ACTIVITY_ID"])),
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
                            TRANS_STATUS_DESCRIPTION = row["ISA_COMMENTS_1333"] == DBNull.Value ? null : ReplacePipe((String)(row["ISA_COMMENTS_1333"])),
                            TRANSACTION_STATUS  = ReplacePipe((String)(row["STATUS_MSG"])),
                            TRANSACTION_STATUS_CODE = "PENDING"
                        }).ToList();




                    string jsontest = JsonConvert.SerializeObject(new
                    {
                        _postporeceipt = target
                    });


                    StringBuilder sb = new StringBuilder();
                    sb.Append("{'_postporeceipt_batch_req':");
                    sb.Append(jsontest);
                    sb.Append("}");

                    JObject resultSet = JObject.Parse(sb.ToString());
                    //JObject resultSet = JObject.Parse(jsonSampleData);
                    using (var client = new WebClient())
                    {
                        m_oLogger.LogMessage("postPOReceiptMappingData", "POST PO Mapping Data to PMC starts here");
                        m_oLogger.LogMessage("postPOReceiptMappingData", "POST POMapping Data" + resultSet.ToString());
                        m_oLogger.LogMessage("postPOReceiptMappingData", "POST POMapping Data URL : https://10.118.13.27:8243/SDIOutboundPOReceiptAPI/v1_0");

                        client.Headers.Add("Authorization: Basic YWRtaW46YWRtaW4=");
                        client.Headers.Add("Content-Type:application/json");
                        client.Headers.Add("Accept:application/json");
                        System.Net.ServicePointManager.CertificatePolicy = new AlwaysIgnoreCertPolicy();
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var result = client.UploadString("https://10.118.13.27:8243/SDIOutboundPOReceiptAPI/v1_0", resultSet.ToString());
                       // var result = client.UploadString("https://10.118.13.27:8243/SDIOutboundPOReceiptAPI/v1_0", resultSet.ToString());
                      
                        // Console.WriteLine(result);
                        var parsed = JObject.Parse(result);
                        strResponse = parsed.SelectToken("REQUEST_STATUS").Value<string>();
                        m_oLogger.LogMessage("postPOReceiptMappingData", "POST POMapping data to PMC server status " + strResponse);

                        // strResponse = JsonConvert.SerializeObject(result);
                    }
                }

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("postPOReceiptMappingData", "Error trying to POST data to PMS server.", ex);
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
