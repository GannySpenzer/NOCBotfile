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
                  string jsonSampleData = @"{	
	'_postporeceipt_batch_req': {
		'_postporeceipt': [{
			'XXPMC_SDI_RECORD_ID': '100',
			'PROCESSING_STATUS_CODE': '',
			'RECEIPT_SOURCE_CODE': '',
			'TRANSACTION_TYPE': '',
			'VENDOR_ID': '',
			'EXPECTED_RECEIPT_DATE': '',
			'VALIDATION_FLAG': 'T',
			'TRANSACTION_DATE': '',
			'PROCESSING_MODE_CODE': '',
			'TRANSACTION_STATUS_CODE': '',
			'PO_HEADER_ID': '',
			'PO_LINE_ID': '',
			'ITEM_ID': '',
			'QUANTITY': '',
			'UNIT_OF_MEASURE': '',
			'PO_LINE_LOCATION_ID': '',
			'AUTO_TRANSACT_CODE': '',
			'TO_ORGANIZATION_CODE': '',
			'SOURCE_DOCUMENT_CODE': '',
			'DOCUMENT_NUM': '',
			'DESTINATION_TYPE_CODE': '',
			'DELIVER_TO_PERSON_ID': '',
			'DELIVER_TO_LOCATION_ID': '',
			'SUBINVENTORY': ''
		}]
	}
}";
                POReceiptsMappingDAL objPOReceiptsMappingDAL = new POReceiptsMappingDAL();
                m_oLogger.LogMessage("getPOReceiptMappingData", "Getting PO Receipt Mapping Data starts here");
                dtResponse = objPOReceiptsMappingDAL.getPOReceiptMappingData(m_oLogger);
                if (dtResponse.Rows.Count != 0)
                {
                    List<POReceiptsMappingBO> target = dtResponse.AsEnumerable()
                        .Select(row => new POReceiptsMappingBO
                        {
                            XXPMC_SDI_RECORD_ID = "101",
                            PROCESSING_STATUS_CODE = "",
                            RECEIPT_SOURCE_CODE = (String)(row["RECEIPT_SOURCE"]),
                            TRANSACTION_TYPE = (String)(row["TRANS_TYPE"]),
                            VENDOR_ID = ((Decimal)(row["ISA_VENDOR_NUM"])).ToString(),
                            EXPECTED_RECEIPT_DATE = ((DateTime)(row["ISA_RECEIVING_DATE"])).ToString("yyyy/MM/dd"),
                          //  EXPECTED_RECEIPT_DATE ="",
                            VALIDATION_FLAG = "T",
                           // TRANSACTION_DATE = "",
                            TRANSACTION_DATE = ((DateTime)(row["TRANSACTION_DATE"])).ToString("yyyy/MM/dd"),
                            PROCESSING_MODE_CODE = "",
                            TRANSACTION_STATUS_CODE = ((Decimal)(row["ISA_STATUS_NUM"])).ToString(),
                            PO_HEADER_ID = (String)(row["ISA_CUST_PO_ID"]),
                            PO_LINE_ID = ((Decimal)(row["ISA_CUST_PO_LINE"])).ToString(),
                            ITEM_ID = ((Decimal)(row["ITEM_FIELD_N15_A"])).ToString(),
                            UNIT_OF_MEASURE = (String)(row["ISA_CUSTOMER_UOM"]),
                            PO_LINE_LOCATION_ID = "",
                            AUTO_TRANSACT_CODE = (String)(row["ISA_AUTO_TRANS_CD"]) == " " ? "" : (String)(row["ISA_AUTO_TRANS_CD"]),
                           // AUTO_TRANSACT_CODE = "",
                            TO_ORGANIZATION_CODE = "",
                            SOURCE_DOCUMENT_CODE = (String)(row["SOURCE_DOC"]),
                            DOCUMENT_NUM = ((Decimal)(row["ISA_CUST_PO_LINE"])).ToString(),
                            DESTINATION_TYPE_CODE = (String)(row["ISA_DEST_TYPE_CODE"]),
                            DELIVER_TO_PERSON_ID = (String)(row["RECIPIENT"]) == "|" ? "" : (String)(row["RECIPIENT"]),
                            DELIVER_TO_LOCATION_ID = (String)(row["ISA_UNLOADING_PT"]) == "|" ? "" : (String)(row["ISA_UNLOADING_PT"]),                      
                            SUBINVENTORY = "",
                            QUANTITY = ((Decimal)(row["QTY"])).ToString()                              
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
    }
}
